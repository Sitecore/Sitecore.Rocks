// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ProjectExtensions;
using Sitecore.Rocks.Extensions.ProjectItemExtensions;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.Projects.Dialogs
{
    public partial class AddWindow
    {
        private readonly ListViewSorter listViewSorter;

        private List<AddItem> addItems = new List<AddItem>();

        public AddWindow()
        {
            InitializeComponent();
            this.InitializeDialog();

            listViewSorter = new ListViewSorter(ListView);
        }

        public void Initialize([NotNull] List<EnvDTE.ProjectItem> items)
        {
            Assert.ArgumentNotNull(items, nameof(items));

            addItems = new List<AddItem>();

            foreach (var item in items)
            {
                GetAddItems(item);
            }

            ListView.ItemsSource = addItems;
        }

        private void Add()
        {
            var logWindow = new ProjectLogWindow
            {
                Maximum = addItems.Count
            };

            logWindow.AutoStart = delegate
            {
                var iterator = new ElementIterator<AddItem>(addItems);

                iterator.Process = delegate(AddItem element)
                {
                    logWindow.Increment();
                    if (!element.IsChecked)
                    {
                        iterator.Next();
                        return;
                    }

                    var project = element.Project;

                    var fileName = project.GetProjectItemFileName(element.Item);
                    var projectItem = ProjectFileItem.Load(project, fileName);
                    project.Add(projectItem);

                    logWindow.Write(projectItem.Path, Rocks.Resources.AddWindow_Add_added, string.Empty);

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

        private void GetAddItems([NotNull] EnvDTE.ProjectItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var project = ProjectManager.GetProject(item);
            if (project == null)
            {
                return;
            }

            if (!IsFolder(item))
            {
                if (!project.Contains(item))
                {
                    addItems.Add(new AddItem(project, item));
                }
            }

            foreach (var subProjectItem in item.ProjectItems)
            {
                var subitem = subProjectItem as EnvDTE.ProjectItem;
                if (subitem != null)
                {
                    GetAddItems(subitem);
                }
            }
        }

        private void HeaderClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            listViewSorter.HeaderClick(sender, e);
        }

        private bool IsFolder([NotNull] EnvDTE.ProjectItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            return item.GetFileName().EndsWith(@"\");
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Add();
            this.Close(true);
        }

        private void SaveProjects()
        {
            var projects = new List<Project>();
            foreach (var addItem in addItems)
            {
                var project = addItem.Project;
                if (projects.Contains(project))
                {
                    continue;
                }

                project.Save();
                projects.Add(project);
            }
        }

        protected class AddItem
        {
            public AddItem([NotNull] Project project, [NotNull] EnvDTE.ProjectItem item)
            {
                Assert.ArgumentNotNull(project, nameof(project));
                Assert.ArgumentNotNull(item, nameof(item));

                Project = project;
                Item = item;
                Path = Project.GetProjectItemFileName(item);
                Extension = System.IO.Path.GetExtension(Path);

                IsChecked = true;
            }

            [NotNull]
            public string Extension { get; private set; }

            public bool IsChecked { get; set; }

            [NotNull]
            public EnvDTE.ProjectItem Item { get; }

            [NotNull]
            public string Path { get; }

            [NotNull]
            public Project Project { get; }
        }
    }
}
