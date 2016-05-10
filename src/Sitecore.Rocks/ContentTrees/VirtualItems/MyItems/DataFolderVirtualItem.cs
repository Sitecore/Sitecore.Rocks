// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentTrees.VirtualItems.MyItems
{
    [VirtualItem(1100), Feature(FeatureNames.SitecoreExplorer.Files)]
    public class DataFolderVirtualItem : IVirtualItem
    {
        public bool CanAddItem(BaseTreeViewItem parent)
        {
            var siteTreeViewItem = parent as SiteTreeViewItem;
            if (siteTreeViewItem == null)
            {
                return false;
            }

            if (siteTreeViewItem.Site.DataServiceName != HardRockWebService.Name)
            {
                return false;
            }

            return true;
        }

        public BaseTreeViewItem GetItem(BaseTreeViewItem parent)
        {
            var siteTreeViewItem = parent as SiteTreeViewItem;
            if (siteTreeViewItem == null)
            {
                throw Exceptions.InvalidOperation();
            }

            var fileUri = new FileUri(siteTreeViewItem.Site, "/", FileUriBaseFolder.Data, true);

            var result = new RootFileTreeViewItem(fileUri)
            {
                Text = @"Data Folder",
                Margin = new Thickness(0)
            };

            result.MakeExpandable();

            return result;
        }
    }
}
