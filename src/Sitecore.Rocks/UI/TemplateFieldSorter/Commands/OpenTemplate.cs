// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Dialogs;

namespace Sitecore.Rocks.UI.TemplateFieldSorter.Commands
{
    [Command]
    public class OpenTemplate : CommandBase
    {
        public OpenTemplate()
        {
            Text = "Open Template...";
            Group = "View";
            SortingValue = 2000;
        }

        public override bool CanExecute(object parameter)
        {
            return false;
            /*
            var context = parameter as TemplateFieldSorterContext;
            if (context == null)
            {
                return false;
            }

            return true;
            */
        }

        public override void Execute(object parameter)
        {
            var context = parameter as TemplateFieldSorterContext;
            if (context == null)
            {
                return;
            }

            var dialog = new AddFromTemplateDialog
            {
                DatabaseUri = context.TemplateFieldSorter.TemplateUri.DatabaseUri
            };

            dialog.AddInsertOptionsCheckBox.Visibility = Visibility.Collapsed;

            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            var selectedTemplate = dialog.SelectedTemplate;
            if (selectedTemplate == null)
            {
                return;
            }

            context.TemplateFieldSorter.AddTemplate(selectedTemplate.TemplateUri, false);
        }
    }
}
