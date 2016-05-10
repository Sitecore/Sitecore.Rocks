// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.UI.TemplateDesigners.Dialogs.BuildSourceDialogs.Parameters.PropertyTypeDialogs
{
    public partial class PropertyTypeDialog
    {
        private static readonly char[] Pipe =
        {
            '|'
        };

        public PropertyTypeDialog([NotNull] string propertyType)
        {
            Assert.ArgumentNotNull(propertyType, nameof(propertyType));

            InitializeComponent();
            this.InitializeDialog();

            PropertyType = propertyType;
            PropertyTypeComboBox.Text = propertyType;

            var values = AppHost.Settings.GetString("/TemplateDesigner/Parameters", "PropertyTypes", string.Empty);

            foreach (var value in values.Split(Pipe, StringSplitOptions.RemoveEmptyEntries).OrderBy(s => s))
            {
                PropertyTypeComboBox.Items.Add(value);
            }
        }

        [NotNull]
        public string PropertyType { get; private set; }

        private void HandleKeyDown([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (!PropertyTypeComboBox.IsDropDownOpen)
            {
                return;
            }

            if (e.Key != Key.Delete)
            {
                return;
            }

            if (PropertyTypeComboBox.SelectedIndex < 0)
            {
                return;
            }

            var value = PropertyTypeComboBox.Items[PropertyTypeComboBox.SelectedIndex];

            var values = AppHost.Settings.GetString("/TemplateDesigner/Parameters", "PropertyTypes", string.Empty).Split(Pipe, StringSplitOptions.RemoveEmptyEntries).ToList();
            values.Remove(value as string);
            AppHost.Settings.SetString("/TemplateDesigner/Parameters", "PropertyTypes", string.Join("|", values));

            PropertyTypeComboBox.Items.Remove(value);

            e.Handled = true;
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            PropertyType = PropertyTypeComboBox.Text;

            var values = AppHost.Settings.GetString("/TemplateDesigner/Parameters", "PropertyTypes", string.Empty).Split(Pipe, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (values.All(s => s != PropertyType))
            {
                values.Add(PropertyType);
                AppHost.Settings.SetString("/TemplateDesigner/Parameters", "PropertyTypes", string.Join("|", values));
            }

            this.Close(true);
        }
    }
}
