// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ProjectExtensions;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.Projects.Dialogs
{
    public partial class RevertWindow
    {
        private readonly ListViewSorter listViewSorter;

        private List<RevertItem> revertItems;

        public RevertWindow()
        {
            InitializeComponent();
            this.InitializeDialog();

            listViewSorter = new ListViewSorter(ListView);
        }

        public void Initialize([NotNull] List<EnvDTE.ProjectItem> items)
        {
            Assert.ArgumentNotNull(items, nameof(items));

            revertItems = new List<RevertItem>();

            foreach (var item in items)
            {
                GetRevertItems(item);
            }

            ListView.ItemsSource = revertItems;
        }

        private void GetRevertItems([NotNull] EnvDTE.ProjectItem item)
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

                if (projectItem.IsAdded)
                {
                    continue;
                }

                var p = projectItem;
                if (revertItems.Any(revertItem => revertItem.ProjectItem == p))
                {
                    continue;
                }

                revertItems.Add(new RevertItem(projectItem));
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

            Revert();
            this.Close(true);
        }

        private void Revert()
        {
            var logWindow = new ProjectLogWindow
            {
                Maximum = revertItems.Count
            };

            logWindow.AutoStart = delegate
            {
                var iterator = new ElementIterator<RevertItem>(revertItems);

                iterator.Process = delegate(RevertItem element)
                {
                    if (!element.IsChecked)
                    {
                        logWindow.Increment();
                        iterator.Next();
                        return;
                    }

                    var projectItem = element.ProjectItem;

                    projectItem.Revert(delegate(object sender, ProcessedEventArgs args)
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

        private void SaveProjects()
        {
            var projects = new List<ProjectBase>();
            foreach (var commitItem in revertItems)
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
                projectLogWindow.Dispatcher.Invoke(new Action(() => projectLogWindow.Write(projectItem.Path, args.Text, args.Comment)));
            }
        }

        protected class RevertItem
        {
            public RevertItem([NotNull] ProjectItem projectItem)
            {
                Assert.ArgumentNotNull(projectItem, nameof(projectItem));

                ProjectItem = projectItem;

                Path = projectItem.Path;
                Extension = System.IO.Path.GetExtension(Path);

                if (projectItem.IsConflict)
                {
                    Status = Rocks.Resources.CommitItem_CommitItem_conflict;
                    IsChecked = true;

                    switch (projectItem.ConflictResolution)
                    {
                        case ConflictResolution.UseLocalVersion:
                            Status = Rocks.Resources.CommitItem_CommitItem_resolved_using_local_version;
                            IsChecked = false;
                            break;
                        case ConflictResolution.UseServerVersion:
                            Status = Rocks.Resources.CommitItem_CommitItem_resolved_using_server_version;
                            break;
                    }
                }
                else if (!projectItem.IsValid)
                {
                    Status = Rocks.Resources.CommitItem_CommitItem_missing;
                    IsChecked = true;
                }
                else if (projectItem.IsModified)
                {
                    Status = Rocks.Resources.CommitItem_CommitItem_modified;
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
