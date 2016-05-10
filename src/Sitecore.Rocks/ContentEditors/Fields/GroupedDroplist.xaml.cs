// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.ContentEditors.Fields
{
    [FieldControl("Grouped Droplist"), FieldControl("Grouped Droplink")]
    public partial class GroupedDroplist : IReusableFieldControl
    {
        private bool changingSource;

        private string currentValue;

        public GroupedDroplist()
        {
            InitializeComponent();
        }

        public Control GetFocusableControl()
        {
            return this;
        }

        public string GetValue()
        {
            return currentValue;
        }

        public bool IsSupported(Field sourceField)
        {
            Assert.ArgumentNotNull(sourceField, nameof(sourceField));

            return true;
        }

        public void SetField(Field sourceField)
        {
            Assert.ArgumentNotNull(sourceField, nameof(sourceField));

            currentValue = sourceField.Value;
            DropComboBox.ItemsSource = null;

            if (string.IsNullOrEmpty(sourceField.Lookup))
            {
                return;
            }

            var doc = XDocument.Parse(sourceField.Lookup);
            if (doc.Root == null)
            {
                return;
            }

            var hasSelectedItem = false;
            var items = new List<Option>();

            foreach (var section in doc.Root.Elements())
            {
                var groupName = section.GetAttributeValue("name");

                foreach (var item in section.Elements())
                {
                    var name = item.GetAttributeValue("name");
                    var value = item.GetAttributeValue("value");

                    items.Add(new Option(name, value, groupName));

                    if (currentValue == value)
                    {
                        hasSelectedItem = true;
                    }
                }
            }

            if (!hasSelectedItem)
            {
                items.Insert(0, new Option(string.Format(Rocks.Resources.GroupedDroplist_SetField_Item_not_found___0_, currentValue), currentValue, Rocks.Resources.GroupedDroplist_SetField_Not_Found));
            }

            var view = new ListCollectionView(items);
            var observableCollection = view.GroupDescriptions;
            if (observableCollection != null)
            {
                observableCollection.Add(new PropertyGroupDescription(@"GroupName"));
            }

            changingSource = true;

            DropComboBox.ItemsSource = view;

            changingSource = false;

            DropComboBox.SelectedItem = DropComboBox.Items.OfType<Option>().FirstOrDefault(i => i.Value == currentValue);
        }

        public void SetValue(string value)
        {
            Assert.ArgumentNotNull(value, nameof(value));

            var changed = currentValue != value;
            if (!changed)
            {
                return;
            }

            DropComboBox.SelectedItem = DropComboBox.Items.OfType<Option>().FirstOrDefault(i => i.Value == currentValue);

            currentValue = value;
            RaiseModified();
        }

        public void UnsetField()
        {
            DropComboBox.ItemsSource = null;
        }

        public event ValueModifiedEventHandler ValueModified;

        Control IFieldControl.GetControl()
        {
            return this;
        }

        private void RaiseModified()
        {
            var modified = ValueModified;
            if (modified != null)
            {
                modified();
            }
        }

        private void SetSelected([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (changingSource)
            {
                return;
            }

            var option = DropComboBox.SelectedItem as Option;
            if (option == null)
            {
                return;
            }

            currentValue = option.Value;
            RaiseModified();
        }

        public class Option
        {
            public Option([NotNull] string name, [NotNull] string value, [NotNull] string groupName)
            {
                Assert.ArgumentNotNull(name, nameof(name));
                Assert.ArgumentNotNull(value, nameof(value));
                Assert.ArgumentNotNull(groupName, nameof(groupName));

                Name = name;
                Value = value;
                GroupName = groupName;
            }

            [NotNull]
            public string GroupName { get; private set; }

            [NotNull]
            public string Name { get; private set; }

            [NotNull]
            public string Value { get; }
        }
    }
}
