// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Controls.ListViews
{
    public class MultSelectListView : ListView
    {
        [NotNull]
        public Style ItemStyle { get; set; }

        protected override DependencyObject GetContainerForItemOverride()
        {
            var multiSelectListViewItem = new MultiSelectListViewItem
            {
                Style = ItemStyle
            };

            return multiSelectListViewItem;
        }
    }
}
