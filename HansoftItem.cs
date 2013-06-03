using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HPMSdk;

namespace Hansoft.ObjectWrapper
{
    /// <summary>
    /// Base class for all items in Hansoft that has a Unique ID.
    /// </summary>
    public abstract class HansoftItem
    {

        HPMUniqueID uniqueID;
        internal HansoftItem(HPMUniqueID uniqueID)
        {
            this.uniqueID = uniqueID;
        }

        /// <summary>
        /// Returns the Unique ID for the item. For Task subclasses this will return the task reference in the applicable view (Backlog, Schedule or Bugs).
        /// </summary>
        public HPMUniqueID UniqueID
        {
            get
            {
                return uniqueID;
            }
        }

        /// <summary>
        /// The unique id as an integer. 
        /// </summary>
        public int Id
        {
            get
            {
                return uniqueID.m_ID;
            }
        }

        /// <summary>
        /// The name of the type of this instance
        /// </summary>
        public string TypeName
        {
            get
            {
                return this.GetType().Name;
            }
        }

        /// <summary>
        /// The currently connected (through the SessionManager) SDK Session
        /// </summary>
        static internal HPMSdkSession SdkSession
        {
            get { return SessionManager.Session; }
        }

        /// <summary>
        /// The currently connected (through the SessionManager) SDK Session
        /// </summary>
        internal HPMSdkSession Session
        {
            get { return HansoftItem.SdkSession; }
        }

        /// <summary>
        /// Override of object.Equals that compares the Hansoft Unique ID of items.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            HansoftItem other = obj as HansoftItem;
            if (obj != null)
                return uniqueID.m_ID == other.uniqueID.m_ID;
            else
                return base.Equals(obj);
        }

        /// <summary>
        /// Override of object.Hashcode to make sure that objects with the same Unique ID
        /// returns the same Hashcode.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return UniqueID.m_ID;
        }

        /// <summary>
        /// The item that this item is arranged under in the particular view/hierarchy.
        /// </summary>
        public abstract HansoftItem Parent
        {
            get;
        }

        /// <summary>
        /// The full chain of parents to the top of the view. 
        /// </summary>
        public abstract List<HansoftItem> AllParents
        {
            get;
        }

        /// <summary>
        /// Indicated whether this item has children.
        /// </summary>
        public abstract bool HasChildren
        {
            get;
        }

        /// <summary>
        /// The direct children of this item.
        /// </summary>
        public abstract List<HansoftItem> Children
        {
            get;
        }

        /// <summary>
        /// The direct and recursive children of this item.
        /// </summary>
        public abstract List<HansoftItem> DeepChildren
        {
            get;
        }

        /// <summary>
        /// The direct children of this itme that also are leaf items, i.e., they don't have any children in their turn.
        /// </summary>
        public List<HansoftItem> Leaves
        {
            get { return Children.FindAll(item => !item.HasChildren); }
        }

        /// <summary>
        /// The direct and recursive children of this item that also are leaf items, i.e., they don't have any children in their turn.
        /// </summary>
        public List<HansoftItem> DeepLeaves
        {
            get { return DeepChildren.FindAll(item => !item.HasChildren); }
        }

        /// <summary>
        /// The name of the item
        /// </summary>
        public abstract string Name
        {
            get;
            set;
        }
    }
}
