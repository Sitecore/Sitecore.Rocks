// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Controls.ItemDependencyListViews
{
    public class ItemDependencyContext : ICommandContext
    {
        public ItemDependencyContext([NotNull] ItemDependencyListView control, [NotNull] IEnumerable<ItemUri> selectedItems)
        {
            Assert.ArgumentNotNull(control, nameof(control));
            Assert.ArgumentNotNull(selectedItems, nameof(selectedItems));

            Control = control;
            SelectedItems = selectedItems;
        }

        public ItemDependencyListView Control { get; private set; }

        public IEnumerable<ItemUri> SelectedItems { get; set; }
    }
}
