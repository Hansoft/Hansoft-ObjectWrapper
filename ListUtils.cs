using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hansoft.ObjectWrapper
{
    internal class ListUtils
    {
        internal static string ToString(List<HansoftItem> items)
        {
            return ListUtils.ToString(items, ',');
        }

        public static string ToString(List<HansoftItem> items, char separator)
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
