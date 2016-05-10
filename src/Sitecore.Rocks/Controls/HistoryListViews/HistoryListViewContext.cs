// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Controls.HistoryListViews
{
    public class HistoryListViewContext : ICommandContext
    {
        public HistoryListViewContext([NotNull] HistoryListView control, [NotNull] IEnumerable<ItemUri> selectedItems)
        {
            Assert.ArgumentNotNull(control, nameof(control));
            Assert.ArgumentNotNull(selectedItems, nameof(selectedItems));

            ListView = control;
            SelectedItems = selectedItems;
        }

        [NotNull]
        public HistoryListView ListView { get; private set; }

        [NotNull]
        public IEnumerable<ItemUri> SelectedItems { get; set; }
    }
}
