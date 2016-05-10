// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items.SelectedObjects;
using Sitecore.Rocks.ContentTrees.Pipelines.GetChildren;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.Sites.Connections;

namespace Sitecore.Rocks.ContentTrees.Items
{
    public class LocalConnectionFolderTreeViewItem : BaseTreeViewItem, IConnectionFolder, ICanRefresh, ICanDrop, ISelectable
    {
        public LocalConnectionFolderTreeViewItem([NotNull] string folder)
        {
            Assert.ArgumentNotNull(folder, nameof(folder));

            Folder = folder;
            ToolTip = folder;
            Icon = new Icon("Resources/16x16/folderclosed.png");
            Text = "<Local IIS Sites>";

            RelativeFolder = IO.File.GetRelativePath(folder, ConnectionManager.GetConnectionFolder());
            if (string.IsNullOrEmpty(RelativeFolder))
            {
                RelativeFolder = ConnectionFolderTreeViewItem.ConnectionsRelativeFolder;
            }

            Loaded += ControlLoaded;
            Expanded += SetExpanderState;
            Collapsed += SetExpanderState;
        }

        [NotNull]
        public string Folder { get; }

        [NotNull]
        protected string RelativeFolder { get; set; }

        public override bool GetChildren(GetChildrenDelegate callback, bool async)
        {
            Assert.ArgumentNotNull(callback, nameof(callback));

            callback(Enumerable.Empty<BaseTreeViewItem>());
            return true;
        }

        public override void Refresh()
        {
            SiteManager.RefreshLocalSites();
            base.Refresh();
        }

        protected override void GetVirtualChildren(ICollection<BaseTreeViewItem> items)
        {
            Debug.ArgumentNotNull(items, nameof(items));

            GetChildrenPipeline.Run().WithParameters(items, this, Folder);
        }

        protected void MoveFolders([NotNull] ItemTreeView itemTreeView, [NotNull] IEnumerable<ConnectionFolderTreeViewItem> items, [NotNull] string folder)
        {
            Debug.ArgumentNotNull(itemTreeView, nameof(itemTreeView));
            Debug.ArgumentNotNull(items, nameof(items));
            Debug.ArgumentNotNull(folder, nameof(folder));

            foreach (var item in items.ToList())
            {
                var oldFolder = item.Folder;
                var newFolder = Path.Combine(folder, Path.GetFileName(oldFolder) ?? string.Empty);

                if (string.Compare(oldFolder, newFolder, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    continue;
                }

                Directory.Move(oldFolder, newFolder);
                ConnectionManager.MoveFolder(oldFolder, newFolder);

                var parent = item.GetAncestor<ItemsControl>();
                if (parent != null)
                {
                    parent.Items.Remove(item);
                }
                else
                {
                    itemTreeView.TreeView.Items.Remove(item);
                }
            }
        }

        protected void MoveSites([NotNull] IEnumerable<SiteTreeViewItem> sites, [NotNull] string folder)
        {
            Debug.ArgumentNotNull(sites, nameof(sites));
            Debug.ArgumentNotNull(folder, nameof(folder));

            foreach (var site in sites.ToList())
            {
                var oldFileName = site.Site.Connection.FileName;
                var newFileName = Path.Combine(folder, Path.GetFileName(oldFileName) ?? string.Empty);

                if (string.Compare(oldFileName, newFileName, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    continue;
                }

                if (AppHost.Files.FileExists(newFileName))
                {
                    AppHost.Files.Delete(newFileName);
                }

                File.Move(site.Site.Connection.FileName, newFileName);
                site.Site.Connection.FileName = newFileName;

                var parent = site.GetAncestor<ItemsControl>();
                if (parent != null)
                {
                    parent.Items.Remove(site);
                }
            }
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            IsExpanded = GetExpanderState();
        }

        private bool GetExpanderState()
        {
            return AppHost.Settings.GetBool(ConnectionFolderTreeViewItem.RegistryPath, RelativeFolder, false);
        }

        object ISelectable.GetSelectedObject()
        {
            return new FolderSelectedObject(Folder);
        }

        void ICanDrop.HandleDragOver(object sender, DragEventArgs e, BaseTreeViewItem treeViewItem)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));
            Debug.ArgumentNotNull(treeViewItem, nameof(treeViewItem));

            if (!e.Data.GetDataPresent(SiteTreeViewItem.DragIdentifier))
            {
                return;
            }

            var data = e.Data.GetData(SiteTreeViewItem.DragIdentifier) as IEnumerable<BaseTreeViewItem>;
            if (data == null)
            {
                return;
            }

            var items = data.ToList();
            if (items.Any(i => !(i is SiteTreeViewItem)))
            {
                return;
            }

            if (items.Any(IsAncestor))
            {
                return;
            }

            if (items.Any(i => i == this))
            {
                return;
            }

            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }

        void ICanDrop.HandleDrop(object sender, DragEventArgs e, BaseTreeViewItem treeViewItem)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));
            Debug.ArgumentNotNull(treeViewItem, nameof(treeViewItem));

            if (!e.Data.GetDataPresent(SiteTreeViewItem.DragIdentifier))
            {
                return;
            }

            var items = e.Data.GetData(SiteTreeViewItem.DragIdentifier) as IEnumerable<BaseTreeViewItem>;
            if (items == null)
            {
                return;
            }

            var itemTreeView = this.GetAncestorOrSelf<ItemTreeView>();
            if (itemTreeView == null)
            {
                return;
            }

            MoveFolders(itemTreeView, items.OfType<ConnectionFolderTreeViewItem>(), Folder);
            MoveSites(items.OfType<SiteTreeViewItem>(), Folder);

            if (!IsExpanded)
            {
                IsExpanded = true;
            }

            Refresh();

            e.Handled = true;
        }

        private bool IsAncestor([NotNull] BaseTreeViewItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var i = Parent as BaseTreeViewItem;

            while (i != null)
            {
                if (i == item)
                {
                    return true;
                }

                i = i.Parent as BaseTreeViewItem;
            }

            return false;
        }

        private void SetExpanderState([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AppHost.Settings.SetBool(ConnectionFolderTreeViewItem.RegistryPath, RelativeFolder, IsExpanded);
        }
    }
}
