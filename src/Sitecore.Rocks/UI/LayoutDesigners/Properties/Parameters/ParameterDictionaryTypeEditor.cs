// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.LayoutDesigners.Dialogs;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.Properties.Parameters
{
    public class ParameterDictionaryTypeEditor : UITypeEditor
    {
        public override bool IsDropDownResizable => true;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            Assert.ArgumentNotNull(context, nameof(context));
            Assert.ArgumentNotNull(provider, nameof(provider));

            var instance = context.Instance;
            if (instance == null)
            {
                return value;
            }

            IEnumerable<RenderingItem> renderingItems = null;

            var renderingItem = instance as RenderingItem;
            if (renderingItem != null)
            {
                renderingItems = new[]
                {
                    renderingItem
                };
            }

            var objects = instance as object[];
            if (objects != null)
            {
                renderingItems = objects.OfType<RenderingItem>();
            }

            if (renderingItems == null || !renderingItems.Any())
            {
                return value;
            }

            var container = renderingItems.First().RenderingContainer;
            if (container == null)
            {
                return value;
            }

            var parameterDictionary = value as ParameterDictionary;
            if (parameterDictionary == null)
            {
                return value;
            }

            var dialog = new ParametersDialog();

            dialog.Initialize(parameterDictionary.Parameters);
            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return value;
            }

            var result = new ParameterDictionary(dialog.Parameters);

            return result;
        }

        public override UITypeEditorEditStyle GetEditStyle([CanBeNull] ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}
