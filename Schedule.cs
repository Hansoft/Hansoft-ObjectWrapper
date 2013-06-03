using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HPMSdk;

namespace Hansoft.ObjectWrapper
{
    /// <summary>
    /// Represents the project schedule view of a Hansoft project.
    /// </summary>
    public class Schedule : ProjectView
    {

        internal static Schedule GetSchedule(HPMUniqueID uniqueID) {
            return new Schedule(uniqueID);
        }

        private Schedule(HPMUniqueID uniqueID) : base(uniqueID)
        {
        }

        /// <summary>
        /// Returns a string useful for referring to the Schedule view to an end user. Calls to the Setter will be ignored.
        /// </summary>
        public override string Name
        {
            get
            {
                // TODO: Get rid of hardcoded string
                return "Project Schedule";
            }
            set { }
        }

        internal override EHPMReportViewType ReportViewType
        {
            get { return EHPMReportViewType.ScheduleMainProject; }
        }
    }
}
 