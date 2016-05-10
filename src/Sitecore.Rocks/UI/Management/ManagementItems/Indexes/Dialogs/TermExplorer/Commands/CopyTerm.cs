// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Indexes.Dialogs.TermExplorer.Commands
{
    [Command]
    public class CopyTerm : CommandBase
    {
        public CopyTerm()
        {
            Text = "Copy Term to Clipboard";
            Group = "Clipboard";
            SortingValue = 1100;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as TermExplorerContext;
            if (context == null)
            {
                return false;
            }

            if (context.TermExplorer.TermList.SelectedItem == null)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as TermExplorerContext;
            if (context == null)
            {
                return;
            }

            var selectedItem = context.TermExplorer.TermList.SelectedItem as TermExplorerDialog.TermDescriptor;
            if (selectedItem == null)
            {
                return;
            }

            AppHost.Clipboard.SetText(selectedItem.Text);
        }
    }
}
