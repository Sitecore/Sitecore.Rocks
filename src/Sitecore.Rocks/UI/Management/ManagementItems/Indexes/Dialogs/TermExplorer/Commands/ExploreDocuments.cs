// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.Management.ManagementItems.Indexes.Dialogs.DocumentExplorer;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Indexes.Dialogs.TermExplorer.Commands
{
    [Command]
    public class ExploreDocuments : CommandBase
    {
        public ExploreDocuments()
        {
            Text = "Explore Documents...";
            Group = "Explore";
            SortingValue = 1000;
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

            var dialog = new DocumentExplorerDialog(context.TermExplorer.Site, context.TermExplorer.IndexName, context.TermExplorer.FieldName, selectedItem.Text);
            AppHost.Shell.ShowDialog(dialog);
        }
    }
}
