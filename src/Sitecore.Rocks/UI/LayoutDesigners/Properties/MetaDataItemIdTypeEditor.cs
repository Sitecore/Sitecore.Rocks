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
    public class MetaDataItemIdTypeEditor : UITypeEditor
    {
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            Assert.ArgumentNotNull(context, nameof(context));
            Assert.ArgumentNotNull(provider, nameof(provider));

            var placeholderItem = context.Instance as PlaceholderItem;
            if (placeholderItem == null)
            {
                return string.Empty;
            }

            var selectedItem = value as string ?? string.Empty;

            var dialog = new SelectItemDialog
            {
                Title = Resources.Browse,
                DatabaseUri = placeholderItem.DatabaseUri,
                InitialItemPath = selectedItem
            };

            if (!dialog.ShowDialog())
            {
                return string.Empty;
            }

            return dialog.SelectedItemUri.ItemId.ToString();
        }

        public override UITypeEditorEditStyle GetEditStyle([CanBeNull] ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}
