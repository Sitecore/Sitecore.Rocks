// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ContextMenuExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.ContentEditors.Skins.Default
{
    public partial class Label
    {
        private Field field;

        public Label()
        {
            InitializeComponent();
        }

        [NotNull]
        public ContentEditor ContentEditor { get; set; }

        [CanBeNull]
        public Field Field
        {
            get { return field; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                field = value;
                Render(field);
            }
        }

        public bool ShowColon
        {
            get { return Colon.Visibility == Visibility.Visible; }

            set { Colon.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
        }

        private void Render([NotNull] Field sourceField)
        {
            Debug.ArgumentNotNull(sourceField, nameof(sourceField));

            HelpTextBlock.Visibility = Visibility.Collapsed;
            InformationTextBlock.Visibility = Visibility.Collapsed;

            if (ContentEditor.AppearanceOptions.FieldDisplayTitles)
            {
                TextRun.Text = string.IsNullOrWhiteSpace(sourceField.Title) ? sourceField.Name : sourceField.Title;
            }
            else
            {
                TextRun.Text = sourceField.Name;
            }

            var helpText = string.Empty;
            if (!string.IsNullOrEmpty(sourceField.ToolTip))
            {
                helpText = sourceField.ToolTip;
                if (helpText.EndsWith("."))
                {
                    helpText = helpText.Left(helpText.Length - 1);
                }

                helpText = "- " + helpText;
            }

            HelpTextBlock.Text = helpText;
            HelpTextBlock.Visibility = string.IsNullOrEmpty(helpText) ? Visibility.Collapsed : Visibility.Visible;

            if (!ContentEditor.AppearanceOptions.FieldInformation)
            {
                return;
            }

            var infoText = string.Empty;
            if (sourceField.Shared)
            {
                infoText = Rocks.Resources.Label_Update_shared;
            }

            if (sourceField.Unversioned)
            {
                if (!string.IsNullOrEmpty(infoText))
                {
                    infoText += @", ";
                }

                infoText += Rocks.Resources.Label_Update_unversioned;
            }

            if (sourceField.StandardValue)
            {
                if (!string.IsNullOrEmpty(infoText))
                {
                    infoText += @", ";
                }

                infoText += Rocks.Resources.Label_Update_standard_value;
            }

            if (!string.IsNullOrEmpty(infoText))
            {
                infoText += @", ";
            }

            infoText += sourceField.Type;

            if (!string.IsNullOrEmpty(infoText))
            {
                infoText = string.Format(@"[{0}]", infoText);
            }

            InformationTextBlock.Text = infoText;
            InformationTextBlock.Visibility = string.IsNullOrEmpty(infoText) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ShowContextMenu([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ContextMenu = null;

            var fld = Field;
            if (fld == null)
            {
                return;
            }

            var context = new ContentEditorFieldContext(ContentEditor, fld, this);

            var commands = CommandManager.GetCommands(context);
            if (!commands.Any())
            {
                return;
            }

            var contextMenu = new ContextMenu
            {
                Placement = PlacementMode.Bottom,
                PlacementTarget = this
            };

            contextMenu.Build(commands, context);

            ContextMenu = contextMenu;

            ContextMenu.IsOpen = true;

            e.Handled = true;
        }
    }
}
