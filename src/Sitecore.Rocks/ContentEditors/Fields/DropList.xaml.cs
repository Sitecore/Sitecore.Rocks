// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Fields
{
    [FieldControl("droplink"), FieldControl("droplist"), FieldControl("lookup"), FieldControl("valuelookup")]
    public partial class DropList : IReusableFieldControl
    {
        public DropList()
        {
            InitializeComponent();
        }

        public Control GetControl()
        {
            return this;
        }

        public Control GetFocusableControl()
        {
            return Field;
        }

        public string GetValue()
        {
            var selectedItem = Field.SelectedItem as ComboBoxItem;
            if (selectedItem == null)
            {
                return Field.Text;
            }

            return selectedItem.Tag as string ?? string.Empty;
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

            Field.Items.Clear();

            var emptyItem = new ComboBoxItem
            {
                Content = "[Select an item]",
                Tag = string.Empty,
                Foreground = SystemColors.GrayTextBrush
            };

            Field.Items.Add(emptyItem);

            foreach (var valueItem in sourceField.ValueItems)
            {
                var comboBoxItem = new ComboBoxItem
                {
                    Content = valueItem.Name,
                    Tag = valueItem.Value
                };

                Field.Items.Add(comboBoxItem);
            }

            ResizeToFit();
        }

        public void SetValue(string value)
        {
            Assert.ArgumentNotNull(value, nameof(value));

            foreach (var item in Field.Items)
            {
                var valueItem = item as ComboBoxItem;
                if (valueItem == null)
                {
                    continue;
                }

                if (valueItem.Tag as string == value)
                {
                    Field.SelectedItem = item;
                    return;
                }
            }

            Field.Text = value;
        }

        public void UnsetField()
        {
            Field.Items.Clear();
        }

        public event ValueModifiedEventHandler ValueModified;

        private void FieldSelectionChanged([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var valueModified = ValueModified;
            if (valueModified != null)
            {
                valueModified();
            }
        }

        private void ResizeToFit()
        {
            var maxSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

            var width = 0D;
            foreach (ComboBoxItem item in Field.Items)
            {
                item.Measure(maxSize);
                if (item.DesiredSize.Width > width)
                {
                    width = item.DesiredSize.Width;
                }
            }

            Field.Measure(maxSize);
            Field.Width = width + Field.DesiredSize.Width + 32;
        }
    }
}
