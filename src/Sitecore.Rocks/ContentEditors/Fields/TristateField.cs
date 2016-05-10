// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Fields
{
    [FieldControl("tristate")]
    public class TristateField : CheckBox, IReusableFieldControl
    {
        public TristateField()
        {
            IsThreeState = true;

            Checked += CheckedChanged;
            Unchecked += CheckedChanged;
            Indeterminate += CheckedChanged;
        }

        public Control GetFocusableControl()
        {
            return this;
        }

        public string GetValue()
        {
            if (IsChecked == false)
            {
                return @"0";
            }

            if (IsChecked == true)
            {
                return @"1";
            }

            return @"-";
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

            switch (value)
            {
                case "1":
                    IsChecked = true;
                    break;

                case "0":
                    IsChecked = false;
                    break;

                default:
                    IsChecked = null;
                    break;
            }
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

        Control IFieldControl.GetControl()
        {
            return this;
        }
    }
}
