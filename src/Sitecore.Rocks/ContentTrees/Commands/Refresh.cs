// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows.Input;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands
{
    [Command, CommandId(CommandIds.SitecoreExplorer.Refresh, typeof(ContentTreeContext))]
    public class Refresh : CommandBase
    {
        public Refresh()
        {
            Text = Resources.Refresh;
            Group = "Refresh";
            SortingValue = 9999;
            Icon = new Icon("Resources/16x16/refresh.png");
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

            var item = context.SelectedItems.FirstOrDefault() as ICanRefresh;
            if (item == null)
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

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                var itemTreeViewItem = context.SelectedItems.FirstOrDefault() as ItemTreeViewItem;
                if (itemTreeViewItem != null)
                {
                    itemTreeViewItem.RefreshPreservingSelection();
                    return;
                }
            }

            var item = context.SelectedItems.FirstOrDefault() as ICanRefresh;
            if (item == null)
            {
                return;
            }

            var baseTreeViewItem = item as BaseTreeViewItem;
            if (baseTreeViewItem != null)
            {
                baseTreeViewItem.RefreshAndExpand();
                return;
            }

            item.Refresh();
        }
    }
}
