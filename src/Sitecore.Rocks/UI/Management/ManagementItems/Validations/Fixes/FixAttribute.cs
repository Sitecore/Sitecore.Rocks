// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Fixes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false), MeansImplicitUse]
    public class FixAttribute : ExtensibilityAttribute
    {
        public override void Initialize([NotNull] Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            FixManager.LoadType(type, this);
        }
    }
}
