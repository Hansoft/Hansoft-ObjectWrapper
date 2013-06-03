using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HPMSdk;

namespace Hansoft.ObjectWrapper
{
    /// <summary>
    /// Base class for resources (what tasks can be assigned to) in Hansoft.
    /// </summary>
    abstract public class Resource : HansoftItem
    {
        internal Resource(HPMUniqueID uniqueID)
            : base(uniqueID)
        {
        }

        /// <summary>
        /// Will always return null as resources don't have parents.
        /// </summary>
        public override HansoftItem Parent
        {
            get { return null; }
        }

        /// <summary>
        /// Will always return an empty list as resources don't have parents.
        /// </summary>
        public override List<HansoftItem> AllParents
        {
            get
            {
                return new List<HansoftItem>();
            }
        }

        /// <summary>
        /// Will always return false resources don't have children.
        /// </summary>
        public override bool HasChildren
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Will always return an empty list as resources don't have children.
        /// </summary>
        public override List<HansoftItem> Children
        {
            get
            {
                return new List<HansoftItem>();
            }
        }

        /// <summary>
        /// Will always return an empty list as resources don't have children.
        /// </summary>
        public override List<HansoftItem> DeepChildren
        {
            get
            {
                return new List<HansoftItem>();
            }
        }
    }
}
