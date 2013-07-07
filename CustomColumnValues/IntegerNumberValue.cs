using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HPMSdk;

namespace Hansoft.ObjectWrapper.CustomColumnValues
{
    /// <summary>
    /// Encapsulates a tasks value for custom column of integer type.
    /// </summary>
    public class IntegerNumberValue : CustomColumnValue
    {
        private long integerValue;

        internal static new IntegerNumberValue FromInternalValue(Task task, HPMProjectCustomColumnsColumn customColumn, string internalValue)
        {
            long integerValue;
            if (!Int64.TryParse(internalValue, out integerValue))
                integerValue = 0;
            return new IntegerNumberValue(task, customColumn, internalValue, integerValue);
        }

        internal static IntegerNumberValue FromInteger(Task task, HPMProjectCustomColumnsColumn customColumn, long integerValue)
        {
            string internalValue;
            internalValue = integerValue.ToString();
            return new IntegerNumberValue(task, customColumn, internalValue, integerValue);
        }

        private IntegerNumberValue(Task task, HPMProjectCustomColumnsColumn customColumn, string internalValue, long integerValue)
            : base(task, customColumn, internalValue)
        {
            this.integerValue = integerValue;
        }

        /// <summary>
        /// The integer number as a string.
        /// </summary>
        /// <returns>The string</returns>
        public override string ToString()
        {
            return InternalValue;
        }

        public override long ToInt()
        {
            return integerValue;
        }

        public override double ToDouble()
        {
            throw new NotImplementedException();
        }

        public override DateTime ToDateTime(IFormatProvider provider)
        {
            return HPMUtilities.FromHPMDateTime((ulong)integerValue);
        }

        /// <summary>
        /// Implementation of IComparable
        /// </summary>
        /// <param name="obj">The other object to compare with.</param>
        /// <returns>The result of the comparison</returns>
        public override int CompareTo(object obj)
        {
            if (obj is IntegerNumberValue)
                return integerValue.CompareTo(((IntegerNumberValue)obj).integerValue);
            else if (obj is int)
                return integerValue.CompareTo((int)obj);
            else
            {
                long otherInt;
                if (long.TryParse(obj.ToString(), out otherInt))
                    return integerValue.CompareTo(otherInt);
                else
                    return InternalValue.CompareTo(obj.ToString());
            }
        }
    }
}
