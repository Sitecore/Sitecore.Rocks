// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Commands
{
    [Command]
    public class FilterWarnings : CommandBase
    {
        public FilterWarnings()
        {
            Text = "Warnings";
            Group = "Severity";
            SortingValue = 1010;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ValidationContext;
            if (context == null)
            {
                return false;
            }

            IsChecked = context.ValidationViewer.ShowWarnings;

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ValidationContext;
            if (context == null)
            {
                return;
            }

            context.ValidationViewer.ShowWarnings = !context.ValidationViewer.ShowWarnings;
            AppHost.Settings.Set("Validation\\Site\\View", "Warnings", context.ValidationViewer.ShowWarnings ? "1" : "0");
            context.ValidationViewer.RenderValidations();
        }
    }
}
