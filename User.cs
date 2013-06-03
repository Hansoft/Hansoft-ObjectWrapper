using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HPMSdk;

namespace Hansoft.ObjectWrapper
{
    /// <summary>
    /// Represents a user in Hansoft.
    /// </summary>
    public class User : Resource
    {
        /// <summary>
        /// Factory method for User.
        /// </summary>
        /// <param name="uniqueID">The ID of the user.</param>
        /// <returns></returns>
        static public User GetUser(HPMUniqueID uniqueID)
        {
            return new User(uniqueID);
        }

        private User(HPMUniqueID uniqueID)
            : base(uniqueID)
        {
        }

        /// <summary>
        /// The name of the User.
        /// </summary>
        public override string Name
        {
            get
            {
                HPMResourceProperties props = SessionManager.Session.ResourceGetProperties(UniqueID);
                return props.m_Name;
            }
            set
            {
                if (Name != value)
                {
                    HPMResourceProperties props = SessionManager.Session.ResourceGetProperties(UniqueID);
                    props.m_Name = value;
                    SessionManager.Session.ResourceSetProperties(UniqueID, props, null);
                }
            }
        }

        /// <summary>
        /// The Groups that the User is a member of.
        /// </summary>
        public List<Group> Groups
        {
            get
            {
                List<Group> groups = new List<Group>();
                HPMResourceProperties properties = Session.ResourceGetProperties(UniqueID);
                foreach (HPMUniqueID groupId in properties.m_MemberOfResourceGroups)
                    groups.Add(Group.GetGroup(groupId));
                return groups;
            }
        }

        /// <summary>
        /// The Projects that the User is a member of:
        /// </summary>
        public List<Project> Projects
        {
            get
            {
                return HPMUtilities.GetProjects().FindAll(p => p.Members.Contains(this));
            }
        }
    }
}
