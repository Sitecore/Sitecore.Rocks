// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Extensions.ProjectExtensions;
using Sitecore.Rocks.Projects;
using Sitecore.Rocks.Projects.Commands;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.UI.FolderSynchronization.Commands
{
    [Command, ShellMenuCommand(CommandIds.StopFolderSync)]
    public class StopFolderSync : SolutionCommand
    {
        public override bool CanExecute(object parameter)
        {
            IsVisible = false;

            if (!(parameter is ShellContext))
            {
                return false;
            }

            var selectedItems = GetSelectedItems();
            if (selectedItems.Count() != 1)
            {
                return false;
            }

            var items = GetProjectItems(selectedItems);
            if (items.Count() != 1)
            {
                return false;
            }

            var item = items.FirstOrDefault();
            if (item == null)
            {
                return false;
            }

            if (!item.ContainingProject.IsValidProjectKind())
            {
                return false;
            }

            var project = ProjectManager.GetProject(item);
            if (project == null)
            {
                return false;
            }

            var folderSynchronizationManager = AppHost.Container.Get<FolderSynchronizationManager>();
            if (!folderSynchronizationManager.IsSynced(project, item))
            {
                return false;
            }

            IsVisible = true;

            return IsVisible;
        }

        public override void Execute(object parameter)
        {
            var selectedItems = GetSelectedItems();
            var items = GetProjectItems(selectedItems);

            var item = items.FirstOrDefault();
            if (item == null)
            {
                return;
            }

            var project = ProjectManager.GetProject(item);
            if (project == null)
            {
                return;
            }

            var site = project.Site;
            if (site == null)
            {
                return;
            }

            var folderSynchronizationManager = AppHost.Container.Get<FolderSynchronizationManager>();

            var sourceFolder = folderSynchronizationManager.GetFolderFileName(project, item);

            folderSynchronizationManager.Remove(project, sourceFolder);
        }
    }
}
