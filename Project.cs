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
                HPMUniqueID qaProjectID = Session.ProjectUtilGetQA(UniqueID);
                if (qaProjectID.m_ID == -1)
                    qaProjectID = Session.ProjectUtilGetQA(UniqueID);
                return qaProjectID;
            }
        }

        HPMUniqueID BacklogProjectID
        {
            get
            {
                HPMUniqueID backlogProjectID = Session.ProjectUtilGetBacklog(UniqueID);
                if (backlogProjectID.m_ID == -1)
                    backlogProjectID = Session.ProjectUtilGetBacklog(UniqueID);
                return backlogProjectID;
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
        public IEnumerable<Task> AgileItems
        {
            get
            {
                return Schedule.DeepChildren.Cast<Task>();
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
        public IEnumerable<Task> ScheduledItems
        {
            get
            {
                return Schedule.DeepChildren.Cast<Task>();
            }
        }

        /// <summary>
        /// Will return all items (recursively) in the Product Backlog view
        /// </summary>
        public IEnumerable<ProductBacklogItem> ProductBacklogItems
        {
            get
            {
                return ProductBacklog.DeepChildren.Cast<ProductBacklogItem>();
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

        /// <summary>
        /// Will return all product backlog items committed as scheduled tasks
        /// </summary>
        public List<ProductBacklogItemInSchedule> ProductBacklogItemsInSchedule
        {
            get
            {
                List<ProductBacklogItemInSchedule> found = new List<ProductBacklogItemInSchedule>();
                foreach (Task t in ScheduledItems)
                    if (t is ProductBacklogItemInSchedule)
                        found.Add((ProductBacklogItemInSchedule)t);
                return found;
            }
        }

        /// <summary>
        /// Checks if a certain date in the project is a working day according to the project calendar.
        /// </summary>
        /// <param name="date">The date to check.</param>
        /// <returns>true if date is a working day in the project otherwise false.</returns>
        public bool IsWorkingDay(DateTime date)
        {
            HPMCalendarDayInfo info = Session.ProjectGetCalendarDayInfo(UniqueID, -1, HPMUtilities.HPMDateTime(date.Date));
            return info.m_bWorkingDay;
        }

        /// <summary>
        /// Get the previous working based on the project calendar.
        /// </summary>
        /// <param name="day">The day to get the previous day for.</param>
        /// <returns>The previous day.</returns>
        public DateTime GetPreviousWorkingDay(DateTime day)
        {
            DateTime previousDay = day.AddDays(-1);
            while (!IsWorkingDay(previousDay))
                previousDay = previousDay.AddDays(-1);
            return previousDay;
        }

        /// <summary>
        /// Get the sprint prediction method in number of days as set in the project options.
        /// </summary>
        public int AverageVelocitySpan
        {
            get
            {
                EHPMProjectSprintPredictionMethod method = Session.ProjectGetSettings(UniqueID).m_SprintPredictionMethod;
                switch (method)
                {
                    case EHPMProjectSprintPredictionMethod.N3Days_WeightedAverage:
                        return 3;
                    case EHPMProjectSprintPredictionMethod.N5Days_WeightedAverage:
                        return 5;
                    case EHPMProjectSprintPredictionMethod.N7Days_WeightedAverage:
                        return 7;
                    case EHPMProjectSprintPredictionMethod.N10Days_WeightedAverage:
                        return 10;
                    case EHPMProjectSprintPredictionMethod.N14Days_WeightedAverage:
                    default:
                        return 14;
                }
            }
        }

        /// <summary>
        /// Create a replica of this project including tasks, reports, workflows/kanbans, and view presets. The intent is to
        /// use one project as a template and instantiate new projects from this template.
        /// </summary>
        /// <param name="newName"></param>
        /// <returns></returns>
        public Project Clone(String newName)
        {
            HPMProjectProperties templateProperties = Session.ProjectGetProperties(UniqueID);
            templateProperties.m_Name = newName;
            HPMUniqueID newProjectID = Session.ProjectCreate(templateProperties);
            Project newProject = Project.GetProject(newProjectID);

            CloneColumns(this.Schedule.UniqueID, newProject.Schedule.UniqueID);
            CloneColumns(this.ProductBacklog.UniqueID, newProject.ProductBacklog.UniqueID);
            CloneColumns(this.BugTracker.UniqueID, newProject.BugTracker.UniqueID);

            CloneReports(this.Schedule.UniqueID, newProject.Schedule.UniqueID);
            CloneReports(this.ProductBacklog.UniqueID, newProject.ProductBacklog.UniqueID);
            CloneReports(this.BugTracker.UniqueID, newProject.BugTracker.UniqueID);

            ClonePresets(this.Schedule.UniqueID, newProject.Schedule.UniqueID);
            ClonePresets(this.ProductBacklog.UniqueID, newProject.ProductBacklog.UniqueID);
            ClonePresets(this.BugTracker.UniqueID, newProject.BugTracker.UniqueID);

            CloneWorkflows(this.UniqueID, newProject.UniqueID);

            CloneChildTasks(this.Schedule, newProject.Schedule, newProject.Schedule, Session.ProjectCustomColumnsGet(this.Schedule.UniqueID));
            CloneChildTasks(this.ProductBacklog, newProject.ProductBacklog, newProject.ProductBacklog, Session.ProjectCustomColumnsGet(this.ProductBacklog.UniqueID));

            CloneBugWorkflow(this.BugTracker.UniqueID, newProject.BugTracker.UniqueID);

            // TODO; Modify the workflow in the Bugtracker
            // Optionally: Set the start/finish dates for any sprints/scheduled tasks so that they are offseted based on the current date
            // Optonally: replicate tasks in the QA view
            // Optionally: Loop over again to recreate any links / committed items / release tags / delegation / applied or default pipelines and workflows

            return newProject;
        }

        private void CloneColumns(HPMUniqueID sourceProjectID, HPMUniqueID targetProjectID)
        {
            HPMProjectCustomColumns columns = Session.ProjectCustomColumnsGet(sourceProjectID);

            HPMProjectCustomColumnChanges newColumns = new HPMProjectCustomColumnChanges();
            newColumns.m_ProjectID = sourceProjectID;

            List<HPMProjectCustomColumnsColumn> added = new List<HPMProjectCustomColumnsColumn>();
       
            foreach (HPMProjectCustomColumnsColumn column in columns.m_ShowingColumns)
            {
                added.Add(column);
            }
            foreach (HPMProjectCustomColumnsColumn column in columns.m_HiddenColumns)
            {
                added.Add(column);
            }

            newColumns.m_Added = added.ToArray();

            Session.ProjectCustomColumnsSet(newColumns);
        }

        private void CloneReports(HPMUniqueID sourceProjectID, HPMUniqueID targetProjectID)
        {
            foreach (HPMUniqueID resourceID in Session.ProjectEnumReportResources(sourceProjectID).m_Resources)
                Session.ProjectSetReports(targetProjectID, resourceID, Session.ProjectGetReports(sourceProjectID, resourceID));
        }

        private void ClonePresets(HPMUniqueID sourceProjectID, HPMUniqueID targetProjectID)
        {
            foreach (HPMProjectViewPreset preset in Session.ProjectGetViewPresets(sourceProjectID).m_Presets)
                Session.ProjectCreateViewPreset(targetProjectID, preset);
        }

        private void CloneWorkflows(HPMUniqueID sourceProjectID, HPMUniqueID targetProjectID)
        {
            foreach (uint iWorkflow in Session.ProjectWorkflowEnum(sourceProjectID, true).m_Workflows)
            {
                HPMProjectWorkflowSettings settings = Session.ProjectWorkflowGetSettings(sourceProjectID, iWorkflow);
                Session.ProjectWorkflowCreate(targetProjectID, settings.m_Properties);
                Session.ProjectWorkflowSetSettings(targetProjectID, settings.m_Identifier,settings);
            }
        }

        private void CloneBugWorkflow(HPMUniqueID sourceQAProjectID, HPMUniqueID targetQAProjectID)
        {
        }

        private void CloneChildTasks(HansoftItem sourceParent, HansoftItem targetParent, ProjectView targetProject, HPMProjectCustomColumns customColumns)
        {
            Task newTask = null;
            foreach (Task task in sourceParent.Children)
            {
                HPMTaskCreateUnifiedReference prevRefID = new HPMTaskCreateUnifiedReference();
                if (newTask == null)
                    prevRefID.m_RefID = -1;
                else
                    prevRefID.m_RefID = newTask.UniqueTaskID;
                prevRefID.m_bLocalID = false;

                HPMTaskCreateUnifiedReference prevWorkPrioRefID = new HPMTaskCreateUnifiedReference();
                prevWorkPrioRefID.m_RefID = -2;
                prevWorkPrioRefID.m_bLocalID = false;

                HPMTaskCreateUnifiedReference[] parentRefIds = new HPMTaskCreateUnifiedReference[1];
                parentRefIds[0] = new HPMTaskCreateUnifiedReference();
                parentRefIds[0].m_RefID = targetParent.Id; // This should be a taskref, which it should be
                parentRefIds[0].m_bLocalID = false;

                HPMTaskCreateUnified createTaskData = new HPMTaskCreateUnified();
                createTaskData.m_Tasks = new HPMTaskCreateUnifiedEntry[1];
                createTaskData.m_Tasks[0] = new HPMTaskCreateUnifiedEntry();
                createTaskData.m_Tasks[0].m_bIsProxy = false;
                createTaskData.m_Tasks[0].m_LocalID = -1;
                createTaskData.m_Tasks[0].m_ParentRefIDs = parentRefIds;
                createTaskData.m_Tasks[0].m_PreviousRefID = prevRefID;
                createTaskData.m_Tasks[0].m_PreviousWorkPrioRefID = prevWorkPrioRefID;
                createTaskData.m_Tasks[0].m_NonProxy_ReuseID = 0;
                createTaskData.m_Tasks[0].m_TaskLockedType = Session.TaskGetLockedType(task.UniqueTaskID);
                createTaskData.m_Tasks[0].m_TaskType = Session.TaskGetType(task.UniqueTaskID);


                HPMChangeCallbackData_TaskCreateUnified createdData = Session.TaskCreateUnifiedBlock(targetProject.UniqueID, createTaskData);
                if (createdData.m_Tasks.Length == 1)
                {
                    newTask = Task.GetTask(createdData.m_Tasks[0].m_TaskRefID);
                    newTask.Category = task.Category;
                    newTask.Confidence = task.Confidence;
                    newTask.DetailedDescription = task.DetailedDescription;
                    newTask.EstimatedDays = task.EstimatedDays;
                    newTask.Hyperlink = task.Hyperlink;
                    newTask.Name = task.Name;
                    newTask.Points = task.Points;
                    newTask.Priority = task.Priority;
                    newTask.Risk = task.Risk;
                    newTask.Severity = task.Severity;
                    newTask.Status = task.Status;
                    newTask.WorkRemaining = task.WorkRemaining;
                    Session.TaskSetFullyCreated(newTask.UniqueTaskID);
                    foreach (HPMProjectCustomColumnsColumn column in customColumns.m_ShowingColumns)
                        newTask.SetCustomColumnValue(column.m_Name, task.GetCustomColumnValue(column.m_Name));
                    CloneChildTasks(task, newTask, targetProject, customColumns);
                }
            }
        }

    }
}
