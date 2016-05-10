// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Extensions.ProjectExtensions;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI;

namespace Sitecore.Rocks.Projects.Commands
{
    [Command, ShellMenuCommand(CommandIds.LocateFileItemInContentTree)]
    public class LocateFileInContentTree : SolutionCommand
    {
        public override bool CanExecute(object parameter)
        {
            IsVisible = false;

            if (!(parameter is ShellContext))
            {
                return false;
            }

            var selectedItems = GetSelectedItems();
            if (selectedItems.Count != 1)
            {
                return false;
            }

            var items = GetProjectItems(selectedItems);
            if (items.Count != 1)
            {
                return false;
            }

            if (!items.All(i => i.ContainingProject.IsValidProjectKind()))
            {
                return false;
            }

            var item = items[0];
            if (item == null)
            {
                return false;
            }

            var project = ProjectManager.GetProject(item);
            if (project == null)
            {
                return false;
            }

            var projectFile = project.GetProjectItem(item) as ProjectFileItem;
            if (projectFile == null)
            {
                return false;
            }

            if (projectFile.Items.Count != 1)
            {
                return false;
            }

            IsVisible = true;

            return ActiveContext.ActiveContentTree != null;
        }

        public override void Execute(object parameter)
        {
            var selectedItems = GetSelectedItems();
            if (selectedItems.Count == 0)
            {
                return;
            }

            var items = GetProjectItems(selectedItems);

            var item = items[0];
            if (item == null)
            {
                return;
            }

            var project = ProjectManager.GetProject(item);
            if (project == null)
            {
                return;
            }

            var projectFile = project.GetProjectItem(item) as ProjectFileItem;
            if (projectFile == null)
            {
                return;
            }

            if (projectFile.Items.Count != 1)
            {
                return;
            }

            var itemUri = projectFile.Items[0];

            var contentTree = ActiveContext.ActiveContentTree;
            if (contentTree != null)
            {
                contentTree.Locate(itemUri);
            }
        }
    }
}
