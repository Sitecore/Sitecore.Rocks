// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Data
{
    public class FieldComparer : IComparer<Field>
    {
        public int Compare([NotNull] Field field1, [NotNull] Field field2)
        {
            Assert.ArgumentNotNull(field1, nameof(field1));
            Assert.ArgumentNotNull(field2, nameof(field2));

            var comparison = CompareSections(field1, field2);

            if (comparison != 0)
            {
                return comparison;
            }

            return CompareFields(field1, field2);
        }

        private int CompareFields([NotNull] Field field1, [NotNull] Field field2)
        {
            Debug.ArgumentNotNull(field2, nameof(field2));
            Debug.ArgumentNotNull(field1, nameof(field1));

            var sort1 = field1.SortOrder;
            var sort2 = field2.SortOrder;

            if (sort1 != sort2)
            {
                return sort1 - sort2;
            }

            var name1 = field1.Name;
            var name2 = field2.Name;

            if (name1.Length > 0 && name2.Length > 0)
            {
                if (name1[0] == '_' && name2[0] != '_')
                {
                    return 1;
                }

                if (name2[0] == '_' && name1[0] != '_')
                {
                    return -1;
                }
            }

            return string.Compare(name1, name2, StringComparison.InvariantCulture);
        }

        private int CompareSections([NotNull] Field field1, [NotNull] Field field2)
        {
            Debug.ArgumentNotNull(field2, nameof(field2));
            Debug.ArgumentNotNull(field1, nameof(field1));

            var sort1 = field1.Section.SortOrder;
            var sort2 = field2.Section.SortOrder;

            if (sort1 != sort2)
            {
                return sort1 - sort2;
            }

            var name1 = field1.Section.Name;
            var name2 = field2.Section.Name;

            if (name1.Length > 0 && name2.Length > 0)
            {
                if (name1[0] == '_' && name2[0] != '_')
                {
                    return 1;
                }

                if (name2[0] == '_' && name1[0] != '_')
                {
                    return -1;
                }
            }

            return string.Compare(name1, name2, StringComparison.InvariantCulture);
        }
    }
}
