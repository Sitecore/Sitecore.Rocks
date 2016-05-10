// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ProjectExtensions;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.Projects.Commands
{
    public abstract class ResolveCommand : SolutionCommand
    {
        public ConflictResolution ConflictResolution { get; set; }

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

            if (AnyProjectItem(items, IsLocalSitecore))
            {
                return false;
            }

            IsVisible = AnyItem(items, IsUnresolvedConflict);
            return IsVisible;
        }

        public override void Execute(object parameter)
        {
            var selectedItems = GetSelectedItems();
            if (selectedItems.Count == 0)
            {
                return;
            }

            var projects = new List<Project>();

            var projectItems = GetProjectItems(selectedItems);
            foreach (var item in projectItems)
            {
                Traverse(projects, item);
            }

            foreach (var project in projects)
            {
                project.Save();
            }
        }

        private bool IsLocalSitecore([NotNull] ProjectItem projectItem)
        {
            Assert.ArgumentNotNull(projectItem, nameof(projectItem));

            return !projectItem.Project.IsRemoteSitecore;
        }

        private bool IsUnresolvedConflict([NotNull] EnvDTE.ProjectItem item)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            var project = ProjectManager.GetProject(item);
            if (project == null)
            {
                return false;
            }

            var projectItem = project.GetProjectItem(item);
            if (projectItem == null)
            {
                return false;
            }

            return projectItem.ConflictResolution == ConflictResolution.NotResolved;
        }

        private void Traverse([NotNull] List<Project> projects, [NotNull] EnvDTE.ProjectItem item)
        {
            Assert.ArgumentNotNull(projects, nameof(projects));
            Assert.ArgumentNotNull(item, nameof(item));

            var project = ProjectManager.GetProject(item);
            if (project == null)
            {
                return;
            }

            var projectItem = project.GetProjectItem(item);
            if (projectItem != null && projectItem.IsConflict)
            {
                projectItem.ConflictResolution = ConflictResolution;

                if (!projects.Contains(project))
                {
                    projects.Add(project);
                }
            }

            foreach (var child in item.ProjectItems)
            {
                var i = child as EnvDTE.ProjectItem;
                if (i != null)
                {
                    Traverse(projects, i);
                }
            }
        }
    }
}
