// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ProjectExtensions;
using Sitecore.Rocks.Projects.Dialogs;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.Projects.Commands
{
    [Command, ShellMenuCommand(CommandIds.UpdateFromSitecore)]
    public class Update : SolutionCommand
    {
        private List<UpdateItem> updateItems = new List<UpdateItem>();

        public Update()
        {
            Text = Resources.Update;
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

            if (!AnyItem(items, IsUpdatable))
            {
                return false;
            }

            IsVisible = true;
            return true;
        }

        public override void Execute(object parameter)
        {
            var selectedItems = GetSelectedItems();
            if (selectedItems.Count == 0)
            {
                return;
            }

            GetUpdateItems(selectedItems);

            var logWindow = new ProjectLogWindow
            {
                Maximum = updateItems.Count
            };

            logWindow.AutoStart = delegate
            {
                var iterator = new ElementIterator<UpdateItem>(updateItems);

                iterator.Process = delegate(UpdateItem element)
                {
                    var projectItem = element.ProjectItem;

                    projectItem.Update(delegate(object sender, ProcessedEventArgs args)
                    {
                        if (!args.Ignore)
                        {
                            logWindow.Write(projectItem.Path, args.Text, args.Comment);
                        }

                        logWindow.Increment();
                        iterator.Next();
                    });
                };

                iterator.Finish = delegate
                {
                    logWindow.Write(Resources.Finished, string.Empty, string.Empty);
                    logWindow.Finish();
                };

                iterator.Start();
            };

            AppHost.Shell.ShowDialog(logWindow);

            SaveProjects();
        }

        private void GetUpdateItems([NotNull] List<SelectedItem> selectedItems)
        {
            Debug.ArgumentNotNull(selectedItems, nameof(selectedItems));

            updateItems = new List<UpdateItem>();

            var projectItems = GetProjectItems(selectedItems);
            foreach (var item in projectItems)
            {
                Traverse(item);
            }
        }

        private bool IsLocalSitecore([NotNull] ProjectItem projectItem)
        {
            Debug.ArgumentNotNull(projectItem, nameof(projectItem));

            return !projectItem.Project.IsRemoteSitecore;
        }

        private bool IsUpdatable([NotNull] EnvDTE.ProjectItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

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

            return !projectItem.IsAdded;
        }

        private void SaveProjects()
        {
            var projects = new List<ProjectBase>();
            foreach (var updateItem in updateItems)
            {
                var project = updateItem.ProjectItem.Project;

                if (projects.Contains(project))
                {
                    continue;
                }

                project.Save();
                projects.Add(project);
            }
        }

        private void Traverse([NotNull] EnvDTE.ProjectItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var project = ProjectManager.GetProject(item);
            if (project == null)
            {
                return;
            }

            var projectItem = project.GetProjectItem(item);
            if (projectItem != null)
            {
                updateItems.Add(new UpdateItem(projectItem));
            }

            foreach (var child in item.ProjectItems)
            {
                var i = child as EnvDTE.ProjectItem;
                if (i != null)
                {
                    Traverse(i);
                }
            }
        }

        public class UpdateItem
        {
            public UpdateItem([NotNull] ProjectItem projectItem)
            {
                Assert.ArgumentNotNull(projectItem, nameof(projectItem));

                ProjectItem = projectItem;
            }

            [NotNull]
            public ProjectItem ProjectItem { get; }
        }
    }
}
