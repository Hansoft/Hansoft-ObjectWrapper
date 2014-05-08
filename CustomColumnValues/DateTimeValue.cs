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
    /// Encapsulates a tasks value for custom column of date and time type.
    /// </summary>
    public class DateTimeValue : CustomColumnValue
    {
        private ulong hpmDateTime;

        internal static new DateTimeValue FromInternalValue(Task task, HPMProjectCustomColumnsColumn customColumn, string internalValue)
        {
            ulong hpmDateTime;
            if (internalValue != "")
                hpmDateTime = SessionManager.Session.UtilDecodeCustomColumnDateTimeValue(internalValue);
            else
                hpmDateTime = 0;
            return new DateTimeValue(task, customColumn, internalValue, hpmDateTime);
        }

        /// <summary>
        /// Create a new instance from an internal value belonging to a certain task and custom column.
        /// </summary>
        /// <param name="task">The task the value belongs to.</param>
        /// <param name="customColumn">The custom column the value belongs to.</param>
        /// <param name="hpmDateTime">The internal Hansoft time value.</param>
        /// <returns>The new instance.</returns>
        public static DateTimeValue FromHpmDateTime(Task task, HPMProjectCustomColumnsColumn customColumn, ulong hpmDateTime)
        {
            string internalValue;
            internalValue = SessionManager.Session.UtilEncodeCustomColumnDateTimeValue(hpmDateTime);
            return new DateTimeValue(task, customColumn, internalValue, hpmDateTime);
        }

        /// <summary>
        /// Crate a new instance from a DateTime value for a certain task and custom column.
        /// </summary>
        /// <param name="task">The task the value belongs to.</param>
        /// <param name="customColumn">The custom column the value belongs to.</param>
        /// <param name="dateTime">The DateTime value</param>
        /// <returns>The new instance.</returns>
        public static DateTimeValue FromDateTime(Task task, HPMProjectCustomColumnsColumn customColumn, DateTime dateTime)
        {
            return FromHpmDateTime(task, customColumn, HPMUtilities.HPMDateTime(dateTime, false));
        }

        private DateTimeValue(Task task, HPMProjectCustomColumnsColumn customColumn, string internalValue, ulong hpmDateTime)
            : base(task, customColumn, internalValue)
        {
            this.hpmDateTime = hpmDateTime;
        }

        /// <summary>
        /// The date and time value as a string.
        /// </summary>
        /// <returns>The string</returns>
        public override string ToString()
        {
            if (InternalValue != string.Empty)
            {
                return HPMUtilities.HPMDateTimeToDateTimeString(hpmDateTime);
            }
            else
                return string.Empty;
        }

        /// <summary>
        /// Method that converts a custom column value to a string list. (Not implemented)
        /// </summary>
        /// <returns>The CustomColumn value corresponding to the given parameters.</returns>
        public override IList ToStringList()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The value as a Hansoft time value, i.e., microseconds since Jan 1 1970 UTC.
        /// </summary>
        /// <returns>The value</returns>
        public ulong ToHpmDateTime()
        {
            return hpmDateTime;
        }

        /// <summary>
        /// The value as a Hansoft time value, i.e., microseconds since Jan 1 1970 UTC.
        /// </summary>
        /// <returns>The value</returns>
        public override long ToInt()
        {
            return (long)hpmDateTime;
        }

        /// <summary>
        /// The value as a Hansoft time value, i.e., microseconds since Jan 1 1970 UTC.
        /// </summary>
        /// <returns>The value</returns>
        public override double ToDouble()
        {
            return hpmDateTime;
        }

        /// <summary>
        /// The value as a DateTime value, with the provided IFormatProvider
        /// </summary>
        /// <param name="provider">An IFormatProvider</param>
        /// <returns>The DateTime value.</returns>
        public override DateTime ToDateTime(IFormatProvider provider)
        {
            return ToDateTime();
        }

        /// <summary>
        /// Returns the value as a DateTime value.
        /// </summary>
        /// <returns>The DateTime value.</returns>
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
            if (obj is DateTimeValue)
                return ToDateTime().CompareTo(((DateValue)obj).ToDateTime());
            else if (obj is DateValue)
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
