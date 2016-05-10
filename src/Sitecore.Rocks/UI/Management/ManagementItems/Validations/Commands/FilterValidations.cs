// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.Management.ManagementItems.Validations.Dialogs;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Commands
{
    [Command]
    public class FilterValidations : CommandBase
    {
        public FilterValidations()
        {
            Text = Resources.ConfigureCheckers_ConfigureCheckers_Configure_Sanity_Checkers___;
            Group = "View";
            SortingValue = 9000;
        }

        public override bool CanExecute(object parameter)
        {
            return parameter as ValidationContext != null;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ValidationContext;
            if (context == null)
            {
                return;
            }

            var d = new FilterValidationsDialog(context.ValidationViewer.Context.Site);

            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            context.ValidationViewer.Rerun();
        }
    }
}
