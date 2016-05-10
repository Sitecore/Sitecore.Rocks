// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.Dialogs.SelectDatabaseDialogs;

namespace Sitecore.Rocks.Commands.Commands.Databases
{
    [Command]
    public class SwitchDatabase : CommandBase
    {
        public SwitchDatabase()
        {
            Text = Resources.SwitchDatabase_SwitchDatabase_Switch_Database___;
            Group = "Database";
            SortingValue = 9500;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IDatabaseUriContext;
            if (context == null)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IDatabaseUriContext;
            if (context == null)
            {
                return;
            }

            var dialog = new SelectDatabaseDialog
            {
                SelectedDatabaseUri = context.DatabaseUri
            };

            if (!dialog.ShowDialog())
            {
                return;
            }

            var databaseUri = dialog.SelectedDatabaseUri;
            if (databaseUri != DatabaseUri.Empty)
            {
                context.SetDatabaseUri(databaseUri);
            }
        }
    }
}
