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
    [FieldControl("name value list")]
    public partial class NameValueField : IReusableFieldControl
    {
        private readonly List<KeyValuePair<string, string>> itemsList;

        private readonly Style textBoxStyle;

        public NameValueField()
        {
            InitializeComponent();

            itemsList = new List<KeyValuePair<string, string>>();
            textBoxStyle = Resources[@"TextBoxStyle"] as Style;
        }

        public Control GetFocusableControl()
        {
            return this;
        }

        public string GetValue()
        {
            var sb = new StringBuilder();
            var iterator = itemsList.GetEnumerator();
            var hasFirst = iterator.MoveNext();
            while (hasFirst)
            {
                sb.Append(HttpUtility.UrlEncode(iterator.Current.Key));
                sb.Append(@"=");
                sb.Append(HttpUtility.UrlEncode(iterator.Current.Value));
                if (!iterator.MoveNext())
                {
                    break;
                }

                sb.Append(@"&");
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
                var namevaluepair = item.Split('=');
                if (namevaluepair.Length < 2)
                {
                    continue;
                }

                i++;

                var lastIndex = itemsList.Count;

                var itemName = HttpUtility.UrlDecode(namevaluepair[0]);
                var itemValue = HttpUtility.UrlDecode(namevaluepair[1]);

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
                else
                {
                    var changedName = itemsList[i].Key != itemName;
                    var changedValue = itemsList[i].Value != itemValue;
                    if (changedName || changedValue)
                    {
                        changed = true;
                        itemsList[i] = new KeyValuePair<string, string>(itemName, itemValue);

                        var currentNameBox = (FocusedTextBox)NameStack.Children[i];
                        var currentValueBox = (FocusedTextBox)ValueStack.Children[i];
                        if (currentNameBox == null || currentValueBox == null)
                        {
                            continue;
                        }

                        currentNameBox.Text = itemName;
                        currentValueBox.Text = itemValue;
                    }
                }
            }

            i++;
            var remainederCount = itemsList.Count - i;

            NameStack.Children.RemoveRange(i, remainederCount);
            ValueStack.Children.RemoveRange(i, remainederCount);
            itemsList.RemoveRange(i, remainederCount);

            var lastNameBox = new FocusedTextBox
            {
                Style = textBoxStyle
            };
            lastNameBox.TextChanged += LastTextChanged;

            var lastValueBox = new FocusedTextBox
            {
                Style = textBoxStyle
            };
            lastValueBox.TextChanged += LastTextChanged;

            NameStack.Children.Add(lastNameBox);
            ValueStack.Children.Add(lastValueBox);

            if (changed)
            {
                ValueModifiedNotify();
            }
        }

        public void UnsetField()
        {
            itemsList.Clear();
        }

        public event ValueModifiedEventHandler ValueModified;

        private void AddItem([NotNull] string name, [NotNull] string value, int index)
        {
            Debug.ArgumentNotNull(name, nameof(name));
            Debug.ArgumentNotNull(value, nameof(value));

            itemsList.Add(new KeyValuePair<string, string>(name, value));

            // There's only 2 tababale fields for each item.
            var firstTabIndex = index << 1;

            var itemNameBox = new FocusedTextBox
            {
                Text = name,
                Style = textBoxStyle,
                TabIndex = firstTabIndex
            };
            itemNameBox.TextChanged += ItemTextChanged(index);
            itemNameBox.LostFocus += ItemLostFocus(index);

            var itemValueBox = new FocusedTextBox
            {
                Text = value,
                Style = textBoxStyle,
                TabIndex = firstTabIndex + 1
            };
            itemValueBox.TextChanged += ItemTextChanged(index);
            itemValueBox.LostFocus += ItemLostFocus(index);

            NameStack.Children.Add(itemNameBox);
            ValueStack.Children.Add(itemValueBox);
        }

        Control IFieldControl.GetControl()
        {
            return this;
        }

        [NotNull]
        private RoutedEventHandler ItemLostFocus(int itemIndex)
        {
            return delegate
            {
                var itemNameBox = (FocusedTextBox)NameStack.Children[itemIndex];
                var itemValueBox = (FocusedTextBox)ValueStack.Children[itemIndex];
                if (itemNameBox == null || itemValueBox == null)
                {
                    return;
                }

                if (itemNameBox.Text != string.Empty || itemValueBox.Text != string.Empty)
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
        private TextChangedEventHandler ItemTextChanged(int itemIndex)
        {
            return delegate
            {
                if (itemIndex < 0 || itemIndex >= NameStack.Children.Count)
                {
                    return;
                }

                if (itemIndex >= ValueStack.Children.Count)
                {
                    return;
                }

                var itemNameBox = (FocusedTextBox)NameStack.Children[itemIndex];
                var itemValueBox = (FocusedTextBox)ValueStack.Children[itemIndex];

                if (itemNameBox == null || itemValueBox == null)
                {
                    return;
                }

                if (itemIndex < 0 || itemIndex >= itemsList.Count())
                {
                    return;
                }

                itemsList[itemIndex] = new KeyValuePair<string, string>(itemNameBox.Text, itemValueBox.Text);

                ValueModifiedNotify();
            };
        }

        private void LastTextChanged([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var lastIndex = itemsList.Count;

            var lastNameBox = (FocusedTextBox)NameStack.Children[lastIndex];
            var lastValueBox = (FocusedTextBox)ValueStack.Children[lastIndex];
            if (lastNameBox == null || lastValueBox == null)
            {
                return;
            }

            // If there's no content in either of the boxes, return.
            if (lastNameBox.Text == string.Empty && lastValueBox.Text == string.Empty)
            {
                return;
            }

            // Add the newly created item to the model.
            itemsList.Add(new KeyValuePair<string, string>(lastNameBox.Text, lastValueBox.Text));

            var currentTabIndex = lastValueBox.TabIndex;

            // Add some new last boxes.
            var newLastNameBox = new FocusedTextBox
            {
                TabIndex = ++currentTabIndex,
                Style = textBoxStyle
            };
            newLastNameBox.TextChanged += LastTextChanged;
            NameStack.Children.Add(newLastNameBox);

            var newLastValueBox = new FocusedTextBox
            {
                TabIndex = ++currentTabIndex,
                Style = textBoxStyle
            };
            newLastValueBox.TextChanged += LastTextChanged;
            ValueStack.Children.Add(newLastValueBox);

            // Unbind last text listeners (the delegate boxes are no longer last).
            lastNameBox.TextChanged -= LastTextChanged;
            lastValueBox.TextChanged -= LastTextChanged;

            lastNameBox.TextChanged += ItemTextChanged(lastIndex);
            lastValueBox.TextChanged += ItemTextChanged(lastIndex);
            lastNameBox.LostFocus += ItemLostFocus(lastIndex);
            lastValueBox.LostFocus += ItemLostFocus(lastIndex);
        }

        private void ValueModifiedNotify()
        {
            if (ValueModified != null)
            {
                ValueModified();
            }
        }
    }
}
