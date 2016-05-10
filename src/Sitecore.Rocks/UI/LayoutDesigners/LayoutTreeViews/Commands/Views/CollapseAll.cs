// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Commands.Views
{
    [Command]
    public class CollapseAll : CommandBase
    {
        public CollapseAll()
        {
            Text = "Collapse All";
            Group = "Views";
            SortingValue = 8000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as LayoutTreeViewContext;
            if (context == null)
            {
                return false;
            }

            if (context.TreeViewItems.Any(i => i.Items.Count == 0))
            {
                return false;
            }

            return context.TreeViewItems.Any();
        }

        public override void Execute(object parameter)
        {
            var context = parameter as LayoutTreeViewContext;
            if (context == null)
            {
                return;
            }

            var items = context.TreeViewItems;

            foreach (var baseTreeViewItem in items)
            {
                Collapse(baseTreeViewItem);
            }
        }

        private void Collapse([NotNull] BaseTreeViewItem baseTreeViewItem)
        {
            Debug.ArgumentNotNull(baseTreeViewItem, nameof(baseTreeViewItem));

            baseTreeViewItem.IsExpanded = false;

            foreach (var item in baseTreeViewItem.Items.OfType<BaseTreeViewItem>())
            {
                Collapse(item);
            }
        }
    }
}
