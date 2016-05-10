// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentTrees.Commands.Navigating
{
    [Command, Feature(FeatureNames.AdvancedNavigation)]
    public class SetAsActiveSite : CommandBase
    {
        public SetAsActiveSite()
        {
            Text = "Set as Active Database";
            Group = "Navigate";
            SortingValue = 4900;
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

            var item = context.SelectedItems.First() as DatabaseTreeViewItem;
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

            var item = context.SelectedItems.First() as DatabaseTreeViewItem;
            if (item == null)
            {
                return;
            }

            AppHost.Settings.ActiveDatabaseUri = item.DatabaseUri;
        }
    }
}
