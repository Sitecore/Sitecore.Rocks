// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.UI.Management.ManagementItems.Validations.Skins.ActionSkins;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Skins.TreeViewSkins
{
    [ValidationViewerSkin("Tree View", 2000)]
    public partial class ValidationViewerTreeView : IValidationViewerSkin
    {
        private readonly List<TreeViewItem> entries = new List<TreeViewItem>();

        private IEnumerable<ValidationDescriptor> items;

        public ValidationViewerTreeView()
        {
            InitializeComponent();

            Group = AppHost.Settings.GetString("Management\\Validation", "TreeViewGroup", "ct");

            switch (Group)
            {
                case "ct":
                    GroupCT.IsSelected = true;
                    break;
                case "sct":
                    GroupSCT.IsSelected = true;
                    break;
                case "cst":
                    GroupCST.IsSelected = true;
                    break;
                case "st":
                    GroupST.IsSelected = true;
                    break;
                case "t":
                    GroupT.IsSelected = true;
                    break;
                case "p":
                    GroupP.IsSelected = true;
                    break;
            }
        }

        public IValidationViewer ValidationViewer { get; set; }

        [NotNull]
        protected string Group { get; set; }

        public Control GetControl()
        {
            return this;
        }

        public void Hide(ValidationDescriptor item)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            ValidationViewer.Hide(item);

            var treeViewItem = entries.FirstOrDefault(e => e.Tag == item);
            if (treeViewItem == null)
            {
                return;
            }

            var parent = treeViewItem.GetAncestor<TreeViewItem>();
            if (parent != null)
            {
                parent.Items.Remove(treeViewItem);
            }

            treeViewItem = parent;
            while (treeViewItem != null)
            {
                parent = treeViewItem.GetAncestor<TreeViewItem>();

                if (treeViewItem.Items.Count == 0)
                {
                    if (parent != null)
                    {
                        parent.Items.Remove(treeViewItem);
                    }
                }
                else
                {
                    var header = treeViewItem.Header as TreeViewHeader;
                    if (header != null)
                    {
                        header.Count--;
                    }
                }

                treeViewItem = parent;
            }
        }

        public void RenderValidations(IEnumerable<ValidationDescriptor> validations)
        {
            Assert.ArgumentNotNull(validations, nameof(validations));

            items = validations;

            TreeView.Items.Clear();
            entries.Clear();

            var group = Group.ToCharArray();

            foreach (var item in validations)
            {
                var itemsCollection = TreeView.Items;

                foreach (var g in group)
                {
                    switch (g)
                    {
                        case 'c':
                            itemsCollection = GetTreeViewItem(itemsCollection, item.Category);
                            break;
                        case 't':
                            itemsCollection = GetTreeViewItem(itemsCollection, item.Title);
                            break;
                        case 's':
                            itemsCollection = GetTreeViewItem(itemsCollection, item.Severity.ToString());
                            break;
                        case 'p':
                            var path = item.ItemPath;
                            if (!string.IsNullOrEmpty(path))
                            {
                                foreach (var s in path.Split('/', StringSplitOptions.RemoveEmptyEntries))
                                {
                                    itemsCollection = GetTreeViewItem(itemsCollection, s);
                                }
                            }

                            break;
                    }
                }

                var treeViewItem = new TreeViewItem
                {
                    Header = new TreeViewHeader(item.Severity, item.Problem),
                    Tag = item
                };

                entries.Add(treeViewItem);
                itemsCollection.Add(treeViewItem);
            }

            foreach (var item in TreeView.Items)
            {
                var treeViewItem = item as TreeViewItem;
                if (treeViewItem == null)
                {
                    continue;
                }

                CountLeaves(treeViewItem);
            }
        }

        private int CountLeaves([NotNull] TreeViewItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            if (item.Items.Count == 0)
            {
                return 1;
            }

            var count = 0;

            foreach (var subitem in item.Items)
            {
                var treeViewItem = subitem as TreeViewItem;
                if (treeViewItem == null)
                {
                    continue;
                }

                var leaves = CountLeaves(treeViewItem);

                count += leaves;
            }

            if (count > 1)
            {
                var header = item.Header as TreeViewHeader;
                if (header != null)
                {
                    header.Count = count;
                }
            }

            return count;
        }

        [NotNull]
        private ItemCollection GetTreeViewItem([NotNull] ItemCollection itemsCollection, [NotNull] string header)
        {
            Debug.ArgumentNotNull(itemsCollection, nameof(itemsCollection));
            Debug.ArgumentNotNull(header, nameof(header));

            foreach (var item in itemsCollection)
            {
                var treeViewItem = item as TreeViewItem;
                if (treeViewItem == null)
                {
                    continue;
                }

                var h = treeViewItem.Header as TreeViewHeader;
                if (h == null)
                {
                    continue;
                }

                if (h.Text == header)
                {
                    return treeViewItem.Items;
                }
            }

            var treeViewHeader = new TreeViewHeader(SeverityLevel.None, header);

            var result = new TreeViewItem
            {
                Header = treeViewHeader
            };

            itemsCollection.Add(result);

            return result.Items;
        }

        private void SetGroup([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var comboBoxItem = Grouping.SelectedItem as ComboBoxItem;
            if (comboBoxItem == null)
            {
                return;
            }

            if (items == null)
            {
                return;
            }

            Group = (string)comboBoxItem.Tag;
            RenderValidations(items);

            AppHost.Settings.Set("Management\\Validation", "TreeViewGroup", Group);
        }

        private void SetSelectedItem([NotNull] object sender, [NotNull] RoutedPropertyChangedEventArgs<object> e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedItem = TreeView.SelectedItem as TreeViewItem;
            if (selectedItem == null)
            {
                return;
            }

            var item = selectedItem.Tag as ValidationDescriptor;
            if (item == null)
            {
                Details.BorderThickness = new Thickness(1);
                Details.Child = null;
                return;
            }

            var control = new ValidationPresenter(this, item);

            Details.BorderThickness = new Thickness(0);
            Details.Child = control;
        }
    }
}
