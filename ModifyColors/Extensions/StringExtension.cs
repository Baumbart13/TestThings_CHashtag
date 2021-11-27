using System.Collections.Generic;
using System.Text;

namespace ModifyColors.Extensions
{
    public static class StringExtension
    {
        public static string RemoveElement(this string str, char toRemove)
        {
            var sb = new StringBuilder(str.Length);

            foreach (var c in str)
            {
                if (c == toRemove)
                {
                    continue;
                }

                sb.Append(c);
            }

            return sb.ToString();
        }
    }
}