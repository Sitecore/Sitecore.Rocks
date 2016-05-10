// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using System.Drawing.Design;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Dialogs;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.LayoutDesigners.Properties
{
    public class TemplateIdEditor : UITypeEditor
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

            var dialog = new AddFromTemplateDialog
            {
                DatabaseUri = item.ItemUri.DatabaseUri
            };

            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return value;
            }

            var selectedTemplate = dialog.SelectedTemplate;
            if (selectedTemplate == null)
            {
                return value;
            }

            return selectedTemplate.TemplateUri.ItemId.ToString();
        }

        public override UITypeEditorEditStyle GetEditStyle([CanBeNull] ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}
