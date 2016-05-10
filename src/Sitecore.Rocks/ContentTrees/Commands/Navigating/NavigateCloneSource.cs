// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.UI;

namespace Sitecore.Rocks.ContentTrees.Commands.Navigating
{
    // [Command(Submenu = "Navigate")]
    [Feature(FeatureNames.Cloning)]
    public class NavigateCloneSource : CommandBase
    {
        public NavigateCloneSource()
        {
            Text = "Clone Source";
            Group = "Navigate";
            SortingValue = 2200;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return false;
            }

            if (context.SelectedItems.Count() != 1)
            {
                return false;
            }

            if (ActiveContext.ActiveContentTree == null)
            {
                return false;
            }

            var item = context.SelectedItems.First() as ItemTreeViewItem;
            if (item == null)
            {
                return false;
            }

            return /*item.Item.IsClone &&*/ !string.IsNullOrEmpty(item.Item.Source);
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return;
            }

            if (context.SelectedItems.Count() != 1)
            {
                return;
            }

            var itemTreeViewItem = context.SelectedItems.First() as ItemTreeViewItem;
            if (itemTreeViewItem == null)
            {
                return;
            }

            var contentTree = ActiveContext.ActiveContentTree;
            if (contentTree == null)
            {
                return;
            }

            var parts = itemTreeViewItem.Item.Source.Split('|');
            var databaseUri = new DatabaseUri(itemTreeViewItem.ItemUri.Site, new DatabaseName(parts[0]));
            var itemId = new ItemId(new Guid(parts[1]));

            var itemUri = new ItemUri(databaseUri, itemId);

            contentTree.Locate(itemUri);
        }
    }
}
