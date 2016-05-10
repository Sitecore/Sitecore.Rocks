// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DateTimeExtensions;

namespace Sitecore.Rocks.ContentEditors.Fields
{
    [FieldControl("date")]
    public partial class DateField : IReusableFieldControl
    {
        private bool isDateMacro;

        public DateField()
        {
            InitializeComponent();
            Date.SelectedDateChanged += DateChanged;
        }

        public bool IsDateMacro
        {
            get { return isDateMacro; }

            set
            {
                if (isDateMacro == value)
                {
                    return;
                }

                isDateMacro = value;
                Update();

                RaiseModified();
            }
        }

        public Control GetControl()
        {
            return this;
        }

        public Control GetFocusableControl()
        {
            return Date;
        }

        public string GetValue()
        {
            if (IsDateMacro)
            {
                return "$date";
            }

            var selectedDate = Date.SelectedDate;
            if (selectedDate == null)
            {
                return string.Empty;
            }

            return DateTimeExtensions.ToIsoDate((DateTime)selectedDate);
        }

        public bool IsSupported(Field sourceField)
        {
            Assert.ArgumentNotNull(sourceField, nameof(sourceField));

            return true;
        }

        public void SetField(Field sourceField)
        {
            Assert.ArgumentNotNull(sourceField, nameof(sourceField));

            isDateMacro = sourceField.Value == "$date";
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

            if (string.IsNullOrEmpty(value))
            {
                Date.SelectedDate = null;
                return;
            }

            Date.SelectedDate = DateTimeExtensions.FromIso(value);
        }

        public void UnsetField()
        {
        }

        public event ValueModifiedEventHandler ValueModified;

        private void DateChanged([NotNull] object sender, [NotNull] SelectionChangedEventArgs selectionChangedEventArgs)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(selectionChangedEventArgs, nameof(selectionChangedEventArgs));

            var valueModified = ValueModified;
            if (valueModified != null)
            {
                valueModified();
            }
        }

        private void HandleKeyDown([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.Key < Key.D0 || e.Key > Key.D9)
            {
                return;
            }

            RaiseModified();
        }

        private void RaiseModified()
        {
            var valueModified = ValueModified;
            if (valueModified != null)
            {
                valueModified();
            }
        }

        private void Update()
        {
            Date.Visibility = IsDateMacro ? Visibility.Collapsed : Visibility.Visible;
            DateMacro.Visibility = IsDateMacro ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
