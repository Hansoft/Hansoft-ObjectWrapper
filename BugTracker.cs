using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HPMSdk;

namespace Hansoft.ObjectWrapper
{
    /// <summary>
    /// Represents the QA view in Hansoft.
    /// </summary>
    public class BugTracker : ProjectView
    {
        internal static BugTracker GetBugTracker(HPMUniqueID uniqueID)
        {
            return new BugTracker(uniqueID);
        }

        private BugTracker(HPMUniqueID uniqueID)
            : base(uniqueID)
        {
        }

        /// <summary>
        /// All bugs in the project.
        /// </summary>
        public override List<HansoftItem> Children
        {
            get
            {
                return new List<HansoftItem>(MainProject.Bugs);
            }
        }

        /// <summary>
        /// All bugs in the project.
        /// </summary>
        public override List<HansoftItem> DeepChildren
        {
            get
            {
                return Children;
            }
        }

        /// <summary>
        /// Returns a string useful for referring to the QA view to an end user. Calls to the Setter will be ignored.
        /// </summary>
        public override string Name
        {
            get
            {
                // TODO: Get rid of hardcoded string
                return "QA Project";
            }
            set { }
        }

        internal override EHPMReportViewType ReportViewType
        {
            get { return EHPMReportViewType.AllBugsInProject; }
        }
    }
}
