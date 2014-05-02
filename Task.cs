using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HPMSdk;
using Hansoft.ObjectWrapper.CustomColumnValues;

namespace Hansoft.ObjectWrapper
{
    /// <summary>
    /// Abstract base class for all kinds of Hansoft tasks (bugs, scheduled tasks, releases, subprojects, sprints, backlog items).
    /// </summary>
    public abstract class Task : HansoftItem
    {

        private HPMUniqueID uniqueTaskID;

        /// <summary>
        /// Factory method for creating a Task (subclass of).
        /// </summary>
        /// <param name="uniqueID">The Task ID or TaskRef ID that a Task shuld be created for</param>
        /// <param name="viewType">The view that the task appears in (primarily  important for product backlog items which can appear in mutiple views).</param>
        /// <returns>A concrete task acting as a wrapper to access the Hansoft API for the task in question.</returns>
        internal static Task GetTask(HPMUniqueID uniqueID, EHPMReportViewType viewType)
        {
            if ((viewType == EHPMReportViewType.ScheduleMainProject || viewType == EHPMReportViewType.AgileMainProject) && SdkSession.UtilIsIDTask(uniqueID) && SdkSession.UtilIsIDBacklogProject(SdkSession.TaskGetContainer(uniqueID)))
                return GetTask(SdkSession.TaskGetProxy(uniqueID));
            else
                return GetTask(uniqueID);
        }

        /// <summary>
        /// Factory method for creating a Task (subclass of).
        /// </summary>
        /// <param name="uniqueID">The Task ID or TaskRef ID that a Task shuld be created for. If a Task ID is given then the MainRef of the task will be used to determine which view is applicable.</param>
        /// <returns>A concrete task acting as a wrapper to access the Hansoft API for the task in question.</returns>
        public static Task GetTask(HPMUniqueID uniqueID)
        {
            HPMUniqueID refID;
            if (SdkSession.UtilIsIDTask(uniqueID))
                refID = SdkSession.TaskGetMainReference(uniqueID);
            else
                refID = uniqueID;
            return CreateTask(refID);
        }

        private static Task CreateTask(HPMUniqueID uniqueID)
        {
            HPMUniqueID uniqueTaskID = SdkSession.TaskRefGetTask(uniqueID);
            EHPMTaskLockedType lockedType = SdkSession.TaskGetLockedType(uniqueTaskID);
            switch (lockedType)
            {
                case EHPMTaskLockedType.BacklogItem:
                    if (SdkSession.UtilIsIDBacklogProject(SdkSession.TaskGetContainer(uniqueTaskID)))
                    {
                        if (SdkSession.TaskGetMainReference(uniqueTaskID).m_ID == uniqueID.m_ID)
                            return ProductBacklogItem.GetProductBacklogItem(uniqueID, uniqueTaskID);
                        else
                            return ProductBacklogItemInSprint.GetProductBacklogItemInSprint(uniqueID, uniqueTaskID);
                    }
                    else
                        return SprintBacklogItem.GetSprintBacklogItem(uniqueID, uniqueTaskID);
                case EHPMTaskLockedType.QABug:
                    return Bug.GetBug(uniqueID, uniqueTaskID);
                case EHPMTaskLockedType.SprintItem:
                    return Sprint.GetSprint(uniqueID, uniqueTaskID);
                case EHPMTaskLockedType.Normal:
                default:
                    if (SdkSession.TaskGetForceSubProject(uniqueTaskID))
                    {
                        return SubProject.GetSubProject(uniqueID, uniqueTaskID);
                    }
                    else
                    {
                        EHPMTaskType taskType = SdkSession.TaskGetType(uniqueTaskID);
                        switch (taskType)
                        {
                            case EHPMTaskType.Milestone:
                                return Release.GetRelease(uniqueID, uniqueTaskID);
                            case EHPMTaskType.Planned:
                            default:
                                if (SdkSession.UtilIsIDBacklogProject(SdkSession.TaskGetContainer(uniqueTaskID)))
                                    return ProductBacklogItemInSchedule.GetProductBacklogItemInSchedule(uniqueID, uniqueTaskID);
                                else
                                    return ScheduledTask.GetScheduledTask(uniqueID, uniqueTaskID);
                        }
                    }
            }
        }

        internal Task(HPMUniqueID uniqueID, HPMUniqueID uniqueTaskID)
            : base(uniqueID)
        {
            this.uniqueTaskID = uniqueTaskID;
        }

        /// <summary>
        /// The Unique ID of the task.
        /// </summary>
        public HPMUniqueID UniqueTaskID
        {
            get
            {
                return uniqueTaskID;
            }
        }

        /// <summary>
        /// The ID of the project (view) that the task belongs to can be eithe the Main Project, the Product Backlog Project, or the QA Project.
        /// </summary>
        public HPMUniqueID ProjectID
        {
            get
            {
                return Session.TaskGetContainer(UniqueTaskID);
            }
        }

        /// <summary>
        /// The project that this task belongs to.
        /// </summary>
        public Project Project
        {
            get
            {
                return Project.GetProject(MainProjectID);
            }
        }

        /// <summary>
        /// The project view that this task belongs to.
        /// </summary>
        public abstract ProjectView ProjectView
        {
            get;
        }

        /// <summary>
        /// The ID of the Main Project of the Task.
        /// </summary>
        public HPMUniqueID MainProjectID
        {
            get
            {
                return Session.UtilGetRealProjectIDFromProjectID(ProjectID);
            }
        }

        /// <summary>
        /// The parent of this task.
        /// </summary>
        public override HansoftItem Parent
        {
            get
            {
                HPMUniqueID parentRef = Session.TaskRefUtilGetParent(UniqueID);
                if (parentRef.m_ID == Project.GetProject(MainProjectID).ProductBacklog.UniqueID.m_ID)
                    return Project.GetProject(MainProjectID).ProductBacklog;
                if (parentRef.m_ID == Project.GetProject(MainProjectID).BugTracker.UniqueID.m_ID)
                    return Project.GetProject(MainProjectID).BugTracker;
                if (parentRef.m_ID == Project.GetProject(MainProjectID).Schedule.UniqueID.m_ID)
                    return Project.GetProject(MainProjectID).Schedule;
                else
                    return Task.GetTask(parentRef);
            }
        }

        /// <summary>
        /// The full list of parents (recursively) of this task.
        /// </summary>
        public override List<HansoftItem> AllParents
        {
            get
            {
                List<HansoftItem> parents = new List<HansoftItem>();
                HansoftItem p = Parent;
                while (p != null)
                {
                    parents.Add(p);
                    p = p.Parent;
                }
                return parents;
            }
        }

        /// <summary>
        /// The name of the task.
        /// </summary>
        public override string Name
        {
            get
            {
                string aString = Session.TaskGetDescription(UniqueTaskID);
                return aString;
            }
            set { if (Name != value) Session.TaskSetDescription(UniqueTaskID, value); }
        }

        /// <summary>
        /// True if this task has children, False otherwise.
        /// </summary>
        public override bool HasChildren
        {
            get { return TaskHelper.HasChildren(UniqueID); }
        }

        /// <summary>
        /// The direct children of this task.
        /// </summary>
        public override List<HansoftItem> Children
        {
            get { return TaskHelper.GetChildren(UniqueID); }
        }

        /// <summary>
        /// All children (recursively) of this task.
        /// </summary>
        public override List<HansoftItem> DeepChildren
        {
            get { return TaskHelper.GetDeepChildren(UniqueID); }
        }

        /// <summary>
        /// Get the predecessor of this task in the hierarchy view
        /// </summary>
        public Task Predecessor
        {
            get
            {
                HPMUniqueID prevRefID = Session.TaskRefGetPreviousID(UniqueID);
                if (prevRefID.m_ID == -1)
                    return null;
                else
                    return Task.GetTask(prevRefID);
            }
        }

        /// <summary>
        /// The time the task was last updated.
        /// </summary>
        public DateTime LastUpdated
        {
            get
            {
                return HPMUtilities.FromHPMDateTime(Session.TaskGetLastUpdatedTime(UniqueTaskID));
            }
        }

        /// <summary>
        /// Check whether this task is tagged to a release.
        /// </summary>
        /// <param name="release">The release.</param>
        /// <returns>true if this task is tagged to the release, false otherwise.</returns>
        public bool IsTaggedToRelease(Release release)
        {
            HPMTaskLinkedToMilestones relIDs = Session.TaskGetLinkedToMilestones(UniqueTaskID);
            foreach (HPMUniqueID relID in relIDs.m_Milestones)
            {
                if (relID.m_ID == release.UniqueID.m_ID)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// The releases that this task is tagged to.
        /// </summary>
        public List<Release> TaggedToReleases
        {
            get
            {
                List<Release> releases = new List<Release>();
                HPMTaskLinkedToMilestones relIDs = Session.TaskGetLinkedToMilestones(UniqueTaskID);
                foreach (HPMUniqueID relID in relIDs.m_Milestones)
                {
                    if (Session.UtilIsIDValid(relID))
                        releases.Add((Release)(Task.GetTask(Session.TaskRefGetTask(relID))));
                }
                return releases;
            }
        }

        /// <summary>
        /// The tasks that are linked to this task.
        /// </summary>
        public List<Task> LinkedTasks
        {
            get
            {
                List<Task> linkedTasks = new List<Task>();
                HPMTaskLinkedTo linked = Session.TaskGetLinkedTo(UniqueTaskID);
                foreach (HPMTaskLinkedToLink linkedTask in linked.m_LinkedTo)
                {
                    // Need to check that the TaskRef is valid since there might be dangling refs returned
                    if (Session.UtilIsIDValid(linkedTask.m_LinkedTo))
                        linkedTasks.Add(Task.CreateTask(linkedTask.m_LinkedTo));
                }
                return linkedTasks;
            }
        }

        /// <summary>
        /// The builtin column Risk.
        /// </summary>
        public HansoftEnumValue Risk
        {
            get
            {
                EHPMTaskRisk risk = Session.TaskGetRisk(UniqueTaskID);
                return new HansoftEnumValue(MainProjectID, EHPMProjectDefaultColumn.Risk, risk, (int)risk);
            }
            set
            {
                if (Risk != value) Session.TaskSetRisk(UniqueTaskID, (EHPMTaskRisk)value.Value);
            }
        }

        /// <summary>
        /// The builtin column Confidence.
        /// </summary>
        public HansoftEnumValue Confidence
        {
            get
            {
                EHPMTaskConfidence confidence = Session.TaskGetConfidence(UniqueTaskID);
                return new HansoftEnumValue(MainProjectID, EHPMProjectDefaultColumn.Confidence, confidence, (int)confidence);
            }
            set
            {
                if (Confidence != value) Session.TaskSetConfidence(UniqueTaskID, (EHPMTaskConfidence)value.Value);
            }
        }

        /// <summary>
        /// The builtin column Severity.
        /// </summary>
        public HansoftEnumValue Severity
        {
            get
            {
                EHPMTaskSeverity severity = Session.TaskGetSeverity(UniqueTaskID);
                return new HansoftEnumValue(MainProjectID, EHPMProjectDefaultColumn.Severity, severity, (int)severity);
            }
            set
            {
                if (Severity != value) Session.TaskSetSeverity(UniqueTaskID, (EHPMTaskSeverity)value.Value);
            }
        }

        /// <summary>
        /// The builtin column Category.
        /// </summary>
        public HansoftEnumValue Category
        {
            get
            {
                EHPMTaskBacklogCategory category = Session.TaskGetBacklogCategory(UniqueTaskID);
                return new HansoftEnumValue(MainProjectID, EHPMProjectDefaultColumn.BacklogCategory, category, (int)category);
            }
            set
            {
                if (Category != value) Session.TaskSetBacklogCategory(UniqueTaskID, (EHPMTaskBacklogCategory)value.Value);
            }
        }

        /// <summary>
        /// The builtin column Status.
        /// </summary>
        public HansoftEnumValue Status
        {
            get
            {
                EHPMTaskStatus status = Session.TaskGetStatus(UniqueTaskID);
                return new HansoftEnumValue(MainProjectID, EHPMProjectDefaultColumn.ItemStatus, status, (int)status);
            }
            set
            {
                if (Status != value) Session.TaskSetStatus(UniqueTaskID, (EHPMTaskStatus)value.Value, true, EHPMTaskSetStatusFlag.All);
            }
        }

        /// <summary>
        /// The aggregated status value over all children as it is displayed in the Hansoft client
        /// </summary>
        public virtual HansoftEnumValue AggregatedStatus
        {
            get
            {
                if (HasChildren)
                {
                    EHPMTaskStatus status = Session.TaskRefGetSummary(UniqueID).m_TaskStatus;
                    return new HansoftEnumValue(MainProjectID, EHPMProjectDefaultColumn.ItemStatus, status, (int)status);
                }
                else
                    return Status;
            }
        }
        
        /// <summary>
        /// The builtin column Priority.
        /// </summary>
        public abstract HansoftEnumValue Priority
        {
            get;
            set;
        }

        /// <summary>
        /// The builtin column Points.
        /// </summary>
        public int Points
        {
            get { return Session.TaskGetComplexityPoints(UniqueTaskID); }
            set { if (Points != value) Session.TaskSetComplexityPoints(UniqueTaskID, value); }
        }

        /// <summary>
        /// The aggregated points value over all children as it is displayed in the Hansoft client
        /// </summary>
        public virtual int AggregatedPoints
        {
            get
            {
                if (HasChildren)
                    return Session.TaskRefGetSummary(UniqueID).m_ComplexityPoints;
                else
                    return Points;
            }
        }

        /// <summary>
        /// The builtin column Work remaining.
        /// </summary>
        public double WorkRemaining
        {
            get { return Session.TaskGetWorkRemaining(UniqueTaskID); }
            set { if (WorkRemaining != value) Session.TaskSetWorkRemaining(UniqueTaskID, (float)value); }
        }

        /// <summary>
        /// The aggregated work remaining value over all children as it is displayed in the Hansoft client
        /// </summary>
        public virtual double AggregatedWorkRemaining
        {
            get
            {
                if (HasChildren)
                {
                    // Can't use TaskRefGetSummary for workremianing since it will return infinity unless all items have a set value for workRemaining
                    double aggregatedWorkRemaining = 0;
                    foreach (Task item in DeepLeaves)
                        aggregatedWorkRemaining += double.IsInfinity(item.WorkRemaining) ? 0 : item.WorkRemaining;
                    return aggregatedWorkRemaining;
                }
                else
                    return double.IsInfinity(WorkRemaining) ? 0 : WorkRemaining;
            }
        }

        /// <summary>
        /// The builtin column Estimated days.
        /// </summary>
        public double EstimatedDays
        {
            get { return Session.TaskGetEstimatedIdealDays(UniqueTaskID); }
            set { if (EstimatedDays != value) Session.TaskSetEstimatedIdealDays(UniqueTaskID, value); }
        }

        /// <summary>
        /// The aggregated estimated days value over all children as it is displayed in the Hansoft client
        /// </summary>
        public virtual double AggregatedEstimatedDays
        {
            get
            {
                if (HasChildren)
                    return Session.TaskRefGetSummary(UniqueID).m_EstimatedIdealDays;
                else
                    return EstimatedDays;
            }
        }

        /// <summary>
        /// The builtin column Hyperlink.
        /// </summary>
        public string Hyperlink
        {
            get { return Session.TaskGetHyperlink(UniqueTaskID); }
            set { if (Hyperlink != value) Session.TaskSetHyperlink(UniqueTaskID, value); }
        }

        /// <summary>
        /// The builtin column Percent complete.
        /// </summary>
        public double PercentComplete
        {
            get { return Session.TaskGetPercentComplete(UniqueTaskID); }
        }

        /// <summary>
        /// Gets the status of any attached workflow as a localized string.
        /// 
        /// NOTE: There is no way to set what translation to user and the UK English localization will always be used.
        /// </summary>
        public string WorkflowStatusString
        {
            get
            {
                int workFlowID = (int)Session.TaskGetWorkflow(UniqueTaskID);
                if (workFlowID == -1)
                    return "";
                return Session.LocalizationTranslateString(Session.LocalizationGetDefaultLanguage(), Session.UtilGetWorkflowObjectName(MainProjectID, workFlowID, WorkflowStatus));
            }
        }

        /// <summary>
        /// Gets the status of any attached workflow as a localized string.
        /// 
        /// NOTE: There is no way to set what translation to user and the UK English localization will always be used.
        /// </summary>
        public bool HasWorkFlow
        {
            get
            {
                int workFlowID = (int)Session.TaskGetWorkflow(UniqueTaskID);
                return workFlowID != -1;
            }
        }

        /// <summary>
        /// Gets the status of any attached workflow as an integer.
        /// </summary>
        public int WorkflowStatus
        {
            get
            {
                return Session.TaskGetWorkflowStatus(UniqueTaskID);
            }
        }

        /// <summary>
        /// The builtin column Detailed description / User story in Hansoft.
        /// </summary>
        public string DetailedDescription
        {
            get { return Session.TaskGetDetailedDescription(UniqueTaskID); }
            set { if (DetailedDescription != value) Session.TaskSetDetailedDescription(UniqueTaskID, value); }
        }

        /// <summary>
        /// The Hansoft Url that will open up Hansoft and navigate to this task.
        /// </summary>
        public string Url
        {
            get { return Session.UtilGetHansoftURL(UniqueID.m_ID.ToString()); }
        }

        /// <summary>
        /// Get the value of a custom column for this task.
        /// </summary>
        /// <param name="columnName">The name of the column to get the value for</param>
        /// <returns>The requested value wrapped by a subclass of CustomColumnValue</returns>
        public CustomColumnValue GetCustomColumnValue(string columnName)
        {
            HPMProjectCustomColumnsColumn customColumn = ProjectView.GetCustomColumn(columnName);
            if (customColumn != null)
                return GetCustomColumnValue(customColumn);
            else
                return CustomColumnValue.FromInternalValue(this, null, "");
        }

        /// <summary>
        /// Get the aggregated value of a custom column over all children for this task as it is displayed in the Hansoft client.
        /// </summary>
        /// <param name="columnName">The name of the column to get the value for</param>
        /// <returns>The requested value wrapped by a subclass of CustomColumnValue</returns>
        public CustomColumnValue GetAggregatedCustomColumnValue(string columnName)
        {
            HPMProjectCustomColumnsColumn customColumn = ProjectView.GetCustomColumn(columnName);
            if (customColumn != null)
                return GetAggregatedCustomColumnValue(customColumn);
            else
                return CustomColumnValue.FromInternalValue(this, null, "");
        }


        /// <summary>
        /// Get the value of a custom column for this task.
        /// </summary>
        /// <param name="customColumn">The custom column to get the value for.</param>
        /// <returns>The requested value wrapped by a subclass of CustomColumnValue</returns>
        public CustomColumnValue GetCustomColumnValue(HPMProjectCustomColumnsColumn customColumn)
        {
            string cDataString = Session.TaskGetCustomColumnData(UniqueTaskID, customColumn.m_Hash);
            return CustomColumnValue.FromInternalValue(this, customColumn, cDataString);
        }

        /// <summary>
        /// Get the aggregated value of a custom column over all children for this task as it is displayed in the Hansoft client.
        /// </summary>
        /// <param name="customColumn">The custom column to get the value for.</param>
        /// <returns>The requested value wrapped by a subclass of CustomColumnValue</returns>
        internal virtual CustomColumnValue GetAggregatedCustomColumnValue(HPMProjectCustomColumnsColumn customColumn)
        {
            if (HasChildren && (customColumn.m_Type == EHPMProjectCustomColumnsColumnType.AccumulatedTime || customColumn.m_Type == EHPMProjectCustomColumnsColumnType.FloatNumber || customColumn.m_Type == EHPMProjectCustomColumnsColumnType.IntegerNumber))
            {
                HPMTaskCustomSummaryValue[] summaryValues = Session.TaskRefGetSummary(UniqueID).m_CustomSummaryValues;
                foreach (HPMTaskCustomSummaryValue value in summaryValues)
                {
                    if (value.m_Hash == customColumn.m_Hash)
                    {
                        if (customColumn.m_Type == EHPMProjectCustomColumnsColumnType.IntegerNumber)
                        {
                            long aggregatedValue = value.m_IntegerValue;
                            return IntegerNumberValue.FromInteger(this, customColumn, aggregatedValue);
                        }
                        else
                        {
                            double aggregatedValue = value.m_FloatValue;
                            if (customColumn.m_Type == EHPMProjectCustomColumnsColumnType.FloatNumber)
                                return FloatNumberValue.FromFloat(this, customColumn, aggregatedValue);
                            else
                                return AccumulatedTimeValue.FromFloat(this, customColumn, aggregatedValue);
                        }
                    }
                }
                // No summary value was found, return 0
                if (customColumn.m_Type == EHPMProjectCustomColumnsColumnType.IntegerNumber)
                    return IntegerNumberValue.FromInteger(this, customColumn, 0);
                else if (customColumn.m_Type == EHPMProjectCustomColumnsColumnType.FloatNumber)
                    return FloatNumberValue.FromFloat(this, customColumn, 0);
                else
                    return AccumulatedTimeValue.FromFloat(this, customColumn, 0);
            }
            else
                return GetCustomColumnValue(customColumn);
        }
        
        /// <summary>
        /// Sets the value of a custom column for this task.
        /// </summary>
        /// <param name="customColumn">The custom column to set the value for.</param>
        /// <param name="value">The value to be set, can either be an instance of CustomColumnValue or any other type that can reasonably be converted
        /// to the end user consumable string representation of the value, i.e., as it is displayed in the Hansoft client.</param>
        public void SetCustomColumnValue(HPMProjectCustomColumnsColumn customColumn, object value)
        {
            if (value != null)
            {
                if (value is CustomColumnValue)
                    SetCustomColumnValueInternal(customColumn, ((CustomColumnValue)value).InternalValue);
                else // TODO: Not right to set US culture here, it will for example screw up DateTime values
                    SetCustomColumnValue(customColumn, CustomColumnValue.FromEndUserString(this, customColumn, Convert.ToString(value, new System.Globalization.CultureInfo("en-US"))));
            }
            else
                SetCustomColumnValueInternal(customColumn, string.Empty);
        }

        /// <summary>
        /// Sets the value of a custom column for this task. If the column doesn't exist in the tasks context it will do nothing
        /// </summary>
        /// <param name="customColumnName">The name of the custom column to set the value for</param>
        /// <param name="value">The value to be set, can either be an instance of CustomColumnValue or any other type that can reasonably be converted
        /// to the end user consumable string representation of the value, i.e., as it is displayed in the Hansoft client.</param>
        public void SetCustomColumnValue(string customColumnName, object value)
        {
            // Ensure that we get the custom column of the right project
            HPMProjectCustomColumnsColumn actualCustomColumn = ProjectView.GetCustomColumn(customColumnName);
            if (actualCustomColumn != null)
                SetCustomColumnValue(actualCustomColumn, value);
        }

        private void SetCustomColumnValueInternal(HPMProjectCustomColumnsColumn customColumn, string value)
        {
            if (Session.TaskGetCustomColumnData(UniqueTaskID, customColumn.m_Hash) != value)
                Session.TaskSetCustomColumnData(UniqueTaskID, customColumn.m_Hash, value, true);
        }


        /// <summary>
        /// Get the value of a built in column for this task.
        /// </summary>
        /// <param name="eHPMProjectDefaultColumn">The column to get the value for.</param>
        /// <returns>The requested value, ints, floats, strings are returned as such. Enumerables are returned as an instance of HansoftEnumValue.</returns>
        public object GetDefaultColumnValue(EHPMProjectDefaultColumn eHPMProjectDefaultColumn)
        {
            switch (eHPMProjectDefaultColumn)
            {
                case EHPMProjectDefaultColumn.Risk:
                    return Risk;
                case EHPMProjectDefaultColumn.BacklogPriority:
                    return Priority;
                case EHPMProjectDefaultColumn.EstimatedIdealDays:
                    return EstimatedDays;
                case EHPMProjectDefaultColumn.BacklogCategory:
                    return Category;
                case EHPMProjectDefaultColumn.ComplexityPoints:
                    return Points;
                case EHPMProjectDefaultColumn.ItemStatus:
                    return Status;
                case EHPMProjectDefaultColumn.Confidence:
                    return Confidence;
                case EHPMProjectDefaultColumn.Hyperlink:
                    return Hyperlink;
                case EHPMProjectDefaultColumn.ItemName:
                    return Name;
                case EHPMProjectDefaultColumn.WorkRemaining:
                    return WorkRemaining;
                default:
                    throw new ArgumentException("Unsupported default column in GetDefaultColumnValue/1: " + eHPMProjectDefaultColumn);
            }
        }

        /// <summary>
        /// Set the value of a built in column for this task.
        /// </summary>
        /// <param name="eHPMProjectDefaultColumn">The column to set the value for.</param>
        /// <param name="sourceValue">The value to be set. Can be of any form that reasonably can be converted to a suitable value for the column.</param>
        public void SetDefaultColumnValue(EHPMProjectDefaultColumn eHPMProjectDefaultColumn, object sourceValue)
        {
            switch (eHPMProjectDefaultColumn)
            {
                case EHPMProjectDefaultColumn.Risk:
                    Risk = HansoftEnumValue.FromObject(MainProjectID, eHPMProjectDefaultColumn, sourceValue);
                    break;
                case EHPMProjectDefaultColumn.BacklogPriority:
                    Priority = HansoftEnumValue.FromObject(MainProjectID, eHPMProjectDefaultColumn, sourceValue);
                    break;
                case EHPMProjectDefaultColumn.EstimatedIdealDays:
                    EstimatedDays = Convert.ToDouble(sourceValue, new System.Globalization.CultureInfo("en-US"));
                    break;
                case EHPMProjectDefaultColumn.BacklogCategory:
                    Category = HansoftEnumValue.FromObject(MainProjectID, eHPMProjectDefaultColumn, sourceValue);
                    break;
                case EHPMProjectDefaultColumn.ComplexityPoints:
                    Points = Convert.ToInt32(sourceValue);
                    break;
                case EHPMProjectDefaultColumn.ItemStatus:
                    Status = HansoftEnumValue.FromObject(MainProjectID, eHPMProjectDefaultColumn, sourceValue);
                    break;
                case EHPMProjectDefaultColumn.Confidence:
                    Confidence = HansoftEnumValue.FromObject(MainProjectID, eHPMProjectDefaultColumn, sourceValue);
                    break;
                case EHPMProjectDefaultColumn.Hyperlink:
                    Hyperlink = sourceValue.ToString();
                    break;
                case EHPMProjectDefaultColumn.ItemName:
                    Name = sourceValue.ToString();
                    break;
                case EHPMProjectDefaultColumn.WorkRemaining:
                    WorkRemaining = Convert.ToDouble(sourceValue, new System.Globalization.CultureInfo("en-US"));
                    break;
                default:
                    throw new ArgumentException("Unsupported default column in GetDefaultColumnValue/1: " + eHPMProjectDefaultColumn);
            }
        }
    }
}
