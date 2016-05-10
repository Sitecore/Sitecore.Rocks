// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.Controls.ItemDependencyListViews.Commands
{
    [Command]
    public class SelectLayout : CommandBase
    {
        public SelectLayout()
        {
            Text = "Select Layout and Renderings";
            Group = "Selection";
            SortingValue = 2000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ItemDependencyContext;
            if (context == null)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ItemDependencyContext;
            if (context == null)
            {
                return;
            }

            context.Control.SetCheckBoxes(delegate(ItemDependencyListView.ItemDescriptor i)
            {
                if (i.IsChecked)
                {
                    return;
                }

                i.IsChecked = i.Item.Path.StartsWith("/sitecore/layout/", StringComparison.CurrentCultureIgnoreCase);
            });

            context.Control.RefreshItems();
        }
    }
}
