// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Fields
{
    [FieldControl("checklist")]
    public partial class CheckList : IReusableFieldControl
    {
        public CheckList()
        {
            InitializeComponent();
        }

        public Control GetControl()
        {
            return this;
        }

        public Control GetFocusableControl()
        {
            return CheckBoxList;
        }

        public string GetValue()
        {
            var result = new StringBuilder();
            var first = true;

            foreach (ListBoxItem selectedItem in CheckBoxList.SelectedItems)
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

            CheckBoxList.Items.Clear();

            Resizer.FieldId = sourceField.FieldUris.First().FieldId;

            foreach (var valueItem in sourceField.ValueItems)
            {
                var item = new ListBoxItem
                {
                    Content = valueItem.Name,
                    Tag = valueItem
                };

                CheckBoxList.Items.Add(item);
            }
        }

        public void SetValue(string value)
        {
            Assert.ArgumentNotNull(value, nameof(value));

            CheckBoxList.SelectedItem = null;

            var separator = new[]
            {
                '|'
            };
            var selected = value.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            foreach (var v in selected)
            {
                foreach (ListBoxItem item in CheckBoxList.Items)
                {
                    var valueItem = item.Tag as ValueItem;
                    if (valueItem == null)
                    {
                        continue;
                    }

                    var isSelected = string.Compare(valueItem.Value, v, StringComparison.InvariantCultureIgnoreCase) == 0;
                    if (isSelected)
                    {
                        item.IsSelected = true;
                    }
                }
            }
        }

        public void UnsetField()
        {
        }

        public event ValueModifiedEventHandler ValueModified;

        private void SelectionChanged([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var valueModified = ValueModified;
            if (valueModified != null)
            {
                valueModified();
            }
        }
    }
}
