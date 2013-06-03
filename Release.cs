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
        internal static Release GetRelease(HPMUniqueID uniqueID, HPMUniqueID uniqueTaskID)
        {
            return new Release(uniqueID, uniqueTaskID);
        }

        private Release(HPMUniqueID uniqueID, HPMUniqueID uniqueTaskID)
            : base(uniqueID, uniqueTaskID)
        {
        }

        /// <summary>
        /// The date of the release.
        /// </summary>
        public string Date
        {
            get
            {
                HPMTaskTimeZones tzData = Session.TaskGetTimeZones(UniqueID);
                return HPMUtilities.HPMDateTimeToDateString(tzData.m_Zones[0].m_Start);
            }
            //TODO: Implement the setter.
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
                    if (s.TaggedToReleases.Contains(this))
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
                    if (s.TaggedToReleases.Contains(this))
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
                foreach (ProductBacklogItem s in Project.GetProject(MainProjectID).ProductBacklogItems)
                    if (s.TaggedToReleases.Contains(this))
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
