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
    public partial class ExcludeWindow
    {
        private readonly ListViewSorter listViewSorter;

        private List<ExcludeItem> excludeItems;

        public ExcludeWindow()
        {
            InitializeComponent();
            this.InitializeDialog();

            listViewSorter = new ListViewSorter(ListView);
        }

        public void Initialize([NotNull] List<EnvDTE.ProjectItem> items)
        {
            Assert.ArgumentNotNull(items, nameof(items));

            excludeItems = new List<ExcludeItem>();

            foreach (var item in items)
            {
                GetExcludeItems(item);
            }

            ListView.ItemsSource = excludeItems;
        }

        private void Exclude()
        {
            var logWindow = new ProjectLogWindow
            {
                Maximum = excludeItems.Count
            };

            logWindow.AutoStart = delegate
            {
                var iterator = new ElementIterator<ExcludeItem>(excludeItems);

                iterator.Process = delegate(ExcludeItem element)
                {
                    if (element.IsChecked)
                    {
                        var projectItem = element.ProjectItem;
                        projectItem.Project.Remove(projectItem);

                        logWindow.Write(projectItem.Path, Rocks.Resources.ExcludeWindow_Exclude_excluded, string.Empty);
                    }

                    logWindow.Increment();
                    iterator.Next();
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

        private void GetExcludeItems([NotNull] EnvDTE.ProjectItem item)
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

                var p = projectItem;
                if (excludeItems.Any(commitItem => commitItem.ProjectItem == p))
                {
                    continue;
                }

                excludeItems.Add(new ExcludeItem(projectItem));
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

            Exclude();
            this.Close(true);
        }

        private void SaveProjects()
        {
            var projects = new List<ProjectBase>();
            foreach (var commitItem in excludeItems)
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

        protected class ExcludeItem
        {
            public ExcludeItem([NotNull] ProjectItem projectItem)
            {
                Assert.ArgumentNotNull(projectItem, nameof(projectItem));

                ProjectItem = projectItem;

                Path = projectItem.Path;
                Extension = System.IO.Path.GetExtension(Path);

                IsChecked = true;
            }

            [NotNull]
            public string Extension { get; set; }

            public bool IsChecked { get; set; }

            [NotNull]
            public string Path { get; set; }

            [NotNull]
            public ProjectItem ProjectItem { get; set; }
        }
    }
}
