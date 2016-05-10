// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.TemplateHierarchies.Items;

namespace Sitecore.Rocks.UI.TemplateHierarchies.Commands
{
    [Command]
    public class LocateInContentTree : CommandBase
    {
        public LocateInContentTree()
        {
            Text = Resources.LocateInContentTree_LocateInContentTree_Locate_in_Sitecore_Explorer;
            Group = "Navigate";
            SortingValue = 100;
            Icon = new Icon("Resources/16x16/synchronize.png");
        }

        public override bool CanExecute(object parameter)
        {
            var selection = parameter as TemplateHierarchyItemTreeViewContext;
            if (selection == null)
            {
                return false;
            }

            if (selection.SelectedItems.Count() != 1)
            {
                return false;
            }

            return ActiveContext.ActiveContentTree != null;
        }

        public override void Execute(object parameter)
        {
            var selection = parameter as TemplateHierarchyItemTreeViewContext;
            if (selection == null)
            {
                return;
            }

            if (selection.SelectedItems.Count() != 1)
            {
                return;
            }

            var item = selection.SelectedItems.FirstOrDefault() as TemplateHierarchyTreeViewItem;
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
