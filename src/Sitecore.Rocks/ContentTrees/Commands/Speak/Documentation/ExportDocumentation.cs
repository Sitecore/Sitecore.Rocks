// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Linq;
using Microsoft.Win32;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.ContentTrees.Commands.Documentation
{
    [Command(Submenu = DocumentationSubmenu.Name)]
    public class ExportDocumentation : CommandBase
    {
        public ExportDocumentation()
        {
            Text = "Export...";
            Group = "Documentation";
            SortingValue = 1000;
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

            var dialog = new SaveFileDialog
            {
                Title = "Export SPEAK Documentation",
                CheckPathExists = true,
                OverwritePrompt = true,
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

            Site.RequestCompleted completed = delegate(string response)
            {
                File.WriteAllText(dialog.FileName, response);
                AppHost.Files.OpenFile(dialog.FileName);
            };

            selectedItem.ItemUri.Site.Execute("Documentation.Export", completed, selectedItem.ItemUri.DatabaseName.ToString(), selectedItem.ItemUri.ItemId.ToString());
        }
    }
}
