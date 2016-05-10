// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Commands
{
    [Command]
    public class FilterErrors : CommandBase
    {
        public FilterErrors()
        {
            Text = "Errors";
            Group = "Severity";
            SortingValue = 1000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ValidationContext;
            if (context == null)
            {
                return false;
            }

            IsChecked = context.ValidationViewer.ShowErrors;

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ValidationContext;
            if (context == null)
            {
                return;
            }

            context.ValidationViewer.ShowErrors = !context.ValidationViewer.ShowErrors;
            AppHost.Settings.Set("Validation\\Site\\View", "Errors", context.ValidationViewer.ShowErrors ? "1" : "0");
            context.ValidationViewer.RenderValidations();
        }
    }
}
