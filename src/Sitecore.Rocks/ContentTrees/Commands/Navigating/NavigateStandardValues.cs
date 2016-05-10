// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentTrees.Commands.Navigating
{
    [Command(Submenu = "Navigate"), CommandId(CommandIds.SitecoreExplorer.NavigateStandardValues, typeof(ContentTreeContext), Text = "Navigate to Standard Values", ToolBar = ToolBars.SitecoreExplorer.Id, Group = ToolBars.SitecoreExplorer.Navigating, Icon = "Resources/16x16/standardvalues_navigate.png", Priority = 0x0210)]
    public class NavigateStandardValues : CommandBase, IToolbarElement
    {
        public NavigateStandardValues()
        {
            Text = Resources.NavigateStandardValues_NavigateStandardValues_Standard_Values;
            Group = "Navigate";
            SortingValue = 2000;
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

            return item.Item.StandardValuesId != ItemId.Empty;
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

            var templateUri = new ItemUri(itemTreeViewItem.ItemUri.DatabaseUri, itemTreeViewItem.Item.StandardValuesId);

            contentTree.Locate(templateUri);
        }
    }
}
