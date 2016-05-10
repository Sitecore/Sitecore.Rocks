// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentEditors.InfoPanes
{
    [ExtensibilityInitialization(PreInit = "Clear")]
    public static class InfoPaneManager
    {
        private static readonly string infoPaneInterface = typeof(IInfoPane).FullName;

        private static readonly List<InfoPaneDescriptor> panes = new List<InfoPaneDescriptor>();

        [NotNull]
        public static IEnumerable<InfoPaneDescriptor> Panes
        {
            get { return panes; }
        }

        public static void Add([NotNull] InfoPaneDescriptor descriptor)
        {
            Assert.ArgumentNotNull(descriptor, nameof(descriptor));

            panes.Add(descriptor);
        }

        [UsedImplicitly]
        public static void Clear()
        {
            panes.Clear();
        }

        public static void LoadType([NotNull] Type type, [NotNull] InfoPaneAttribute attribute)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(attribute, nameof(attribute));

            var i = type.GetInterface(infoPaneInterface);
            if (i == null)
            {
                return;
            }

            var descriptor = new InfoPaneDescriptor(type, attribute.PaneName, attribute.Priority);

            Add(descriptor);
        }

        public static void Remove([NotNull] InfoPaneDescriptor descriptor)
        {
            Assert.ArgumentNotNull(descriptor, nameof(descriptor));

            panes.Remove(descriptor);
        }

        public class InfoPaneDescriptor
        {
            public InfoPaneDescriptor([NotNull] Type type, [NotNull] string header, double priority)
            {
                Assert.ArgumentNotNull(type, nameof(type));
                Assert.ArgumentNotNull(header, nameof(header));

                Priority = priority;
                Type = type;
                Header = header;
            }

            [NotNull]
            public string Header { get; private set; }

            public double Priority { get; private set; }

            [NotNull]
            public Type Type { get; }

            [NotNull]
            public IInfoPane GetInstance()
            {
                return (IInfoPane)Activator.CreateInstance(Type);
            }
        }
    }
}
