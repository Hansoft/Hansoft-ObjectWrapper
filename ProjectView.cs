using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HPMSdk;

namespace Hansoft.ObjectWrapper
{
    /// <summary>
    /// Abstract class for representing the different views in Hansoft, e.g., the Product Backlog, the Schedule, and the 
    /// Bug Tracker (QA View).
    /// </summary>
    public abstract class ProjectView : HansoftItem
    {
        internal ProjectView(HPMUniqueID uniqueID)
            : base(uniqueID)
        {
        }

        /// <summary>
        /// The Project that this ProjectView is associated with.
        /// </summary>
        public Project MainProject
        {
            get
            {
                return Project.GetProject(Session.UtilGetRealProjectIDFromProjectID(UniqueID));
            }
        }

        /// <summary>
        /// The Project that this ProjectView is associated with.
        /// </summary>
        public override HansoftItem Parent
        {
            get
            {
                return MainProject;
            }
        }

        /// <summary>
        /// A list with a sinle item which is the Project that this ProjectView is associated with.
        /// </summary>
        public override List<HansoftItem> AllParents
        {
            get
            {
                List<HansoftItem> parents = new List<HansoftItem>();
                parents.Add(Parent);
                return parents;
            }
        }

        /// <summary>
        /// Returns True if any items have been created in this ProjectView, False otherwise.
        /// </summary>
        public override bool HasChildren
        {
            get
            {
                return TaskHelper.HasChildren(UniqueID);
            }
        }

        /// <summary>
        /// The top level items of this ProjectView.
        /// </summary>
        public override List<HansoftItem> Children
        {
            get
            {
                return TaskHelper.GetChildren(UniqueID, false);
            }
        }

        /// <summary>
        /// All items in this ProjectView.
        /// </summary>
        public override List<HansoftItem> DeepChildren
        {
            get
            {
                return TaskHelper.GetChildren(UniqueID, true);
            }
        }

        /// <summary>
        /// Run a Find query in the view.
        /// </summary>
        /// <param name="query">The query to run.</param>
        /// <returns>The results of the query.</returns>
        public List<Task> Find(string query)
        {
            return Find(query, ReportViewType);
        }

        internal List<Task> Find(string findString, EHPMReportViewType viewType)
        {
            List<Task> found = new List<Task>();
            HPMFindContext findContext = new HPMFindContext();
            HPMFindContextData data = SessionManager.Session.UtilPrepareFindContext(findString, UniqueID, viewType, findContext);
            HPMTaskEnum items = SessionManager.Session.TaskFind(data, EHPMTaskFindFlag.None);
            foreach (HPMUniqueID taskID in items.m_Tasks)
                found.Add(Task.GetTask(taskID, viewType));
            return found;
        }

        /// <summary>
        /// Finds a custom column in the project view.
        /// </summary>
        /// <param name="columnName">The name of the column to find.</param>
        /// <returns>The column if found, otherwise null.</returns>
        public HPMProjectCustomColumnsColumn GetCustomColumn(string columnName)
        {
            HPMProjectCustomColumns allColumns = Session.ProjectCustomColumnsGet(UniqueID);
            foreach (HPMProjectCustomColumnsColumn customColumn in allColumns.m_ShowingColumns)
            {
                if ((customColumn.m_Name).Equals(columnName))
                    return customColumn;
            }
            return null;
        }

        /// <summary>
        /// Finds a custom column in the project view.
        /// </summary>
        /// <param name="columnHash">The hash of the column to find.</param>
        /// <returns>The column if found, otherwise null.</returns>
        public HPMProjectCustomColumnsColumn GetCustomColumn(uint columnHash)
        {
            return Session.ProjectGetCustomColumn(UniqueID, columnHash);
        }

        // TODO: Subject to refactoring
        internal abstract EHPMReportViewType ReportViewType { get; }
    }
}
