using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace UWPSettingsEditor
{
    public static class MethodHelpers
    {
        public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> source, int count)
        {
            var e = source.GetEnumerator();
            var cache = new Queue<T>(count + 1);

            using (e = source.GetEnumerator())
            {
                bool hasRemainingItems;
                do
                {
                    if (hasRemainingItems = e.MoveNext())
                    {
                        cache.Enqueue(e.Current);
                        if (cache.Count > count)
                            yield return cache.Dequeue();
                    }
                } while (hasRemainingItems);
            }
        }

        public static T VisualUpwardSearch<T>(this DependencyObject source) where T : class
        {
            while (source != null && !(source is T))
                source = VisualTreeHelper.GetParent(source);

            return source as T;
        }

        //ToDo: find a better name for this pls
        /// <summary>
        /// Method for splitting the data into the actual data + timestamp 
        /// </summary>
        /// <param name="data">Complete data raw</param>
        /// <returns>KeyValuePair of data and timestamp</returns>
        public static KeyValuePair<byte[], byte[]> SplitDataRaw(byte[] dataRaw)
        {
            byte[] data = dataRaw.SkipLast(8).ToArray();
            byte[] timestamp = dataRaw.Skip(data.Length).ToArray();

            return new KeyValuePair<byte[], byte[]>(data, timestamp);
        }

        public static string ReplaceMultilineWithSymbols(string str)
        {
            var repStr = str.Replace("\r\n", "\\r\\n").Replace("\r", "\\r").Replace("\n", "\\n");

            if (repStr.Length > 256)
                repStr = repStr.Substring(0, 256) + "...";

            return repStr;
        }
    }
}
