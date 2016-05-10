// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Navigating
{
    [Command, CommandId(CommandIds.SitecoreExplorer.CollapseAll, typeof(ContentTreeContext))]
    public class CollapseAll : CommandBase
    {
        public CollapseAll()
        {
            Text = Resources.CollapseAll_CollapseAll_Collapse_All;
            Group = "Refresh";
            SortingValue = 9500;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return false;
            }

            var selectedItems = context.SelectedItems;

            var treeView = context.ContentTree.TreeView;

            if (!selectedItems.All(i => treeView.Items.Contains(i)))
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return;
            }

            var treeView = context.ContentTree.TreeView;

            Collapse(treeView);
        }

        private void Collapse([NotNull] ItemsControl treeView)
        {
            Debug.ArgumentNotNull(treeView, nameof(treeView));

            foreach (var item in treeView.Items)
            {
                var itemsControl = item as ItemsControl;
                if (itemsControl == null)
                {
                    continue;
                }

                Collapse(itemsControl);
            }

            foreach (var item in treeView.Items)
            {
                var treeViewItem = item as TreeViewItem;
                if (treeViewItem == null)
                {
                    continue;
                }

                treeViewItem.IsExpanded = false;
            }
        }
    }
}
