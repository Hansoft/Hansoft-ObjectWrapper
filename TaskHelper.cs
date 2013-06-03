using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HPMSdk;

namespace Hansoft.ObjectWrapper
{
    internal static class TaskHelper
    {
        internal static bool HasChildren(HPMUniqueID uniqueID)
        {
            // Work around for SDK bug in TaskRefUtilHasChildren when uniqueID is the product backlog
            return Session.TaskRefUtilEnumChildren(uniqueID, false).m_Tasks.Length >0 ;
        }

        internal static List<HansoftItem> GetChildren(HPMUniqueID uniqueID, bool deep)
        {
            List<HansoftItem> children = new List<HansoftItem>();
            if (HasChildren(uniqueID))
            {
                HPMTaskEnum tasks = Session.TaskRefUtilEnumChildren(uniqueID, deep);
                foreach (HPMUniqueID id in tasks.m_Tasks)
                    children.Add(Task.GetTask(id));
            }
            return children;
        }

        internal static List<HansoftItem> GetChildren(HPMUniqueID uniqueID)
        {
            return GetChildren(uniqueID, false);
        }

        internal static List<HansoftItem> GetDeepChildren(HPMUniqueID uniqueID)
        {
            return GetChildren(uniqueID, true);
        }

        internal static string AssignedAsString(Task task)
        {
            StringBuilder sb = new StringBuilder();
            HPMTaskResourceAllocationResource[] allocations = SessionManager.Session.TaskGetResourceAllocation(task.UniqueTaskID).m_Resources;
            for (int i = 0; i < allocations.Length; i += 1)
            {
                HPMUniqueID resourceId = allocations[i].m_ResourceID;
                int percentage = allocations[i].m_PercentAllocated;
                sb.Append(User.GetUser(resourceId).Name);
                if (percentage != 100)
                {
                    sb.Append('[');
                    sb.Append(percentage.ToString());
                    sb.Append("]");
                }
                if (i < allocations.Length - 1)
                    sb.Append(", ");
            }
            return sb.ToString();
        }

        private static HPMSdkSession Session
        {
            get
            {
                return SessionManager.Session;
            }
        }

        internal static Sprint GetCommittedToSprint(Task task) 
        {
        HPMUniqueID uid = Session.TaskGetLinkedToSprint(task.UniqueID);
        if (uid.m_ID != -1)
            return (Sprint) Task.GetTask(uid);
        else
            return null;
    }

        internal static List<User> GetAssignees(Task task)
        {
            List<User> assignees = new List<User>();
            HPMTaskResourceAllocationResource[] allocations = Session.TaskGetResourceAllocation(task.UniqueTaskID).m_Resources;
            foreach (HPMTaskResourceAllocationResource ra in allocations)
                assignees.Add(User.GetUser(ra.m_ResourceID));
            return assignees;
        }

        internal static int GetAssignmentPercentage(Task task, User user)
        {
            HPMTaskResourceAllocationResource[] allocations = Session.TaskGetResourceAllocation(task.UniqueTaskID).m_Resources;
            foreach (HPMTaskResourceAllocationResource ra in allocations)
                if (ra.m_ResourceID.m_ID == user.UniqueID.m_ID)
                    return ra.m_PercentAllocated;
            return 0;
        }

        internal static bool IsAssigned(Task task)
        {
            HPMTaskResourceAllocationResource[] allocations = Session.TaskGetResourceAllocation(task.UniqueTaskID).m_Resources;
            if (allocations.Length == 0)
                return false;
            else
                return (allocations[0].m_ResourceID.m_ID != -1);
        }
    }
}
