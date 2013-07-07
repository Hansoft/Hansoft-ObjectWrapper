using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HPMSdk;

namespace Hansoft.ObjectWrapper.CustomColumnValues
{
    /// <summary>
    /// Encapsulates a tasks value for custom column of resources/people type.
    /// </summary>
    public class ResourcesValue : CustomColumnValue
    {
        private List<Resource> resources;

        internal static new ResourcesValue FromInternalValue(Task task, HPMProjectCustomColumnsColumn customColumn, string internalValue)
        {
            List<Resource> resources = new List<Resource>();
            HPMResourceDefinitionList resourceList = SessionManager.Session.UtilDecodeCustomColumnResourcesValue(internalValue);
            foreach (HPMResourceDefinition resourceDefinition in resourceList.m_Resources)
            {
                if (resourceDefinition.m_GroupingType == EHPMResourceGroupingType.Resource)
                {
                    if (SessionManager.Session.UtilIsIDValid(resourceDefinition.m_ID))
                        resources.Add(User.GetUser(resourceDefinition.m_ID));
                }
                else if (resourceDefinition.m_GroupingType == EHPMResourceGroupingType.ResourceGroup)
                {
                    if (SessionManager.Session.UtilIsIDValid(resourceDefinition.m_ID))
                        resources.Add(Group.GetGroup(resourceDefinition.m_ID));
                }
                else if (resourceDefinition.m_GroupingType == EHPMResourceGroupingType.AllProjectMembers)
                    resources.Add(AllProjectMembers.Instance);
            }
            return new ResourcesValue(task, customColumn, internalValue, resources);
        }

        internal static ResourcesValue FromResourceList(Task task, HPMProjectCustomColumnsColumn customColumn, List<Resource> resources)
        {
            string internalValue;
            HPMResourceDefinitionList resDefList = new HPMResourceDefinitionList();
            resDefList.m_Resources = new HPMResourceDefinition[resources.Count];
            for (int i=0; i< resources.Count; i += 1)
            {
                resDefList.m_Resources[i] = new HPMResourceDefinition();
                if (resources[i] is User)
                    resDefList.m_Resources[i].m_GroupingType = EHPMResourceGroupingType.Resource;
                else if (resources[i] is Group)
                    resDefList.m_Resources[i].m_GroupingType = EHPMResourceGroupingType.ResourceGroup;
                else if (resources[i] is AllProjectMembers)
                    resDefList.m_Resources[i].m_GroupingType = EHPMResourceGroupingType.AllProjectMembers;
                resDefList.m_Resources[i].m_ID = resources[i].UniqueID;
            }
            internalValue = SessionManager.Session.UtilEncodeCustomColumnResourcesValue(resDefList, task.MainProjectID);
            return new ResourcesValue(task, customColumn, internalValue, resources);
        }

        private ResourcesValue(Task task, HPMProjectCustomColumnsColumn customColumn, string internalValue, List<Resource> resources)
            : base(task, customColumn, internalValue)
        {
            this.resources = resources;
        }

        /// <summary>
        /// The value as a string formatted as in the Hansoft client.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            return ListUtils.ToString(new List<HansoftItem>(resources), ';');
        }

        public override long ToInt()
        {
            throw new NotImplementedException();
        }

        public override double ToDouble()
        {
            throw new NotImplementedException();
        }

        public override DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Implementation of IComparable
        /// </summary>
        /// <param name="obj">The other object to compare with.</param>
        /// <returns>The result of the comparison</returns>
        public override int CompareTo(object obj)
        {
            return this.ToString().CompareTo(obj.ToString());
        }
    }
}
