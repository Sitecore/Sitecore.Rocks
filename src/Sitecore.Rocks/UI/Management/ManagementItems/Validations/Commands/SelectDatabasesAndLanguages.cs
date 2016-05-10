// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.Dialogs.SelectDatabasesAndLanguagesDialogs;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Commands
{
    [Command]
    public class SelectDatabasesAndLanguages : CommandBase
    {
        public SelectDatabasesAndLanguages()
        {
            Text = Resources.SelectDatabasesAndLanguages_SelectDatabasesAndLanguages_Selected_Databases_and_Languages___;
            Group = "View";
            SortingValue = 9050;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ValidationContext;
            if (context == null)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ValidationContext;
            if (context == null)
            {
                return;
            }

            var selected = AppHost.Settings.GetString("Validation\\Site\\DatabasesAndLanguages", context.ValidationViewer.Context.Site.Name, string.Empty);

            var dialog = new SelectDatabasesAndLanguagesDialog(context.ValidationViewer.Context.Site, selected);
            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            AppHost.Settings.Set("Validation\\Site\\DatabasesAndLanguages", context.ValidationViewer.Context.Site.Name, dialog.Selected);

            context.ValidationViewer.Rerun();
        }
    }
}
