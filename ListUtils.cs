using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hansoft.ObjectWrapper
{
    /// <summary>
    /// Utilities for formatting lists of Hansoft Items for display.
    /// </summary>
    public class ListUtils
    {
        
        /// <summary>
        /// Create a comma separated list (string) with the names of a list of HansoftItems
        /// </summary>
        /// <param name="items">The items to format.</param>
        /// <returns>A formatted string suitable for display.</returns>
        public static string ToString(IEnumerable<HansoftItem> items)
        {
            return ListUtils.ToString(items, ',');
        }

        /// <summary>
        /// Create a list (string) with the names of a list of HansoftItems. The items will be separated by the specified character.
        /// </summary>
        /// <param name="items">The items to format.</param>
        /// <param name="separator">The separator ro use</param>
        /// <returns>A formatted string suitable for display.</returns>
        public static string ToString(IEnumerable<HansoftItem> items, char separator)
        {
            StringBuilder sb = new StringBuilder();
            foreach (HansoftItem item in items)
            {
                if (sb.Length > 0)
                {
                    sb.Append(separator);
                    sb.Append(' ');
                }
                sb.Append(item.Name);
            }
            return sb.ToString();
        }
    }
}
