using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HPMSdk;

namespace Hansoft.ObjectWrapper.CustomColumnValues
{
    /// <summary>
    /// Encapsulates a tasks value for custom column of date type.
    /// </summary>
    class DateValue : CustomColumnValue
    {
        private ulong hpmDateTime;

        internal static new DateValue FromInternalValue(Task task, HPMProjectCustomColumnsColumn customColumn, string internalValue)
        {
            ulong hpmDateTime = SessionManager.Session.UtilDecodeCustomColumnDateTimeValue(internalValue);
            return new DateValue(task, customColumn, internalValue, hpmDateTime);
        }

        internal static DateValue FromHpmDateTime(Task task, HPMProjectCustomColumnsColumn customColumn, ulong hpmDateTime)
        {
            string internalValue;
            internalValue = SessionManager.Session.UtilEncodeCustomColumnDateTimeValue(hpmDateTime);
            return new DateValue(task, customColumn, internalValue, hpmDateTime);
        }

        internal static DateValue FromDateTime(Task task, HPMProjectCustomColumnsColumn customColumn, DateTime dateTime)
        {
            return FromHpmDateTime(task, customColumn, HPMUtilities.HPMDateTime(dateTime, true));
        }

        private DateValue(Task task, HPMProjectCustomColumnsColumn customColumn, string internalValue, ulong hpmDateTime)
            : base(task, customColumn, internalValue)
        {
            this.hpmDateTime = hpmDateTime;
        }

        /// <summary>
        /// The date value as a string.
        /// </summary>
        /// <returns>The string</returns>
        public override string ToString()
        {
            if (InternalValue != string.Empty)
            {
                return HPMUtilities.HPMDateTimeToDateString(hpmDateTime);
            }
            else
                return string.Empty;

        }

        internal override long ToInt()
        {
            return (long)hpmDateTime;
        }

        internal override double ToDouble()
        {
            return hpmDateTime;
        }

        public override DateTime ToDateTime(IFormatProvider provider)
        {
            return ToDateTime();
        }

        public DateTime ToDateTime()
        {
            return HPMUtilities.FromHPMDateTime(hpmDateTime);
        }

        /// <summary>
        /// Implementation of IComparable
        /// </summary>
        /// <param name="obj">The other object to compare with.</param>
        /// <returns>The result of the comparison</returns>
        public override int CompareTo(object obj)
        {
            if (obj is DateValue)
                return ToDateTime().CompareTo(((DateValue)obj).ToDateTime());
            else if (obj is DateTimeValue)
                return ToDateTime().CompareTo(((DateTimeValue)obj).ToDateTime());
            else if (obj is DateTime)
                return ToDateTime().CompareTo((DateTime)obj);
            else 
            {

                DateTime otherDate;
                if (DateTime.TryParse(obj.ToString(), out otherDate))
                    return ToDateTime().CompareTo(otherDate);
                else
                    return InternalValue.CompareTo(obj.ToString());
            }
        }
    }
}
