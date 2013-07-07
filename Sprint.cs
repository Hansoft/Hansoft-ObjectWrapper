using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HPMSdk;

namespace Hansoft.ObjectWrapper
{
    /// <summary>
    /// Represents a Sprint in the Schedule view of Hansoft.
    /// </summary>
    public class Sprint : Task
    {
        internal static Sprint GetSprint(HPMUniqueID uniqueID, HPMUniqueID uniqueTaskID)
        {
            return new Sprint(uniqueID, uniqueTaskID);
        }

        private Sprint(HPMUniqueID uniqueID, HPMUniqueID uniqueTaskID)
            : base(uniqueID, uniqueTaskID)
        {
        }

        /// <summary>
        /// The project view that this sprint belongs to.
        /// </summary>
        public override ProjectView ProjectView
        {
            get
            {
                return Project.Schedule;
            }
        }

        /// <summary>
        /// The first day in the sprint
        /// </summary>
        public DateTime Start
        {
            get
            {
                HPMTaskTimeZones tzData = Session.TaskGetTimeZones(UniqueTaskID);
                return HPMUtilities.FromHPMDateTime(tzData.m_Zones[0].m_Start);
            }
        }

        /// <summary>
        /// The last day in the sprint
        /// </summary>
        public DateTime End
        {
            get
            {
                HPMTaskTimeZones tzData = Session.TaskGetTimeZones(UniqueTaskID);
                return HPMUtilities.FromHPMDateTime(tzData.m_Zones[0].m_End);
            }
        }

        /// <summary>
        /// Not supported for Sprints
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
