// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ProjectExtensions;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.Projects.Commands
{
    public class DisconnectFileItem : SolutionCommand
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
            if (items.Count == 0)
            {
                return false;
            }

            if (!items.All(i => i.ContainingProject.IsValidProjectKind()))
            {
                return false;
            }

            if (AnyItem(items, item => ProjectManager.GetProject(item) == null))
            {
                return false;
            }

            IsVisible = true;

            return AnyProjectItem(items, HasItem);
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

            var projects = new List<Project>();

            foreach (var item in items)
            {
                var project = ProjectManager.GetProject(item);
                if (project == null)
                {
                    continue;
                }

                var projectFile = project.GetProjectItem(item) as ProjectFileItem;
                if (projectFile == null)
                {
                    continue;
                }

                projectFile.Items.Clear();

                if (projects.Contains(project))
                {
                    projects.Add(project);
                }
            }

            foreach (var project in projects)
            {
                project.Save();
            }
        }

        private bool HasItem([NotNull] ProjectItem projectItem)
        {
            Assert.ArgumentNotNull(projectItem, nameof(projectItem));

            var projectFile = projectItem as ProjectFileItem;
            if (projectFile == null)
            {
                return false;
            }

            return projectFile.Items.Count > 0;
        }
    }
}
