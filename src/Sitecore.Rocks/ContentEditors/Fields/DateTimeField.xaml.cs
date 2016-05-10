// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DateTimeExtensions;

namespace Sitecore.Rocks.ContentEditors.Fields
{
    [FieldControl("datetime")]
    public partial class DateTimeField : IReusableFieldControl
    {
        private bool _isDateMacro;

        public DateTimeField()
        {
            InitializeComponent();
        }

        public bool IsDateMacro
        {
            get { return _isDateMacro; }

            set
            {
                if (_isDateMacro == value)
                {
                    return;
                }

                _isDateMacro = value;
                Update();

                RaiseModified();
            }
        }

        public Control GetFocusableControl()
        {
            return DateTimePicker;
        }

        public string GetValue()
        {
            if (IsDateMacro)
            {
                return "$date";
            }

            var dateValue = DateTimePicker.Value;

            if (dateValue == null)
            {
                return string.Empty;
            }

            return DateTimeExtensions.ToIsoDate((DateTime)dateValue);
        }

        public bool IsSupported(Field sourceField)
        {
            Assert.ArgumentNotNull(sourceField, nameof(sourceField));

            return true;
        }

        public void SetField(Field sourceField)
        {
            Assert.ArgumentNotNull(sourceField, nameof(sourceField));

            _isDateMacro = sourceField.Value == "$date";
            Update();
        }

        public void SetValue(string value)
        {
            Assert.ArgumentNotNull(value, nameof(value));

            if (value == "$date")
            {
                IsDateMacro = true;
                return;
            }

            IsDateMacro = false;

            if (!string.IsNullOrEmpty(value))
            {
                DateTimePicker.Value = DateTimeExtensions.FromIso(value);
            }
            else
            {
                DateTimePicker.Value = null;
            }
        }

        public void UnsetField()
        {
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

        private void Update()
        {
            DateTimePicker.Visibility = IsDateMacro ? Visibility.Collapsed : Visibility.Visible;
            DateMacro.Visibility = IsDateMacro ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ValueChanged([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            RaiseModified();
        }
    }
}
