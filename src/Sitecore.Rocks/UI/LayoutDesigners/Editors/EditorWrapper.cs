// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using System.Drawing.Design;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Shell.ComponentModel;

namespace Sitecore.Rocks.UI.LayoutDesigners.Editors
{
    public class EditorWrapper : UITypeEditor
    {
        [CanBeNull]
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (context == null)
            {
                return value;
            }

            var descriptor = context.PropertyDescriptor as DynamicPropertyDescriptor;
            if (descriptor == null)
            {
                return value;
            }

            if (descriptor.UITypeEditor == null)
            {
                return value;
            }

            return descriptor.UITypeEditor.EditValue(context, provider, value);
        }

        public override UITypeEditorEditStyle GetEditStyle([CanBeNull] ITypeDescriptorContext context)
        {
            if (context == null)
            {
                return UITypeEditorEditStyle.Modal;
            }

            var descriptor = context.PropertyDescriptor as DynamicPropertyDescriptor;
            if (descriptor == null)
            {
                return UITypeEditorEditStyle.None;
            }

            if (descriptor.UITypeEditor == null)
            {
                return UITypeEditorEditStyle.None;
            }

            return descriptor.UITypeEditor.GetEditStyle(context);
        }
    }
}
