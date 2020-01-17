using System.Collections.Generic;
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
    }
}
