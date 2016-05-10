// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows.Media;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.ContentTrees.VirtualItems.MyItems
{
    public class OwnedItemTreeViewItem : MyItemsTreeViewItemBase
    {
        public OwnedItemTreeViewItem()
        {
            Text = Rocks.Resources.OwnedItemTreeViewItem_OwnedItemTreeViewItem_Owned;
            Foreground = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66));
            Icon = new Icon("Resources/16x16/owner.png");
            ToolTip = "Items owned by you";
        }

        [NotNull]
        protected override string GetQueryText()
        {
            return @"fast://*[@__Owner='" + DatabaseUri.Site.Credentials.UserName + @"']";
        }
    }
}
