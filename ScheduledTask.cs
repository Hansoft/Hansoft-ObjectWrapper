﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HPMSdk;

namespace Hansoft.ObjectWrapper
{
    /// <summary>
    /// Represents a scheduled task in the Schedule view of Hansoft.
    /// </summary>
    public class ScheduledTask : Task
    {
        internal static ScheduledTask GetScheduledTask(HPMUniqueID uniqueID, HPMUniqueID uniqueTaskID)
        {
            return new ScheduledTask(uniqueID, uniqueTaskID);
        }

        private ScheduledTask(HPMUniqueID uniqueID, HPMUniqueID uniqueTaskID)
            : base(uniqueID, uniqueTaskID)
        {
        }

        /// <summary>
        /// The list of users that are assigned to this scheduled task.
        /// </summary>
        public List<User> Assignees
        {
            get
            {
                return TaskHelper.GetAssignees(this);
            }
        }

        /// <summary>
        /// The assignment percentage of a particular user that is assigned to this scheduled task.
        /// </summary>
        /// <param name="user">The name of the user.</param>
        /// <returns>The assignment percentage.</returns>
        public int GetAssignmentPercentage(User user)
        {
            return TaskHelper.GetAssignmentPercentage(this, user);
        }

        /// <summary>
        /// True if this scheduled task is assigned, False otherwise.
        /// </summary>
        public bool IsAssigned
        {
            get
            {
                return TaskHelper.IsAssigned(this);
            }
        }

        /// <summary>
        /// Not applicable for scheduled tasks
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
