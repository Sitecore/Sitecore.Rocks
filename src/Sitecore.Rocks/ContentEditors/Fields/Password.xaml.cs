// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Fields
{
    [FieldControl("password")]
    public partial class Password : IReusableFieldControl
    {
        public Password()
        {
            InitializeComponent();
            PasswordBox.PasswordChanged += PasswordChanged;
        }

        public Control GetFocusableControl()
        {
            return PasswordBox;
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
            return PasswordBox.Password;
        }

        private void PasswordChanged([NotNull] object sender, [NotNull] RoutedEventArgs routedEventArgs)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(routedEventArgs, nameof(routedEventArgs));

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

        void IFieldControl.SetValue(string value)
        {
            Debug.ArgumentNotNull(value, nameof(value));

            PasswordBox.Password = value;
        }
    }
}
