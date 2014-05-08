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
    /// Encapsulation of a tasks value for a custom column in Hansoft.
    /// </summary>
    public abstract class CustomColumnValue : IConvertible, IComparable
    {
        Task task;
        HPMProjectCustomColumnsColumn customColumn;
        string internalValue;

        /// <summary>
        /// Factory method to create a CustomColumnValue from a list of user representations (string) of custom column values in Hansoft.
        /// Currently only implemented for MultipleSelection and Resources
        /// </summary>
        /// <param name="task">The task that the value belongs to.</param>
        /// <param name="customColumn">The custom column that the value belongs to.</param>
        /// <param name="internalValue">The list of strings</param>
        /// <returns>The CustomColumn value corresponding to the given parameters.</returns>
        public static CustomColumnValue FromStringList(Task task, HPMProjectCustomColumnsColumn customColumn, IList value)
        {
            switch (customColumn.m_Type)
            {
                case EHPMProjectCustomColumnsColumnType.MultiSelectionDropList:
                    return MultipleSelectionValue.FromStringList(task, customColumn, value);
                case EHPMProjectCustomColumnsColumnType.Resources:
                    {
                        Project project = Project.GetProject(task.MainProjectID);
                        List<Resource> resources = new List<Resource>();
                        foreach (string rs in value)
                        {
                            string trimmed = rs.Trim();
                            User user = project.Members.Find(u => u.Name == trimmed);
                            if (user != null)
                                resources.Add(user);
                            else
                            {
                                User groupMember = project.Members.Find(u => u.Groups.Find(g => g.Name == trimmed) != null);
                                if (groupMember != null)
                                {
                                    Group group = groupMember.Groups.Find(g => g.Name == trimmed);
                                    resources.Add(group);
                                }
                            }
                        }

                        return ResourcesValue.FromResourceList(task, customColumn, resources);
                    }
                default:
                    return null;
            }
        }


        /// <summary>
        /// Method that converts a custom column value to a string list.
        /// </summary>
        /// <returns>The CustomColumn value corresponding to the given parameters.</returns>
        public abstract IList ToStringList();

        /// <summary>
        /// Factory method to create a CustomColumnValue from the internal endocding of custom column values in Hansoft.
        /// </summary>
        /// <param name="task">The task that the value belongs to.</param>
        /// <param name="customColumn">The custom column that the value belongs to.</param>
        /// <param name="internalValue">The Hansoft internal value.</param>
        /// <returns>The CustomColumn value corresponding to the given parameters.</returns>
        public static CustomColumnValue FromInternalValue(Task task, HPMProjectCustomColumnsColumn customColumn, string internalValue)
        {
            switch (customColumn.m_Type)
            {
                case EHPMProjectCustomColumnsColumnType.AccumulatedTime:
                    return AccumulatedTimeValue.FromInternalValue(task, customColumn, internalValue);
                case EHPMProjectCustomColumnsColumnType.FloatNumber:
                    return FloatNumberValue.FromInternalValue(task, customColumn, internalValue);
                case EHPMProjectCustomColumnsColumnType.IntegerNumber:
                    return IntegerNumberValue.FromInternalValue(task, customColumn, internalValue);
                case EHPMProjectCustomColumnsColumnType.Hyperlink:
                    return HyperlinkValue.FromInternalValue(task, customColumn, internalValue);
                case EHPMProjectCustomColumnsColumnType.Text:
                    return TextValue.FromInternalValue(task, customColumn, internalValue);
                case EHPMProjectCustomColumnsColumnType.MultiLineText:
                    return MultilineTextValue.FromInternalValue(task, customColumn, internalValue);
                case EHPMProjectCustomColumnsColumnType.DateTime:
                    return DateValue.FromInternalValue(task, customColumn, internalValue);
                case EHPMProjectCustomColumnsColumnType.DateTimeWithTime:
                    return DateTimeValue.FromInternalValue(task, customColumn, internalValue);
                case EHPMProjectCustomColumnsColumnType.DropList:
                    return SingleSelectionValue.FromInternalValue(task, customColumn, internalValue);
                case EHPMProjectCustomColumnsColumnType.MultiSelectionDropList:
                    return MultipleSelectionValue.FromInternalValue(task, customColumn, internalValue);
                case EHPMProjectCustomColumnsColumnType.Resources:
                    return ResourcesValue.FromInternalValue(task, customColumn, internalValue);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Factory method to create a CustomColumnValue from the end user representation (string) of custom column values in Hansoft.
        /// </summary>
        /// <param name="task">The task that the value belongs to.</param>
        /// <param name="customColumn">The custom column that the value belongs to.</param>
        /// <param name="endUserString">The string value of the custom column as it is displayed in the Hansoft client.</param>
        /// <returns>The CustomColumn value corresponding to the given parameters.</returns>
        /// <returns></returns>
        public static CustomColumnValue FromEndUserString(Task task, HPMProjectCustomColumnsColumn customColumn, string endUserString)
        {
            switch (customColumn.m_Type)
            {
                case EHPMProjectCustomColumnsColumnType.AccumulatedTime:
                    return AccumulatedTimeValue.FromInternalValue(task, customColumn, endUserString);                    
                case EHPMProjectCustomColumnsColumnType.FloatNumber:
                    return FloatNumberValue.FromInternalValue(task, customColumn, endUserString);                    
                case EHPMProjectCustomColumnsColumnType.IntegerNumber:
                    return IntegerNumberValue.FromInternalValue(task, customColumn, endUserString);                    
                case EHPMProjectCustomColumnsColumnType.Hyperlink:
                    return HyperlinkValue.FromInternalValue(task, customColumn, endUserString);                    
                case EHPMProjectCustomColumnsColumnType.Text:
                    return TextValue.FromInternalValue(task, customColumn, endUserString);                    
                case EHPMProjectCustomColumnsColumnType.MultiLineText:
                    return MultilineTextValue.FromEscapedString(task, customColumn, endUserString);                    
                case EHPMProjectCustomColumnsColumnType.DateTime:
                    DateTime dt;
                    if (!DateTime.TryParse(endUserString, out dt))
                        return DateValue.FromInternalValue(task, customColumn, endUserString);
                    else
                        return DateValue.FromDateTime(task, customColumn, dt.ToUniversalTime());                    
                case EHPMProjectCustomColumnsColumnType.DateTimeWithTime:
                    if (!DateTime.TryParse(endUserString, out dt))
                        return DateValue.FromInternalValue(task, customColumn, endUserString);
                    else
                        return DateTimeValue.FromDateTime(task, customColumn, dt.ToUniversalTime());                    
                case EHPMProjectCustomColumnsColumnType.DropList:
                    return SingleSelectionValue.FromName(task, customColumn, endUserString);
                case EHPMProjectCustomColumnsColumnType.MultiSelectionDropList:
                    return MultipleSelectionValue.FromName(task, customColumn, endUserString);
                case EHPMProjectCustomColumnsColumnType.Resources:
                    Project project = Project.GetProject(task.MainProjectID);
                    List<Resource> resources = new List<Resource>();
                    string[] rStrings = endUserString.Split(new char[]{';'});
                    foreach (string rs in rStrings)
                    {
                        string trimmed = rs.Trim();
                        User user = project.Members.Find(u => u.Name == trimmed);
                        if (user != null)
                            resources.Add(user);
                        else
                        {
                            User groupMember = project.Members.Find(u => u.Groups.Find(g => g.Name == trimmed) != null);
                            if (groupMember != null)
                            {
                                Group group = groupMember.Groups.Find(g => g.Name == trimmed);
                                resources.Add(group);
                            }
                        }
                    }
                    return ResourcesValue.FromResourceList(task, customColumn, resources);
                default:
                    return null;
            }
        }
        
        internal CustomColumnValue(Task task, HPMProjectCustomColumnsColumn customColumn, string internalValue)
        {
            this.task = task;
            this.customColumn = customColumn;
            this.internalValue = internalValue;
        }

        /// <summary>
        /// The Hansoft internal representation of this value.
        /// </summary>
        public string InternalValue
        {
            get
            {
                return internalValue;
            }
        }

        /// <summary>
        /// The custom column that this value applies to.
        /// </summary>
        public HPMProjectCustomColumnsColumn CustomColumn
        {
            get
            {
                return customColumn;
            }
        }

        /// <summary>
        /// Convert (if needed) the underlying value to an integer value.
        /// </summary>
        /// <returns>The integer value.</returns>
        public abstract long ToInt();

        /// <summary>
        /// Convert (if needed) the underlying value to a double value.
        /// </summary>
        /// <returns>The double value</returns>
        public abstract double ToDouble();

        /// <summary>
        /// IConvertible override, returns TypeCode.Object.
        /// </summary>
        /// <returns></returns>
        public TypeCode GetTypeCode()
        {
            return TypeCode.Object;
        }

        /// <summary>
        /// IConvertible override, will throw NotImplementedException if called.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public bool ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// IConvertible override, will throw NotImplementedException if called.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public char ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// IConvertible override, delegated to subclasses.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public abstract DateTime ToDateTime(IFormatProvider provider);

        /// <summary>
        /// ICovertible override
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public byte ToByte(IFormatProvider provider)
        {
            return (byte)ToInt();
        }

        /// <summary>
        /// ICovertible override
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public decimal ToDecimal(IFormatProvider provider)
        {
            return (decimal)ToDouble();
        }

        /// <summary>
        /// ICovertible override
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public double ToDouble(IFormatProvider provider)
        {
            return ToDouble();
        }

        /// <summary>
        /// ICovertible override
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public short ToInt16(IFormatProvider provider)
        {
            return (short)ToInt();
        }

        /// <summary>
        /// ICovertible override
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public int ToInt32(IFormatProvider provider)
        {
            return (int)ToInt();
        }

        /// <summary>
        /// ICovertible override
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public long ToInt64(IFormatProvider provider)
        {
            return ToInt();
        }

        /// <summary>
        /// ICovertible override
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public sbyte ToSByte(IFormatProvider provider)
        {
            return (sbyte)ToInt();
        }

        /// <summary>
        /// ICovertible override
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public float ToSingle(IFormatProvider provider)
        {
            return (float)ToDouble();
        }

        /// <summary>
        /// ICovertible override
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public string ToString(IFormatProvider provider)
        {
            return ToString();
        }

        /// <summary>
        /// IConvertible override, will throw NotImplementedException if called.
        /// </summary>
        /// <param name="conversionType"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public object ToType(Type conversionType, IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// ICovertible override
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public ushort ToUInt16(IFormatProvider provider)
        {
            return (ushort)ToInt();
        }

        /// <summary>
        /// ICovertible override
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public uint ToUInt32(IFormatProvider provider)
        {
            return (uint)ToInt();
        }

        /// <summary>
        /// ICovertible override
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public ulong ToUInt64(IFormatProvider provider)
        {
            return (ulong)ToInt();
        }

        /// <summary>
        /// Implementation of IComparable
        /// </summary>
        /// <param name="obj">The other object to compare with.</param>
        /// <returns>The result of the comparison</returns>
        abstract public int CompareTo(object obj);

        /// <summary>
        /// Implementation of IComparable
        /// </summary>
        /// <param name="obj">The other object to compare with.</param>
        /// <returns>The result of the comparison</returns>
        public override abstract bool Equals(object obj);

    }
}
