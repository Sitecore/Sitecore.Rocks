// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Fields
{
    [FieldControl("checkbox")]
    public class CheckBoxField : CheckBox, IReusableFieldControl
    {
        public CheckBoxField()
        {
            Checked += CheckedChanged;
            Unchecked += CheckedChanged;
            Loaded += ControlLoaded;

            VerticalContentAlignment = VerticalAlignment.Center;
        }

        public Control GetFocusableControl()
        {
            return this;
        }

        public string GetValue()
        {
            return IsChecked == true ? @"1" : string.Empty;
        }

        public bool IsSupported(Field sourceField)
        {
            Assert.ArgumentNotNull(sourceField, nameof(sourceField));

            return true;
        }

        public void SetField(Field sourceField)
        {
            Assert.ArgumentNotNull(sourceField, nameof(sourceField));
        }

        public void SetValue(string value)
        {
            Assert.ArgumentNotNull(value, nameof(value));

            IsChecked = value == @"1";
        }

        public void UnsetField()
        {
        }

        public event ValueModifiedEventHandler ValueModified;

        private void CheckedChanged([NotNull] object sender, [NotNull] RoutedEventArgs routedEventArgs)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(routedEventArgs, nameof(routedEventArgs));

            var valueModified = ValueModified;
            if (valueModified != null)
            {
                valueModified();
            }
        }

        private void ControlLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= ControlLoaded;

            Style = TryFindResource(typeof(CheckBox)) as Style;
        }

        Control IFieldControl.GetControl()
        {
            return this;
        }
    }
}
