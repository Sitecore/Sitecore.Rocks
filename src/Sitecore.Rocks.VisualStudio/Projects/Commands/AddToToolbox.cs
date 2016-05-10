// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ProjectExtensions;
using Sitecore.Rocks.Extensions.ProjectItemExtensions;
using Sitecore.Rocks.Projects.ToolboxItems;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.Projects.Commands
{
    [Command, ShellMenuCommand(CommandIds.AddToToolbox)]
    public class AddToToolbox : SolutionCommand
    {
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

            if (!items.All(i => i.ContainingProject.IsValidProjectKind()))
            {
                return false;
            }

            if (AnyItem(items, item => ProjectManager.GetProject(item) == null))
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
            if (items.Count == 0)
            {
                return;
            }

            foreach (var item in items)
            {
                AddItem(item);
            }
        }

        private void AddItem([NotNull] EnvDTE.ProjectItem item)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            if (!IsFolder(item))
            {
                AddItemToToolbox(item);
            }

            foreach (var subProjectItem in item.ProjectItems)
            {
                var subitem = subProjectItem as EnvDTE.ProjectItem;
                if (subitem != null)
                {
                    AddItem(subitem);
                }
            }
        }

        private void AddItemToToolbox([NotNull] EnvDTE.ProjectItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var project = ProjectManager.GetProject(item);
            if (project == null)
            {
                return;
            }

            var projectItem = project.GetProjectItem(item);
            if (projectItem == null)
            {
                projectItem = AddToProject(project, item);
            }

            if (projectItem.HideFromToolbox)
            {
                return;
            }

            var extension = Path.GetExtension(projectItem.Path) ?? string.Empty;

            var handler = ToolboxItemManager.GetToolboxItemHandler(extension);
            if (handler == null)
            {
                return;
            }

            handler.AddToToolbox(projectItem);
        }

        [NotNull]
        private ProjectFileItem AddToProject([NotNull] Project project, [NotNull] EnvDTE.ProjectItem item)
        {
            Debug.ArgumentNotNull(project, nameof(project));
            Debug.ArgumentNotNull(item, nameof(item));

            var fileName = project.GetProjectItemFileName(item);

            var projectItem = ProjectFileItem.Load(project, fileName);

            project.Add(projectItem);

            return projectItem;
        }

        private bool IsFileItem([NotNull] EnvDTE.ProjectItem item)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            var project = ProjectManager.GetProject(item);
            if (project == null)
            {
                return false;
            }

            var fileName = item.GetFileName();
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            var extension = Path.GetExtension(fileName) ?? string.Empty;

            return ToolboxItemManager.GetToolboxItemHandler(extension) != null;
        }
    }
}
