using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HPMSdk;


namespace Hansoft.ObjectWrapper
{
    /// <summary>
    /// Represents the product backlog of a Hansoft project.
    /// </summary>
    public class ProductBacklog : ProjectView
    {

        internal static ProductBacklog GetProductBacklog(HPMUniqueID uniqueID)
        {
            return new ProductBacklog(uniqueID);
        }

        private ProductBacklog(HPMUniqueID uniqueID)
            : base(uniqueID)
        {
        }

        /// <summary>
        /// Returns a string useful for referring to the product backlog to an end user. Calls to the Setter will be ignored.
        /// </summary>
        public override string Name
        {
            get
            {
                // TODO: Get rid of hardcoded string
                return "Product Backlog";
            }
            set { }
        }

        /// <summary>
        /// Subject to refactoring, create an enum in the project view instead, or simply use the TypeOf
        /// </summary>
        internal override EHPMReportViewType ReportViewType
        {
            get { return EHPMReportViewType.AgileBacklog; }
        }
    }
}
