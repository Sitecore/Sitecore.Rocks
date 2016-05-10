// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Rules;
using Sitecore.Rocks.UI.StartPage;
using Sitecore.Rocks.UI.StartPage.Tabs.DevelopingTab.Serialization.Database;

namespace Sitecore.Rocks.ContentTrees.Commands.Serialization
{
    [Command(Submenu = SerializationSubmenu.Name), RuleAction("update database serialization", "Serialization"), StartPageCommand("Update database", StartPageDatabaseGroup.Name, 2000), Feature(FeatureNames.Serialization)]
    public class UpdateDatabase : SerializeDatabaseCommand, IRuleAction, IStartPageCommand
    {
        public UpdateDatabase()
        {
            Text = Resources.UpdateDatabase_UpdateDatabase_Update_Database;
            Group = "Database";
            SortingValue = 3100;

            SerializationText = Resources.UpdateDatabase_UpdateDatabase_Updating_Database___;
            ConfirmationText = "Are you sure you want to update the database?";
        }

        protected override void Execute(DatabaseUri databaseUri, ExecuteCompleted callback)
        {
            Debug.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Debug.ArgumentNotNull(callback, nameof(callback));

            databaseUri.Site.DataService.UpdateDatabase(databaseUri, callback);
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
    }
}
