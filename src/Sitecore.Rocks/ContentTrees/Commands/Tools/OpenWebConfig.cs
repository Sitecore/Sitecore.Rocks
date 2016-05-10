// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.StartPage;
using Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.Configuring.WebConfig;

namespace Sitecore.Rocks.ContentTrees.Commands.Tools
{
    [Command(Submenu = ToolsSubmenu.Name), StartPageCommand("View the expanded web.config file with all include files", StartPageWebConfigGroup.Name, 9500), Feature(FeatureNames.AdvancedTools)]
    public class OpenWebConfig : CommandBase, IStartPageCommand
    {
        public OpenWebConfig()
        {
            Text = Resources.OpenWebConfig_OpenWebConfig_Open_Expanded_web_config;
            Group = "Tools";
            SortingValue = 500;
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

            var siteTreeViewItem = context.SelectedItems.FirstOrDefault() as SiteTreeViewItem;
            if (siteTreeViewItem == null)
            {
                return false;
            }

            if (!siteTreeViewItem.Site.DataService.CanExecuteAsync("ContentTrees.GetWebConfig"))
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

            if (context.SelectedItems.Count() != 1)
            {
                return;
            }

            var item = context.SelectedItems.FirstOrDefault() as SiteTreeViewItem;
            if (item == null)
            {
                return;
            }

            var site = item.Site;

            Execute(site);
        }

        bool IStartPageCommand.CanExecute(StartPageContext context)
        {
            Debug.ArgumentNotNull(context, nameof(context));

            return this.HasDatabaseUri(context);
        }

        void IStartPageCommand.Execute(StartPageContext context)
        {
            Debug.ArgumentNotNull(context, nameof(context));

            var databaseUri = this.GetDatabaseUri(context);
            if (databaseUri == DatabaseUri.Empty)
            {
                return;
            }

            Execute(context.StartPageViewer.DatabaseUri.Site);
        }

        private void Execute([NotNull] Site site)
        {
            Debug.ArgumentNotNull(site, nameof(site));

            ExecuteCompleted callback = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                var fileName = Path.Combine(Path.GetTempPath(), @"Expanded web.config");
                fileName = IO.File.MakeUniqueFileName(fileName);

                IO.File.Save(fileName, response);

                AppHost.Files.OpenFile(fileName);

                File.Delete(fileName);
            };

            site.DataService.ExecuteAsync("ContentTrees.GetWebConfig", callback);
        }
    }
}
