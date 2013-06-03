using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HPMSdk;

namespace Hansoft.ObjectWrapper.CustomColumnValues
{
    /// <summary>
    /// Encapsulates a tasks custom column value for a column of accumulated time type.
    /// </summary>
    class AccumulatedTimeValue : FloatNumberValue
    {
        internal AccumulatedTimeValue(Task task, HPMProjectCustomColumnsColumn customColumn, string internalValue, float floatValue)
            : base(task, customColumn, internalValue, floatValue)
        {
        }
    }
}
