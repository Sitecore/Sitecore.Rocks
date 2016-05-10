// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.UI.UpdateServerComponents
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true), MeansImplicitUse]
    public class ServerComponentsUpdaterAttribute : ExtensibilityAttribute
    {
        public override void Initialize([NotNull] Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            UpdateServerComponentsManager.LoadType(type, this);
        }
    }
}
