// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.UI.Management
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false), MeansImplicitUse]
    public class ManagementAttribute : ExtensibilityAttribute
    {
        public ManagementAttribute([NotNull, Localizable(false)] string category, double priority)
        {
            Assert.ArgumentNotNull(category, nameof(category));

            Category = category;
            Priority = priority;
        }

        [Localizable(false)]
        public string Category { get; set; }

        public double Priority { get; set; }

        public override void Initialize([NotNull] Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            ManagementManager.LoadType(type, this);
        }
    }
}
