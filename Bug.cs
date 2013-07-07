using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HPMSdk;

namespace Hansoft.ObjectWrapper
{
    /// <summary>
    /// Represents a bug in the Hansoft QA View / Bug Tracker
    /// </summary>
    public class Bug : Task
    {

        internal static Bug GetBug(HPMUniqueID uniqueID, HPMUniqueID uniqueTaskID)
        {
            return new Bug(uniqueID, uniqueTaskID);
        }

        private Bug(HPMUniqueID uniqueID, HPMUniqueID uniqueTaskID)
            : base(uniqueID, uniqueTaskID)
        {
        }

        /// <summary>
        /// The Sprint that this Bug is committed to (if any), or null.
        /// </summary>
        public Sprint CommittedToSprint
        {
            get
            {
                return TaskHelper.GetCommittedToSprint(this);
            }
        }

        /// <summary>
        /// The project view that this bug belongs to.
        /// </summary>
        public override ProjectView ProjectView
        {
            get
            {
                return Project.BugTracker;
            }
        }

        /// <summary>
        /// The list of users that are assigned to this Bug.
        /// </summary>
        public List<User> Assignees
        {
            get
            {
                return TaskHelper.GetAssignees(this);
            }
        }

        /// <summary>
        /// The assignment percentage of a particular User that is assigned to this Bug.
        /// </summary>
        /// <param name="user">The name of the user.</param>
        /// <returns>The assignment percentage.</returns>
        public int GetAssignmentPercentage(User user)
        {
            return TaskHelper.GetAssignmentPercentage(this, user);
        }

        /// <summary>
        /// True if this Bug is assigned, False otherwise.
        /// </summary>
        public bool IsAssigned
        {
            get
            {
                return TaskHelper.IsAssigned(this);
            }
        }

        /// <summary>
        /// The builtin column Bug priority.
        /// </summary>
        public override HansoftEnumValue Priority
        {
            get
            {
                return new HansoftEnumValue(MainProjectID, EHPMProjectDefaultColumn.BugPriority, Session.TaskGetBugPriority(UniqueTaskID), (int)Session.TaskGetBugPriority(UniqueTaskID));
            }
            set
            {
                if (Priority != value) Session.TaskSetBugPriority(UniqueTaskID, (EHPMTaskAgilePriorityCategory)value.Value);
            }
        }

        /// <summary>
        /// Returns the raw content, including any markup, of the builtin column Steps to reproduce.
        /// </summary>
        public string StepsToReproduce
        {
            get { return Session.TaskGetStepsToReproduce(UniqueID); }
        }

        /// <summary>
        /// Returns the builtin column Steps to reproduce, but stripped from markup.
        /// </summary>
        public string StepsToReproduceText
        {
            get { return HPMUtilities.HansoftMarkupToText(StepsToReproduce); }
        }

        /// <summary>
        /// Returns the builtin column Steps to reproduce, but formatted as Html.
        /// </summary>
        public string StepsToReproduceHtml
        {
            get { return HPMUtilities.HansoftMarkupToHtml(StepsToReproduce); }
        }
    }
}
