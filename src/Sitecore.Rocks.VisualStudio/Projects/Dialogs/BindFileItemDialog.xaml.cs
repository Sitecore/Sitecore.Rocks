// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ProjectExtensions;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.Projects.FileItems;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Projects.Dialogs
{
    public partial class BindFileItemDialog
    {
        public BindFileItemDialog()
        {
            InitializeComponent();
            this.InitializeDialog();
        }

        [NotNull]
        public Site Site { get; set; }

        [NotNull]
        protected List<EnvDTE.ProjectItem> Items { get; set; }

        public void EnableButtons()
        {
            var selectedItems = TreeView.SelectedItems;
            if (selectedItems.Count != 1)
            {
                OkButton.IsEnabled = false;
                return;
            }

            var selectedItem = selectedItems[0] as ItemTreeViewItem;
            if (selectedItem == null)
            {
                OkButton.IsEnabled = false;
                return;
            }

            OkButton.IsEnabled = true;
        }

        public void Initialize([NotNull] Site site, [NotNull] List<EnvDTE.ProjectItem> items)
        {
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(items, nameof(items));

            Site = site;
            Items = items;

            Loaded += ControlLoaded;
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            TreeView.SelectedItemsChanged += SelectedItemsChanged;

            EnableButtons();

            var web = new SiteTreeViewItem(Site);

            web.Items.Add(DummyTreeViewItem.Instance);
            TreeView.Items.Add(web);

            if (!ExpandTreeView())
            {
                web.IsExpanded = true;
            }
        }

        private bool ExpandTreeView()
        {
            var item = Items.FirstOrDefault();
            if (item == null)
            {
                return false;
            }

            var extension = Path.GetExtension(item.Name);
            if (string.IsNullOrEmpty(extension))
            {
                return false;
            }

            if (Items.Any(i => string.Compare(Path.GetExtension(i.Name), extension, StringComparison.InvariantCultureIgnoreCase) != 0))
            {
                return false;
            }

            var handler = FileItemManager.GetFileItemHandler(item.Name);
            if (handler == null)
            {
                return false;
            }

            var rootItemId = handler.GetRootItemId();
            if (rootItemId == ItemId.Empty)
            {
                return false;
            }

            var itemUri = new ItemUri(new DatabaseUri(Site, DatabaseName.Master), rootItemId);

            var treeViewItem = TreeView.ExpandTo(itemUri);
            if (treeViewItem != null)
            {
                treeViewItem.IsExpanded = true;
            }

            return true;
        }

        private void LinkItems()
        {
            var selectedItems = TreeView.SelectedItems;
            if (selectedItems.Count != 1)
            {
                return;
            }

            var selectedItem = selectedItems[0] as ItemTreeViewItem;
            if (selectedItem == null)
            {
                return;
            }

            var itemUri = selectedItem.ItemUri;

            var projects = new List<Project>();

            foreach (var item in Items)
            {
                var project = ProjectManager.GetProject(item);
                if (project == null)
                {
                    continue;
                }

                var projectItem = project.GetProjectItem(item);
                if (projectItem == null)
                {
                    var fileName = project.GetProjectItemFileName(item);
                    projectItem = ProjectFileItem.Load(project, fileName);

                    project.Add(projectItem);
                }

                var projectFile = projectItem as ProjectFileItem;
                if (projectFile == null)
                {
                    continue;
                }

                projectFile.Items.Add(itemUri);

                if (!projects.Contains(project))
                {
                    projects.Add(project);
                }
            }

            foreach (var project in projects)
            {
                project.Save();
            }
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            LinkItems();
            this.Close(true);
        }

        private void SelectedItemsChanged([NotNull] object sender, [NotNull] RoutedPropertyChangedEventArgs<object> routedPropertyChangedEventArgs)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(routedPropertyChangedEventArgs, nameof(routedPropertyChangedEventArgs));

            EnableButtons();
        }
    }
}
