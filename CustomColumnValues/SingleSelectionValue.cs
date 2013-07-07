using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HPMSdk;

namespace Hansoft.ObjectWrapper.CustomColumnValues
{
    /// <summary>
    /// Encapsulates a tasks value for custom column of single selection type.
    /// </summary>
    public class SingleSelectionValue : CustomColumnValue
    {
        private int selection;

        internal static new SingleSelectionValue FromInternalValue(Task task, HPMProjectCustomColumnsColumn customColumn, string internalValue)
        {
            int selection;
            if (!Int32.TryParse(internalValue, out selection))
                selection = -1;
            return new SingleSelectionValue(task, customColumn, internalValue, selection);
        }

        internal static SingleSelectionValue FromName(Task task, HPMProjectCustomColumnsColumn customColumn, string name)
        {
            int selection = HPMUtilities.EncodeDroplistValue(name, customColumn.m_DropListItems);
            
            string internalValue = selection.ToString();
            return new SingleSelectionValue(task, customColumn, internalValue, selection);
        }

        private SingleSelectionValue(Task task, HPMProjectCustomColumnsColumn customColumn, string internalValue, int selection)
            : base(task, customColumn, internalValue)
        {
            this.selection = selection;
        }

        /// <summary>
        /// The value as a string formatted as in the Hansoft client.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            return HPMUtilities.DecodeDroplistValue((int)selection, CustomColumn.m_DropListItems);
        }

        public override long ToInt()
        {
            return selection;
        }

        public override double ToDouble()
        {
            return selection;
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
