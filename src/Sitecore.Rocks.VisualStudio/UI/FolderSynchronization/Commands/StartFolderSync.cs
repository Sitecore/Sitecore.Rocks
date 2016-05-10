// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Extensions.ProjectExtensions;
using Sitecore.Rocks.Projects;
using Sitecore.Rocks.Projects.Commands;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.Dialogs.SelectFileDialogs;
using TaskDialogInterop;

namespace Sitecore.Rocks.UI.FolderSynchronization.Commands
{
    [Command, ShellMenuCommand(CommandIds.StartFolderSync)]
    public class StartFolderSync : SolutionCommand
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
            if (folderSynchronizationManager.IsSynced(project, item))
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

            var dialog = new SelectFileDialog
            {
                Site = site
            };

            if (!dialog.ShowDialog())
            {
                return;
            }

            var options = new TaskDialogOptions
            {
                Title = "Folder Synchronization",
                CommonButtons = TaskDialogCommonButtons.None,
                Content = "Select the synchronization mode.",
                MainIcon = VistaTaskDialogIcon.Information,
                DefaultButtonIndex = 0,
                CommandButtons = new[]
                {
                    "Mirror - every change is applied",
                    "Copy - only create and edit changes are applied"
                },
                AllowDialogCancellation = true
            };

            var r = TaskDialog.Show(options).CommandButtonResult;
            if (r == null)
            {
                return;
            }

            var mode = r == 0 ? FolderSynchronizationMode.Mirror : FolderSynchronizationMode.Copy;

            var fileName = dialog.SelectedFilePath.TrimStart('\\');

            var absoluteFileName = Path.Combine(site.WebRootPath, fileName);
            if (File.Exists(absoluteFileName))
            {
                fileName = Path.GetDirectoryName(fileName) ?? string.Empty;
            }

            var folderSynchronizationManager = AppHost.Container.Get<FolderSynchronizationManager>();

            var sourceFolder = folderSynchronizationManager.GetFolderFileName(project, item);
            var destinationFolder = fileName + "\\";

            folderSynchronizationManager.Add(project, sourceFolder, destinationFolder, mode, "*.*");
        }
    }
}
