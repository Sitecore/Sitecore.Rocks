// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Data
{
    public class FieldTypeHeaderComparer : IComparer<FieldTypeHeader>
    {
        public int Compare([NotNull] FieldTypeHeader x, [NotNull] FieldTypeHeader y)
        {
            Assert.ArgumentNotNull(x, nameof(x));
            Assert.ArgumentNotNull(y, nameof(y));

            var result = string.Compare(x.Section, y.Section, StringComparison.InvariantCultureIgnoreCase);

            if (result == 0)
            {
                result = string.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase);
            }

            return result;
        }
    }
}
