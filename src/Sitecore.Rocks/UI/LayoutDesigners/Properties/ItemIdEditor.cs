// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using System.Drawing.Design;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Dialogs.SelectItemDialogs;

namespace Sitecore.Rocks.UI.LayoutDesigners.Properties
{
    public class ItemIdEditor : UITypeEditor
    {
        [CanBeNull]
        public override object EditValue(ITypeDescriptorContext context, [CanBeNull] IServiceProvider provider, object value)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            var item = context.Instance as IItem;
            if (item == null)
            {
                return value;
            }

            var selectedItem = value as string ?? string.Empty;

            var dialog = new SelectItemDialog()
            {
                Title = Resources.Browse,
                DatabaseUri = item.ItemUri.DatabaseUri,
                InitialItemPath = selectedItem
            };

            if (!dialog.ShowDialog())
            {
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
