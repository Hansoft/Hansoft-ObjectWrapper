using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HPMSdk;

namespace Hansoft.ObjectWrapper
{
    /// <summary>
    /// An encapsulation of values for builtin Hansoft columns of enum type. For example Risk, Status and Priority.
    /// </summary>
    public class HansoftEnumValue : IComparable, IConvertible
    {
        object value;
        int iValue;
        HPMUniqueID projectId;
        EHPMProjectDefaultColumn column;

        static Dictionary<EHPMProjectDefaultColumn, uint> maxColumnValuebyColumn = new Dictionary<EHPMProjectDefaultColumn, uint>() {       {EHPMProjectDefaultColumn.Risk,              4}, 
                                                                                                                                            {EHPMProjectDefaultColumn.BacklogPriority,   6}, 
                                                                                                                                            {EHPMProjectDefaultColumn.BacklogCategory,   8}, 
                                                                                                                                            {EHPMProjectDefaultColumn.ItemStatus,        6}, 
                                                                                                                                            {EHPMProjectDefaultColumn.Confidence,        4} };
  
        static HansoftEnumValue FromInt(HPMUniqueID projectId, EHPMProjectDefaultColumn column, int iValue)
        {
            return new HansoftEnumValue(projectId, column, iValue, iValue);
        }

        public static HansoftEnumValue FromString(HPMUniqueID projectId, EHPMProjectDefaultColumn column, string sValue)
        {
            if (!maxColumnValuebyColumn.ContainsKey(column))
                throw new ArgumentException("Unsupported default column in HansoftEnumValue.FromString/3: " + column);

            for (uint i = 0; i <= maxColumnValuebyColumn[column]; i += 1)
            {
                string translated = GetTranslatedString(projectId, column, i);
                if (translated == sValue)
                    return FromInt(projectId, column, (int)i);
            }
            return FromInt(projectId, column, 0);
        }

        internal static HansoftEnumValue FromObject(HPMUniqueID projectId, EHPMProjectDefaultColumn column, object value)
        {
            if (value is HansoftEnumValue)
                return FromInt(projectId, column, ((HansoftEnumValue)value).iValue);
            else
            {
                string sValue = value.ToString();
                int iValue;
                if (Int32.TryParse(sValue, out iValue))
                    return FromInt(projectId, column, iValue);
                else
                    return FromString(projectId, column, sValue);
            }
        }

        internal HansoftEnumValue(HPMUniqueID projectId, EHPMProjectDefaultColumn column, object value, int iValue)
        {
            this.projectId = projectId;
            this.column = column;
            this.value = value;
            this.iValue = iValue;
        }

        /// <summary>
        /// The value used to represent this enum in the Hansoft Sdk, i.e., it will be of a type that is appropriate for the particular column
        /// that the value applies for. For example if the value is for the column EHPMProjectDefaultColumn.Risk, then the value will be of type.
        /// EHPMTaskRisk.
        /// </summary>
        public object Value
        {
            get { return value; }
        }

        /// <summary>
        /// The column type that this value applies to.
        /// </summary>
        public EHPMProjectDefaultColumn Column
        {
            get { return column; }
        }

        /// <summary>
        /// Returns the localized string suitable for end user display corresponding to this enum value.
        /// 
        /// NOTE: There is currently no way to select the localization to be used and the UK English localized string will always be returned.
        /// </summary>
        public String Text
        {
            get { return ToString(); }
        }

        /// <summary>
        /// Will return the integer value corresponding to this enum value.
        /// </summary>
        /// <returns></returns>
        public int ToInt()
        {
            return iValue;
        }

        /// <summary>
        /// Returns the localized string suitable for end user display corresponding to this enum value.
        /// 
        /// NOTE: There is currently no way to select the localization to be used and the UK English localized string will always be returned.
        /// </summary>
        /// <returns>The localized string</returns>
        public override string ToString()
        {
            return GetTranslatedString(projectId, column, (uint)ToInt());
        }

        private static string GetTranslatedString(HPMUniqueID projectId, EHPMProjectDefaultColumn column, uint iValue)
        {
            HPMUntranslatedString unTranslated = SessionManager.Session.UtilGetColumnDataItemFormatted(projectId, column, iValue);
            HPMLanguage language = SessionManager.Session.LocalizationGetDefaultLanguage();
            String translated = SessionManager.Session.LocalizationTranslateString(language, unTranslated);
            return translated;
        }

        /// <summary>
        /// Override of object.Equals
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is HansoftEnumValue)
                return Value.Equals(((HansoftEnumValue)obj).Value);
            else if (obj.GetType() == Value.GetType())
                return Value.Equals(obj);
            else if (obj is string)
                return ToString().Equals(obj);
            else
                return base.Equals(obj);
        }

        /// <summary>
        /// Override of object.GetHashcode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <summary>
        /// Implementation of IComparable.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public int CompareTo(object o)
        {
            if (o is HansoftEnumValue)
            {
                HansoftEnumValue other = (HansoftEnumValue)o;
                return other.ToInt() - ToInt();
            }
            else throw new ArgumentException();
        }

        /// <summary>
        /// IConvertible override, will return TypeCode.Object.
        /// </summary>
        /// <returns></returns>
        public TypeCode GetTypeCode()
        {
            return TypeCode.Object;
        }

        /// <summary>
        /// IConvertible override, NotImplementedException will be thrown if called.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public bool ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// IConvertible override, will return the integer value corresponding to this enum value.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public byte ToByte(IFormatProvider provider)
        {
            return (byte)ToInt();
        }

        /// <summary>
        /// IConvertible override, NotImplementedException will be thrown if called.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public char ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// IConvertible override, NotImplementedException will be thrown if called.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// IConvertible override, will return the integer value corresponding to this enum value.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public decimal ToDecimal(IFormatProvider provider)
        {
            return ToInt();
        }

        /// <summary>
        /// IConvertible override, will return the integer value corresponding to this enum value.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public double ToDouble(IFormatProvider provider)
        {
            return ToInt();
        }

        /// <summary>
        /// IConvertible override, will return the integer value corresponding to this enum value.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public short ToInt16(IFormatProvider provider)
        {
            return (short)ToInt();
        }

        /// <summary>
        /// IConvertible override, will return the integer value corresponding to this enum value.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public int ToInt32(IFormatProvider provider)
        {
            return ToInt();
        }

        /// <summary>
        /// IConvertible override, will return the integer value corresponding to this enum value.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public long ToInt64(IFormatProvider provider)
        {
            return ToInt();
        }

        /// <summary>
        /// IConvertible override, will return the integer value corresponding to this enum value.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public sbyte ToSByte(IFormatProvider provider)
        {
            return (sbyte)ToInt();
        }

        /// <summary>
        /// IConvertible override, will return the integer value corresponding to this enum value.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public float ToSingle(IFormatProvider provider)
        {
            return ToInt();
        }

        /// <summary>
        /// IConvertible override, returns the localized string suitable for end user display corresponding to this enum value.
        /// 
        /// NOTE: There is currently no way to select the localization to be used and the UK English localized string will always be returned.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns>The localized string</returns>
        public string ToString(IFormatProvider provider)
        {
            return ToString();
        }

        /// <summary>
        /// IConvertible override, NotImplementedException will be thrown if called.
        /// </summary>
        /// <param name="conversionType"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public object ToType(Type conversionType, IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// IConvertible override, will return the integer value corresponding to this enum value.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public ushort ToUInt16(IFormatProvider provider)
        {
            return (ushort)ToInt();
        }

        /// <summary>
        /// IConvertible override, will return the integer value corresponding to this enum value.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public uint ToUInt32(IFormatProvider provider)
        {
            return (uint)ToInt();
        }

        /// <summary>
        /// IConvertible override, will return the integer value corresponding to this enum value.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public ulong ToUInt64(IFormatProvider provider)
        {
            return (ulong)ToInt();
        }
    }
}
