using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HPMSdk;

namespace Hansoft.ObjectWrapper.CustomColumnValues
{
    /// <summary>
    /// Encapsulates a tasks value for custom column of text type.
    /// </summary>
    public class TextValue : CustomColumnValue
    {

        internal static new TextValue FromInternalValue(Task task, HPMProjectCustomColumnsColumn customColumn, string internalValue)
        {
            return new TextValue(task, customColumn, internalValue);
        }

        internal static TextValue FromText(Task task, HPMProjectCustomColumnsColumn customColumn, string internalValue)
        {
            return new TextValue(task, customColumn, internalValue);
        }

        internal TextValue(Task task, HPMProjectCustomColumnsColumn customColumn, string internalValue)
            : base(task, customColumn, internalValue)
        {
        }
        
        /// <summary>
        /// The string value itself.
        /// </summary>
        /// <returns>The string</returns>
        public override string ToString()
        {
            return InternalValue;
        }

       /// <summary>
       /// Parses this TextValue and converts it to an Integer (long) value.
       /// </summary>
       /// <returns>The parsed Integer value or 0 if the TextValue was not possible to parse.</returns>
        public override long ToInt()
        {
            long iVal;
            if (!Int64.TryParse(InternalValue, out iVal))
                iVal = 0;
            return iVal;
        }

        /// <summary>
        /// Parses this TextValue and converts it to a float (double) value.
        /// </summary>
        /// <returns>The parsed double value or 0 if the TextValue was not possible to parse.</returns>
        public override double ToDouble()
        {
            double dVal;
            if (!Double.TryParse(InternalValue,System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"), out dVal))
                dVal = 0;
            return dVal;
        }

        /// <summary>
        /// Parses this TextValue and converts it to a DateTime value.
        /// </summary>
        /// <returns>The parsed DateTime or January 1 1970 if the TextValue was not possible to parse.</returns>
        public override DateTime ToDateTime(IFormatProvider provider)
        {
            DateTime dt;
            if (!DateTime.TryParse(InternalValue, out dt))
                dt = new DateTime(1970, 1, 1);
            return dt;
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
