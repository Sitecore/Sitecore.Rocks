// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Fields
{
    [FieldControl("name lookup value list")]
    public partial class NameLookupValueField : IReusableFieldControl
    {
        private readonly Style comboBoxStyle;

        private readonly List<KeyValuePair<string, string>> itemsList;

        private readonly Style textBoxStyle;

        private Field source;

        public NameLookupValueField()
        {
            InitializeComponent();

            itemsList = new List<KeyValuePair<string, string>>();
            textBoxStyle = Resources[@"TextBoxStyle"] as Style;
            comboBoxStyle = Resources[@"ComboBoxStyle"] as Style;
        }

        public Control GetFocusableControl()
        {
            return this;
        }

        public string GetValue()
        {
            var sb = new StringBuilder();

            foreach (var pair in itemsList)
            {
                if (string.IsNullOrEmpty(pair.Key))
                {
                    continue;
                }

                if (sb.Length > 0)
                {
                    sb.Append('&');
                }

                sb.Append(HttpUtility.UrlEncode(pair.Key));
                sb.Append(@"=");
                sb.Append(HttpUtility.UrlEncode(pair.Value));
            }

            return sb.ToString();
        }

        public bool IsSupported(Field sourceField)
        {
            Assert.ArgumentNotNull(sourceField, nameof(sourceField));

            return true;
        }

        public void SetField(Field sourceField)
        {
            Assert.ArgumentNotNull(sourceField, nameof(sourceField));

            source = sourceField;
            Resizer.FieldId = HorizontalResizer.FieldId = sourceField.FieldUris.First().FieldId;
        }

        public void SetValue(string value)
        {
            Assert.ArgumentNotNull(value, nameof(value));

            var items = value.Split('&');
            var i = -1;
            var changed = false;

            foreach (var item in items)
            {
                var parts = item.Split('=');
                if (parts.Length < 2)
                {
                    continue;
                }

                i++;

                var itemName = HttpUtility.UrlDecode(parts[0]) ?? string.Empty;
                var itemValue = HttpUtility.UrlDecode(parts[1]) ?? string.Empty;

                var lastIndex = itemsList.Count;
                if (lastIndex == 0)
                {
                    NameStack.Children.Clear();
                    ValueStack.Children.Clear();
                }

                changed = lastIndex <= i;

                if (changed)
                {
                    AddItem(itemName, itemValue, i);
                }
                else if (itemsList[i].Key != itemName || itemsList[i].Value != itemValue)
                {
                    changed = true;

                    itemsList[i] = new KeyValuePair<string, string>(itemName, itemValue);

                    var currentNameBox = (FocusedTextBox)NameStack.Children[i];
                    var currentValueBox = (ComboBox)ValueStack.Children[i];
                    if (currentNameBox == null || currentValueBox == null)
                    {
                        continue;
                    }

                    currentNameBox.Text = itemName;
                    SelectItem(currentValueBox, itemValue);
                }
            }

            i++;
            var remainderCount = itemsList.Count - i;

            NameStack.Children.RemoveRange(i, remainderCount);
            ValueStack.Children.RemoveRange(i, remainderCount);
            itemsList.RemoveRange(i, remainderCount);

            var lastNameBox = new FocusedTextBox
            {
                Style = textBoxStyle
            };
            lastNameBox.TextChanged += LastTextChanged;

            var lastValueBox = new ComboBox
            {
                Style = comboBoxStyle
            };
            LoadItems(lastValueBox);
            lastValueBox.SelectionChanged += LastTextChanged;

            NameStack.Children.Add(lastNameBox);
            ValueStack.Children.Add(lastValueBox);

            if (changed)
            {
                ValueModifiedNotify();
            }
        }

        public void UnsetField()
        {
            source = null;
            itemsList.Clear();
        }

        public event ValueModifiedEventHandler ValueModified;

        private void AddItem([NotNull] string name, [NotNull] string value, int index)
        {
            Debug.ArgumentNotNull(name, nameof(name));
            Debug.ArgumentNotNull(value, nameof(value));

            itemsList.Add(new KeyValuePair<string, string>(name, value));

            // There's only 2 tabbable fields for each item.
            var firstTabIndex = index << 1;

            var itemNameBox = new FocusedTextBox
            {
                Text = name,
                TabIndex = firstTabIndex,
                Style = textBoxStyle
            };

            itemNameBox.TextChanged += ItemTextChanged(index);
            itemNameBox.LostFocus += ItemLostFocus(index);

            var itemValueBox = new ComboBox
            {
                TabIndex = firstTabIndex + 1,
                Style = comboBoxStyle
            };
            LoadItems(itemValueBox);
            SelectItem(itemValueBox, value);

            itemValueBox.SelectionChanged += ItemSelectionChanged(index);
            itemValueBox.LostFocus += ItemLostFocus(index);

            NameStack.Children.Add(itemNameBox);
            ValueStack.Children.Add(itemValueBox);
        }

        Control IFieldControl.GetControl()
        {
            return this;
        }

        [NotNull]
        private string GetSelectedValue([NotNull] ComboBox comboBox)
        {
            Debug.ArgumentNotNull(comboBox, nameof(comboBox));

            var selectedItem = comboBox.SelectedItem as ValueItem;
            return selectedItem == null ? string.Empty : selectedItem.Value;
        }

        [NotNull]
        private RoutedEventHandler ItemLostFocus(int itemIndex)
        {
            return delegate
            {
                var itemNameBox = (FocusedTextBox)NameStack.Children[itemIndex];
                var itemValueBox = (ComboBox)ValueStack.Children[itemIndex];
                if (itemNameBox == null || itemValueBox == null)
                {
                    return;
                }

                if (itemNameBox.Text != string.Empty || GetSelectedValue(itemValueBox) != string.Empty)
                {
                    return;
                }

                if (itemIndex < 0 || itemIndex >= itemsList.Count())
                {
                    return;
                }

                NameStack.Children.RemoveAt(itemIndex);
                ValueStack.Children.RemoveAt(itemIndex);

                itemsList.RemoveAt(itemIndex);
            };
        }

        [NotNull]
        private SelectionChangedEventHandler ItemSelectionChanged(int itemIndex)
        {
            return delegate
            {
                var itemNameBox = (FocusedTextBox)NameStack.Children[itemIndex];
                var itemValueBox = (ComboBox)ValueStack.Children[itemIndex];

                if (itemNameBox == null || itemValueBox == null)
                {
                    return;
                }

                if (itemIndex < 0 || itemIndex >= itemsList.Count())
                {
                    return;
                }

                itemsList[itemIndex] = new KeyValuePair<string, string>(itemNameBox.Text, GetSelectedValue(itemValueBox));

                ValueModifiedNotify();
            };
        }

        [NotNull]
        private TextChangedEventHandler ItemTextChanged(int itemIndex)
        {
            return delegate
            {
                var itemNameBox = (FocusedTextBox)NameStack.Children[itemIndex];
                var itemValueBox = (ComboBox)ValueStack.Children[itemIndex];

                if (itemNameBox == null || itemValueBox == null)
                {
                    return;
                }

                if (itemIndex < 0 || itemIndex >= itemsList.Count())
                {
                    return;
                }

                itemsList[itemIndex] = new KeyValuePair<string, string>(itemNameBox.Text, GetSelectedValue(itemValueBox));

                ValueModifiedNotify();
            };
        }

        private void LastTextChanged([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            LastTextChanged();
        }

        private void LastTextChanged()
        {
            var lastIndex = itemsList.Count;

            var lastNameBox = (FocusedTextBox)NameStack.Children[lastIndex];
            var lastValueBox = (ComboBox)ValueStack.Children[lastIndex];
            if (lastNameBox == null || lastValueBox == null)
            {
                return;
            }

            // If there's no content in either of the boxes, return.
            if (lastNameBox.Text == string.Empty && GetSelectedValue(lastValueBox) == string.Empty)
            {
                return;
            }

            // Add the newly created item to the model.
            itemsList.Add(new KeyValuePair<string, string>(lastNameBox.Text, GetSelectedValue(lastValueBox)));

            var currentTabIndex = lastValueBox.TabIndex;

            // Add some new last boxes.
            var newLastNameBox = new FocusedTextBox
            {
                TabIndex = ++currentTabIndex,
                Style = textBoxStyle
            };
            newLastNameBox.TextChanged += LastTextChanged;
            NameStack.Children.Add(newLastNameBox);

            var newLastValueBox = new ComboBox
            {
                TabIndex = ++currentTabIndex,
                Style = comboBoxStyle
            };

            LoadItems(newLastValueBox);
            newLastValueBox.SelectionChanged += LastTextChanged;
            ValueStack.Children.Add(newLastValueBox);

            // Unbind last text listeners (the delegate boxes are no longer last).
            lastNameBox.TextChanged -= LastTextChanged;
            lastValueBox.SelectionChanged -= LastTextChanged;

            lastNameBox.TextChanged += ItemTextChanged(lastIndex);
            lastValueBox.SelectionChanged += ItemSelectionChanged(lastIndex);
            lastNameBox.LostFocus += ItemLostFocus(lastIndex);
            lastValueBox.LostFocus += ItemLostFocus(lastIndex);
        }

        private void LastTextChanged([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            LastTextChanged();
        }

        private void LoadItems([NotNull] ComboBox comboBox)
        {
            Debug.ArgumentNotNull(comboBox, nameof(comboBox));

            foreach (var valueItem in source.ValueItems)
            {
                comboBox.Items.Add(valueItem);
            }
        }

        private void SelectItem([NotNull] ComboBox comboBox, [NotNull] string itemValue)
        {
            Debug.ArgumentNotNull(comboBox, nameof(comboBox));
            Debug.ArgumentNotNull(itemValue, nameof(itemValue));

            comboBox.SelectedItem = comboBox.Items.OfType<ValueItem>().FirstOrDefault(v => v.Value == itemValue);
        }

        private void ValueModifiedNotify()
        {
            var handler = ValueModified;
            if (handler != null)
            {
                handler();
            }
        }
    }
}
