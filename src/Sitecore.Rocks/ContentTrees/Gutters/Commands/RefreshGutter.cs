// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.ContentTrees.Gutters.Commands
{
    [Command]
    public class RefreshGutter : CommandBase
    {
        public RefreshGutter()
        {
            Text = Resources.RefreshGutter_RefreshGutter_Refresh_Gutter;
            Group = "Refresh";
            SortingValue = 9999;
            Icon = new Icon("Resources/16x16/refresh.png");
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as GutterContext;
            if (context == null)
            {
                return false;
            }

            if (!context.SelectedItems.All(item => item is ItemTreeViewItem))
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

            foreach (var item in context.SelectedItems.OfType<ItemTreeViewItem>())
            {
                item.UpdateGutters();
            }
        }
    }
}
