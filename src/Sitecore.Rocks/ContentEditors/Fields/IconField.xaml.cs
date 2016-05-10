// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors.Dialogs;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ContextMenuExtensions;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;

namespace Sitecore.Rocks.ContentEditors.Fields
{
    [FieldControl("icon")]
    public partial class IconField : IReusableFieldControl
    {
        private Field sourceField;

        public IconField()
        {
            InitializeComponent();
        }

        public Control GetFocusableControl()
        {
            return TextBox;
        }

        public string GetValue()
        {
            return TextBox.Text ?? string.Empty;
        }

        public bool IsSupported(Field sourceField)
        {
            Assert.ArgumentNotNull(sourceField, nameof(sourceField));

            return true;
        }

        public void ReloadImage()
        {
            Icon icon;

            var path = TextBox.Text;

            if (string.IsNullOrEmpty(path))
            {
                icon = new Icon("Resources/128x128/selection.png");
            }
            else
            {
                path = @"/sitecore/shell/~/icon/" + path.Replace(@"16x16", @"48x48").Replace(@"24x24", @"48x48").Replace(@"32x32", @"48x48");
                icon = new Icon(sourceField.FieldUris.First().Site, path);
            }

            Image.Source = icon.GetSource();
        }

        public void SetField(Field sourceField)
        {
            Assert.ArgumentNotNull(sourceField, nameof(sourceField));

            this.sourceField = sourceField;
            TextBox.Text = sourceField.Value;
        }

        public void SetValue(string value)
        {
            Assert.ArgumentNotNull(value, nameof(value));

            var changed = TextBox.Text != value;

            TextBox.Text = value;
            ReloadImage();

            if (!changed)
            {
                return;
            }

            var modified = ValueModified;
            if (modified != null)
            {
                modified();
            }
        }

        public void UnsetField()
        {
            sourceField = null;
        }

        public event ValueModifiedEventHandler ValueModified;

        private void Browse([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var d = new SetIconDialog();
            d.Initialize(sourceField.FieldUris.First().ItemVersionUri.Site, TextBox.Text);
            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            TextBox.Text = d.FileName;
        }

        Control IFieldControl.GetControl()
        {
            return this;
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ContextMenu = null;

            var contentEditor = this.GetAncestorOrSelf<ContentEditor>();
            if (contentEditor == null)
            {
                return;
            }

            var fld = sourceField;
            if (fld == null)
            {
                e.Handled = true;
                return;
            }

            var context = new ContentEditorFieldContext(contentEditor, fld, this);

            var commands = CommandManager.GetCommands(context);
            if (!commands.Any())
            {
                e.Handled = true;
                return;
            }

            var contextMenu = new ContextMenu();

            contextMenu.Build(commands, context);

            ContextMenu = contextMenu;
        }

        private void TextChanged([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ReloadImage();

            var iconFieldId = IdManager.GetFieldId("/sitecore/templates/System/Templates/Sections/Appearance/Appearance/__Icon");

            if (sourceField.FieldUris.First().FieldId == iconFieldId)
            {
                UpdatedContentEditorIcon();
            }

            var valueModified = ValueModified;
            if (valueModified != null)
            {
                valueModified();
            }
        }

        private void UpdatedContentEditorIcon()
        {
            var frameworkElement = sourceField.Control as FrameworkElement;
            if (frameworkElement == null)
            {
                return;
            }

            var contentEditor = frameworkElement.GetAncestorOrSelf<ContentEditor>();
            if (contentEditor == null)
            {
                return;
            }

            var control = contentEditor.AppearanceOptions.Skin.GetControl();

            var image = control.FindChild<Image>(@"QuickInfoIcon");
            if (image == null)
            {
                return;
            }

            image.Source = Image.Source;
        }
    }
}
