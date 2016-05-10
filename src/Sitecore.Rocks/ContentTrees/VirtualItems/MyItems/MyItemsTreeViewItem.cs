// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows.Media;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.VirtualItems.MyItems
{
    public class MyItemsTreeViewItem : BaseTreeViewItem
    {
        public MyItemsTreeViewItem()
        {
            Text = Rocks.Resources.MyItemsTreeViewItem_MyItemsTreeViewItem_My_Items;
            Foreground = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66));
            ToolTip = "My items";
            Icon = new Icon("Resources/16x16/myitems.png");
        }

        [NotNull]
        public DatabaseUri DatabaseUri { get; set; }

        public override bool GetChildren(GetChildrenDelegate callback, bool async)
        {
            Assert.ArgumentNotNull(callback, nameof(callback));

            callback(Enumerable.Empty<BaseTreeViewItem>());

            return true;
        }
    }
}
