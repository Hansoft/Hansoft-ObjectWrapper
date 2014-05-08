using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HPMSdk;
using System.Collections;

namespace Hansoft.ObjectWrapper.CustomColumnValues
{
    /// <summary>
    /// Encapsulates a tasks value for custom column of multiple selection type.
    /// </summary>
    public class MultipleSelectionValue : CustomColumnValue
    {
        private int[] selections;

        internal static new MultipleSelectionValue FromInternalValue(Task task, HPMProjectCustomColumnsColumn customColumn, string internalValue)
        {
            string[] internalValues = internalValue.Split(new char[] {';'});
            int[] selections = new int[internalValues.Length];
            for (int i = 0; i < internalValues.Length; i += 1)
            {
                if (!Int32.TryParse(internalValues[i], out selections[i]))
                    selections[i] = -1;
            }
            return new MultipleSelectionValue(task, customColumn, internalValue, selections);
        }

        internal static MultipleSelectionValue FromName(Task task, HPMProjectCustomColumnsColumn customColumn, string nameSequence)
        {

            string[] names = nameSequence.Split(new char[]{';'});
            int[] selections = new int[names.Length];
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < names.Length; i += 1)
            {
                selections[i] = HPMUtilities.EncodeDroplistValue(names[i], customColumn.m_DropListItems);
                if (sb.Length > 0)
                    sb.Append(';');
                sb.Append(selections[i].ToString());
            }
            string internalValue = sb.ToString();
            return new MultipleSelectionValue(task, customColumn, internalValue, selections);
        }

        public static MultipleSelectionValue FromStringList(Task task, HPMProjectCustomColumnsColumn customColumn, IList names)
        {

            int[] selections = new int[names.Count];
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < names.Count; i += 1)
            {
                selections[i] = HPMUtilities.EncodeDroplistValue((string)names[i], customColumn.m_DropListItems);
                if (sb.Length > 0)
                    sb.Append(';');
                sb.Append(selections[i].ToString());
            }
            string internalValue = sb.ToString();
            return new MultipleSelectionValue(task, customColumn, internalValue, selections);
        }

        private MultipleSelectionValue(Task task, HPMProjectCustomColumnsColumn customColumn, string internalValue, int[] selections)
            : base(task, customColumn, internalValue)
        {
            this.selections = selections;
        }
        
        /// <summary>
        /// The value as a string formatted as in the Hansoft client.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            return HPMUtilities.DecodeDroplistValues(selections, CustomColumn.m_DropListItems);
        }

        /// <summary>
        /// Returns all the resources in the format of a string list.
        /// </summary>
        /// <returns>all the resources in the format of a string list</returns>
        public override IList ToStringList()
        {
            return HPMUtilities.DecodeDroplistValuesToStringList(selections, CustomColumn.m_DropListItems);
        }

        /// <summary>
        /// Not Implemented.
        /// </summary>
        /// <returns>NotImplementedException</returns>
        public override long ToInt()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented.
        /// </summary>
        /// <returns>NotImplementedException</returns>
        public override double ToDouble()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented.
        /// </summary>
        /// <returns>NotImplementedException</returns>
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

        /// <summary>
        /// Implementation of IComparable
        /// </summary>
        /// <param name="obj">The other object to compare with.</param>
        /// <returns>The result of the comparison</returns>
        public override bool Equals(object obj)
        {
            return CompareTo(obj) == 0;
        }

    }
}
