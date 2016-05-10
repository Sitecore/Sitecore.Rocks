// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Commands
{
    [Command]
    public class FilterSuggestions : CommandBase
    {
        public FilterSuggestions()
        {
            Text = "Suggestions";
            Group = "Severity";
            SortingValue = 1020;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ValidationContext;
            if (context == null)
            {
                return false;
            }

            IsChecked = context.ValidationViewer.ShowSuggestions;

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ValidationContext;
            if (context == null)
            {
                return;
            }

            context.ValidationViewer.ShowSuggestions = !context.ValidationViewer.ShowSuggestions;
            AppHost.Settings.Set("Validation\\Site\\View", "Suggestions", context.ValidationViewer.ShowSuggestions ? "1" : "0");
            context.ValidationViewer.RenderValidations();
        }
    }
}
