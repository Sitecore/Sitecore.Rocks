// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentEditors.Skins
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false), MeansImplicitUse]
    public class SkinAttribute : ExtensibilityAttribute
    {
        public SkinAttribute([NotNull, Localizable(false)] string skinName)
        {
            Assert.ArgumentNotNull(skinName, nameof(skinName));

            SkinName = skinName;
        }

        [Localizable(false)]
        public string SkinName { get; set; }

        public override void Initialize([NotNull] Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            SkinManager.LoadType(type, this);
        }
    }
}
