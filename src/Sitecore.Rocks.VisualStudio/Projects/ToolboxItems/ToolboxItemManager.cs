// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.Projects.ToolboxItems
{
    [ExtensibilityInitialization(PreInit = "Clear")]
    public static class ToolboxItemManager
    {
        private static readonly Dictionary<string, ToolboxItemHandler> extensions = new Dictionary<string, ToolboxItemHandler>();

        [NotNull]
        public static Dictionary<string, ToolboxItemHandler> Extensions
        {
            get { return extensions; }
        }

        public static void Add([NotNull] string extension, [NotNull] ToolboxItemHandler toolboxItemHandler)
        {
            Assert.ArgumentNotNull(extension, nameof(extension));
            Assert.ArgumentNotNull(toolboxItemHandler, nameof(toolboxItemHandler));

            Extensions.Add(extension.ToLowerInvariant(), toolboxItemHandler);
        }

        public static void Clear()
        {
            extensions.Clear();
        }

        [CanBeNull]
        public static ToolboxItemHandler GetToolboxItemHandler([NotNull] string extension)
        {
            Assert.ArgumentNotNull(extension, nameof(extension));

            extension = extension.ToLowerInvariant();

            ToolboxItemHandler type;
            if (Extensions.TryGetValue(extension, out type))
            {
                return type;
            }

            return null;
        }

        public static void LoadType([NotNull] Type type, [NotNull] ToolboxItemHandlerAttribute attribute)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(attribute, nameof(attribute));

            var toolboxItemHandler = Activator.CreateInstance(type) as ToolboxItemHandler;
            if (toolboxItemHandler == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(attribute.Extension))
            {
                Add(attribute.Extension, toolboxItemHandler);
            }
            else
            {
                toolboxItemHandler.AddToToolbox();
            }
        }
    }
}
