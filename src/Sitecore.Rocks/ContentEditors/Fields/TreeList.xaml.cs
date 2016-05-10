// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentEditors.Commands.Fields.TreeListFields;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;
using Sitecore.Rocks.Extensions.ListBoxExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Text;

namespace Sitecore.Rocks.ContentEditors.Fields
{
    [FieldControl("tree list"), FieldControl("treelist"), FieldControl("treelistex")]
    public partial class TreeList : IReusableFieldControl
    {
        private static readonly char[] Pipe =
        {
            '|'
        };

        private string excludeItemsForDisplayList;

        private string excludeTemplatesForDisplayList;

        private string excludeTemplatesForSelectionList;

        private string includeItemsForDisplayList;

        private string includeTemplatesForDisplayList;

        private string includeTemplatesForSelectionList;

        private List<ValueItem> initialItems;

        public TreeList()
        {
            InitializeComponent();

            RightImage.Source = new Icon("Resources/16x16/chevron-right.png").GetSource();
            LeftImage.Source = new Icon("Resources/16x16/chevron-left.png").GetSource();
            UpImage.Source = new Icon("Resources/16x16/chevron-up.png").GetSource();
            DownImage.Source = new Icon("Resources/16x16/chevron-down.png").GetSource();

            ExcludeItemsForDisplay = string.Empty;
            ExcludeTemplatesForDisplay = string.Empty;
            ExcludeTemplatesForSelection = string.Empty;
            IncludeItemsForDisplay = string.Empty;
            IncludeTemplatesForDisplay = string.Empty;
            IncludeTemplatesForSelection = string.Empty;
        }

        public bool DisableExclusions { get; set; }

        [NotNull]
        public string ExcludeItemsForDisplay { get; private set; }

        [NotNull]
        public string ExcludeTemplatesForDisplay { get; private set; }

        [NotNull]
        public string ExcludeTemplatesForSelection { get; private set; }

        [NotNull]
        public string IncludeItemsForDisplay { get; private set; }

        [NotNull]
        public string IncludeTemplatesForDisplay { get; private set; }

        [NotNull]
        public string IncludeTemplatesForSelection { get; private set; }

        [NotNull]
        protected DatabaseUri DatabaseUri { get; private set; }

        [NotNull]
        protected string FieldSource { get; set; }

        public Control GetControl()
        {
            return this;
        }

        public Control GetFocusableControl()
        {
            return Source;
        }

        public string GetValue()
        {
            var result = new StringBuilder();
            var first = true;

            foreach (ListBoxItem selectedItem in Target.Items)
            {
                var valueItem = selectedItem.Tag as ValueItem;
                if (valueItem == null)
                {
                    continue;
                }

                if (!first)
                {
                    result.Append('|');
                }

                result.Append(valueItem.Value);

                first = false;
            }

            return result.ToString();
        }

        public bool IsSupported(Field sourceField)
        {
            Assert.ArgumentNotNull(sourceField, nameof(sourceField));

            var uri = sourceField.FieldUris.FirstOrDefault();
            if (uri == null)
            {
                return false;
            }

            return (uri.Site.DataService.Capabilities & DataServiceCapabilities.GetItemFieldsValueList) == DataServiceCapabilities.GetItemFieldsValueList;
        }

        public void SetField(Field sourceField)
        {
            Assert.ArgumentNotNull(sourceField, nameof(sourceField));

            Source.Items.Clear();

            Resizer.FieldId = sourceField.FieldUris.First().FieldId;

            DatabaseUri = sourceField.FieldUris.First().ItemVersionUri.DatabaseUri;
            FieldSource = sourceField.Source;
            initialItems = sourceField.ValueItems;

            var fieldSource = new UrlString(FieldSource ?? string.Empty);
            ExcludeItemsForDisplay = fieldSource["excludeitemsfordisplay"] ?? string.Empty;
            ExcludeTemplatesForDisplay = fieldSource["excludetemplatesfordisplay"] ?? string.Empty;
            ExcludeTemplatesForSelection = fieldSource["excludetemplatesforselection"] ?? string.Empty;
            IncludeItemsForDisplay = fieldSource["includeitemsfordisplay"] ?? string.Empty;
            IncludeTemplatesForDisplay = fieldSource["includetemplatesfordisplay"] ?? string.Empty;
            IncludeTemplatesForSelection = fieldSource["includetemplatesforselection"] ?? string.Empty;

            excludeItemsForDisplayList = ',' + ExcludeItemsForDisplay.ToLowerInvariant() + ",";
            excludeTemplatesForDisplayList = ',' + ExcludeTemplatesForDisplay.ToLowerInvariant() + ",";
            excludeTemplatesForSelectionList = ',' + ExcludeTemplatesForSelection.ToLowerInvariant() + ",";
            includeItemsForDisplayList = ',' + IncludeItemsForDisplay.ToLowerInvariant() + ",";
            includeTemplatesForDisplayList = ',' + IncludeTemplatesForDisplay.ToLowerInvariant() + ",";
            includeTemplatesForSelectionList = ',' + IncludeTemplatesForSelection.ToLowerInvariant() + ",";

            if (sourceField.Root == null)
            {
                return;
            }

            var itemTreeViewItem = new ItemTreeViewItem(sourceField.Root);

            Source.Items.Add(itemTreeViewItem);

            if (sourceField.Root.HasChildren)
            {
                itemTreeViewItem.MakeExpandable();
            }
        }

        public void SetValue(string value)
        {
            Assert.ArgumentNotNull(value, nameof(value));

            List<ValueItem> items;
            var valueModifiedEventHandler = ValueModified;

            if (initialItems != null)
            {
                items = initialItems;
                initialItems = null;
            }
            else
            {
                items = new List<ValueItem>();

                var selected = value.Split(Pipe, StringSplitOptions.RemoveEmptyEntries);

                foreach (var id in selected)
                {
                    ValueItem valueItem;

                    Guid guid;
                    if (!Guid.TryParse(id, out guid))
                    {
                        continue;
                    }

                    var itemTreeViewItem = Source.FindItem<ItemTreeViewItem>(new ItemId(guid));
                    if (itemTreeViewItem != null)
                    {
                        valueItem = new ValueItem(itemTreeViewItem.Text, id);
                    }
                    else
                    {
                        valueItem = new ValueItem(@"Item: " + id, id);
                    }

                    items.Add(valueItem);
                }

                if (valueModifiedEventHandler != null)
                {
                    valueModifiedEventHandler();
                }
            }

            Target.Items.Clear();

            foreach (var item in items)
            {
                Target.Items.Add(GetListBoxItem(item));
            }
        }

        public void UnsetField()
        {
            initialItems = null;
            DisableExclusions = false;
        }

        public event ValueModifiedEventHandler ValueModified;

        private void AddItem([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Handled = true;
            AddItem();
        }

        private void AddItem()
        {
            var modified = false;

            var list = Source.SelectedItems;
            var first = true;

            foreach (var baseItem in list)
            {
                var item = baseItem as ItemTreeViewItem;
                if (item == null)
                {
                    continue;
                }

                if (!DisableExclusions && !string.IsNullOrEmpty(IncludeTemplatesForSelection))
                {
                    if (includeTemplatesForSelectionList.IndexOf(',' + item.TemplateName + ',', StringComparison.InvariantCultureIgnoreCase) < 0 && includeTemplatesForSelectionList.IndexOf(item.TemplateId.ToString(), StringComparison.InvariantCultureIgnoreCase) < 0)
                    {
                        continue;
                    }
                }

                if (!DisableExclusions && !string.IsNullOrEmpty(ExcludeTemplatesForSelection))
                {
                    if (excludeTemplatesForSelectionList.IndexOf(',' + item.TemplateName + ',', StringComparison.InvariantCultureIgnoreCase) >= 0 || excludeTemplatesForSelectionList.IndexOf(item.TemplateId.ToString(), StringComparison.InvariantCultureIgnoreCase) >= 0)
                    {
                        continue;
                    }
                }

                if (first)
                {
                    Target.SelectedItem = null;
                }

                first = false;

                var listBoxItem = GetListBoxItem(item);

                Target.Items.Add(listBoxItem);
                listBoxItem.IsSelected = true;

                modified = true;
            }

            if (modified)
            {
                RaiseModified();
            }
        }

        private bool CanDisplay([NotNull] ItemHeader item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var result = true;

            var itemName = "," + item.Name + ",";
            var templateName = "," + item.TemplateName + ",";

            if (!string.IsNullOrEmpty(IncludeTemplatesForDisplay))
            {
                result &= includeTemplatesForDisplayList.IndexOf(templateName, StringComparison.InvariantCultureIgnoreCase) >= 0 || includeTemplatesForDisplayList.IndexOf(item.TemplateId.ToString(), StringComparison.InvariantCultureIgnoreCase) >= 0;
            }

            if (!string.IsNullOrEmpty(ExcludeTemplatesForDisplay))
            {
                result &= !(excludeTemplatesForDisplayList.IndexOf(templateName, StringComparison.InvariantCultureIgnoreCase) >= 0 || excludeTemplatesForDisplayList.IndexOf(item.TemplateId.ToString(), StringComparison.InvariantCultureIgnoreCase) >= 0);
            }

            if (!string.IsNullOrEmpty(IncludeItemsForDisplay))
            {
                result &= includeItemsForDisplayList.IndexOf(itemName, StringComparison.InvariantCultureIgnoreCase) >= 0 || includeItemsForDisplayList.IndexOf(item.ItemId.ToString(), StringComparison.InvariantCultureIgnoreCase) >= 0;
            }

            if (!string.IsNullOrEmpty(ExcludeItemsForDisplay))
            {
                result &= !(excludeItemsForDisplayList.IndexOf(itemName, StringComparison.InvariantCultureIgnoreCase) >= 0 || excludeItemsForDisplayList.IndexOf(item.ItemId.ToString(), StringComparison.InvariantCultureIgnoreCase) >= 0);
            }

            return result;
        }

        private void FilterChildren([NotNull] object sender, [NotNull] BaseTreeViewItem baseTreeviewItem, [NotNull] List<BaseTreeViewItem> children)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(baseTreeviewItem, nameof(baseTreeviewItem));
            Debug.ArgumentNotNull(children, nameof(children));

            if (DisableExclusions)
            {
                return;
            }

            for (var index = children.Count - 1; index >= 0; index--)
            {
                var child = children[index] as ItemTreeViewItem;
                if (child == null)
                {
                    continue;
                }

                if (!CanDisplay(child.Item))
                {
                    children.Remove(child);
                }
            }
        }

        [NotNull]
        private ListBoxItem GetListBoxItem([NotNull] ItemTreeViewItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var valueItem = new ValueItem(item.Text, item.ItemUri.ItemId.ToString());

            return GetListBoxItem(valueItem);
        }

        [NotNull]
        private ListBoxItem GetListBoxItem([NotNull] ValueItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            return new ListBoxItem
            {
                Content = item.Name,
                Tag = item
            };
        }

        private void MoveDown([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var modified = false;

            for (var n = Target.Items.Count - 2; n >= 0; n--)
            {
                var item = Target.Items[n] as ListBoxItem;
                if (item == null)
                {
                    continue;
                }

                if (!item.IsSelected)
                {
                    continue;
                }

                Target.Items.RemoveAt(n);

                Target.Items.Insert(n + 1, item);

                item.IsSelected = true;

                modified = true;
            }

            if (modified)
            {
                RaiseModified();
            }
        }

        private void MoveUp([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var modified = false;

            for (var n = 1; n < Target.Items.Count; n++)
            {
                var item = Target.Items[n] as ListBoxItem;
                if (item == null)
                {
                    continue;
                }

                if (!item.IsSelected)
                {
                    continue;
                }

                Target.Items.RemoveAt(n);

                Target.Items.Insert(n - 1, item);

                item.IsSelected = true;

                modified = true;
            }

            if (modified)
            {
                RaiseModified();
            }
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            TargetDockPanel.ContextMenu = null;
            e.Handled = true;

            var listBoxItem = Target.SelectedItem as ListBoxItem;
            if (listBoxItem == null)
            {
                return;
            }

            var valueItem = listBoxItem.Tag as ValueItem;
            if (valueItem == null)
            {
                return;
            }

            Guid guid;
            if (!Guid.TryParse(valueItem.Value, out guid))
            {
                return;
            }

            var contentEditor = this.GetAncestorOrSelf<ContentEditor>();
            if (contentEditor == null)
            {
                return;
            }

            var itemUri = new ItemUri(DatabaseUri, new ItemId(guid));

            var context = new TreeListFieldContext(contentEditor, itemUri);

            e.Handled = false;
            TargetDockPanel.ContextMenu = AppHost.ContextMenus.Build(context, e);
        }

        private void RaiseModified()
        {
            var valueModified = ValueModified;
            if (valueModified != null)
            {
                valueModified();
            }
        }

        private void RemoveItem([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            RemoveItem();
        }

        private void RemoveItem()
        {
            var items = Target.RemoveSelectedItems();
            if (items.Count > 0)
            {
                RaiseModified();
            }
        }

        private void SourceDoubleClick([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AddItem();
        }

        private void SourceFilterTextChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Source.Filter(SourceFilter.Text);
        }

        private void TargetDoubleClick([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            RemoveItem();
        }

        private void TargetFilterTextChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var filterText = TargetFilter.Text;

            foreach (ListBoxItem item in Target.Items)
            {
                var valueItem = item.Tag as ValueItem;
                if (valueItem == null)
                {
                    continue;
                }

                item.Visibility = valueItem.Name.IsFilterMatch(filterText) ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
