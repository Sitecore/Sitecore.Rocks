// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Data
{
    public class ItemSelectionContext : ICommandContext, IItemSelectionContext
    {
        public ItemSelectionContext([NotNull] IEnumerable<IItem> items)
        {
            Assert.ArgumentNotNull(items, nameof(items));

            Items = items;
        }

        public ItemSelectionContext([NotNull] IItem item)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            Items = new List<IItem>
            {
                item
            };
        }

        public IEnumerable<IItem> Items { get; }
    }
}
