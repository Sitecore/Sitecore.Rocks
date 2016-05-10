// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Applications.Storages;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ProjectExtensions;
using Sitecore.Rocks.Extensions.ProjectItemExtensions;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.Projects.FileItems;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Projects.Dialogs
{
    public partial class CreateItemWindow
    {
        public const string CreateItemsDialogRegistryKey = "CreateItemsDialog";

        private readonly ListViewSorter listViewSorter;

        private List<CreateItem> createItems;

        public CreateItemWindow()
        {
            InitializeComponent();
            this.InitializeDialog();

            listViewSorter = new ListViewSorter(ListView);

            Loaded += ControlLoaded;
        }

        [NotNull]
        public Site Site { get; private set; }

        public void Initialize([NotNull] Site site, [NotNull] List<EnvDTE.ProjectItem> items)
        {
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(items, nameof(items));

            Site = site;
            TreeView.SelectedItemsChanged += UpdateItemPaths;

            createItems = new List<CreateItem>();

            foreach (var item in items)
            {
                GetCreateItems(item);
            }

            ListView.ItemsSource = createItems;

            InitializeTreeView(site);
        }

        [NotNull]
        private ProjectItem Add([NotNull] CreateItem element)
        {
            Debug.ArgumentNotNull(element, nameof(element));

            var project = element.Project;
            var fileName = project.GetProjectItemFileName(element.Item);

            var result = ProjectFileItem.Load(project, fileName);

            project.Add(result);

            foreach (var item in element.Item.ProjectItems)
            {
                var subitem = item as EnvDTE.ProjectItem;
                if (subitem == null)
                {
                    continue;
                }

                fileName = project.GetProjectItemFileName(subitem);
                project.Add(ProjectFileItem.Load(project, fileName));
            }

            return result;
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            EnableButtons();
        }

        private void CreateItems()
        {
            var item = TreeView.SelectedItem as ItemTreeViewItem;
            if (item == null)
            {
                return;
            }

            var databaseName = item.Item.ItemUri.DatabaseName;

            var logWindow = new ProjectLogWindow
            {
                Maximum = createItems.Count
            };

            logWindow.AutoStart = delegate
            {
                var iterator = new ElementIterator<CreateItem>(createItems);

                iterator.Process = delegate(CreateItem element)
                {
                    if (!element.IsChecked)
                    {
                        logWindow.Increment();
                        iterator.Next();
                        return;
                    }

                    if (element.ProjectItem == null)
                    {
                        element.ProjectItem = Add(element);
                    }

                    var projectItem = element.ProjectItem;

                    ProcessedEventHandler callback = delegate(object sender, ProcessedEventArgs args)
                    {
                        WriteResult(logWindow, projectItem, args);
                        logWindow.Increment();
                        iterator.Next();
                    };

                    element.Handler.Handle(databaseName, projectItem, element.ItemPath, callback);
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

        private void EnableButtons()
        {
            OkButton.IsEnabled = TreeView.SelectedItem as ItemTreeViewItem != null;
        }

        private void GetCreateItems([NotNull] EnvDTE.ProjectItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var project = ProjectManager.GetProject(item);
            if (project == null)
            {
                return;
            }

            if (!IsFolder(item))
            {
                Process(project, item);
            }

            foreach (var subProjectItem in item.ProjectItems)
            {
                var subitem = subProjectItem as EnvDTE.ProjectItem;
                if (subitem != null)
                {
                    GetCreateItems(subitem);
                }
            }
        }

        private void HeaderClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            listViewSorter.HeaderClick(sender, e);
        }

        private void InitializeTreeView([NotNull] Site site)
        {
            Debug.ArgumentNotNull(site, nameof(site));

            var siteTreeViewItem = new SiteTreeViewItem(site);

            TreeView.Items.Add(siteTreeViewItem);

            var lastSelected = Storage.ReadString(CreateItemsDialogRegistryKey, "LastSelected", string.Empty);
            if (!string.IsNullOrEmpty(lastSelected))
            {
                ItemUri itemUri;
                if (ItemUri.TryParse(lastSelected, out itemUri))
                {
                    if (TreeView.ExpandTo(itemUri) != null)
                    {
                        return;
                    }
                }
            }

            siteTreeViewItem.IsExpanded = true;
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

            var item = TreeView.SelectedItem as ItemTreeViewItem;
            if (item != null)
            {
                Storage.Write(CreateItemsDialogRegistryKey, "LastSelected", item.Item.ItemUri.ToString());
            }

            CreateItems();

            this.Close(true);
        }

        private void Process([NotNull] Project project, [NotNull] EnvDTE.ProjectItem item)
        {
            Debug.ArgumentNotNull(project, nameof(project));
            Debug.ArgumentNotNull(item, nameof(item));

            var projectFile = project.GetProjectItem(item) as ProjectFileItem;
            if (projectFile != null)
            {
                var itemIds = projectFile.Items;
                if (itemIds.Count > 0)
                {
                    return;
                }
            }

            var fileName = item.GetFileName();

            var handler = FileItemManager.GetFileItemHandler(fileName);
            if (handler == null)
            {
                return;
            }

            if (projectFile != null)
            {
                if (createItems.Any(createItem => createItem.ProjectItem == projectFile))
                {
                    return;
                }
            }

            createItems.Add(new CreateItem(item, project, projectFile, handler));
        }

        private void SaveProjects()
        {
            var projects = new List<ProjectBase>();
            foreach (var commitItem in createItems)
            {
                var projectItem = commitItem.ProjectItem;
                if (projectItem == null)
                {
                    continue;
                }

                var project = projectItem.Project;
                if (projects.Contains(project))
                {
                    continue;
                }

                project.Save();
                projects.Add(project);
            }
        }

        private void UpdateItemPaths([NotNull] object sender, [NotNull] RoutedPropertyChangedEventArgs<object> e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedItem = TreeView.SelectedItem as ItemTreeViewItem;
            if (selectedItem == null)
            {
                return;
            }

            var rootPath = selectedItem.GetPath();

            foreach (var item in createItems)
            {
                item.ItemPath = rootPath + "/" + Path.GetFileNameWithoutExtension(item.Path);
            }

            ListView.ItemsSource = null;
            ListView.ItemsSource = createItems;
            ItemPathColumn.Width = 0;
            ItemPathColumn.Width = double.NaN;

            EnableButtons();
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

        protected class CreateItem
        {
            public CreateItem([NotNull] EnvDTE.ProjectItem item, [NotNull] Project project, [CanBeNull] ProjectItem projectItem, [NotNull] IFileItemHandler handler)
            {
                Assert.ArgumentNotNull(item, nameof(item));
                Assert.ArgumentNotNull(project, nameof(project));
                Assert.ArgumentNotNull(handler, nameof(handler));

                Handler = handler;
                ProjectItem = projectItem;
                Path = project.GetRelativeFileName(item.GetFileName());
                Extension = System.IO.Path.GetExtension(Path);
                IsChecked = true;
                Project = project;
                Item = item;
                TemplateName = handler.GetTemplateName();
                ItemPath = System.IO.Path.GetFileNameWithoutExtension(Path);

                if (projectItem != null)
                {
                    Status = Rocks.Resources.CreateItem_CreateItem_create;
                }
                else
                {
                    Status = Rocks.Resources.CreateItem_CreateItem_add_and_create;
                }
            }

            [NotNull]
            public string Extension { get; private set; }

            [NotNull]
            public IFileItemHandler Handler { get; }

            public bool IsChecked { get; set; }

            [NotNull]
            public EnvDTE.ProjectItem Item { get; }

            [NotNull]
            public string ItemPath { get; set; }

            [NotNull]
            public string Path { get; }

            [NotNull]
            public Project Project { get; }

            [CanBeNull]
            public ProjectItem ProjectItem { get; set; }

            [NotNull]
            public string Status { get; set; }

            [NotNull]
            public string TemplateName { get; private set; }
        }
    }
}
