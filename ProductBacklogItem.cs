using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HPMSdk;
using Hansoft.ObjectWrapper.CustomColumnValues;

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
        /// The project view that this product backlog item belongs to.
        /// </summary>
        public override ProjectView ProjectView
        {
            get
            {
                return Project.ProductBacklog;
            }
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


        private int absolutePriority;
        internal int AbsolutePriority
        {
            get
            {
                return absolutePriority;
            }
            set
            {
                absolutePriority = value;
            }
        }

        /// <summary>
        /// The aggregated status value over all children as it is displayed in the Hansoft client
        /// </summary>
        public override HansoftEnumValue AggregatedStatus
        {
            get
            {
                HPMUniqueID proxyID = Session.TaskGetProxy(UniqueTaskID);
                if (proxyID.m_ID != -1 && proxyID.m_ID != UniqueID.m_ID)
                {
                    // This is a bit of a hack to compensate for that TaskRefSummary in the SDK does not account
                    // for if a product backlog item is committed to the schedule and roken down there.
                    Task proxy = Task.GetTask(proxyID);
                    return proxy.AggregatedStatus;
                }
                else
                    return base.AggregatedStatus;
            }
        }

        /// <summary>
        /// The aggregated points value over all children as it is displayed in the Hansoft client
        /// </summary>
        public override int AggregatedPoints
        {
            get
            {
                HPMUniqueID proxyID = Session.TaskGetProxy(UniqueTaskID);
                if (proxyID.m_ID == this.UniqueID.m_ID) // This is an item in the schedule and we will use the base implementation
                    return base.AggregatedPoints;
                else if (proxyID.m_ID != -1) // If it is an item in the product backlog with a proxy we use the proxies summary, hence ignoring breakdown of committed items in the PBL
                    return Task.GetTask(proxyID).AggregatedPoints;
                else if (!HasChildren) // It is a leaf
                    return base.AggregatedPoints;
                else // It is a parent item which is not committed and hence we will iterate over the children.
                {
                    int aggregatedPoints = 0;
                    foreach (ProductBacklogItem item in DeepLeaves)
                    {
                        proxyID = Session.TaskGetProxy(item.UniqueTaskID);
                        if (proxyID.m_ID != -1)
                            aggregatedPoints += Task.GetTask(proxyID).AggregatedPoints;
                        else
                            aggregatedPoints += item.Points;
                    }
                    return aggregatedPoints;
                }
            }
        }

        /// <summary>
        /// The aggregated work remaining value over all children as it is displayed in the Hansoft client
        /// </summary>
        public override double AggregatedWorkRemaining
        {
            get
            {
                HPMUniqueID proxyID = Session.TaskGetProxy(UniqueTaskID);
                if (proxyID.m_ID == this.UniqueID.m_ID) // This is an item in the schedule and we will use the base implementation
                    return base.AggregatedWorkRemaining;
                else if (proxyID.m_ID != -1) // If it is an item in the product backlog with a proxy we use the proxies summary, hence ignoring breakdown of committed items in the PBL
                    return Task.GetTask(proxyID).AggregatedWorkRemaining;
                else if (!HasChildren) // It is a leaf
                    return base.AggregatedWorkRemaining;
                else // It is a parent item which is not committed and hence we will iterate over the children.
                {
                    double aggregatedWorkRemaining = 0;
                    foreach (ProductBacklogItem item in DeepLeaves)
                    {
                        proxyID = Session.TaskGetProxy(item.UniqueTaskID);
                        if (proxyID.m_ID != -1)
                            aggregatedWorkRemaining += Task.GetTask(proxyID).AggregatedWorkRemaining;
                        else
                        {
                            if (!double.IsInfinity(item.WorkRemaining))
                                aggregatedWorkRemaining += item.WorkRemaining;
                        }
                    }
                    return aggregatedWorkRemaining;
                }
            }
        }

        /// <summary>
        /// The aggregated estimated days value over all children as it is displayed in the Hansoft client
        /// </summary>
        public override double AggregatedEstimatedDays
        {
            get
            {
                HPMUniqueID proxyID = Session.TaskGetProxy(UniqueTaskID);
                if (proxyID.m_ID == this.UniqueID.m_ID) // This is an item in the schedule and we will use the base implementation
                    return base.AggregatedEstimatedDays;
                else if (proxyID.m_ID != -1) // If it is an item in the product backlog with a proxy we use the proxies summary, hence ignoring breakdown of committed items in the PBL
                    return Task.GetTask(proxyID).AggregatedEstimatedDays;
                else if (!HasChildren) // It is a leaf
                    return base.AggregatedEstimatedDays;
                else // It is a parent item which is not committed and hence we will iterate over the children.
                {
                    double aggregatedEstimatedDays = 0;
                    foreach (ProductBacklogItem item in DeepLeaves)
                    {
                        proxyID = Session.TaskGetProxy(item.UniqueTaskID);
                        if (proxyID.m_ID != -1)
                            aggregatedEstimatedDays += Task.GetTask(proxyID).AggregatedEstimatedDays;
                        else
                            aggregatedEstimatedDays += item.EstimatedDays;
                    }
                    return aggregatedEstimatedDays;
                }
            }
        }

        internal override CustomColumnValue GetAggregatedCustomColumnValue(HPMProjectCustomColumnsColumn customColumn)
        {
            HPMUniqueID proxyID = Session.TaskGetProxy(UniqueTaskID);
            if (proxyID.m_ID == this.UniqueID.m_ID) // This is an item in the schedule and we will use the base implementation
                return base.GetAggregatedCustomColumnValue(customColumn);
            else if (proxyID.m_ID != -1) // If it is an item in the product backlog with a proxy we use the proxies summary, hence ignoring breakdown of committed items in the PBL
                return Task.GetTask(proxyID).GetAggregatedCustomColumnValue(customColumn);
            else if (!HasChildren) // It is a leaf
                return base.GetAggregatedCustomColumnValue(customColumn);
            else // It is a parent item which is not committed and hence we will iterate over the children.
            {
                if (customColumn.m_Type == EHPMProjectCustomColumnsColumnType.IntegerNumber)
                {
                    long aggregatedInt = 0;
                    foreach (ProductBacklogItem item in DeepLeaves)
                    {
                        proxyID = Session.TaskGetProxy(item.UniqueTaskID);
                        if (proxyID.m_ID != -1)
                            aggregatedInt += Task.GetTask(proxyID).GetAggregatedCustomColumnValue(customColumn).ToInt();
                        else
                            aggregatedInt += item.GetCustomColumnValue(customColumn).ToInt();
                    }
                    return IntegerNumberValue.FromInteger(this, customColumn, aggregatedInt);
                }
                else if (customColumn.m_Type == EHPMProjectCustomColumnsColumnType.FloatNumber)
                {
                    double aggregatedFloat = 0;
                    foreach (ProductBacklogItem item in DeepLeaves)
                    {
                        proxyID = Session.TaskGetProxy(item.UniqueTaskID);
                        if (proxyID.m_ID != -1)
                            aggregatedFloat += Task.GetTask(proxyID).GetAggregatedCustomColumnValue(customColumn).ToDouble();
                        else
                            aggregatedFloat += item.GetCustomColumnValue(customColumn).ToDouble();
                    }
                    return FloatNumberValue.FromFloat(this, customColumn, aggregatedFloat);
                }
                else
                {
                    double aggregatedTime = 0;
                    foreach (ProductBacklogItem item in DeepLeaves)
                    {
                        proxyID = Session.TaskGetProxy(item.UniqueTaskID);
                        if (proxyID.m_ID != -1)
                            aggregatedTime += Task.GetTask(proxyID).GetAggregatedCustomColumnValue(customColumn).ToDouble();
                        else
                            aggregatedTime += item.GetCustomColumnValue(customColumn).ToDouble();
                    }
                    return AccumulatedTimeValue.FromFloat(this, customColumn, aggregatedTime);
                }                
            }
        }
    }
}
