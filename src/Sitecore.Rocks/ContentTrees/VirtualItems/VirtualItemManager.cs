// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentTrees.VirtualItems
{
    [ExtensibilityInitialization(PreInit = @"Clear", PostInit = "Sort")]
    public static class VirtualItemManager
    {
        private static readonly List<VirtualItemDescriptor> virtualItems = new List<VirtualItemDescriptor>();

        [NotNull]
        public static IEnumerable<VirtualItemDescriptor> VirtualItems
        {
            get { return virtualItems; }
        }

        [UsedImplicitly]
        public static void Clear()
        {
            virtualItems.Clear();
        }

        [NotNull, UsedImplicitly]
        public static IEnumerable<BaseTreeViewItem> GetChildren([CanBeNull] BaseTreeViewItem parent)
        {
            var result = new List<BaseTreeViewItem>();

            foreach (var descriptor in virtualItems)
            {
                if (!descriptor.Instance.CanAddItem(parent))
                {
                    continue;
                }

                var item = descriptor.Instance.GetItem(parent);

                result.Add(item);
            }

            return result;
        }

        public static void LoadType([NotNull] Type type, [NotNull] VirtualItemAttribute virtualItemAttribute)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(virtualItemAttribute, nameof(virtualItemAttribute));

            IVirtualItem instance;
            try
            {
                var constructorInfo = type.GetConstructor(Type.EmptyTypes);
                if (constructorInfo == null)
                {
                    Trace.TraceError("Virtual Item constructor not found");
                    return;
                }

                instance = constructorInfo.Invoke(null) as IVirtualItem;
            }
            catch
            {
                Trace.TraceError("Virtual Item threw an exception in the constructor");
                return;
            }

            if (instance == null)
            {
                Trace.TraceError("Virtual Item does not have a parameterless constructor");
                return;
            }

            var descriptor = new VirtualItemDescriptor
            {
                Instance = instance,
                Priority = virtualItemAttribute.Priority
            };

            virtualItems.Add(descriptor);
        }

        [UsedImplicitly]
        public static void Sort()
        {
            virtualItems.Sort((i1, i2) => i1.Priority.CompareTo(i2.Priority));
        }

        public class VirtualItemDescriptor
        {
            public IVirtualItem Instance { get; set; }

            public double Priority { get; set; }
        }
    }
}
