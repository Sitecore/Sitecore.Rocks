// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.Dialogs.SelectSiteDialogs;

namespace Sitecore.Rocks.Commands.Commands.Databases
{
    [Command]
    public class SwitchSite : CommandBase
    {
        public SwitchSite()
        {
            Text = Resources.SwitchSite_SwitchSite_Switch_Site___;
            Group = "Site";
            SortingValue = 9550;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ISiteContext;
            if (context == null)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ISiteContext;
            if (context == null)
            {
                return;
            }

            var d = new SelectSiteDialog
            {
                Site = context.Site
            };

            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            var site = d.Site;
            if (site == null)
            {
                return;
            }

            context.SetSite(site);
        }
    }
}
