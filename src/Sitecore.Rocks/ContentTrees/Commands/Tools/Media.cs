// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.StartPage;
using Sitecore.Rocks.UI.StartPage.Tabs.ContentTab.Media.MediaLibrary;

namespace Sitecore.Rocks.ContentTrees.Commands.Tools
{
    [Command(Submenu = ToolsSubmenu.Name), CommandId(CommandIds.SitecoreExplorer.MediaManager, typeof(ContentTreeContext), ToolBar = ToolBars.SitecoreExplorer.Id, Group = ToolBars.SitecoreExplorer.Tools, Priority = 0x0700, Icon = "Resources/16x16/mediamanager.png"), StartPageCommand("Use the Media Library to upload and search for images and media files", StartPageMediaLibraryGroup.Name, 1000), Feature(FeatureNames.CommonTools)]
    public class Media : CommandBase, IStartPageCommand
    {
        public Media()
        {
            Text = Resources.Media_Media_Media_Library;
            Group = "Applications";
            SortingValue = 8010;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ISiteSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (!context.Site.DataService.CanExecuteAsync("Media.Search"))
            {
                return false;
            }

            return context.Site != Site.Empty;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ISiteSelectionContext;
            if (context == null)
            {
                return;
            }

            AppHost.Windows.Factory.OpenMediaViewer(context.Site);
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

            AppHost.Windows.Factory.OpenMediaViewer(databaseUri.Site);
        }
    }
}
