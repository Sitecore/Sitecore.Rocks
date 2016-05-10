// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using System.Drawing.Design;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Dialogs.SelectItemDialogs;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.Properties
{
    public class DataSourceTypeEditor : UITypeEditor
    {
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            Assert.ArgumentNotNull(context, nameof(context));
            Assert.ArgumentNotNull(provider, nameof(provider));

            var renderingItem = context.Instance as RenderingItem;
            if (renderingItem == null)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                return value;
            }

            var selectedItem = value as string ?? string.Empty;

            var dialog = new SelectItemDialog
            {
                Title = Resources.Browse,
                DatabaseUri = renderingItem.ItemUri.DatabaseUri,
                InitialItemPath = selectedItem,
                ShowAllDatabases = true
            };

            if (!dialog.ShowDialog())
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                return value;
            }

            return dialog.SelectedItemUri.ItemId.ToString();
        }

        public override UITypeEditorEditStyle GetEditStyle([CanBeNull] ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}
