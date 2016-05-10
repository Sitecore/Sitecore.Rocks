// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Items;
using Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Models;

namespace Sitecore.Rocks.UI.LayoutDesigners.Designers
{
    public class RenderingContext : ICommandContext
    {
        public RenderingContext([NotNull] RenderingTreeViewItem renderingTreeViewItem)
        {
            Assert.ArgumentNotNull(renderingTreeViewItem, nameof(renderingTreeViewItem));

            RenderingTreeViewItem = renderingTreeViewItem;
            PageModel = renderingTreeViewItem.DeviceTreeViewItem.Device.PageModel;
        }

        [NotNull]
        public PageModel PageModel { get; private set; }

        [NotNull]
        public RenderingTreeViewItem RenderingTreeViewItem { get; private set; }
    }
}
