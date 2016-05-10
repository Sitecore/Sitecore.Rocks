// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees;

namespace Sitecore.Rocks.UI.Libraries.Commands.Libraries
{
    [Command]
    public class ShowLibraries : CommandBase
    {
        public ShowLibraries()
        {
            Text = "Show Libraries";
            Group = "View";
            SortingValue = 9000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return false;
            }

            IsChecked = AppHost.Settings.GetBool("SitecoreExplorer", "Libraries", true);

            return !context.SelectedItems.Any();
        }

        public override void Execute(object parameter)
        {
            AppHost.Settings.SetBool("SitecoreExplorer", "Libraries", !AppHost.Settings.GetBool("SitecoreExplorer", "Libraries", true));
        }
    }
}
