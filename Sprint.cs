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
        /// Not suppoprted for Sprints
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
