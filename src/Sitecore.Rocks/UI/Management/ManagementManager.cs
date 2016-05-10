// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.UI.Management
{
    [ExtensibilityInitialization(PreInit = "Clear")]
    public static class ManagementManager
    {
        private static readonly List<ManagementItemDescriptor> items = new List<ManagementItemDescriptor>();

        [NotNull]
        public static IEnumerable<ManagementItemDescriptor> Items
        {
            get { return items; }
        }

        [UsedImplicitly]
        public static void Clear()
        {
            items.Clear();
        }

        public static void LoadType([NotNull] Type type, [NotNull] ManagementAttribute attribute)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(attribute, nameof(attribute));

            var descriptor = new ManagementItemDescriptor
            {
                Type = type,
                Priority = attribute.Priority,
                Header = attribute.Category
            };

            items.Add(descriptor);
        }

        public class ManagementItemDescriptor
        {
            public string Header { get; set; }

            public double Priority { get; set; }

            public Type Type { get; set; }

            [CanBeNull]
            public IManagementItem GetManagementItem()
            {
                var constructor = Type.GetConstructor(Type.EmptyTypes);
                if (constructor == null)
                {
                    return null;
                }

                return constructor.Invoke(null) as IManagementItem;
            }
        }
    }
}
