// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.Controls.ItemDependencyListViews.Commands
{
    [Command]
    public class InvertSelection : CommandBase
    {
        public InvertSelection()
        {
            Text = "Invert Selection";
            Group = "Operations";
            SortingValue = 5000;
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

            context.Control.SetCheckBoxes(delegate(ItemDependencyListView.ItemDescriptor i) { i.IsChecked = !i.IsChecked; });

            context.Control.RefreshItems();
        }
    }
}
