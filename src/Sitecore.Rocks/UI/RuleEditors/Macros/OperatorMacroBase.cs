// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.RuleEditors.Macros
{
    public abstract class OperatorMacroBase : RuleEditorMacroBase
    {
        [NotNull]
        private readonly Dictionary<string, string> operators = new Dictionary<string, string>();

        private ComboBox comboBox;

        [NotNull]
        public Dictionary<string, string> Operators
        {
            get { return operators; }
        }

        public override object GetEditableControl()
        {
            var value = GetValue();

            comboBox = new ComboBox
            {
                Foreground = Brushes.Blue,
                BorderThickness = new Thickness(0),
                BorderBrush = Brushes.Transparent,
                Background = Brushes.Transparent,
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Top,
            };

            var comboBoxItem = new ComboBoxItem
            {
                Content = DefaultValue,
                Tag = string.Empty
            };

            if (string.IsNullOrEmpty(value) || value == DefaultValue)
            {
                comboBoxItem.IsSelected = true;
            }

            comboBox.Items.Add(comboBoxItem);

            foreach (var pair in operators)
            {
                comboBoxItem = new ComboBoxItem
                {
                    Content = pair.Value,
                    Tag = pair.Key
                };

                if (pair.Key == value)
                {
                    comboBoxItem.IsSelected = true;
                }

                comboBox.Items.Add(comboBoxItem);
            }

            comboBox.SelectionChanged += SetValue;

            return comboBox;
        }

        private void SetValue([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedItem = comboBox.SelectedItem as ComboBoxItem;
            if (selectedItem == null)
            {
                Value = string.Empty;
                return;
            }

            Value = selectedItem.Tag as string ?? string.Empty;
        }
    }
}
