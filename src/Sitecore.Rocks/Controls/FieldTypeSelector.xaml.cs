// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DataServiceExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.Controls
{
    public partial class FieldTypeSelector
    {
        private const string ControlsFieldTypeSelectorRecent = "Controls/FieldTypeSelector/Recent";

        public FieldTypeSelector()
        {
            InitializeComponent();

            Loaded += ControlLoaded;
        }

        public DatabaseUri DatabaseUri { get; set; }

        [CanBeNull]
        public object SelectedItem
        {
            get { return FieldTypes.SelectedItem; }

            set { FieldTypes.SelectedItem = value; }
        }

        public void AddToRecent([NotNull] FieldTypeHeader fieldType)
        {
            Assert.ArgumentNotNull(fieldType, nameof(fieldType));

            var s = fieldType.Name + @"^" + fieldType.ItemUri.ItemId;
            var entries = new List<string>();

            var list = AppHost.Settings.Get(ControlsFieldTypeSelectorRecent, GetStorageKey(), string.Empty) as string ?? string.Empty;

            if (!string.IsNullOrEmpty(list))
            {
                entries.AddRange(list.Split('|'));
            }

            entries.Remove(s);

            entries.Insert(0, s);

            while (entries.Count > 10)
            {
                entries.RemoveAt(10);
            }

            var result = string.Empty;
            foreach (var entry in entries)
            {
                if (!string.IsNullOrEmpty(result))
                {
                    result += @"|";
                }

                result += entry;
            }

            AppHost.Settings.Set(ControlsFieldTypeSelectorRecent, GetStorageKey(), result);
        }

        public event EventHandler FieldTypesLoaded;

        public event SelectionChangedEventHandler SelectionChanged;

        private void AddFieldType([NotNull] FieldTypeHeader fieldType)
        {
            Debug.ArgumentNotNull(fieldType, nameof(fieldType));

            var item = new ListBoxItem
            {
                Tag = fieldType,
                Content = fieldType.Name,
                Margin = new Thickness(16, 0, 0, 0)
            };

            FieldTypes.Items.Add(item);
        }

        private void AddSection([NotNull] string name)
        {
            Debug.ArgumentNotNull(name, nameof(name));

            var sectionItem = new ListBoxItem
            {
                Content = name,
                IsEnabled = false,
                FontWeight = FontWeights.Bold,
                BorderThickness = new Thickness(0, 0, 0, 1),
                BorderBrush = SystemColors.ControlDarkBrush,
                Foreground = SystemColors.WindowTextBrush,
                Margin = new Thickness(4, 8, 4, 0)
            };

            FieldTypes.Items.Add(sectionItem);
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            DatabaseUri.Site.DataService.GetFieldTypes(DatabaseUri, LoadFieldTypes);
        }

        private void FieldTypeSelectionChanged([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var changed = SelectionChanged;
            if (changed != null)
            {
                changed(sender, e);
            }
        }

        private void FilterTextChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            foreach (ListBoxItem item in FieldTypes.Items)
            {
                if (!item.IsEnabled)
                {
                    continue;
                }

                var fieldType = item.Tag as FieldTypeHeader;
                if (fieldType == null)
                {
                    continue;
                }

                item.Visibility = fieldType.Name.IsFilterMatch(Filter.Text) ? Visibility.Visible : Visibility.Collapsed;
            }

            var hasItems = false;

            for (var n = FieldTypes.Items.Count - 1; n >= 0; n--)
            {
                var item = FieldTypes.Items[n] as ListBoxItem;
                if (item == null)
                {
                    continue;
                }

                if (!item.IsEnabled)
                {
                    item.Visibility = hasItems ? Visibility.Visible : Visibility.Collapsed;
                    hasItems = false;
                    continue;
                }

                if (item.Visibility == Visibility.Visible)
                {
                    hasItems = true;
                }
            }
        }

        [NotNull]
        private string GetStorageKey()
        {
            return DatabaseUri.Site.Name;
        }

        private void LoadFieldTypes([NotNull] IEnumerable<FieldTypeHeader> fieldTypes, [NotNull] IEnumerable<FieldValidationHeader> fieldValidations)
        {
            Debug.ArgumentNotNull(fieldTypes, nameof(fieldTypes));
            Debug.ArgumentNotNull(fieldValidations, nameof(fieldValidations));

            FieldTypes.Items.Clear();

            LoadRecent();

            string section = null;

            foreach (var fieldType in fieldTypes)
            {
                if (fieldType.Section != section)
                {
                    AddSection(fieldType.Section);

                    section = fieldType.Section;
                }

                AddFieldType(fieldType);
            }

            Loading.Visibility = Visibility.Collapsed;
            FieldTypes.Visibility = Visibility.Visible;

            var loaded = FieldTypesLoaded;
            if (loaded != null)
            {
                loaded(this, EventArgs.Empty);
            }
        }

        private void LoadRecent()
        {
            var list = AppHost.Settings.Get(ControlsFieldTypeSelectorRecent, GetStorageKey(), string.Empty) as string;
            if (string.IsNullOrEmpty(list))
            {
                return;
            }

            var items = list.Split('|');

            AddSection(Rocks.Resources.FieldTypeSelector_LoadRecent_Recently_Used_Field_Types);

            foreach (var item in items)
            {
                var parts = item.Split('^');

                var name = parts[0];
                var itemId = new ItemId(new Guid(parts[1]));

                var itemUri = new ItemUri(DatabaseUri, itemId);

                var fieldType = new FieldTypeHeader(itemUri, name, string.Empty, string.Empty, "Recent");

                AddFieldType(fieldType);
            }
        }
    }
}
