// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Items;
using Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Models;

namespace Sitecore.Rocks.UI.LayoutDesigners.Designers
{
    public class PlaceHolderContext : ICommandContext
    {
        public PlaceHolderContext([NotNull] PlaceHolderTreeViewItem placeHolderTreeViewItem)
        {
            Assert.ArgumentNotNull(placeHolderTreeViewItem, nameof(placeHolderTreeViewItem));

            PlaceHolderTreeViewItem = placeHolderTreeViewItem;
            PageModel = placeHolderTreeViewItem.DeviceTreeViewItem.Device.PageModel;
        }

        [NotNull]
        public PageModel PageModel { get; private set; }

        [NotNull]
        public PlaceHolderTreeViewItem PlaceHolderTreeViewItem { get; private set; }
    }
}
