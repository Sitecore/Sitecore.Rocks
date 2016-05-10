// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentTrees.VirtualItems
{
    [MeansImplicitUse]
    public class VirtualItemAttribute : ExtensibilityAttribute
    {
        public VirtualItemAttribute(double priority)
        {
            Priority = priority;
        }

        public double Priority { get; set; }

        public override void Initialize([NotNull] Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            VirtualItemManager.LoadType(type, this);
        }
    }
}
