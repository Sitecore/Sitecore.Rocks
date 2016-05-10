// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows.Media;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.VirtualItems.MyItems
{
    public class LockedItemTreeViewItem : MyItemsTreeViewItemBase
    {
        public LockedItemTreeViewItem()
        {
            Text = Rocks.Resources.LockedItemTreeViewItem_LockedItemTreeViewItem_Locked;
            Foreground = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66));
            Icon = new Icon("Resources/16x16/lock.png");
            ToolTip = "Items locked by you";
        }

        protected override string GetQueryText()
        {
            return @"fast://*[@__lock='%""" + DatabaseUri.Site.Credentials.UserName + @"""%']";
        }

        protected override TemplatedTreeViewItem GetTreeViewItem(ItemUri itemUri, string name, Icon icon, ItemId templateId, string templateName)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(name, nameof(name));
            Debug.ArgumentNotNull(icon, nameof(icon));
            Debug.ArgumentNotNull(templateId, nameof(templateId));
            Debug.ArgumentNotNull(templateName, nameof(templateName));

            var result = base.GetTreeViewItem(itemUri, name, icon, templateId, templateName);
            if (result == null)
            {
                return null;
            }

            result.SetData("locked.item", "true");

            return result;
        }
    }
}
