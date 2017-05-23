using System.Linq;
using System.Net;
using System.Windows;
using Microsoft.Win32;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentTrees.Commands.Exporting
{
    // [Command(Submenu = "XML"), Feature(FeatureNames.Exporting)]
    public class ExportAsJson : CommandBase
    {
        public ExportAsJson()
        {
            Text = "Export as JSON...";
            Group = "YAML";
            SortingValue = 5010;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (context.Items.Count() != 1)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            var dialog = new SaveFileDialog
            {
                Title = "Export as JSON",
                CheckPathExists = true,
                OverwritePrompt = true,
                Filter = "*.content.json|*.content.json|*.*|*.*"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            var item = context.Items.First();

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                var client = new WebClient();
                try
                {
                    client.DownloadFile(response, dialog.FileName);
                }
                catch (WebException ex)
                {
                    if (AppHost.MessageBox($"Failed to download the JSON file: {response}\n\nDo you want to report this error?\n\n{ex.Message}\n{ex.StackTrace}", "Information", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                    {
                        AppHost.Shell.HandleException(ex);
                    }
                }
            };

            
            item.ItemUri.Site.DataService.ExecuteAsync("Exporting.ExportAsJson", completed, item.ItemUri.DatabaseName.ToString(), item.ItemUri.ItemId.ToString());
        }
    }
}