using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HPMSdk;

namespace Hansoft.ObjectWrapper
{
    /// <summary>
    /// Represents an explicitly created sub-project in the schedule view of Hansoft.
    /// </summary>
    public class SubProject : Task
    {
        internal static SubProject GetSubProject(HPMUniqueID uniqueID, HPMUniqueID uniqueTaskID)
        {
            return new SubProject(uniqueID, uniqueTaskID);
        }

        private SubProject(HPMUniqueID uniqueID, HPMUniqueID uniqueTaskID)
            : base(uniqueID, uniqueTaskID)
        {
        }

        /// <summary>
        /// Not supported for SubProjects.
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
