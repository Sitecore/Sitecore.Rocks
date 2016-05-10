// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.Media.Skins
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false), MeansImplicitUse]
    public class MediaSkinAttribute : ExtensibilityAttribute
    {
        public MediaSkinAttribute([NotNull, Localizable(false)] string skinName, double priority)
        {
            Assert.ArgumentNotNull(skinName, nameof(skinName));

            SkinName = skinName;
            Priority = priority;
        }

        public double Priority { get; private set; }

        [Localizable(false)]
        public string SkinName { get; private set; }

        public override void Initialize([NotNull] Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            MediaSkinManager.LoadType(type, this);
        }
    }
}
