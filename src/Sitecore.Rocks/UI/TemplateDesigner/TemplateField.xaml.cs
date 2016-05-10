// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ContextMenuExtensions;
using Sitecore.Rocks.UI.TemplateDesigner.Commands;
using Sitecore.Rocks.UI.TemplateDesigner.Dialogs.BuildSourceDialogs;

namespace Sitecore.Rocks.UI.TemplateDesigner
{
    public partial class TemplateField
    {
        public TemplateField()
        {
            InitializeComponent();
        }

        [NotNull]
        public TemplateDesigner.TemplateField Field { get; set; }

        protected TemplateDesigner TemplateDesigner { get; set; }

        public void Commit()
        {
            Field.Name = FieldName.Text;
            Field.Source = FieldSource.Text;
            Field.Type = string.Empty;

            var selectedItem = (ComboBoxItem)FieldType.SelectedItem;
            if (selectedItem != null)
            {
                var fieldTypeHeader = selectedItem.Tag as FieldTypeHeader;
                if (fieldTypeHeader != null)
                {
                    Field.Type = fieldTypeHeader.Name;
                }
            }

            switch (Sharing.SelectedIndex)
            {
                case 0:
                    Field.Shared = false;
                    Field.Unversioned = false;
                    break;
                case 1:
                    Field.Shared = true;
                    Field.Unversioned = false;
                    break;
                case 2:
                    Field.Shared = false;
                    Field.Unversioned = true;
                    break;
            }

            var validations = string.Empty;
            foreach (var item in Validations.Items)
            {
                var checkBox = item as CheckBox;
                if (checkBox == null)
                {
                    continue;
                }

                if (checkBox.IsChecked != true)
                {
                    continue;
                }

                var validation = checkBox.Tag as FieldValidationHeader;
                if (validation == null)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(validations))
                {
                    validations += @"|";
                }

                validations += validation.ItemUri.ItemId.ToString();
            }

            Field.Validations = validations;
        }

        public void Initialize([NotNull] TemplateDesigner templateDesigner, [NotNull] TemplateDesigner.TemplateField field)
        {
            Assert.ArgumentNotNull(templateDesigner, nameof(templateDesigner));
            Assert.ArgumentNotNull(field, nameof(field));

            TemplateDesigner = templateDesigner;
            Field = field;

            Update();
        }

        public bool IsLastField()
        {
            var fieldStack = Field.Section.Control.FieldStack;

            var index = fieldStack.Children.IndexOf(this);

            return index == fieldStack.Children.Count - 1;
        }

        public void LoadFieldOptions([NotNull] IEnumerable<FieldTypeHeader> fieldTypes, [NotNull] IEnumerable<FieldValidationHeader> fieldValidations)
        {
            Assert.ArgumentNotNull(fieldTypes, nameof(fieldTypes));
            Assert.ArgumentNotNull(fieldValidations, nameof(fieldValidations));

            var control = FieldType;
            control.Items.Clear();

            string sectionName = null;
            var selectedFieldType = string.IsNullOrEmpty(Field.Type) ? "Single-Line Text" : Field.Type;

            foreach (var fieldType in fieldTypes.OrderBy(f => f.Name))
            {
                /*
                if (fieldType.Section != sectionName)
                {
                    var sectionItem = new ComboBoxItem
                    {
                        Content = fieldType.Section,
                        IsEnabled = false,
                        BorderBrush = SystemColors.ControlDarkBrush,
                        Foreground = SystemColors.GrayTextBrush,
                        Margin = new Thickness(4, 8, 4, 0)
                    };

                    control.Items.Add(sectionItem);
                    sectionName = fieldType.Section;
                }

                var content = new StackPanel();
                content.Orientation = Orientation.Horizontal;
                content.Children.Add(new Image
                {
                    Source = new Icon(TemplateDesigner.TemplateUri.Site, fieldType.Icon).GetSource(),
                    Width = 16,
                    Height = 16,
                    Margin = new Thickness(0, 0, 4, 0)

                });
                content.Children.Add(new TextBlock(new Run(fieldType.Name)));
                */
                var item = new ComboBoxItem
                {
                    Content = fieldType.Name,
                    Tag = fieldType,
                    IsSelected = string.Compare(fieldType.Name, selectedFieldType, StringComparison.InvariantCultureIgnoreCase) == 0

                    // Margin = new Thickness(16, 0, 0, 0)
                };

                control.Items.Add(item);
            }

            sectionName = @"Field Rules";

            foreach (var fieldValidation in fieldValidations.Where(f => !string.IsNullOrEmpty(f.Name)))
            {
                if (fieldValidation.Section != sectionName)
                {
                    var sectionItem = new ComboBoxItem
                    {
                        Content = fieldValidation.Section,
                        IsEnabled = false,
                        Foreground = SystemColors.GrayTextBrush,
                        Margin = new Thickness(4, 8, 4, 0)
                    };

                    Validations.Items.Add(sectionItem);
                    sectionName = fieldValidation.Section;
                }

                var checkBox = new CheckBox
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Content = fieldValidation.Name,
                    Tag = fieldValidation,
                    Margin = new Thickness(16, 0, 0, 0),
                    IsChecked = Field.Validations.Contains(fieldValidation.ItemUri.ItemId.ToString())
                };

                checkBox.Checked += SetValidationChecked;
                checkBox.Unchecked += SetValidationUnchecked;
                checkBox.PreviewMouseDown += ToggleCheckbox;

                Validations.Items.Add(checkBox);
            }

            UpdateValidationsCount();
        }

        private void Browse([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Commit();

            var field = new BuildSourceField(Field.Type, Field.Source)
            {
                Name = Field.Name,
                Id = Field.Id,
                SectionName = Field.Section.Name,
                Shared = Field.Shared,
                Unversioned = Field.Unversioned,
                Validations = Field.Validations
            };

            var dialog = new BuildSourceDialog(TemplateDesigner.TemplateUri.DatabaseUri, field, FieldSource.Text);
            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            FieldSource.Text = dialog.DataSource;
        }

        private void FieldNameModified([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (TemplateDesigner.IsModifiedTracking > 0)
            {
                return;
            }

            TemplateDesigner.SetModified(true);

            if (string.IsNullOrEmpty(FieldName.Text))
            {
                return;
            }

            if (!IsLastField())
            {
                return;
            }

            var addField = new AddField();

            var context = new TemplateDesignerContext
            {
                TemplateDesigner = TemplateDesigner,
                Section = Field.Section
            };

            AppHost.Usage.ReportCommand(addField, context);
            addField.Execute(context);
        }

        private void HandleGotFocus(object sender, RoutedEventArgs e)
        {
            TemplateDesigner.HandleGotFocus(this);
        }

        private void HandleLostFocus(object sender, RoutedEventArgs e)
        {
            TemplateDesigner.HandleLostFocus(this);
        }

        private void HandleMouseLeftButtonDown([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            TemplateDesigner.HandleMouseLeftButtonDown(this, e);
        }

        private void HandleMouseMove([NotNull] object sender, [NotNull] MouseEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            TemplateDesigner.HandleMouseMove(this, e);
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ContextMenu = null;

            var context = new TemplateDesignerContext
            {
                TemplateDesigner = TemplateDesigner,
                Field = Field
            };

            var commands = Rocks.Commands.CommandManager.GetCommands(context);
            if (!commands.Any())
            {
                e.Handled = true;
                return;
            }

            var contextMenu = new ContextMenu();

            contextMenu.Build(commands, context);

            ContextMenu = contextMenu;
        }

        private void SelectionChanged([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (TemplateDesigner == null)
            {
                return;
            }

            TemplateDesigner.SetModified(true);
        }

        private void SetValidationChecked([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Handled = true;

            var checkBox = sender as CheckBox;
            if (checkBox == null)
            {
                return;
            }

            TemplateDesigner.SetModified(true);
            UpdateValidationsCount();
        }

        private void SetValidationUnchecked([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Handled = true;

            var checkBox = sender as CheckBox;
            if (checkBox == null)
            {
                return;
            }

            TemplateDesigner.SetModified(true);
            UpdateValidationsCount();
        }

        private void TextModified([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            TemplateDesigner.SetModified(true);
        }

        private void ToggleCheckbox([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Handled = true;

            var checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                checkBox.IsChecked = checkBox.IsChecked != true;
            }
        }

        private void Update()
        {
            FieldName.Text = Field.Name;
            FieldSource.Text = Field.Source;
            FieldType.SelectedValue = Field.Type;

            if (Field.Shared)
            {
                Sharing.SelectedIndex = 1;
            }
            else if (Field.Unversioned)
            {
                Sharing.SelectedIndex = 2;
            }
            else
            {
                Sharing.SelectedIndex = 0;
            }

            var fieldTypes = TemplateDesigner.FieldTypes;
            var fieldValidations = TemplateDesigner.FieldValidations;
            if (fieldTypes != null && fieldValidations != null)
            {
                LoadFieldOptions(fieldTypes, fieldValidations);
            }
        }

        private void UpdateValidationsCount()
        {
            var validationsCount = 0;
            foreach (var item in Validations.Items)
            {
                var checkBox = item as CheckBox;
                if (checkBox == null)
                {
                    continue;
                }

                if (checkBox.IsChecked != true)
                {
                    continue;
                }

                var validation = checkBox.Tag as FieldValidationHeader;
                if (validation != null)
                {
                    validationsCount++;
                }
            }

            if (validationsCount == 1)
            {
                ValidationsCount.Content = "1 validation";
            }
            else
            {
                ValidationsCount.Content = string.Format("{0} validations", validationsCount);
            }
        }

        private void ValidationsChanged([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Handled = true;
            UpdateValidationsCount();
            ValidationsCount.IsSelected = true;

            if (TemplateDesigner == null)
            {
                return;
            }

            TemplateDesigner.SetModified(true);
        }
    }
}
