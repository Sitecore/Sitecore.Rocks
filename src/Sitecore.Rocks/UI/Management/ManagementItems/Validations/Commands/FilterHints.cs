// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Commands
{
    [Command]
    public class FilterHints : CommandBase
    {
        public FilterHints()
        {
            Text = "Hints";
            Group = "Severity";
            SortingValue = 1030;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ValidationContext;
            if (context == null)
            {
                return false;
            }

            IsChecked = context.ValidationViewer.ShowHints;

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ValidationContext;
            if (context == null)
            {
                return;
            }

            context.ValidationViewer.ShowHints = !context.ValidationViewer.ShowHints;
            AppHost.Settings.Set("Validation\\Site\\View", "Hints", context.ValidationViewer.ShowHints ? "1" : "0");
            context.ValidationViewer.RenderValidations();
        }
    }
}
