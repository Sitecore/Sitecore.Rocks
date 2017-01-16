// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Caches.Commands
{
    [Command]
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
            var context = parameter as CacheViewerContext;
            if (context == null)
            {
                return false;
            }

            if (context.CacheViewer.Context.Site.SitecoreVersion >= Constants.Versions.Version82)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as CacheViewerContext;
            if (context == null)
            {
                return;
            }

            context.CacheViewer.LoadCaches();
        }
    }
}
