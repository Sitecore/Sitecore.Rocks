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

namespace Sitecore.Rocks.UI.LayoutDesigners.Properties.PlaceHolders
{
    public class PlaceholderKeyTypeEditor : UITypeEditor
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

            var key = string.Empty;

            var placeHolderKey = value as PlaceHolderKey;
            if (placeHolderKey != null)
            {
                key = placeHolderKey.Key ?? string.Empty;
            }

            var dialog = new PlaceholderDialog();

            dialog.Initialize(Resources.Browse, key, container);
            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return value;
            }

            return new PlaceHolderKey(dialog.SelectedValue);
        }

        public override UITypeEditorEditStyle GetEditStyle([CanBeNull] ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}
