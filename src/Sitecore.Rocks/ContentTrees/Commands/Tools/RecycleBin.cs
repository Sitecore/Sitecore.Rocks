// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.StartPage;
using Sitecore.Rocks.UI.StartPage.Tabs.SystemTab.Maintenance.Archives;

namespace Sitecore.Rocks.ContentTrees.Commands.Tools
{
    [Command(Submenu = ToolsSubmenu.Name), CommandId(CommandIds.SitecoreExplorer.RecycleBin, typeof(ContentTreeContext)), StartPageCommand("Open the Recycle Bin to restore deleted items", StartPageArchivesGroup.Name, 1000), Feature(FeatureNames.AdvancedTools)]
    public class RecycleBin : CommandBase, IStartPageCommand
    {
        public RecycleBin()
        {
            Text = Resources.OpenRecycleBin_OpenRecycleBin_Recycle_Bin;
            Group = "Applications";
            SortingValue = 8410;
            Icon = new Icon("Resources/16x16/recyclebin.png");
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IDatabaseSelectionContext;
            if (context == null)
            {
                return false;
            }

            return context.DatabaseUri != DatabaseUri.Empty;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IDatabaseSelectionContext;
            if (context == null)
            {
                return;
            }

            Execute(context.DatabaseUri);
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

            Execute(databaseUri);
        }

        private void Execute([NotNull] DatabaseUri databaseUri)
        {
            Debug.ArgumentNotNull(databaseUri, nameof(databaseUri));

            var header = string.Format(@"Recycle Bin - {0} - {1}", databaseUri.Site.Name, databaseUri.DatabaseName);

            AppHost.Windows.Factory.OpenRecycleBin(databaseUri, header);
        }
    }
}
