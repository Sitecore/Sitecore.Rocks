// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.ContentTrees.VirtualItems.Favorites;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.Libraries.ContentTrees.VirtualItems;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.Libraries.Commands.Navigating
{
    [Command, CommandId(CommandIds.ItemEditor.LocateInSitecoreExplorer, typeof(ContentEditorContext), ToolBar = "SitecoreItemEditor", Group = "Navigate", Icon = "Resources/16x16/synchronize.png", Priority = 0x0100), ToolbarElement(typeof(IItemSelectionContext), "Home", Icon = "Resources/32x32/Nudge-Left.png", Text = "Locate")]
    public class LocateInContentTree : CommandBase, IToolbarElement
    {
        public LocateInContentTree()
        {
            Text = Resources.LocateInContentTree_LocateInContentTree_Locate_in_Sitecore_Explorer;
            Group = "Navigate";
            SortingValue = 1000;
            Icon = new Icon("Resources/16x16/synchronize.png");
        }

        public override bool CanExecute(object parameter)
        {
            var selection = parameter as IItemSelectionContext;
            if (selection == null)
            {
                return false;
            }

            if (selection.Items.Count() != 1)
            {
                return false;
            }

            var item = selection.Items.First();

            if (!(item is FavoriteTreeViewItem) && !(item is LibraryItemTreeViewItem) && parameter is ContentTreeContext)
            {
                return false;
            }

            return ActiveContext.ActiveContentTree != null;
        }

        public override void Execute(object parameter)
        {
            var selection = parameter as IItemSelectionContext;
            if (selection == null)
            {
                return;
            }

            if (selection.Items.Count() != 1)
            {
                return;
            }

            var item = selection.Items.FirstOrDefault();
            if (item == null)
            {
                return;
            }

            var contentTree = ActiveContext.ActiveContentTree;
            if (contentTree != null)
            {
                contentTree.Locate(item.ItemUri);
            }
        }
    }
}
