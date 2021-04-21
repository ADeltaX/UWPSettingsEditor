using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace UWPSettingsEditor
{ 
    public static class MethodHelpers
    {
        private const int SUBSTRING_ELLIPSIS_LENGTH = 128;

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
        /// Method for splitting the raw data into the actual timestamp + data
        /// </summary>
        /// <param name="data">Complete data raw</param>
        /// <returns>KeyValuePair timestamp and data</returns>
        public static KeyValuePair<byte[], byte[]> SplitDataRaw(byte[] dataRaw)
        {
            byte[] data = dataRaw.SkipLast(8).ToArray();
            byte[] timestamp = dataRaw.Skip(data.Length).ToArray();

            return new KeyValuePair<byte[], byte[]>(timestamp, data);
        }

        public static string ReplaceMultilineWithSymbols(this string str)
        {
            var repStr = str.Replace("\r\n", "\\r\\n").Replace("\r", "\\r").Replace("\n", "\\n");

            if (repStr.Length + 3 > SUBSTRING_ELLIPSIS_LENGTH)
                repStr = repStr.Substring(0, SUBSTRING_ELLIPSIS_LENGTH - 3) + "...";

            return repStr;
        }

        //From here: https://stackoverflow.com/a/33307903
        public static unsafe bool EqualBytesLongUnrolled(byte[] data1, byte[] data2)
        {
            if (data1 == data2)
                return true;

            if (data1.Length != data2.Length)
                return false;

            fixed (byte* bytes1 = data1, bytes2 = data2)
            {
                int len = data1.Length;
                int rem = len % (sizeof(long) * 16);
                long* b1 = (long*)bytes1;
                long* b2 = (long*)bytes2;
                long* e1 = (long*)(bytes1 + len - rem);

                while (b1 < e1)
                {
                    if (*(b1) != *(b2) || *(b1 + 1) != *(b2 + 1) ||
                        *(b1 + 2) != *(b2 + 2) || *(b1 + 3) != *(b2 + 3) ||
                        *(b1 + 4) != *(b2 + 4) || *(b1 + 5) != *(b2 + 5) ||
                        *(b1 + 6) != *(b2 + 6) || *(b1 + 7) != *(b2 + 7) ||
                        *(b1 + 8) != *(b2 + 8) || *(b1 + 9) != *(b2 + 9) ||
                        *(b1 + 10) != *(b2 + 10) || *(b1 + 11) != *(b2 + 11) ||
                        *(b1 + 12) != *(b2 + 12) || *(b1 + 13) != *(b2 + 13) ||
                        *(b1 + 14) != *(b2 + 14) || *(b1 + 15) != *(b2 + 15))
                        return false;
                    b1 += 16;
                    b2 += 16;
                }

                for (int i = 0; i < rem; i++)
                    if (data1[len - 1 - i] != data2[len - 1 - i])
                        return false;

                return true;
            }
        }
    }
}
