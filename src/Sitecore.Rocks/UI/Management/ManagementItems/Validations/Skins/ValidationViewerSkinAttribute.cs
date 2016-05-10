// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Skins
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false), MeansImplicitUse]
    public class ValidationViewerSkinAttribute : ExtensibilityAttribute
    {
        public ValidationViewerSkinAttribute([NotNull, Localizable(false)]  string skinName, double priority)
        {
            Assert.ArgumentNotNull(skinName, nameof(skinName));

            SkinName = skinName;
            Priority = priority;
        }

        public double Priority { get; set; }

        [NotNull, Localizable(false)]
        public string SkinName { get; private set; }

        public override void Initialize(Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            ValidationViewerSkinManager.LoadType(type, this);
        }
    }
}
