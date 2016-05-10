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
    [Command(Submenu = SerializationSubmenu.Name), RuleAction("serialize database", "Serialization"), StartPageCommand("Serialize database", StartPageDatabaseGroup.Name, 1000), Feature(FeatureNames.Serialization)]
    public class SerializeDatabase : SerializeDatabaseCommand, IRuleAction, IStartPageCommand
    {
        public SerializeDatabase()
        {
            Text = Resources.SerializeDatabase_SerializeDatabase_Serialize_Database;
            Group = "Database";
            SortingValue = 3000;

            SerializationText = Resources.SerializeDatabase_SerializeDatabase_Serializing_Database___;
            ConfirmationText = "Are you sure you want to serialize the database?";
        }

        protected override void Execute(DatabaseUri databaseUri, ExecuteCompleted callback)
        {
            Debug.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Debug.ArgumentNotNull(callback, nameof(callback));

            databaseUri.Site.DataService.SerializeDatabase(databaseUri, callback);
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
