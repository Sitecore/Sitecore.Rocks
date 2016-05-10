// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Text;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Text.NaturalLanguage
{
    public class PhraseList
    {
        [NotNull]
        public static string JoinWithTail([NotNull] IEnumerable<object> items)
        {
            Assert.ArgumentNotNull(items, nameof(items));

            return JoinWithTail(items, ", ", " and ");
        }

        [NotNull]
        public static string JoinWithTail([NotNull] IEnumerable<object> items, [NotNull] string infixDelimeter, [NotNull] string tailDelimeter)
        {
            Assert.ArgumentNotNull(items, nameof(items));
            Assert.ArgumentNotNull(infixDelimeter, nameof(infixDelimeter));
            Assert.ArgumentNotNull(tailDelimeter, nameof(tailDelimeter));

            var iterator = items.GetEnumerator();
            if (!iterator.MoveNext())
            {
                return string.Empty;
            }

            var currentValue = iterator.Current;
            var nextIsEnd = !iterator.MoveNext();

            var sb = new StringBuilder();
            while (true)
            {
                sb.Append(currentValue);
                if (nextIsEnd)
                {
                    break;
                }

                currentValue = iterator.Current;
                nextIsEnd = !iterator.MoveNext();

                sb.Append(!nextIsEnd ? infixDelimeter : tailDelimeter);
            }

            return sb.ToString();
        }
    }
}
