// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Fields
{
    [FieldControl("integer")]
    public partial class IntegerField : IReusableFieldControl
    {
        public IntegerField()
        {
            InitializeComponent();
        }

        public Control GetFocusableControl()
        {
            return IntBox;
        }

        public bool IsSupported(Field sourceField)
        {
            Assert.ArgumentNotNull(sourceField, nameof(sourceField));

            return true;
        }

        public void UnsetField()
        {
        }

        public event ValueModifiedEventHandler ValueModified;

        Control IFieldControl.GetControl()
        {
            return this;
        }

        string IFieldControl.GetValue()
        {
            var value = IntBox.Value;
            if (value == null)
            {
                return string.Empty;
            }

            return value.ToString();
        }

        private void Modified([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var valueModified = ValueModified;
            if (valueModified != null)
            {
                valueModified();
            }
        }

        void IFieldControl.SetField(Field sourceField)
        {
            Debug.ArgumentNotNull(sourceField, nameof(sourceField));
        }

        void IFieldControl.SetValue(string newValue)
        {
            Debug.ArgumentNotNull(newValue, nameof(newValue));

            if (!string.IsNullOrEmpty(newValue))
            {
                int value;
                if (int.TryParse(newValue, out value))
                {
                    IntBox.Value = value;
                }
                else
                {
                    IntBox.Value = null;
                }
            }
            else
            {
                IntBox.Value = null;
            }
        }
    }
}
