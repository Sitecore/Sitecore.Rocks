// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.Projects.FileItems
{
    [ExtensibilityInitialization(PreInit = "Clear")]
    public static class FileItemManager
    {
        private static readonly List<IFileItemHandler> handlers = new List<IFileItemHandler>();

        [NotNull]
        public static IEnumerable<IFileItemHandler> Handlers
        {
            get { return handlers; }
        }

        public static void Add([NotNull] IFileItemHandler fileItemHandler)
        {
            Assert.ArgumentNotNull(fileItemHandler, nameof(fileItemHandler));

            handlers.Add(fileItemHandler);
        }

        [UsedImplicitly]
        public static void Clear()
        {
            handlers.Clear();
        }

        [CanBeNull]
        public static IFileItemHandler GetFileItemHandler([NotNull] string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            return handlers.FirstOrDefault(h => h.CanHandle(fileName));
        }

        public static void LoadType([NotNull] Type type, [NotNull] FileItemHandlerAttribute attribute)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(attribute, nameof(attribute));

            var fileItemHandler = Activator.CreateInstance(type) as IFileItemHandler;
            if (fileItemHandler == null)
            {
                return;
            }

            Add(fileItemHandler);
        }

        public static void Remove([NotNull] IFileItemHandler fileItemHandler)
        {
            Assert.ArgumentNotNull(fileItemHandler, nameof(fileItemHandler));

            handlers.Remove(fileItemHandler);
        }
    }
}
