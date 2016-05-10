// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using System.Drawing.Design;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Dialogs.SelectFileDialogs;

namespace Sitecore.Rocks.UI.LayoutDesigners.Properties
{
    public class FilePathEditor : UITypeEditor
    {
        public override object EditValue(ITypeDescriptorContext context, [CanBeNull] IServiceProvider provider, object value)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            var renderingItem = context.Instance as IItem;
            if (renderingItem == null)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                return value;
            }

            var dialog = new SelectFileDialog
            {
                Site = renderingItem.ItemUri.Site
            };

            if (!dialog.ShowDialog())
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                return value;
            }

            return dialog.SelectedFilePath.Replace("\\", "/");
        }

        public override UITypeEditorEditStyle GetEditStyle([CanBeNull] ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}
