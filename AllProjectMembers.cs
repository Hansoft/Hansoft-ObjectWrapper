using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hansoft.ObjectWrapper
{
    /// <summary>
    /// Represents assignment to all users in the project. This class is a singleton.
    /// </summary>
    public class AllProjectMembers : Resource
    {

        static private AllProjectMembers instance;

        /// <summary>
        ///  The single instance of this class.
        /// </summary>
        static public AllProjectMembers Instance
        {
            get
            {
                if (instance == null)
                    instance = new AllProjectMembers();
                return instance;
            }
        }

        private AllProjectMembers()
            : base(null)
        {
        }

        /// <summary>
        /// Returns the localized string representing all project members in the Hansoft user interface. Cannot be set.
        /// </summary>
        public override string Name
        {
            get
            {
                return HPMUtilities.GetLocalizedStringForFullId("NLocalize/NDatabaseFunctions/NUsersProjects/AllProjectMembers");
            }
            set
            {
            }
        }

    }
}
