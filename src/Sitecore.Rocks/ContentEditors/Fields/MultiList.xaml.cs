// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentEditors.Commands.Fields.MultiListFields;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.ContentEditors.Fields
{
    [FieldControl("multilist")]
    public partial class MultiList : IReusableFieldControl
    {
        private List<ValueItem> items;

        private List<ValueItem> selectedItems;

        private Timer timer;

        public MultiList()
        {
            InitializeComponent();

            RightImage.Source = new Icon("Resources/16x16/chevron-right.png").GetSource();
            LeftImage.Source = new Icon("Resources/16x16/chevron-left.png").GetSource();
            UpImage.Source = new Icon("Resources/16x16/chevron-up.png").GetSource();
            DownImage.Source = new Icon("Resources/16x16/chevron-down.png").GetSource();
        }

        [NotNull]
        protected DatabaseUri DatabaseUri { get; private set; }

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

            DatabaseUri = sourceField.FieldUris.First().ItemVersionUri.DatabaseUri;

            Resizer.FieldId = sourceField.FieldUris.First().FieldId;

            items = sourceField.ValueItems;
            selectedItems = new List<ValueItem>();
        }

        public void SetValue(string value)
        {
            Assert.ArgumentNotNull(value, nameof(value));

            selectedItems.Clear();

            var separator = new[]
            {
                '|'
            };
            var selected = value.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            foreach (var id in selected)
            {
                foreach (var valueItem in items)
                {
                    if (valueItem.Value == id)
                    {
                        selectedItems.Add(valueItem);
                    }
                }
            }

            Target.Items.Clear();
            foreach (var item in selectedItems)
            {
                Target.Items.Add(GetListBoxItem(item));
            }

            Source.Items.Clear();
            foreach (var item in items)
            {
                if (selectedItems.Contains(item))
                {
                    continue;
                }

                Source.Items.Add(GetListBoxItem(item));
            }
        }

        public void UnsetField()
        {
            items = null;
            selectedItems = null;
        }

        public event ValueModifiedEventHandler ValueModified;

        private void AddItem([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AddItem();
        }

        private void AddItem()
        {
            var modified = false;

            var list = new List<object>();
            foreach (var item in Source.SelectedItems)
            {
                list.Add(item);
            }

            var selectedIndex = int.MaxValue;
            var first = true;

            foreach (ListBoxItem item in list)
            {
                var valueItem = item.Tag as ValueItem;
                if (valueItem == null)
                {
                    continue;
                }

                if (first)
                {
                    Target.SelectedItem = null;
                }

                first = false;

                var index = Source.Items.IndexOf(item);
                if (index >= 0 && index < selectedIndex)
                {
                    selectedIndex = index;
                }

                var listBoxItem = GetListBoxItem(valueItem);

                Target.Items.Add(listBoxItem);
                listBoxItem.IsSelected = true;

                Source.Items.Remove(item);

                modified = true;
            }

            if (selectedIndex >= Source.Items.Count)
            {
                selectedIndex = Source.Items.Count - 1;
            }

            if (selectedIndex >= 0)
            {
                Source.SelectedIndex = selectedIndex;
            }

            if (modified)
            {
                RaiseModified();
            }
        }

        private void Filter([CanBeNull] object state)
        {
            var action = state as Action;
            if (action == null)
            {
                return;
            }

            Dispatcher.Invoke(action);
        }

        private void Filter([NotNull] ListBox listBox, [NotNull] string filterText)
        {
            Debug.ArgumentNotNull(listBox, nameof(listBox));
            Debug.ArgumentNotNull(filterText, nameof(filterText));

            foreach (ListBoxItem item in listBox.Items)
            {
                var valueItem = item.Tag as ValueItem;
                if (valueItem == null)
                {
                    continue;
                }

                item.Visibility = valueItem.Name.IsFilterMatch(filterText) ? Visibility.Visible : Visibility.Collapsed;
            }
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

            DockPanel dockPanel;
            ListBoxItem listBoxItem;

            e.Handled = true;

            if (sender == Source)
            {
                dockPanel = SourceDockPanel;
                listBoxItem = Source.SelectedItem as ListBoxItem;
            }
            else
            {
                dockPanel = TargetDockPanel;
                listBoxItem = Target.SelectedItem as ListBoxItem;
            }

            dockPanel.ContextMenu = null;
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

            var context = new MultiListFieldContext(contentEditor, itemUri);

            e.Handled = false;
            dockPanel.ContextMenu = AppHost.ContextMenus.Build(context, e);
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
            var modified = false;

            var list = new List<object>();
            foreach (var item in Target.SelectedItems)
            {
                list.Add(item);
            }

            var selectedIndex = int.MaxValue;

            foreach (ListBoxItem item in list)
            {
                var valueItem = item.Tag as ValueItem;
                if (valueItem == null)
                {
                    continue;
                }

                var index = Target.Items.IndexOf(item);
                if (index >= 0 && index < selectedIndex)
                {
                    selectedIndex = index;
                }

                var listBoxItem = GetListBoxItem(valueItem);

                Source.Items.Add(listBoxItem);
                listBoxItem.IsSelected = true;

                Target.Items.Remove(item);

                modified = true;
            }

            if (selectedIndex >= Target.Items.Count)
            {
                selectedIndex = Target.Items.Count - 1;
            }

            if (selectedIndex >= 0)
            {
                Target.SelectedIndex = selectedIndex;
            }

            if (modified)
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

        private void SourceFilterTextChanged([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            StopTimer();

            var action = new Action(() => Filter(Source, SourceFilter.Text));

            timer = new Timer(Filter, action, 350, int.MaxValue);
        }

        private void StopTimer()
        {
            if (timer == null)
            {
                return;
            }

            timer.Dispose();
            timer = null;
        }

        private void TargetDoubleClick([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            RemoveItem();
        }

        private void TargetFilterTextChanged([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            StopTimer();

            var action = new Action(() => Filter(Target, TargetFilter.Text));

            timer = new Timer(Filter, action, 350, int.MaxValue);
        }
    }
}
