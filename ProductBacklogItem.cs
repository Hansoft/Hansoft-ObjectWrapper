using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HPMSdk;

namespace Hansoft.ObjectWrapper
{
    /// <summary>
    /// Represents an item in the Product Backlog in Hansoft.
    /// </summary>
    public class ProductBacklogItem : Task
    {
        internal static ProductBacklogItem GetProductBacklogItem(HPMUniqueID uniqueID, HPMUniqueID uniqueTaskID)
        {
            return new ProductBacklogItem(uniqueID, uniqueTaskID);
        }

        /// <summary>
        /// General constructor
        /// </summary>
        /// <param name="uniqueID">The TaskRef ID of the item</param>
        /// <param name="uniqueTaskID">The Task ID of the item</param>
        internal ProductBacklogItem(HPMUniqueID uniqueID, HPMUniqueID uniqueTaskID)
            : base(uniqueID, uniqueTaskID)
        {
        }

        /// <summary>
        /// The list of users that are assigned to this product backlog item.
        /// </summary>
        public List<User> Assignees
        {
            get
            {
                return TaskHelper.GetAssignees(this);
            }
        }

        /// <summary>
        /// The assignment percentage of a particular user that is assigned to this product backlog item.
        /// </summary>
        /// <param name="user">The name of the user.</param>
        /// <returns>The assignment percentage.</returns>
        public int GetAssignmentPercentage(User user)
        {
            return TaskHelper.GetAssignmentPercentage(this, user);
        }

        /// <summary>
        /// True if this product backlog item is assigned, False otherwise.
        /// </summary>
        public bool IsAssigned
        {
            get
            {
                return TaskHelper.IsAssigned(this);
            }
        }

        /// <summary>
        /// The sprint that this product backlog item is committed to (if any), or null.
        /// </summary>
        public Sprint CommittedToSprint
        {
            get
            {
                return TaskHelper.GetCommittedToSprint(this);
            }
        }

        /// <summary>
        /// The product backlog priority of this product backlog item.
        /// </summary>
        public override HansoftEnumValue Priority
        {
            get
            {
                return new HansoftEnumValue(MainProjectID, EHPMProjectDefaultColumn.BacklogPriority, Session.TaskGetBacklogPriority(UniqueTaskID), (int)Session.TaskGetBacklogPriority(UniqueTaskID));
            }
            set
            {
                if (Priority != value) Session.TaskSetBacklogPriority(UniqueTaskID, (EHPMTaskAgilePriorityCategory)value.Value);
            }
        }
    }
}
