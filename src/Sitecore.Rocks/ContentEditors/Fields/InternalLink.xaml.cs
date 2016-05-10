// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Dialogs.SelectItemDialogs;

namespace Sitecore.Rocks.ContentEditors.Fields
{
    [FieldControl("internal link")]
    public partial class InternalLinkField : IReusableFieldControl
    {
        public InternalLinkField()
        {
            InitializeComponent();
        }

        [CanBeNull]
        public Field SourceField { get; private set; }

        public Control GetFocusableControl()
        {
            return InternalLinkTextBox;
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

        protected void Browse([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var sourceField = SourceField;
            if (sourceField == null)
            {
                return;
            }

            var databaseUri = sourceField.FieldUris.First().ItemVersionUri.DatabaseUri;

            var dialog = new SelectItemDialog()
            {
                Title = Rocks.Resources.Browse,
                DatabaseUri = databaseUri,
                InitialItemPath = InternalLinkTextBox.Text ?? string.Empty
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            dialog.GetSelectedItemPath(delegate(string itemPath)
            {
                InternalLinkTextBox.Text = itemPath;
                RaiseModified();
            });
        }

        protected void RaiseModified()
        {
            var valueModified = ValueModified;
            if (valueModified != null)
            {
                valueModified();
            }
        }

        Control IFieldControl.GetControl()
        {
            return this;
        }

        string IFieldControl.GetValue()
        {
            return InternalLinkTextBox.Text ?? string.Empty;
        }

        private void RaiseModified([NotNull] object sender, [NotNull] TextChangedEventArgs textChangedEventArgs)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(textChangedEventArgs, nameof(textChangedEventArgs));

            RaiseModified();
        }

        void IFieldControl.SetField(Field sourceField)
        {
            Debug.ArgumentNotNull(sourceField, nameof(sourceField));

            SourceField = sourceField;
        }

        void IFieldControl.SetValue(string newValue)
        {
            Debug.ArgumentNotNull(newValue, nameof(newValue));

            InternalLinkTextBox.Text = newValue;
        }
    }
}
