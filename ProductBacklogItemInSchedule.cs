using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HPMSdk;

namespace Hansoft.ObjectWrapper
{
    /// <summary>
    /// Represents a Product Backlog item that has been committed to the Schedue as a scheduled task.
    /// </summary>
    public class ProductBacklogItemInSchedule : ProductBacklogItem
    {
        internal static ProductBacklogItemInSchedule GetProductBacklogItemInSchedule(HPMUniqueID uniqueID, HPMUniqueID uniqueTaskID)
        {
            return new ProductBacklogItemInSchedule(uniqueID, uniqueTaskID);
        }

        private ProductBacklogItemInSchedule(HPMUniqueID uniqueID, HPMUniqueID uniqueTaskID)
            : base(uniqueID, uniqueTaskID)
        {
        }
    }
}
