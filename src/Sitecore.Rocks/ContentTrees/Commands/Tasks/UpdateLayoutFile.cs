// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Text;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.ContentTrees.Commands.Tasks
{
    [Command(Submenu = TasksSubmenu.Name)]
    public class UpdateLayoutFile : CommandBase
    {
        public UpdateLayoutFile()
        {
            Text = "Update Layout File";
            Group = "Items";
            SortingValue = 1090;
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

            var project = AppHost.Projects.GetProjectContainingLinkedItem(selectedItem.ItemUri);
            if (project == null)
            {
                return false;
            }

            var fileName = project.GetLinkedFileName(selectedItem.ItemUri);
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            if (!AppHost.Files.FileExists(project.MakeAbsoluteFileName(fileName)))
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

            var project = AppHost.Projects.GetProjectContainingLinkedItem(selectedItem.ItemUri);
            if (project == null)
            {
                return;
            }

            var fileName = project.GetLinkedFileName(selectedItem.ItemUri);
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            UpdateFile(project.MakeAbsoluteFileName(fileName), selectedItem.ItemUri, selectedItem);
        }

        private void UpdateFile([NotNull] string fileName, [NotNull] ItemUri itemUri, [NotNull] IItem selectedItem)
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
                AppHost.Files.OpenFile(fileName);
            };

            AppHost.Server.XmlLayouts.GetXmlLayout(itemUri, itemUri.Site.Name.GetSafeCodeIdentifier(), "True", completed);
        }
    }
}
