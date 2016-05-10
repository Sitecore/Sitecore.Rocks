// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Extensions.IEnumerableExtensions
{
    public static class EnumerableExtensions
    {
        public static bool AllHasValue<T>([NotNull] this IEnumerable<T> source, [NotNull] Func<T, object> getValue)
        {
            Assert.ArgumentNotNull(source, nameof(source));
            Assert.ArgumentNotNull(getValue, nameof(getValue));

            if (!source.Any())
            {
                return true;
            }

            var first = source.First();
            var value = getValue(first);

            return source.All(i => getValue(i) == value);
        }

        public static void Following<T>([NotNull] this IEnumerable<T> source, T anchor, [NotNull] Action<T> action)
        {
            Assert.ArgumentNotNull(source, nameof(source));
            Assert.ArgumentNotNull(action, nameof(action));

            source.Following(anchor, EqualityComparer<T>.Default, action);
        }

        public static void Following<T>([NotNull] this IEnumerable<T> source, T anchor, [NotNull] IEqualityComparer<T> comparer, [NotNull] Action<T> action)
        {
            Assert.ArgumentNotNull(source, nameof(source));
            Assert.ArgumentNotNull(comparer, nameof(comparer));
            Assert.ArgumentNotNull(action, nameof(action));

            foreach (var item in source.Following(anchor))
            {
                action(item);
            }
        }

        [NotNull]
        public static IEnumerable<T> Following<T>([NotNull] this IEnumerable<T> source, T anchor)
        {
            Assert.ArgumentNotNull(source, nameof(source));

            return source.Following(anchor, EqualityComparer<T>.Default);
        }

        [NotNull]
        public static IEnumerable<T> Following<T>([NotNull] this IEnumerable<T> source, T anchor, [NotNull] IEqualityComparer<T> comparer)
        {
            Assert.ArgumentNotNull(source, nameof(source));
            Assert.ArgumentNotNull(comparer, nameof(comparer));

            var isFollowing = false;

            foreach (var item in source)
            {
                if (comparer.Equals(item, anchor))
                {
                    isFollowing = true;
                }
                else if (isFollowing)
                {
                    yield return item;
                }
            }
        }

        public static void ForEach<T>([NotNull] this IEnumerable<T> source, [NotNull] Action<T> action)
        {
            Assert.ArgumentNotNull(source, nameof(source));
            Assert.ArgumentNotNull(action, nameof(action));

            foreach (var item in source)
            {
                action(item);
            }
        }

        public static T Next<T>([NotNull] this IEnumerable<T> source, T anchor)
        {
            Assert.ArgumentNotNull(source, nameof(source));

            return source.Next(anchor, EqualityComparer<T>.Default);
        }

        public static T Next<T>([NotNull] this IEnumerable<T> source, T anchor, [NotNull] IEqualityComparer<T> comparer)
        {
            Assert.ArgumentNotNull(source, nameof(source));
            Assert.ArgumentNotNull(comparer, nameof(comparer));

            var previous = default(T);

            foreach (var item in source)
            {
                if (comparer.Equals(previous, anchor))
                {
                    return item;
                }

                previous = item;
            }

            return default(T);
        }

        [NotNull]
        public static IEnumerable<T> NotNull<T>([NotNull] this IEnumerable<T> source) where T : class
        {
            Assert.ArgumentNotNull(source, nameof(source));

            return source.Where(t => t != null);
        }

        public static T Previous<T>([NotNull] this IEnumerable<T> source, T anchor)
        {
            Assert.ArgumentNotNull(source, nameof(source));

            return source.Previous(anchor, EqualityComparer<T>.Default);
        }

        public static T Previous<T>([NotNull] this IEnumerable<T> source, T anchor, [NotNull] IEqualityComparer<T> comparer)
        {
            Assert.ArgumentNotNull(source, nameof(source));
            Assert.ArgumentNotNull(comparer, nameof(comparer));

            var last = default(T);

            foreach (var item in source)
            {
                if (comparer.Equals(item, anchor))
                {
                    return last;
                }

                last = item;
            }

            return default(T);
        }
    }
}
