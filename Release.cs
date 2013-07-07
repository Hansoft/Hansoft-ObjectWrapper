using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HPMSdk;

namespace Hansoft.ObjectWrapper
{
    /// <summary>
    /// Represents a Release item in the Schedule view of Hansoft.
    /// </summary>
    public class Release : Task
    {
        static Dictionary<int, CachedData> cachedData = new Dictionary<int, CachedData>();
        CachedData cache;

        internal class CachedData
        {
            internal DateTime remainingWorkdaysCachedUpdated = DateTime.MinValue;
            internal int remainingWorkDaysCached;

            // Index 0: Points, Index 1: Estimated Days, Index 2: WorkRemaining
            internal NormalizedBurndownHistory[] normalizedHistoryCached = new NormalizedBurndownHistory[3];

            // Index 0: Points, Index 1: Estimated Days, Index 2: WorkRemaining
            internal RawBurndownHistory[] rawHistoryCached = new RawBurndownHistory[3];

            internal Dictionary<int, double>[] predictedVelocityCache = new Dictionary<int, double>[3];
        }


        internal static Release GetRelease(HPMUniqueID uniqueID, HPMUniqueID uniqueTaskID)
        {
            return new Release(uniqueID, uniqueTaskID);
        }

        private Release(HPMUniqueID uniqueID, HPMUniqueID uniqueTaskID)
            : base(uniqueID, uniqueTaskID)
        {
            if (cachedData.ContainsKey(uniqueTaskID.m_ID))
                cache = cachedData[uniqueTaskID.m_ID];
            else
            {
                cache = new CachedData();
                cachedData.Add(uniqueTaskID.m_ID, cache);
            }
        }

        /// <summary>
        /// The project view that this release belongs to.
        /// </summary>
        public override ProjectView ProjectView
        {
            get
            {
                return Project.Schedule;
            }
        }

        /// <summary>
        /// The date of the release.
        /// </summary>
        public DateTime Date
        {
            get
            {
                HPMTaskTimeZones tzData = Session.TaskGetTimeZones(UniqueTaskID);
                return HPMUtilities.FromHPMDateTime(tzData.m_Zones[0].m_Start);
            }
            //TODO: Implement the setter.
        }

        /// <summary>
        /// Returns true if the release has been started (a burndown exists based on a sprint is tagged to the release)
        /// </summary>
        public bool Started
        {
            get
            {
                return Sprints.Count > 0;
            }
        }

        /// <summary>
        /// The start of the release, i.e., the first day of the earliest sprint that is tagged to this release.
        /// </summary>
        public DateTime Start
        {
            get
            {
                if (Started)
                    return Sprints.Min(s => s.Start);
                else
                    return DateTime.MinValue;
            }
        }


        /// <summary>
        /// The number of remaining working days before the release date counted from tomorrow.
        /// </summary>
        public int RemainingWorkDays
        {
            get
            {
                if (cache.remainingWorkDaysCached >= 0 && cache.remainingWorkdaysCachedUpdated != DateTime.Now.Date)
                {

                    cache.remainingWorkDaysCached = 0;

                    DateTime day = DateTime.Now.Date;
                    cache.remainingWorkdaysCachedUpdated = day;
                    day = day.AddDays(1);
                    DateTime end = Date.Date;
                    while (day <= end)
                    {
                        if (Project.IsWorkingDay(day))
                            cache.remainingWorkDaysCached += 1;
                        day = day.AddDays(1);
                    }
                }

                return cache.remainingWorkDaysCached;
            }
        }

        /// <summary>
        /// Get all items currently contributing to the burndown, i.e., the items that either are in a sprint tagged to
        /// the release, or items that are not committed to a sprint but tagged to the release in the product backlog.
        /// </summary>
        public List<ProductBacklogItem> ContributingItems
        {
            get
            {
                List<ProductBacklogItem> contributors = new List<ProductBacklogItem>();
                foreach (Sprint sprint in Sprints)
                    foreach (ProductBacklogItemInSprint item in sprint.DeepChildren.FindAll(item => item is ProductBacklogItemInSprint))
                        contributors.Add(item);

                // Note!!! This is inlined to optimize performance needed for assigning release risks.
                HPMTaskEnum tasks = Session.TaskRefUtilEnumChildren(Project.ProductBacklog.UniqueID, true);
                foreach (HPMUniqueID id in tasks.m_Tasks)
                {
                    if (Session.UtilIsIDValid(id) && !Session.TaskRefUtilHasChildren(id))
                    {
                        HPMUniqueID idTask = Session.TaskRefGetTask(id);
                        if (Session.TaskGetLinkedToSprint(idTask) == -1)
                        {
                            HPMTaskLinkedToMilestones relIDs = Session.TaskGetLinkedToMilestones(idTask);
                            foreach (HPMUniqueID relID in relIDs.m_Milestones)
                            {
                                if (relID.m_ID == UniqueID.m_ID)
                                {
                                    contributors.Add((ProductBacklogItem)Task.GetTask(id));
                                    break;
                                }
                            }
                        }
                    }
                }
                return contributors;
            }
        }

        enum HistoryKind {Points = 0, EstimatedDays = 1, WorkRemaining = 2}


        internal class RawBurndownHistory
        {
            ulong lastTimeEntry;
            DateTime[] times;
            double[] values;

            internal RawBurndownHistory(int length, ulong lastTimeEntry)
            {
                this.lastTimeEntry = lastTimeEntry;
                this.times = new DateTime[length];
                this.values = new double[length];
            }

            internal void InsertAt(uint index, DateTime dt, double value)
            {
                times[index] = dt;
                values[index] = value;
            }

            internal int Length
            {
                get { return values.Length; }
            }

            internal ulong LastTimeEntry
            {
                get { return lastTimeEntry; }
            }

            internal DateTime[] Times
            {
                get { return times; }
            }

            internal double[] Values
            {
                get { return values; }
            }
        }

        private RawBurndownHistory GetRawHistory(HistoryKind historyKind)
        {
            HPMDataHistoryGetHistoryParameters getHistoryParameters = new HPMDataHistoryGetHistoryParameters();
            if (historyKind == HistoryKind.Points)
                getHistoryParameters.m_FieldID = EHPMStatisticsField.ComplexityPoints;
            else if (historyKind == HistoryKind.EstimatedDays)
                getHistoryParameters.m_FieldID = EHPMStatisticsField.EstimatedIdealDays;
            getHistoryParameters.m_DataIdent0 = EHPMStatisticsScope.Milestone;
            getHistoryParameters.m_DataIdent1 = (uint)UniqueID.m_ID;

            int nTries = 0;
            HPMDataHistory history = null;
            while (nTries < 25)
            {
                // TODO: Consider to move the reponsibility to the client to register a callback instead. It really isn't kosher to do polling here.
                history = Session.DataHistoryGetHistory(getHistoryParameters);
                if (history == null)
                {
                    ++nTries;
                    System.Threading.Thread.Sleep(100);
                }
                else
                    break;
            }
            if (history != null && (cache.rawHistoryCached[(int)historyKind] == null  || history.m_Latests.m_Time > cache.rawHistoryCached[(int)historyKind].LastTimeEntry))
            {
                cache.rawHistoryCached[(int)historyKind] = new RawBurndownHistory(history.m_HistoryEntries.Length, history.m_Latests.m_Time);
                for (uint j = 0; j < history.m_HistoryEntries.Length; j += 1 )
                {
                    HPMDataHistoryEntry historyEntry = history.m_HistoryEntries[j];
                    ulong time = historyEntry.m_Time;

                    DateTime date = HPMUtilities.FromHPMDateTime(time);
                    if (historyEntry.m_bHasDataRecorded && historyEntry.m_EntryType == EHPMDataHistoryEntryType.Statistics_AbsoluteValue)
                    {
                        HPMVariantData data = Session.DataHistoryGetEntryData(history, j);
                        HPMStatisticsMultiFrequency value = Session.VariantDecode_HPMStatisticsMultiFrequency(data);
                        cache.rawHistoryCached[(int)historyKind].InsertAt(j, date, value.m_FrequencyEntries[1].m_FrequencyFP);
                    }
                }
                cache.normalizedHistoryCached[(int)historyKind] = null;
            }
            return cache.rawHistoryCached[(int)historyKind];
        }

        /// <summary>
        /// The historic burndown data based on points as stored in the Hansoft database
        /// </summary>
        internal RawBurndownHistory RawPointsHistory
        {
            get
            {
                return GetRawHistory(HistoryKind.Points);
            }
        }

        /// <summary>
        /// The historic burndown data based on points as stored in the Hansoft database
        /// </summary>
        internal RawBurndownHistory RawEstimatedDaysHistory
        {
            get
            {
                return GetRawHistory(HistoryKind.EstimatedDays);
            }
        }

        public class NormalizedBurndownHistory
        {
            DateTime start;
            DateTime end;
            int length;
            double[] values;

            internal NormalizedBurndownHistory(DateTime start, DateTime end)
            {
                this.start = start.Date;
                this.end = end.Date;

                length = (end - start).Days + 1;
                values = new double[length];
            }

            public double ValueAt(DateTime day)
            {
                return values[(day.Date - start).Days];
            }

            public int Length
            {
                get { return length; }
            }

            public DateTime Start
            {
                get { return start; }
            }

            public DateTime End
            {
                get { return end; }
            }

            public double[] Values
            {
                get { return values; }
            }

            internal void SetValueAt(int index, double value)
            {
                values[index] = value;
            }

            internal void PruneTo(int index)
            {
                if (index > 0)
                {
                    this.start = start.AddDays(index + 1);
                    length = length - (index+1);
                    double[] newValues = new double[length];
                    for (int i = 0; i < length; i += 1)
                    {
                        newValues[i] = values [i + index + 1];
                    }
                    values = newValues;
                }
            }
        }

        /// <summary>
        /// The historic burndown data based on points in a such a way that it reflects the burndown graph in the Hansoft client meaning,
        /// there will be a single value in the dataset from the start of the release to the current day. Values will be interpolated and/or pruned
        /// as necessary.  If no history exists null is returned.
        /// </summary>
        public NormalizedBurndownHistory NormalizedPointsHistory
        {
            get
            {
                if (Started)
                    return NormalizeHistory(HistoryKind.Points, GetRawHistory(HistoryKind.Points), RemainingPoints);
                else
                    return null;
            }
        }

        /// <summary>
        /// The historic burndown data based on estimated days in a such a way that it reflects the burndown graph in the Hansoft client meaning,
        /// there will be a single value in the dataset from the start of the release to the current day. Values will be interpolated and/or pruned
        /// as necessary. If no history exists null is returned.
        /// </summary>
        public NormalizedBurndownHistory NormalizedEstimatedDaysHistory
        {
            get
            {
                if (Started)
                    return NormalizeHistory(HistoryKind.EstimatedDays, GetRawHistory(HistoryKind.EstimatedDays), RemainingEstimatedDays);
                else
                    return null;
            }
        }

        private int NextDay(int current, RawBurndownHistory rawHistory)
        {
            int ind = ScanToLastDayEntry(current, rawHistory);
            return ind+1;
        }

        private int ScanToLastDayEntry(int current, RawBurndownHistory rawHistory)
        {
            int ind = current;
            while ( ind < (rawHistory.Length - 1) && rawHistory.Times[ind+1].Date == rawHistory.Times[ind].Date)
                ind += 1;
            return ind;
        }

        private NormalizedBurndownHistory NormalizeHistory(HistoryKind historyKind, RawBurndownHistory rawHistory, double remainingValue)
        {
            if (!Started || rawHistory == null)
            {
                cache.predictedVelocityCache = null;
                return null;
            }

            if (cache.normalizedHistoryCached[(int)historyKind] != null && cache.normalizedHistoryCached[(int)historyKind].End.Date == DateTime.Now.Date)
            {
                if (remainingValue != cache.normalizedHistoryCached[(int)historyKind].Values[cache.normalizedHistoryCached[(int)historyKind].Length - 1])
                {
                    cache.normalizedHistoryCached[(int)historyKind].SetValueAt(cache.normalizedHistoryCached[(int)historyKind].Length - 1, remainingValue);
                    cache.predictedVelocityCache = null;
                }
            }
            else
            {
                cache.predictedVelocityCache = null;
                DateTime end = DateTime.Now.Date;
                cache.normalizedHistoryCached[(int)historyKind] = new NormalizedBurndownHistory(Start, end);

                DateTime dayToCalculate = Start.Date;
                int pruneTo = -1;
                int rawInd = 0;

                for (int iValue = 0; iValue < cache.normalizedHistoryCached[(int)historyKind].Length; iValue += 1)
                {
                    if (dayToCalculate.Date == end.Date)
                    {
                        cache.normalizedHistoryCached[(int)historyKind].SetValueAt(iValue, remainingValue);
                        break;
                    }
                    if (dayToCalculate.Date < rawHistory.Times[0].AddDays(-1).Date)
                    {
                        pruneTo = iValue;
                        cache.normalizedHistoryCached[(int)historyKind].SetValueAt(iValue, 0);
                        dayToCalculate = dayToCalculate.AddDays(1);
                    }
                    else if (rawInd >= rawHistory.Length)
                    {
                        cache.normalizedHistoryCached[(int)historyKind].SetValueAt(iValue, rawHistory.Values[rawHistory.Length - 1]);
                        dayToCalculate = dayToCalculate.AddDays(1);
                    }
                    else if (dayToCalculate.Date == rawHistory.Times[rawInd].AddDays(-1).Date)
                    {
                        cache.normalizedHistoryCached[(int)historyKind].SetValueAt(iValue, rawHistory.Values[rawInd]);
                        dayToCalculate = dayToCalculate.AddDays(1);
                        rawInd = NextDay(rawInd, rawHistory);
                    }
                    else if (dayToCalculate.Date < rawHistory.Times[rawInd].Date.AddDays(-1))
                    {
                        // There is a missing value in the middle of the sequence interpolate a value
                        DateTime precedingDate = dayToCalculate.AddDays(-1);
                        double precedingValue = cache.normalizedHistoryCached[(int)historyKind].Values[iValue - 1];
                        DateTime followingDate = rawHistory.Times[rawInd].Date;
                        double followingValue = rawHistory.Values[rawInd];
                        if (Project.GetProject(MainProjectID).IsWorkingDay(dayToCalculate))
                        {
                            TimeSpan span = followingDate - precedingDate;
                            double delta = (followingValue - precedingValue) / span.Days;
                            cache.normalizedHistoryCached[(int)historyKind].SetValueAt(iValue, precedingValue + delta);
                            dayToCalculate = dayToCalculate.AddDays(1);
                        }
                        else
                        {
                            cache.normalizedHistoryCached[(int)historyKind].SetValueAt(iValue, precedingValue);
                            dayToCalculate = dayToCalculate.AddDays(1);
                        }
                    }
                }
                cache.normalizedHistoryCached[(int)historyKind].PruneTo(pruneTo);
            }

            return cache.normalizedHistoryCached[(int)historyKind];
        }

        /// <summary>
        /// The predicted veclocity in points. The length of the average is determined by the sprint prediction method set
        /// in the project options in Hansoft.
        /// </summary>
        public double PredictedPointsVelocity
        {
            get
            {
                NormalizedBurndownHistory normalizedPointsHistory = NormalizedPointsHistory;
                if (normalizedPointsHistory!= null && normalizedPointsHistory.Length > 1)
                    return GetPredictedVelocity(HistoryKind.Points, normalizedPointsHistory, true);
                else
                    return 0;
            }
        }

        /// <summary>
        /// The predicted veclocity in estimated datys. The length of the average is determined by the sprint prediction method set
        /// in the project options in Hansoft.
        /// </summary>
        public double PredictedEstimatedDaysVelocity
        {
            get
            {
                NormalizedBurndownHistory normalizedEstimatedDaysHistory = NormalizedEstimatedDaysHistory;
                if (normalizedEstimatedDaysHistory != null && normalizedEstimatedDaysHistory.Length > 1)
                    return GetPredictedVelocity(HistoryKind.EstimatedDays, normalizedEstimatedDaysHistory, true);
                else
                    return 0;
            }
        }

        private double GetPredictedVelocity(HistoryKind historyKind, NormalizedBurndownHistory normalizedHistory, bool ignoreNonWorkingdays)
        {
            int nDays = Project.GetProject(MainProjectID).AverageVelocitySpan;
            if (cache.predictedVelocityCache != null && cache.predictedVelocityCache[(int)historyKind].ContainsKey(nDays))
                return cache.predictedVelocityCache[(int)historyKind][nDays];
            double weightedValueSum = 0;
            double weightedDaysSum = 0;
            int iDay = nDays;
            DateTime day = DateTime.Now.Date;
            if (ignoreNonWorkingdays && !Project.IsWorkingDay(day))
                day = Project.GetPreviousWorkingDay(day);
            DateTime previousDay = day.AddDays(-1);
            if (ignoreNonWorkingdays && !Project.IsWorkingDay(previousDay))
                Project.GetPreviousWorkingDay(previousDay);
            int index = normalizedHistory.Length - 1;
            for (int i = 0; i < nDays; i += 1)
            {
                if (previousDay < normalizedHistory.Start)
                    break;
                double velocityOneDay = normalizedHistory.ValueAt(previousDay.Date) - normalizedHistory.ValueAt(day.Date);
                weightedValueSum += velocityOneDay * iDay;
                weightedDaysSum += iDay;
                index -= 1;
                iDay -= 1;
                day = previousDay;
                previousDay = previousDay.AddDays(-1);
                if (ignoreNonWorkingdays && !Project.IsWorkingDay(previousDay))
                    previousDay = Project.GetPreviousWorkingDay(day);
            }
            double prediction = 0;
            if (weightedDaysSum > 0)
                prediction =  weightedValueSum / weightedDaysSum;

            if (cache.predictedVelocityCache == null)
            {
                cache.predictedVelocityCache = new Dictionary<int, double>[3];
                for (int i = 0; i < cache.predictedVelocityCache.Length - 1; i += 1)
                    cache.predictedVelocityCache[i] = new Dictionary<int, double>();
            }
            cache.predictedVelocityCache[(int)historyKind].Add(nDays, prediction);
            return prediction;
        }

        /// <summary>
        /// The future predicted burndown based on the points history.
        /// </summary>
        public Dictionary<DateTime, double> PointsPredictedBurndown
        {
            get
            {
                return GetPredictedBurndown(ExcessPointsPrediction, PredictedPointsVelocity);
            }
        }

        /// <summary>
        /// The future predicted burndown based on the estimated days history.
        /// </summary>
        public Dictionary<DateTime, double> EstimatedDaysPredictedBurndown
        {
            get
            {
                return GetPredictedBurndown(ExcessEstimatedDaysPrediction, PredictedEstimatedDaysVelocity);
            }
        }

        private Dictionary<DateTime, double> GetPredictedBurndown(double excessValue, double predictedVelocity)
        {
            Dictionary<DateTime, double> predictedBurndown = new Dictionary<DateTime, double>();
            double excess = excessValue;
            DateTime day = DateTime.Now.Date.AddDays(1);
            while (day.Date <= Date.Date && excess > 0)
            {
                if (Project.IsWorkingDay(day))
                    excess -= predictedVelocity;
                predictedBurndown.Add(day.Date, excess);
            }
            return predictedBurndown;
        }

        /// <summary>
        /// The estimated number of points that will be undone on the current release date.
        /// </summary>
        public double ExcessPointsPrediction
        {
            get
            {
                return RemainingPoints - RemainingWorkDays * PredictedPointsVelocity;
            }
        }

        /// <summary>
        /// The estimated number of estimated days that will be undone on the current release date.
        /// </summary>
        public double ExcessEstimatedDaysPrediction
        {
            get
            {
                return RemainingEstimatedDays - RemainingWorkDays * PredictedEstimatedDaysVelocity;
            }
        }

        /// <summary>
        /// The predicted date when all items will be completed based on points
        /// </summary>
        public DateTime PredictedPointsCompletion
        {
            get
            {
                return GetPredictedCompletion(ExcessPointsPrediction, PredictedPointsVelocity);
            }
        }

        /// <summary>
        /// The predicted date when all items will be completed based on estimated days
        /// </summary>
        public DateTime PredictedEstimatedDaysCompletion
        {
            get
            {
                return GetPredictedCompletion(ExcessEstimatedDaysPrediction, PredictedEstimatedDaysVelocity);
            }
        }

        private DateTime GetPredictedCompletion(double predictedExcess, double predictedVelocity)
        {
            double excess = predictedExcess;
            if (excess == 0)
                return Date;
            if (predictedVelocity <= 0)
                return DateTime.MaxValue;

            DateTime prediction = Date.Date;
            if (excess > 0)
            {
                while (excess > 0)
                {
                    prediction = prediction.AddDays(1);
                    if (Project.IsWorkingDay(prediction))
                        excess -= predictedVelocity;
                }
                return prediction;
            }
            else if (excess < 0)
            {
                while (excess < 0 && Math.Abs(excess) > predictedVelocity)
                {
                    prediction = prediction.AddDays(-1);
                    if (Project.IsWorkingDay(prediction))
                        excess += predictedVelocity;
                }
                return prediction;
            }
            return prediction;
        }

        /// <summary>
        /// The current items that are completed and tagged to the release.
        /// </summary>
        public List<ProductBacklogItem> CompletedItems
        {
            get
            {
                List<ProductBacklogItem> completed = new List<ProductBacklogItem>();
                foreach (ProductBacklogItem item in ContributingItems)
                    if (item is ProductBacklogItemInSprint && item.HasChildren)
                    {
                        if (!item.DeepChildren.Exists(subitem => (EHPMTaskStatus)((Task)subitem).Status.Value != EHPMTaskStatus.Completed))
                            completed.Add(item);
                    }
                    else if ((EHPMTaskStatus)item.Status.Value == EHPMTaskStatus.Completed)
                        completed.Add(item);
                return completed;
            }
        }

        /// <summary>
        /// The current items that are remaining and tagged to the release.
        /// </summary>
        public List<ProductBacklogItem> RemainingItems
        {
            get
            {
                List<ProductBacklogItem> remainingItems = new List<ProductBacklogItem>();
                List<ProductBacklogItem> completedItems = CompletedItems;
                foreach (ProductBacklogItem item in ContributingItems)
                {
                    bool found = false;
                    foreach (ProductBacklogItem cItem in completedItems)
                    {
                        if (cItem.UniqueTaskID.m_ID == item.UniqueTaskID.m_ID)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        remainingItems.Add(item);
                }
                return remainingItems; 
            }
        }

        /// <summary>
        /// The sum of the points for all non-completed product backlog items in the release
        /// </summary>
        public double RemainingPoints
        {
            get
            {
                return RemainingItems.Sum(item => item.Points);
            }
        }

        /// <summary>
        /// The sum of the estimated days for all non-completed product backlog items in the release
        /// </summary>
        public double RemainingEstimatedDays
        {
            get
            {
                return RemainingItems.Sum(item => item.EstimatedDays);
            }
        }

        /// <summary>
        /// The non-completed items in the release sorted by priority from higher to lower.
        /// </summary>
        public List<ProductBacklogItem> RemainingItemsSortedByPriority
        {
            get
            {
                return HPMUtilities.SortByPriority(Project, RemainingItems);
            }
        }

        private double[] GetRiskLimits(int remainingWorkDays, double predictedVelocity, double factor)
        {
            double[] limits = new double[2];
            limits[0] = remainingWorkDays * predictedVelocity * (1 - factor);
            limits[1] = remainingWorkDays * predictedVelocity * (1 + factor);
            return limits;
        }

        private double[] GetRiskLimitsPoints(double factor)
        {
            return GetRiskLimits(RemainingWorkDays, PredictedPointsVelocity, factor);
        }

        private double[] GetRiskLimitsEstimatedDays(double factor)
        {
            return GetRiskLimits(RemainingWorkDays, PredictedEstimatedDaysVelocity, factor);
        }

        /// <summary>
        /// Based on the points history, get the items categorized in to low/medium/high risk to complete by the release date.
        /// </summary>
        /// <param name="factor"></param>
        /// <returns></returns>
        public List<ProductBacklogItem>[] GetRemainingItemsByRiskPoints(double factor)
        {
            return RemainingItemsByRisk(RemainingItemsSortedByPriority, HistoryKind.Points, GetRiskLimitsPoints(factor));
        }

        /// <summary>
        /// Based on the estimated days history, get the items categorized in to low/medium/high risk to complete by the release date.
        /// </summary>
        /// <param name="factor"></param>
        /// <returns></returns>
        public List<ProductBacklogItem>[] GetRemainingItemsByRiskEstimatedDays(double factor)
        {
            return RemainingItemsByRisk(RemainingItemsSortedByPriority, HistoryKind.EstimatedDays, GetRiskLimitsEstimatedDays(factor));
        }

        private List<ProductBacklogItem>[] RemainingItemsByRisk(List<ProductBacklogItem> remainingItemsPriorityOrder, HistoryKind historyKind, double[] limits)
        {
            List<ProductBacklogItem>[] remainingItemsByRisk = new List<ProductBacklogItem>[3];
            remainingItemsByRisk[0] = new List<ProductBacklogItem>();
            remainingItemsByRisk[1] = new List<ProductBacklogItem>();
            remainingItemsByRisk[2] = new List<ProductBacklogItem>();

            double aggregated = 0;
            foreach (ProductBacklogItem item in remainingItemsPriorityOrder)
            {
                double delta = 0;
                if (historyKind == HistoryKind.Points)
                    delta = item.Points;
                else if (historyKind == HistoryKind.EstimatedDays)
                     delta = item.EstimatedDays;
                if (aggregated+delta >= limits[1])
                    remainingItemsByRisk[2].Add(item);
                else if (aggregated+delta >= limits[0])
                    remainingItemsByRisk[1].Add(item);
                else
                    remainingItemsByRisk[0].Add(item);
                aggregated += delta;
            }
            return remainingItemsByRisk;
        }

        /// <summary>
        /// The sprints that are tagged to this release.
        /// </summary>
        public List<Sprint> Sprints
        {
            get
            {
                List<Sprint> taggedToRelease = new List<Sprint>();
                foreach (Sprint s in Project.GetProject(MainProjectID).Sprints)
                    if (s.IsTaggedToRelease(this))
                        taggedToRelease.Add(s);
                return taggedToRelease;
            }
        }

        /// <summary>
        /// The scheduled tasks that are tagged to this release.
        /// </summary>
        public List<ScheduledTask> ScheduledTasks
        {
            get
            {
                List<ScheduledTask> taggedToRelease = new List<ScheduledTask>();
                foreach (ScheduledTask s in Project.GetProject(MainProjectID).ScheduledTasks)
                    if (s.IsTaggedToRelease(this))
                        taggedToRelease.Add(s);
                return taggedToRelease;
            }
        }

        /// <summary>
        /// The product backlog items that are tagged to this release.
        /// </summary>
        public List<ProductBacklogItem> ProductBacklogItems
        {
            get
            {
                List<ProductBacklogItem> taggedToRelease = new List<ProductBacklogItem>();
                foreach (ProductBacklogItem s in Project.ProductBacklogItems)
                    if (s.IsTaggedToRelease(this))
                        taggedToRelease.Add(s);
                return taggedToRelease;
            }
        }

        /// <summary>
        /// Not applicable for releases.
        /// </summary>
        public override HansoftEnumValue Priority
        {
            get
            {
                return new HansoftEnumValue(MainProjectID, EHPMProjectDefaultColumn.SprintPriority, EHPMTaskAgilePriorityCategory.None, (int)EHPMTaskAgilePriorityCategory.None);
            }
            set
            {
            }
        }
    }
}
