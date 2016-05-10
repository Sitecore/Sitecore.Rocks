// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Fields
{
    [FieldControl("text"), FieldControl("single-line text")]
    public class TextField : TextBox, IReusableFieldControl, ISupportsTextOperations
    {
        public TextField()
        {
            TextChanged += EditChanged;
            Loaded += ControlLoaded;

            MinHeight = 21;
            VerticalContentAlignment = VerticalAlignment.Center;
        }

        [NotNull]
        protected string FieldName { get; set; }

        public Control GetFocusableControl()
        {
            return this;
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

        private void ControlLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= ControlLoaded;
            Style = TryFindResource(typeof(TextBox)) as Style;
        }

        private void EditChanged([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var valueModified = ValueModified;
            if (valueModified != null)
            {
                valueModified();
            }
        }

        [CanBeNull]
        Control IFieldControl.GetControl()
        {
            return this;
        }

        string IFieldControl.GetValue()
        {
            return Text;
        }

        void IFieldControl.SetField(Field sourceField)
        {
            Debug.ArgumentNotNull(sourceField, nameof(sourceField));

            FieldName = sourceField.Name;
        }

        void IFieldControl.SetValue(string value)
        {
            Debug.ArgumentNotNull(value, nameof(value));

            Text = value;
        }
    }
}
