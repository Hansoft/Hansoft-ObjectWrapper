using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HPMSdk;

namespace Hansoft.ObjectWrapper
{
    /// <summary>
    /// Represents a Hansoft project.
    /// </summary>
    public class Project : HansoftItem
    {
        internal static Project GetProject(HPMUniqueID uniqueID)
        {
            return new Project(uniqueID);
        }

        private Project(HPMUniqueID projID)
            : base(projID)
        {
        }

        /// <summary>
        /// The name of the project.
        /// </summary>
        public override string Name
        {
            get
            {
                HPMProjectProperties props = Session.ProjectGetProperties(UniqueID);
                return props.m_Name;
            }

            set
            {
                if (Name != value)
                {
                    HPMProjectProperties props = Session.ProjectGetProperties(UniqueID);
                    props.m_Name = value;
                    Session.ProjectSetProperties(UniqueID, props);
                }
            }
        }

        HPMUniqueID QAProjectID
        {
            get
            {
                return Session.ProjectUtilGetQA(UniqueID);
            }
        }

        HPMUniqueID BacklogProjectID
        {
            get
            {
                return Session.ProjectUtilGetBacklog(UniqueID);
            }
        }

        /// <summary>
        /// Will always return null as projects are top level items.
        /// </summary>
        public override HansoftItem Parent
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Will always return an empty list as projects are top level items.
        /// </summary>
        public override List<HansoftItem> AllParents
        {
            get
            {
                return new List<HansoftItem>();
            }
        }

        /// <summary>
        /// Will always return True as Projects already has children in the form of their project views (Schedule, Product Backlog and so on).
        /// </summary>
        public override bool HasChildren
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Will return a list with the ProjectViews of the project, namely the Schedule, the Bug Tracker/QA View, and the Product Backlog of the project.
        /// </summary>
        public override List<HansoftItem> Children
        {
            get
            {
                List<HansoftItem> children = new List<HansoftItem>();
                children.Add(Schedule);
                children.Add(ProductBacklog);
                children.Add(BugTracker);
                return children;
            }
        }

        /// <summary>
        /// Will return a list with the three project views of the project (the Schedule, the Bug Tracker/QA View, and the Product Backlog) and
        /// all the items (recursively) contained in the project views.
        /// </summary>
        public override List<HansoftItem> DeepChildren
        {
            get
            {
                List<HansoftItem> children = new List<HansoftItem>();
                children.Add(Schedule);
                children.AddRange(Schedule.DeepChildren);
                children.Add(ProductBacklog);
                children.AddRange(ProductBacklog.DeepChildren);
                children.Add(BugTracker);
                children.AddRange(BugTracker.DeepChildren);
                return children;
            }
        }

        /// <summary>
        /// Lists all users that are members of this project.
        /// </summary>
        public List<User> Members
        {
            get
            {
                List<User> members = new List<User>();
                HPMProjectResourceEnum resources = Session.ProjectResourceEnum(UniqueID);
                foreach (HPMUniqueID resId in resources.m_Resources)
                    members.Add(User.GetUser(resId));
                return members;
            }
        }

        /// <summary>
        /// The bug tracker/QA view of the project.
        /// </summary>
        public BugTracker BugTracker
        {
            get
            {
                return BugTracker.GetBugTracker(QAProjectID);
            }
        }

        /// <summary>
        /// The schedule view of the project.
        /// </summary>
        public Schedule Schedule
        {
            get
            {
                return Schedule.GetSchedule(UniqueID);
            }
        }

        /// <summary>
        /// The product backlog of the project.
        /// </summary>
        public ProductBacklog ProductBacklog
        {
            get
            {
                return ProductBacklog.GetProductBacklog(BacklogProjectID);
            }
        }

        /// <summary>
        /// Will return all items (recursively) in the Schedule view
        /// </summary>
        public List<Task> AgileItems
        {
            get
            {
                return Schedule.Find("");
            }
        }

        /// <summary>
        /// Will return all bugs in the project.
        /// </summary>
        public List<Bug> Bugs
        {
            get
            {
                List<Task> items = BugTracker.Find("");
                List<Bug> bugs = new List<Bug>();
                foreach (Task t in items)
                    bugs.Add((Bug)t);
                return bugs;
            }
        }

        /// <summary>
        /// Will return all items (recursively) in the Schedule view
        /// </summary>
        public List<Task> ScheduledItems
        {
            get
            {
                return Schedule.Find("");
            }
        }

        /// <summary>
        /// Will return all items (recursively) in the Product Backlog view
        /// </summary>
        public List<ProductBacklogItem> ProductBacklogItems
        {
            get
            {
                List<Task> items = ProductBacklog.Find("");
                List<ProductBacklogItem> backlogItems = new List<ProductBacklogItem>();
                foreach (HansoftItem t in items)
                    backlogItems.Add((ProductBacklogItem)t);
                return backlogItems;
            }
        }

        /// <summary>
        /// Will return all the Releases in the project.
        /// </summary>
        public List<Release> Releases
        {
            get
            {
                List<Release> found = new List<Release>();
                foreach (HansoftItem t in ScheduledItems)
                    if (t is Release)
                        found.Add((Release)t);
                return found;
            }
        }

        /// <summary>
        /// Will return all Sprints in the project.
        /// </summary>
        public List<Sprint> Sprints
        {
            get
            {
                List<Sprint> found = new List<Sprint>();
                foreach (Task t in AgileItems)
                    if (t is Sprint)
                        found.Add((Sprint)t);
                return found;
            }
        }

        /// <summary>
        /// Will return all scheduled tasks in the project.
        /// </summary>
        public List<ScheduledTask> ScheduledTasks
        {
            get
            {
                List<ScheduledTask> found = new List<ScheduledTask>();
                foreach (Task t in ScheduledItems)
                    if (t is ScheduledTask)
                        found.Add((ScheduledTask)t);
                return found;
            }
        }
    }
}
