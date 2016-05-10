// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.ContentTrees.Pipelines.DefaultActions;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ContextMenuExtensions;
using Sitecore.Rocks.Extensions.DataServiceExtensions;
using Sitecore.Rocks.Extensions.TreeViewItemExtensions;
using Sitecore.Rocks.Extensions.UserControlExtensions;

namespace Sitecore.Rocks.UI.Links
{
    public partial class LinkTab : IContextProvider
    {
        private Point dragOrigin;

        private string filterText;

        public LinkTab()
        {
            InitializeComponent();

            filterText = string.Empty;

            Notifications.RegisterItemEvents(this, deleted: ItemDeleted, renamed: ItemRenamed);
        }

        [NotNull]
        public ItemUri ItemUri { get; set; }

        [NotNull]
        public LinkViewer LinkViewer { get; set; }

        [CanBeNull]
        public List<ItemHeader> References { get; set; }

        [CanBeNull]
        public List<ItemHeader> Referrers { get; set; }

        [NotNull]
        public TabItem TabItem { get; set; }

        [NotNull]
        public object GetContext()
        {
            var result = new LinksContext(this);

            var selectedItem = TreeView.SelectedItem as TreeViewItem;
            if (selectedItem != null)
            {
                result.SelectedItem = selectedItem.Tag as ItemHeader;
            }

            return result;
        }

        public void Initialize([NotNull] ItemUri itemUri)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            ItemUri = itemUri;

            Refresh();
        }

        private void FilterNodesChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            filterText = Filter.Text;

            TreeView.FilterTreeViewItems(filterText, GetHeaderText);
        }

        [NotNull]
        private string GetGroupBy()
        {
            var group = string.Empty;

            var selectedItem = GroupBy.SelectedItem as ComboBoxItem;
            if (selectedItem != null)
            {
                group = selectedItem.Tag as string ?? string.Empty;
            }

            if (string.IsNullOrEmpty(group))
            {
                group = @"k";
            }

            return group;
        }

        [NotNull]
        private string GetHeaderText([NotNull] TreeViewItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var itemHeader = item.Header as ItemHeaderTreeViewItemHeader;
            if (itemHeader != null)
            {
                return itemHeader.ItemName.Text + '|' + itemHeader.ItemPath.Text + '|' + itemHeader.TemplateName.Text;
            }

            return item.Header as string ?? string.Empty;
        }

        [NotNull]
        private TreeViewItem GetNode([NotNull] TreeViewPathItem pathItem)
        {
            Debug.ArgumentNotNull(pathItem, nameof(pathItem));

            return new TreeViewItem
            {
                Header = pathItem.Text,
                Tag = pathItem.Tag,
                IsExpanded = true
            };
        }

        [NotNull]
        private IEnumerable<TreeViewPathItem> GetPath([NotNull] string groupBy, [NotNull] ItemHeader item, [NotNull] string kind)
        {
            Debug.ArgumentNotNull(groupBy, nameof(groupBy));
            Debug.ArgumentNotNull(item, nameof(item));
            Debug.ArgumentNotNull(kind, nameof(kind));

            var result = new List<TreeViewPathItem>();

            foreach (var c in groupBy.ToCharArray())
            {
                switch (c)
                {
                    case 'k':
                        result.Add(new TreeViewPathItem
                        {
                            Tag = kind,
                            Text = kind
                        });
                        break;
                    case 'c':
                        result.Add(new TreeViewPathItem
                        {
                            Tag = item.Category,
                            Text = item.Category
                        });
                        break;
                    case 'n':
                        result.Add(new TreeViewPathItem
                        {
                            Tag = item.Name,
                            Text = item.Name
                        });
                        break;
                    case 't':
                        result.Add(new TreeViewPathItem
                        {
                            Tag = item.TemplateName,
                            Text = item.TemplateName
                        });
                        break;
                    case 'p':
                        result.Add(new TreeViewPathItem
                        {
                            Tag = item.Path,
                            Text = item.Path
                        });
                        break;
                }
            }

            return result;
        }

        private void GroupByChanged([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            RenderItems();
        }

        private void HandleDoubleClick([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedItem = TreeView.SelectedItem as TreeViewItem;
            if (selectedItem == null)
            {
                return;
            }

            var itemHeader = selectedItem.Tag as ItemHeader;
            if (itemHeader == null)
            {
                return;
            }

            DefaultActionPipeline.Run().WithParameters(new ItemSelectionContext(new TemplatedItemDescriptor(itemHeader.ItemUri, string.Empty, itemHeader.TemplateId, itemHeader.TemplateName)));
        }

        private void HandleMouseLeftButtonDown([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            DragManager.HandleMouseDown(this, e, out dragOrigin);
        }

        private void HandleMouseMove([NotNull] object sender, [NotNull] MouseEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (!DragManager.IsDragStart(this, e, ref dragOrigin))
            {
                return;
            }

            var selectedItem = TreeView.SelectedItem as TreeViewItem;
            if (selectedItem == null)
            {
                return;
            }

            var itemHeader = selectedItem.Tag as ItemHeader;
            if (itemHeader == null)
            {
                return;
            }

            var list = new List<ItemHeader>
            {
                itemHeader
            };

            var dragData = DragManager.SetData(list);

            DragManager.DoDragDrop(this, dragData, DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link);

            e.Handled = true;
        }

        private void ItemDeleted([NotNull] object sender, [NotNull] ItemUri itemuri)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(itemuri, nameof(itemuri));

            var changed = RemoveTreeViewItems(References, itemuri);
            changed |= RemoveTreeViewItems(Referrers, itemuri);

            if (changed)
            {
                RenderItems();
            }
        }

        private void ItemRenamed([NotNull] object sender, [NotNull] ItemUri itemuri, [NotNull] string newname)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(itemuri, nameof(itemuri));
            Debug.ArgumentNotNull(newname, nameof(newname));

            var changed = RenameTreeViewItems(References, itemuri, newname);
            changed |= RenameTreeViewItems(Referrers, itemuri, newname);

            if (changed)
            {
                RenderItems();
            }
        }

        private void LoadItems([NotNull] string itemName, [NotNull] IEnumerable<ItemHeader> references, [NotNull] IEnumerable<ItemHeader> referrers)
        {
            Debug.ArgumentNotNull(itemName, nameof(itemName));
            Debug.ArgumentNotNull(references, nameof(references));
            Debug.ArgumentNotNull(referrers, nameof(referrers));

            var header = new TabItemHeader
            {
                Header = itemName,
                Tag = this
            };
            header.Click += delegate { LinkViewer.CloseTab(TabItem); };

            TabItem.Header = header;

            References = new List<ItemHeader>(references);
            Referrers = new List<ItemHeader>(referrers);

            RenderItems();
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ContextMenu = null;

            var context = GetContext();

            var commands = Rocks.Commands.CommandManager.GetCommands(context);
            if (!commands.Any())
            {
                e.Handled = true;
                return;
            }

            var contextMenu = new ContextMenu();

            contextMenu.Build(commands, context);

            ContextMenu = contextMenu;
        }

        private void PreselectItem([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var treeViewItem = TreeViewItemExtensions.VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            if (treeViewItem != null)
            {
                treeViewItem.IsSelected = true;
            }
        }

        private void Refresh()
        {
            TreeView.Items.Clear();
            ItemUri.Site.DataService.GetLinks(ItemUri, LoadItems);
        }

        private void RefreshClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Refresh();
        }

        private bool RemoveTreeViewItems([CanBeNull] List<ItemHeader> list, [NotNull] ItemUri itemuri)
        {
            Debug.ArgumentNotNull(itemuri, nameof(itemuri));

            if (list == null)
            {
                return false;
            }

            var result = false;

            for (var index = list.Count - 1; index >= 0; index--)
            {
                var itemHeader = list[index];
                if (itemHeader.ItemUri != itemuri)
                {
                    continue;
                }

                list.Remove(itemHeader);
                result = true;
            }

            return result;
        }

        private bool RenameTreeViewItems([CanBeNull] List<ItemHeader> list, [NotNull] ItemUri itemuri, [NotNull] string newname)
        {
            Debug.ArgumentNotNull(itemuri, nameof(itemuri));
            Debug.ArgumentNotNull(newname, nameof(newname));

            if (list == null)
            {
                return false;
            }

            var result = false;
            for (var index = list.Count - 1; index >= 0; index--)
            {
                var itemHeader = list[index];
                if (itemHeader.ItemUri != itemuri)
                {
                    continue;
                }

                itemHeader.Name = newname;
                result = true;
            }

            return result;
        }

        private void RenderItem([NotNull] ItemHeader itemHeader, [NotNull] string groupBy, [NotNull] string kind)
        {
            Debug.ArgumentNotNull(itemHeader, nameof(itemHeader));
            Debug.ArgumentNotNull(groupBy, nameof(groupBy));
            Debug.ArgumentNotNull(kind, nameof(kind));

            var path = GetPath(groupBy, itemHeader, kind);

            var collection = TreeView.FindPath(path, GetNode);
            if (collection == null)
            {
                return;
            }

            var linkHeader = new ItemHeaderTreeViewItemHeader();
            linkHeader.Initialize(itemHeader);

            var treeViewItem = new TreeViewItem
            {
                Header = linkHeader,
                IsExpanded = true,
                Tag = itemHeader
            };

            collection.Add(treeViewItem);
        }

        private void RenderItems()
        {
            if (TreeView == null)
            {
                return;
            }

            TreeView.Items.Clear();

            var groupBy = GetGroupBy();

            if (References != null)
            {
                foreach (var itemHeader in References)
                {
                    RenderItem(itemHeader, groupBy, @"References");
                }
            }

            if (Referrers != null)
            {
                foreach (var itemHeader in Referrers)
                {
                    RenderItem(itemHeader, groupBy, @"Referrers");
                }
            }

            TreeView.FilterTreeViewItems(filterText, GetHeaderText);
        }

        private void ToolBarLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.InitializeToolBar(sender);
        }
    }
}
