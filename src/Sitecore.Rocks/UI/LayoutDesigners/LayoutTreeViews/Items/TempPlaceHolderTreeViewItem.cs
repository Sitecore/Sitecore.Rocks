// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Items
{
    public class TempPlaceHolderTreeViewItem : LayoutTreeViewItemBase
    {
        public TempPlaceHolderTreeViewItem([NotNull] DeviceTreeViewItem deviceTreeViewItem, [NotNull] string placeHolderName)
        {
            Assert.ArgumentNotNull(deviceTreeViewItem, nameof(deviceTreeViewItem));
            Assert.ArgumentNotNull(placeHolderName, nameof(placeHolderName));

            DeviceTreeViewItem = deviceTreeViewItem;
            Text = placeHolderName;
            Icon = new Icon("/Resources/16x16/Window.png");
            DataContext = this;

            Loaded += ControlLoaded;
        }

        [NotNull]
        public DeviceTreeViewItem DeviceTreeViewItem { get; private set; }

        public override bool GetChildren(GetChildrenDelegate callback, bool async)
        {
            Assert.ArgumentNotNull(callback, nameof(callback));

            callback(Enumerable.Empty<BaseTreeViewItem>());

            return true;
        }

        public override void Unload()
        {
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            var value = TryFindResource(@"UnusedRenderingsTreeViewItem") as Style;
            if (value != null)
            {
                Style = value;
            }
        }
    }
}
