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
    [Command(Submenu = SerializationSubmenu.Name), RuleAction("revert database", "Serialization"), StartPageCommand("Revert database", StartPageDatabaseGroup.Name, 3000), Feature(FeatureNames.Serialization)]
    public class RevertDatabase : SerializeDatabaseCommand, IRuleAction, IStartPageCommand
    {
        public RevertDatabase()
        {
            Text = Resources.RevertDatabase_RevertDatabase_Revert_Database;
            Group = "Database";
            SortingValue = 3200;

            SerializationText = Resources.RevertDatabase_RevertDatabase_Reverting_Database___;
            ConfirmationText = "Are you sure you want to revert the database?";
        }

        protected override void Execute(DatabaseUri databaseUri, ExecuteCompleted callback)
        {
            Debug.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Debug.ArgumentNotNull(callback, nameof(callback));

            databaseUri.Site.DataService.RevertDatabase(databaseUri, callback);
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
            if (databaseUri != DatabaseUri.Empty)
            {
                Execute(databaseUri);
            }
        }
    }
}
