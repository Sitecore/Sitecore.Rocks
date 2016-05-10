// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Net;
using System.Windows;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentTrees.Commands.WebAdministration
{
    [Command(Submenu = WebServerSubmenu.Name), Feature(FeatureNames.WebServer)]
    public class StartProcess : CommandBase
    {
        public StartProcess()
        {
            Text = "Start IIS Worker Process";
            Group = "Process";
            SortingValue = 2000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return false;
            }

            var selectedItems = context.SelectedItems;
            if (selectedItems.Count() != 1)
            {
                return false;
            }

            var item = selectedItems.FirstOrDefault() as SiteTreeViewItem;
            if (item == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(item.Site.GetHost()))
            {
                return false;
            }

            if (!item.Site.DataService.HasWebSite)
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

            var item = context.SelectedItems.FirstOrDefault() as SiteTreeViewItem;
            if (item == null)
            {
                return;
            }

            var url = item.Site.GetHost() + "/sitecore/service/keepalive.aspx";

            var webClient = new WebClient();

            try
            {
                webClient.DownloadString(url);
            }
            catch (WebException)
            {
                AppHost.MessageBox(string.Format("Failed to open '{0}'.", url), "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            AppHost.MessageBox("Process started.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
