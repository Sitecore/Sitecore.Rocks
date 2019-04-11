// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.VisualStudio.Shell;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ProjectExtensions;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.Projects.Dialogs
{
    public partial class CommitWindow
    {
        private readonly ListViewSorter listViewSorter;

        private List<CommitItem> commitItems;

        public CommitWindow()
        {
            InitializeComponent();
            this.InitializeDialog();

            listViewSorter = new ListViewSorter(ListView);
        }

        public void Initialize([NotNull] List<EnvDTE.ProjectItem> items)
        {
            Assert.ArgumentNotNull(items, nameof(items));

            commitItems = new List<CommitItem>();

            foreach (var item in items)
            {
                GetCommitItems(item);
            }

            ListView.ItemsSource = commitItems;
        }

        private void Commit()
        {
            var logWindow = new ProjectLogWindow
            {
                Maximum = commitItems.Count
            };

            logWindow.AutoStart = delegate
            {
                var iterator = new ElementIterator<CommitItem>(commitItems);

                iterator.Process = delegate(CommitItem element)
                {
                    if (!element.IsChecked)
                    {
                        logWindow.Increment();
                        iterator.Next();
                        return;
                    }

                    var projectItem = element.ProjectItem;

                    projectItem.Commit(delegate(object sender, ProcessedEventArgs args)
                    {
                        WriteResult(logWindow, projectItem, args);
                        logWindow.Increment();
                        iterator.Next();
                    });
                };

                iterator.Finish = delegate
                {
                    logWindow.Write(Rocks.Resources.Finished, string.Empty, string.Empty);
                    logWindow.Finish();
                };

                iterator.Start();
            };

            AppHost.Shell.ShowDialog(logWindow);

            SaveProjects();
        }

        private void GetCommitItems([NotNull] EnvDTE.ProjectItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var project = ProjectManager.GetProject(item);
            if (project == null)
            {
                return;
            }

            var path = project.GetProjectItemFileName(item);

            foreach (var projectItem in project.ProjectItems.OfType<ProjectItem>())
            {
                if (!projectItem.Path.StartsWith(path, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                if (!projectItem.IsModified && !projectItem.IsAdded && !projectItem.IsConflict && projectItem.IsValid)
                {
                    continue;
                }

                var p = projectItem;
                if (commitItems.Any(commitItem => commitItem.ProjectItem == p))
                {
                    continue;
                }

                commitItems.Add(new CommitItem(projectItem));
            }
        }

        private void HeaderClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            listViewSorter.HeaderClick(sender, e);
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Commit();
            this.Close(true);
        }

        private void SaveProjects()
        {
            var projects = new List<ProjectBase>();
            foreach (var commitItem in commitItems)
            {
                var project = commitItem.ProjectItem.Project;
                if (projects.Contains(project))
                {
                    continue;
                }

                project.Save();
                projects.Add(project);
            }
        }

        private void WriteResult([NotNull] ProjectLogWindow projectLogWindow, [NotNull] ProjectItem projectItem, [NotNull] ProcessedEventArgs args)
        {
            Debug.ArgumentNotNull(projectLogWindow, nameof(projectLogWindow));
            Debug.ArgumentNotNull(projectItem, nameof(projectItem));
            Debug.ArgumentNotNull(args, nameof(args));

            if (!args.Ignore)
            {
				ThreadHelper.JoinableTaskFactory.Run(async delegate
				{
					await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
					projectLogWindow.Write(projectItem.Path, args.Text, args.Comment);
				});
			}
        }

        protected class CommitItem
        {
            public CommitItem([NotNull] ProjectItem projectItem)
            {
                Assert.ArgumentNotNull(projectItem, nameof(projectItem));

                ProjectItem = projectItem;

                Path = projectItem.Path;
                Extension = System.IO.Path.GetExtension(Path);

                if (projectItem.IsConflict)
                {
                    Status = Rocks.Resources.CommitItem_CommitItem_conflict;

                    switch (projectItem.ConflictResolution)
                    {
                        case ConflictResolution.UseLocalVersion:
                            Status = Rocks.Resources.CommitItem_CommitItem_resolved_using_local_version;
                            IsChecked = true;
                            break;
                        case ConflictResolution.UseServerVersion:
                            Status = Rocks.Resources.CommitItem_CommitItem_resolved_using_server_version;
                            break;
                    }
                }
                else if (projectItem.IsAdded)
                {
                    Status = Rocks.Resources.CommitItem_CommitItem_added;
                    IsChecked = true;
                }
                else if (projectItem.IsModified)
                {
                    Status = Rocks.Resources.CommitItem_CommitItem_modified;
                    IsChecked = true;
                }
                else if (!projectItem.IsValid)
                {
                    Status = Rocks.Resources.CommitItem_CommitItem_missing;
                    IsChecked = true;
                }
                else
                {
                    Status = Rocks.Resources.CommitItem_CommitItem_unchanged;
                }
            }

            [NotNull]
            public string Extension { get; private set; }

            public bool IsChecked { get; set; }

            [NotNull]
            public string Path { get; }

            [NotNull]
            public ProjectItem ProjectItem { get; }

            [NotNull]
            public string Status { get; set; }
        }
    }
}
