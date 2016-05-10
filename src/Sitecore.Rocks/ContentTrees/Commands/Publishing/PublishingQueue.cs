// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.UI.StartPage;
using Sitecore.Rocks.UI.StartPage.Tabs.ContentTab.Publishing.Publishing;

namespace Sitecore.Rocks.ContentTrees.Commands.Publishing
{
    [Command(Submenu = PublishSubmenu.Name), StartPageCommand("View items that are in the publish queue", StartPagePublishToolsGroup.Name, 1000), Feature(FeatureNames.AdvancedPublishing)]
    public class PublishingQueue : CommandBase, IStartPageCommand
    {
        public PublishingQueue()
        {
            Text = Resources.PublishingQueue_PublishingQueue_Publishing_Queue;
            Group = "PublishQueue";
            SortingValue = 8000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IDatabaseSelectionContext;
            if (context == null)
            {
                return false;
            }

            var databaseUri = context.DatabaseUri;
            if (databaseUri == DatabaseUri.Empty)
            {
                return false;
            }

            if ((databaseUri.Site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.Publish) != DataServiceFeatureCapabilities.Publish)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IDatabaseSelectionContext;
            if (context == null)
            {
                return;
            }

            AppHost.Windows.Factory.OpenPublishingQueueViewer(context.DatabaseUri);
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

            AppHost.Windows.Factory.OpenPublishingQueueViewer(databaseUri);
        }
    }
}
