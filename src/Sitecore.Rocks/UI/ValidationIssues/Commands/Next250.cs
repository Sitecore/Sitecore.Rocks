// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.UI.ValidationIssues.Commands
{
    [Command]
    public class Next250 : CommandBase
    {
        public Next250()
        {
            Text = Resources.Next100_Next100_Next_250;
            Group = "Reload";
            SortingValue = 9000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ValidationIssuesContext;
            if (context == null)
            {
                return false;
            }

            if (context.ValidationIssues.NextItemId == ItemUri.Empty)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ValidationIssuesContext;
            if (context == null)
            {
                return;
            }

            if (context.ValidationIssues.NextItemId == ItemUri.Empty)
            {
                return;
            }

            context.ValidationIssues.Next();
        }
    }
}
