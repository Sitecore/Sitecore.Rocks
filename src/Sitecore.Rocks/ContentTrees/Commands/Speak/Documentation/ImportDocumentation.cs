// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.ContentTrees.Commands.Documentation
{
    [Command(Submenu = DocumentationSubmenu.Name)]
    public class ImportDocumentation : CommandBase
    {
        public ImportDocumentation()
        {
            Text = "Import...";
            Group = "Documentation";
            SortingValue = 2000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return false;
            }

            if (context.SelectedItems.Count() != 1)
            {
                return false;
            }

            var item = context.SelectedItems.FirstOrDefault() as ItemTreeViewItem;
            if (item == null)
            {
                return false;
            }

            if (!item.ItemUri.Site.IsSitecoreVersion(SitecoreVersion.Version70))
            {
                return false;
            }

            if (!item.Item.Path.StartsWith("/sitecore/client"))
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return;
            }

            var dialog = new OpenFileDialog
            {
                Title = "Import",
                CheckFileExists = true,
                DefaultExt = @".xml",
                Filter = @"Xml files|*.xml|All files|*.*"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            var selectedItem = context.SelectedItems.FirstOrDefault() as ItemTreeViewItem;
            if (selectedItem == null)
            {
                return;
            }

            var xml = File.ReadAllText(dialog.FileName);

            Site.RequestCompleted completed = delegate(string response)
            {
                if (string.IsNullOrEmpty(response))
                {
                    AppHost.MessageBox("Import succeeded.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    AppHost.MessageBox("Import succeeded with messages:\n\n" + response, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            };

            selectedItem.ItemUri.Site.Execute("Documentation.Import", completed, selectedItem.ItemUri.DatabaseName.ToString(), xml);
        }
    }
}
