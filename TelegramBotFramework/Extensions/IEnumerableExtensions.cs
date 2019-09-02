using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TelegramBotFramework.Extensions {

    /// <summary>
    /// Extensions methods for IEnumerable 
    /// </summary>
    public static class IEnumerableExtensions {

        /// <summary>
        /// <para>Splice the IEnumerable based on an offset</para>
        /// <para>offset: 1 = start = 1, end = Enumerable end</para>
        /// <para>offset: -1 = start = 0, end = Enumerable end - 1</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="offset"></param>
        public static IEnumerable<T> Splice<T>(this IEnumerable<T> enumerable, int offset) {
            int start = 0;
            int end = enumerable.Count();
            if (offset >= 0) {
                start = offset;
            } else {
                end += offset;
            }
            return Splice(enumerable, start, end);
        }

        /// <summary>
        /// Splice the IEnumerable
        /// </summary>
        /// <typeparam name="T">Type of the IEnumerable<T></typeparam>
        /// <param name="enumerable">Enumerable on which to apply the splice</param>
        /// <param name="start">Inclusive start index</param>
        /// <param name="end">Inclusive end index</param>
        public static IEnumerable<T> Splice<T>(this IEnumerable<T> enumerable, int start, int end) {
            List<T> result = new List<T>();
            int index = 0;
            foreach (T val in enumerable) {
                if (index >= start) {
                    if (index == end) {
                        break;
                    } else {
                        result.Add(val);
                    }
                }
                index++;
            }
            return result;
        }

        /// <summary>
        /// Join the values of this IEnumerable into a string
        /// </summary>
        /// <param name="enumerable">String enumerable to join</param>
        /// <param name="separator">Separator</param>
        public static string Join(this IEnumerable<string> enumerable, char separator) {
            return string.Join(separator, enumerable);
        }

        /// <summary>
        /// Join the values of this IEnumerable into a string
        /// </summary>
        /// <param name="enumerable">String enumerable to join</param>
        /// <param name="separator">Separator</param>
        public static string Join(this IEnumerable<string> enumerable, string separator) {
            return string.Join(separator, enumerable);
        }

        /// <summary>
        /// Join the values of this IEnumerable into a single string
        /// </summary>
        /// <param name="enumerable">String enumerable to join</param>
        public static string Join(this IEnumerable<string> enumerable) {
            StringBuilder builder = new StringBuilder();
            foreach (string val in enumerable) {
                builder.Append(val);
            }
            return builder.ToString();
        }
    }
}
