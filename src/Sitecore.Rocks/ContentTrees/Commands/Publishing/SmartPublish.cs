// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.StartPage;
using Sitecore.Rocks.UI.StartPage.Tabs.ContentTab.Publishing.Publishing;

namespace Sitecore.Rocks.ContentTrees.Commands.Publishing
{
    [Command(Submenu = PublishSubmenu.Name), CommandId(CommandIds.SitecoreExplorer.SmartPublish, typeof(ContentTreeContext), ToolBar = ToolBars.SitecoreExplorer.Id, Group = ToolBars.SitecoreExplorer.Publishing, Priority = 0x0540, Icon = "Resources/16x16/database_publish.png"), StartPageCommand("Publish the database using Smart Publish", StartPagePublishDatabaseGroup.Name, 1000), Feature(FeatureNames.Publishing)]
    public class SmartPublish : PublishDatabaseCommand, IStartPageCommand
    {
        public SmartPublish()
        {
            Text = Resources.SmartPublish_SmartPublish_Smart_Publish;
            Group = "PublishDatabase";
            PublishingText = Resources.SmartPublish_SmartPublish_Smart_Publishing___;
            Mode = 2;
            SortingValue = 5100;
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

            Publish(databaseUri);
        }
    }
}
