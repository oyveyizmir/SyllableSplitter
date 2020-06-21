using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SyllableSplitter.Extensions
{
    static class IEnumerableExtensions
    {
        public static bool Parse<T>(this IEnumerable<T> enumerable, string stringToParse, ref int index, Func<T, string> itemToString, Action<T> itemFound)
        {
            foreach (T item in enumerable)
            {
                string substring = itemToString(item);
                if ((index + substring.Length <= stringToParse.Length) && stringToParse.Substring(index, substring.Length) == substring)
                {
                    itemFound(item);
                    index += substring.Length;
                    return true;
                }
            }
            return false;
        }

        public static bool Parse(this IEnumerable<string> enumerable, string stringToParse, ref int index, Action<string> itemFound) =>
            Parse(enumerable, stringToParse, ref index, x => x, itemFound);
    }
}
