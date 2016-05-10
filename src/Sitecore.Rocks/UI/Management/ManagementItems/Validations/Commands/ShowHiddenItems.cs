// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Commands
{
    [Command]
    public class ShowHiddenItems : CommandBase
    {
        public ShowHiddenItems()
        {
            Text = Resources.ShowHiddenItems_ShowHiddenItems_Show_Hidden_Items;
            Group = "View";
            SortingValue = 9100;
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

            AppHost.Settings.Set("Validation\\Site\\Hidden", context.ValidationViewer.Context.Site.Name, string.Empty);

            context.ValidationViewer.HiddenItems = string.Empty;
            context.ValidationViewer.RenderValidations();
        }
    }
}
