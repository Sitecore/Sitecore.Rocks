// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Windows;
using System.Windows.Media;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DateTimeExtensions;
using Xceed.Wpf.Toolkit;

namespace Sitecore.Rocks.UI.RuleEditors.Macros
{
    [RuleEditorMacro("datetime")]
    public class DateTimeMacro : RuleEditorMacroBase
    {
        private DateTimePicker _dateTimePicker;

        public override object GetEditableControl()
        {
            var value = GetValue();

            _dateTimePicker = new DateTimePicker
            {
                Foreground = Brushes.Blue,
                BorderThickness = new Thickness(0),
                BorderBrush = Brushes.Transparent,
                Background = Brushes.Transparent,
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Top,
                FontFamily = new FontFamily(@"Segoe UI"),
                FontSize = 11
            };

            TextOptions.SetTextRenderingMode(_dateTimePicker, TextRenderingMode.ClearType);
            TextOptions.SetTextFormattingMode(_dateTimePicker, TextFormattingMode.Display);

            _dateTimePicker.Value = DateTimeExtensions.FromIso(value, DateTime.MinValue);

            _dateTimePicker.ValueChanged += SetValue;

            return _dateTimePicker;
        }

        protected override string GetDisplayValue()
        {
            var d = DateTimeExtensions.FromIso(Value);

            if (d == DateTime.MinValue)
            {
                return Resources.DateTimeMacro_GetDisplayValue__Not_set_;
            }

            return d.ToString();
        }

        private void SetValue([NotNull] object sender, [NotNull] RoutedEventArgs propertyChangedRoutedEventArgs)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(propertyChangedRoutedEventArgs, nameof(propertyChangedRoutedEventArgs));

            var dateValue = _dateTimePicker.Value;

            if (dateValue == null)
            {
                Value = string.Empty;
                return;
            }

            Value = DateTimeExtensions.ToIsoDate((DateTime)dateValue);
        }
    }
}
