// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.Management.ManagementItems.Indexes.Dialogs.TermExplorer;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Indexes.Commands
{
    [Command]
    public class ExploreTerms : CommandBase
    {
        public ExploreTerms()
        {
            Text = "Explore Terms...";
            Group = "Explore";
            SortingValue = 1000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IndexViewerContext;
            if (context == null)
            {
                return false;
            }

            if (context.ClickTarget != IndexViewerContext.FieldList)
            {
                return false;
            }

            var indexDescriptor = context.IndexViewer.GetSelectedIndex();
            if (indexDescriptor == null)
            {
                return false;
            }

            if (context.IndexViewer.FieldsList.SelectedItem == null)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IndexViewerContext;
            if (context == null)
            {
                return;
            }

            var indexDescriptor = context.IndexViewer.GetSelectedIndex();
            if (indexDescriptor == null)
            {
                return;
            }

            var field = context.IndexViewer.FieldsList.SelectedItem as IndexViewer.IndexFieldDescriptor;
            if (field == null)
            {
                return;
            }

            var dialog = new TermExplorerDialog(context.IndexViewer.Context.Site, indexDescriptor.Name, field.Name);
            AppHost.Shell.ShowDialog(dialog);
        }
    }
}
