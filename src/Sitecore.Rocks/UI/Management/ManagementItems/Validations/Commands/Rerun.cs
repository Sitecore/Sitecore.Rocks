// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Commands
{
    [Command]
    public class Rerun : CommandBase
    {
        public Rerun()
        {
            Text = Resources.Rerun_Rerun_Rerun_Sanity_Checks;
            Group = "Rerun";
            SortingValue = 100;
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

            context.ValidationViewer.StopTimer();
            context.ValidationViewer.Rerun();
        }
    }
}
