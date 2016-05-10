// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ProjectExtensions;
using Sitecore.Rocks.Extensions.ProjectItemExtensions;
using Sitecore.Rocks.Projects.Dialogs;
using Sitecore.Rocks.Projects.FileItems;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.Projects.Commands.Registering
{
    [Command, ShellMenuCommand(CommandIds.RegisterFiles)]
    public class CreateFileItem : SolutionCommand
    {
        public CreateFileItem()
        {
            Text = Resources.CreateFileItem_CreateFileItem_Create_Item_From_File;
        }

        public override bool CanExecute(object parameter)
        {
            IsVisible = false;

            if (!(parameter is ShellContext))
            {
                return false;
            }

            var selectedItems = GetSelectedItems();
            if (selectedItems.Count == 0)
            {
                return false;
            }

            var items = GetProjectItems(selectedItems);

            if (AnyItem(items, HasNoProject))
            {
                return false;
            }

            if (!items.All(i => i.ContainingProject.IsValidProjectKind()))
            {
                return false;
            }

            IsVisible = AnyItem(items, IsFileItem);

            return IsVisible;
        }

        public override void Execute(object parameter)
        {
            var selectedItems = GetSelectedItems();
            if (selectedItems.Count == 0)
            {
                return;
            }

            var items = GetProjectItems(selectedItems);

            var project = ProjectManager.GetProject(items[0]);
            if (project == null)
            {
                return;
            }

            var site = project.Site;
            if (site == null)
            {
                return;
            }

            var w = new CreateItemWindow();
            w.Initialize(site, items);
            AppHost.Shell.ShowDialog(w);
        }

        private bool HasNoProject([NotNull] EnvDTE.ProjectItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var project = ProjectManager.GetProject(item);
            if (project == null)
            {
                return true;
            }

            var site = project.Site;
            if (site == null)
            {
                return true;
            }

            return (site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.Projects) != DataServiceFeatureCapabilities.Projects;
        }

        private bool IsFileItem([NotNull] EnvDTE.ProjectItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var project = ProjectManager.GetProject(item);
            if (project == null)
            {
                return false;
            }

            var projectFile = project.GetProjectItem(item) as ProjectFileItem;
            if (projectFile != null)
            {
                var itemIds = projectFile.Items;
                if (itemIds.Count > 0)
                {
                    return false;
                }
            }

            var fileName = item.GetFileName();
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            return FileItemManager.GetFileItemHandler(fileName) != null;
        }
    }
}
