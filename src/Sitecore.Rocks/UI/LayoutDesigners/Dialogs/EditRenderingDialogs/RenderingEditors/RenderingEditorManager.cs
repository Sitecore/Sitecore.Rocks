// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.Dialogs.EditRenderingDialogs.RenderingEditors
{
    [ExtensibilityInitialization(PreInit = "Clear")]
    public static class RenderingEditorManager
    {
        private static readonly List<RenderingEditorDescriptor> descriptors = new List<RenderingEditorDescriptor>();

        [UsedImplicitly]
        public static void Clear()
        {
            descriptors.Clear();
        }

        [CanBeNull]
        public static IRenderingEditor GetRenderingEditor([NotNull] RenderingItem renderingItem)
        {
            Assert.ArgumentNotNull(renderingItem, nameof(renderingItem));

            var renderingId = renderingItem.ItemId;

            var descriptor = descriptors.FirstOrDefault(d => string.Compare(d.RenderingId, renderingId, StringComparison.InvariantCultureIgnoreCase) == 0);
            if (descriptor == null)
            {
                return null;
            }

            var instance = Activator.CreateInstance(descriptor.Type) as IRenderingEditor;
            if (instance == null)
            {
                return null;
            }

            instance.RenderingItem = renderingItem;

            return instance;
        }

        public static void LoadType([NotNull] Type type, [NotNull] RenderingEditorAttribute attribute)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(attribute, nameof(attribute));

            var descriptor = new RenderingEditorDescriptor(type, attribute.RenderingId, attribute);

            descriptors.Add(descriptor);
        }

        public class RenderingEditorDescriptor
        {
            public RenderingEditorDescriptor([NotNull] Type type, [NotNull] string renderingId, [NotNull] RenderingEditorAttribute attribute)
            {
                Assert.ArgumentNotNull(type, nameof(type));
                Assert.ArgumentNotNull(renderingId, nameof(renderingId));
                Assert.ArgumentNotNull(attribute, nameof(attribute));

                Type = type;
                RenderingId = renderingId;
                Attribute = attribute;
            }

            [NotNull]
            public RenderingEditorAttribute Attribute { get; private set; }

            [NotNull]
            public string RenderingId { get; }

            [NotNull]
            public Type Type { get; }
        }
    }
}
