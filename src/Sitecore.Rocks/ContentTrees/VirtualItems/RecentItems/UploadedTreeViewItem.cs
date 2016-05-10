// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.VirtualItems.RecentItems
{
    public class UploadedTreeViewItem : RecentItemsTreeViewItemBase
    {
        public UploadedTreeViewItem()
        {
            Text = Rocks.Resources.UploadedTreeViewItem_UploadedTreeViewItem_Uploaded;
            Icon = new Icon("Resources/16x16/upload.png");
            ToolTip = "Recently uploaded items";

            Notifications.RegisterMediaEvents(this, MediaUploaded);
        }

        protected override string GetStorageKey()
        {
            return "ContentTree\\Uploaded";
        }

        private void MediaUploaded([NotNull] object sender, [NotNull] ItemHeader itemHeader)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(itemHeader, nameof(itemHeader));

            AddToJournal(itemHeader);
        }
    }
}
