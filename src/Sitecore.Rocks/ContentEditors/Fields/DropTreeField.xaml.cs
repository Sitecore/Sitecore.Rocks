// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Text;
using Sitecore.Rocks.UI.Dialogs.SelectItemDialogs;

namespace Sitecore.Rocks.ContentEditors.Fields
{
    [FieldControl("droptree"), FieldControl("reference"), FieldControl("datasource")]
    public partial class DropTreeField : IReusableFieldControl
    {
        private bool isUpdating;

        public DropTreeField()
        {
            InitializeComponent();

            DatabaseName = string.Empty;
        }

        [NotNull]
        protected string DatabaseName { get; private set; }

        [CanBeNull]
        protected Field SourceField { get; private set; }

        public Control GetControl()
        {
            return this;
        }

        public Control GetFocusableControl()
        {
            return TextBox;
        }

        public string GetValue()
        {
            return TextBox.Text;
        }

        public bool IsSupported(Field sourceField)
        {
            Assert.ArgumentNotNull(sourceField, nameof(sourceField));

            return true;
        }

        public void SetField(Field sourceField)
        {
            Assert.ArgumentNotNull(sourceField, nameof(sourceField));

            SourceField = sourceField;

            SetText(sourceField.Value);
            Path.Text = GetPathFromDisplayData();

            var source = new UrlString(sourceField.Source);

            DatabaseName = source["database"] ?? string.Empty;
            if (string.IsNullOrEmpty(DatabaseName))
            {
                DatabaseName = source["databasename"] ?? string.Empty;
            }
        }

        public void SetValue(string value)
        {
            Assert.ArgumentNotNull(value, nameof(value));

            if (TextBox.Text != value)
            {
                TextBox.Text = value;
            }
        }

        public void UnsetField()
        {
            SourceField = null;
        }

        public event ValueModifiedEventHandler ValueModified;

        private void Browse([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var sourceField = SourceField;
            if (sourceField == null)
            {
                return;
            }

            var itemId = ItemId.Empty;

            var value = TextBox.Text;
            if (!string.IsNullOrEmpty(value))
            {
                Guid guid;
                if (Guid.TryParse(value, out guid))
                {
                    itemId = new ItemId(guid);
                }
            }

            if (itemId == ItemId.Empty)
            {
                itemId = new ItemId(DatabaseTreeViewItem.RootItemGuid);
            }

            var databaseUri = sourceField.FieldUris.First().ItemVersionUri.DatabaseUri;
            if (!string.IsNullOrEmpty(DatabaseName))
            {
                databaseUri = new DatabaseUri(databaseUri.Site, new DatabaseName(DatabaseName));
            }

            var itemUri = new ItemUri(databaseUri, itemId);

            var dialog = new SelectItemDialog();

            dialog.Initialize(Rocks.Resources.Browse, itemUri);

            if (SourceField != null)
            {
                dialog.SettingsKey = "ContentEditor\\DropTree\\" + SourceField.TemplateFieldId;
            }

            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            itemUri = dialog.SelectedItemUri;

            SetText(itemUri.ItemId.ToString());

            dialog.GetSelectedItemPath(path =>
            {
                Path.Text = path;
                RaiseModified();
            });
        }

        [NotNull]
        private string GetPathFromDisplayData()
        {
            var sourceField = SourceField;
            if (sourceField == null)
            {
                return "[Empty]";
            }

            var root = sourceField.DisplayData.ToXElement();
            if (root == null)
            {
                return "[Empty]";
            }

            var element = root.Element("link");
            if (element == null)
            {
                return "[Empty]";
            }

            return element.GetAttributeValue("path");
        }

        private void RaiseModified()
        {
            var valueModified = ValueModified;
            if (valueModified != null)
            {
                valueModified();
            }
        }

        private void SetText([NotNull] string value)
        {
            Debug.ArgumentNotNull(value, nameof(value));

            isUpdating = true;
            try
            {
                TextBox.Text = value;
            }
            finally
            {
                isUpdating = false;
            }
        }

        private void TextChanged([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (isUpdating)
            {
                return;
            }

            RaiseModified();

            Path.Text = "[Field has been updated]";

            var sourceField = SourceField;
            if (sourceField == null)
            {
                return;
            }

            var fieldUri = sourceField.FieldUris.FirstOrDefault();
            if (fieldUri == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(TextBox.Text))
            {
                Path.Text = "[No item]";
                return;
            }

            Path.Text = "[Updating path]";

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result, true))
                {
                    Path.Text = "[Item not found]";
                    return;
                }

                var element = response.ToXElement();
                if (element == null)
                {
                    return;
                }

                var itemHeader = ItemHeader.Parse(fieldUri.DatabaseUri, element);

                Path.Text = itemHeader.Path;
            };

            var databaseUri = fieldUri.DatabaseUri;
            if (!string.IsNullOrEmpty(DatabaseName))
            {
                databaseUri = new DatabaseUri(databaseUri.Site, new DatabaseName(DatabaseName));
            }

            fieldUri.Site.DataService.ExecuteAsync("Items.GetItemHeader", completed, TextBox.Text, databaseUri.DatabaseName.ToString());
        }
    }
}
