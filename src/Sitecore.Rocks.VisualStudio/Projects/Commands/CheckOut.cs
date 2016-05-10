// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ProjectExtensions;
using Sitecore.Rocks.Extensions.ProjectItemExtensions;
using Sitecore.Rocks.Projects.Dialogs;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.Projects.Commands
{
    [Command, ShellMenuCommand(CommandIds.CheckOut)]
    public class CheckOut : SolutionCommand
    {
        public CheckOut()
        {
            Text = Resources.CheckOut_CheckOut_Check_Out;
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

            var items = GetProjects(selectedItems);
            if (items.Count != 1)
            {
                return false;
            }

            if (!items.All(i => i.IsValidProjectKind()))
            {
                return false;
            }

            foreach (var item in items)
            {
                var project = ProjectManager.GetProject(item.FileName);
                if (project == null)
                {
                    return false;
                }

                var site = project.Site;
                if (site == null)
                {
                    return false;
                }

                if ((site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.Projects) != DataServiceFeatureCapabilities.Projects)
                {
                    return false;
                }

                if (!project.IsRemoteSitecore)
                {
                    return false;
                }
            }

            IsVisible = true;
            return true;
        }

        public override void Execute(object parameter)
        {
            var selectedItems = GetSelectedItems();
            if (selectedItems.Count != 1)
            {
                return;
            }

            var items = GetProjects(selectedItems);

            var project = ProjectManager.GetProject(items[0].FileName);
            if (project == null)
            {
                return;
            }

            var site = project.Site;
            if (site == null)
            {
                return;
            }

            var dialog = new CheckOutDialog(site);
            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            var projectItems = CreateProjectItems(project, dialog.Files.SelectedItems);

            Download(projectItems);

            AddItems(projectItems);

            project.Save();
        }

        private void AddItems([NotNull] List<ProjectItem> projectItems)
        {
            Debug.ArgumentNotNull(projectItems, nameof(projectItems));

            foreach (var projectItem in projectItems)
            {
                var projectFile = projectItem as ProjectFileItem;
                if (projectFile != null)
                {
                    projectFile.AddToVisualStudioProject();
                }
            }
        }

        [CanBeNull]
        private ProjectItem CreateProjectItem([NotNull] Project project, [NotNull] FileTreeViewItem fileTreeViewItem)
        {
            Debug.ArgumentNotNull(project, nameof(project));
            Debug.ArgumentNotNull(fileTreeViewItem, nameof(fileTreeViewItem));

            var file = fileTreeViewItem.FileUri.FileName;

            if (project.Contains(file))
            {
                return null;
            }

            var result = new ProjectFileItem(project)
            {
                File = file
            };

            return result;
        }

        [NotNull]
        private List<ProjectItem> CreateProjectItems([NotNull] Project project, [NotNull] List<BaseTreeViewItem> items)
        {
            Debug.ArgumentNotNull(project, nameof(project));
            Debug.ArgumentNotNull(items, nameof(items));

            var result = new List<ProjectItem>();

            foreach (var item in items)
            {
                var fileTreeViewItem = item as FileTreeViewItem;
                if (fileTreeViewItem == null)
                {
                    continue;
                }

                if (fileTreeViewItem.FileUri.IsFolder)
                {
                    continue;
                }

                var projectItem = CreateProjectItem(project, fileTreeViewItem);
                if (projectItem == null)
                {
                    continue;
                }

                result.Add(projectItem);
                project.Add(projectItem);
            }

            return result;
        }

        private void Download([NotNull] List<ProjectItem> projectItems)
        {
            Debug.ArgumentNotNull(projectItems, nameof(projectItems));

            var logWindow = new ProjectLogWindow
            {
                Maximum = projectItems.Count
            };

            logWindow.AutoStart = delegate
            {
                var iterator = new ElementIterator<ProjectItem>(projectItems);

                iterator.Process = delegate(ProjectItem element)
                {
                    element.Revert(delegate(object sender, ProcessedEventArgs args)
                    {
                        if (!args.Ignore)
                        {
                            if (args.Text == @"reverted")
                            {
                                args.Text = @"checked out";
                            }

                            logWindow.Write(element.Path, args.Text, args.Comment);
                        }

                        logWindow.Increment();
                        iterator.Next();
                    });
                };

                iterator.Finish = delegate
                {
                    logWindow.Write(@"Finished", string.Empty, string.Empty);
                    logWindow.Finish();
                };

                iterator.Start();
            };

            AppHost.Shell.ShowDialog(logWindow);
        }
    }
}
