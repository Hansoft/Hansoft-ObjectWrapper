using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HPMSdk;

namespace Hansoft.ObjectWrapper.CustomColumnValues
{
    /// <summary>
    /// Encapsulates a tasks value for custom column of float type.
    /// </summary>
    class FloatNumberValue : CustomColumnValue
    {
        private double floatValue;

        internal static new FloatNumberValue FromInternalValue(Task task, HPMProjectCustomColumnsColumn customColumn, string internalValue)
        {
            double floatValue;
            if (!Double.TryParse(internalValue, System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"), out floatValue))
                floatValue = 0;
            return new FloatNumberValue(task, customColumn, internalValue, floatValue);
        }

        internal static FloatNumberValue FromFloat(Task task, HPMProjectCustomColumnsColumn customColumn, double floatValue)
        {
            string internalValue;
            internalValue = String.Format(new System.Globalization.CultureInfo("en-US"), "{0:F1}", floatValue);
            return new FloatNumberValue(task, customColumn, internalValue, floatValue);
        }

        internal FloatNumberValue(Task task, HPMProjectCustomColumnsColumn customColumn, string internalValue, double floatValue)
            : base(task, customColumn, internalValue)
        {
            this.floatValue = floatValue;
        }
        
        /// <summary>
        /// The float number as a string.
        /// </summary>
        /// <returns>The string</returns>
        public override string ToString()
        {
            return InternalValue;
        }

        internal override long ToInt()
        {
            return (long)floatValue;
        }

        internal override double ToDouble()
        {
            return floatValue;
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
            if (obj is FloatNumberValue)
                return floatValue.CompareTo(((FloatNumberValue)obj).floatValue);
            else if (obj is int)
                return floatValue.CompareTo((double)obj);
            else
            {
                double otherFloat;
                if (double.TryParse(obj.ToString(), out otherFloat))
                    return floatValue.CompareTo(otherFloat);
                else
                    return InternalValue.CompareTo(obj.ToString());
            }
        }
    }
}
