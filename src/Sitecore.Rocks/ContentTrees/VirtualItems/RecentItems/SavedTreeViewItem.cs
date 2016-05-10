// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentEditors;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.VirtualItems.RecentItems
{
    public class SavedTreeViewItem : RecentItemsTreeViewItemBase
    {
        public SavedTreeViewItem()
        {
            Text = Rocks.Resources.RecentlySavedTreeViewItem_RecentlySavedTreeViewItem_Saved;
            Icon = new Icon("Resources/16x16/save.png");
            ToolTip = "Recently saved items";

            Notifications.RegisterItemEvents(this, saved: ItemSaved);
        }

        protected override string GetStorageKey()
        {
            return "ContentTree\\Saved";
        }

        private void ItemSaved([NotNull] object sender, [NotNull] ContentModel contentModel)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(contentModel, nameof(contentModel));

            if (!contentModel.IsSingle)
            {
                return;
            }

            var item = contentModel.FirstItem;

            AddToJournal(item);
        }
    }
}
