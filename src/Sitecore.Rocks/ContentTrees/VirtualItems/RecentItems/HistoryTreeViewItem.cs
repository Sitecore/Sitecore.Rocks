// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI;

namespace Sitecore.Rocks.ContentTrees.VirtualItems.RecentItems
{
    public class HistoryTreeViewItem : RecentItemsTreeViewItemBase
    {
        public HistoryTreeViewItem()
        {
            Text = Rocks.Resources.HistoryTreeViewItem_HistoryTreeViewItem_History;
            MaxItems = 25;
            Icon = new Icon("Resources/16x16/history.png");
            ToolTip = "History";

            ActiveContext.ContentModelChanged += ContentModelChanged;
        }

        protected override string GetStorageKey()
        {
            return "ContentTree\\History";
        }

        private void ContentModelChanged([CanBeNull] object sender, [CanBeNull] EventArgs e)
        {
            if (ActiveContext.ActiveContentEditor == null)
            {
                return;
            }

            var contentModel = ActiveContext.ContentModel;
            if (!contentModel.IsSingle)
            {
                return;
            }

            AddToJournal(contentModel.FirstItem);
        }
    }
}
