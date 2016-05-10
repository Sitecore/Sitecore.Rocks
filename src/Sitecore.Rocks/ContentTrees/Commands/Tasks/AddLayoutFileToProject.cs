// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.UI.LayoutDesigners;

namespace Sitecore.Rocks.ContentTrees.Commands.Tasks
{
    [Command(Submenu = TasksSubmenu.Name)]
    public class AddLayoutFileToProject : CommandBase
    {
        public AddLayoutFileToProject()
        {
            Text = "Add Layout File to Project";
            Group = "Items";
            SortingValue = 1205;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (AppHost.Shell.ShellIdentifier != Constants.SitecoreRocksVisualStudio)
            {
                return false;
            }

            if (!AppHost.Projects.IsSolutionOpen())
            {
                return false;
            }

            if (context.Items.Count() != 1)
            {
                return false;
            }

            var selectedItem = context.Items.FirstOrDefault();
            if (selectedItem == null)
            {
                return false;
            }

            if (AppHost.Projects.All(p => p.Site != selectedItem.ItemUri.Site))
            {
                return false;
            }

            var project = AppHost.Projects.GetProjectContainingLinkedItem(selectedItem.ItemUri);
            if (project != null)
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

            var selectedItem = context.Items.FirstOrDefault();
            if (selectedItem == null)
            {
                return;
            }

            string initialDirectory = null;

            var project = AppHost.Projects.FirstOrDefault(p => p.Site == selectedItem.ItemUri.Site);
            if (project != null)
            {
                initialDirectory = AppHost.Settings.GetString(@"Layouts\\XmlLayoutFolder", @"Last", project.FolderName);
            }

            var dialog = new SaveFileDialog
            {
                Title = "Add Layout File to Project",
                CheckPathExists = true,
                OverwritePrompt = true,
                InitialDirectory = initialDirectory,
                FileName = selectedItem.Name + ".layout.xml",
                Filter = "Layout (*.layout.xml)|*.layout.xml|All Files|(*.*)"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            AppHost.Settings.SetString(@"Layouts\\XmlLayoutFolder", @"Last", Path.GetDirectoryName(dialog.FileName) ?? string.Empty);

            var itemUri = new ItemVersionUri(selectedItem.ItemUri, LanguageManager.CurrentLanguage, Version.Latest);

            LayoutSchemaHelper.GenerateSchema(dialog.FileName, itemUri.ItemUri, selectedItem, AddLayout);
        }

        private void AddLayout([NotNull] string fileName, [NotNull] ItemUri itemUri, [NotNull] IItem selectedItem)
        {
            Debug.ArgumentNotNull(fileName, nameof(fileName));
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(selectedItem, nameof(selectedItem));

            ExecuteCompleted completed = delegate(string layout, ExecuteResult result)
            {
                if (!DataService.HandleExecute(layout, result))
                {
                    return;
                }

                AppHost.Files.WriteAllText(fileName, layout, Encoding.UTF8);

                var project = AppHost.Projects.AddFileToProject(Path.GetDirectoryName(fileName) ?? string.Empty, fileName, false);
                if (project != null)
                {
                    var relativeFileName = project.GetRelativeFileName(fileName);
                    if (!string.IsNullOrEmpty(relativeFileName))
                    {
                        project.LinkItemAndFile(itemUri, relativeFileName);
                    }
                }

                AppHost.Files.OpenFile(fileName);
            };

            AppHost.Server.XmlLayouts.GetXmlLayout(itemUri, itemUri.Site.Name.GetSafeCodeIdentifier(), "True", completed);
        }
    }
}
