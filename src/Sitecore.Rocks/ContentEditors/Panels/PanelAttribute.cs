// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentEditors.Panels
{
    [Localizable(false), MeansImplicitUse]
    public class PanelAttribute : ExtensibilityAttribute
    {
        private readonly string name;

        public PanelAttribute([NotNull] string name, double priority)
        {
            Assert.ArgumentNotNull(name, nameof(name));

            this.name = name;
            Priority = priority;
        }

        public bool EnabledByDefault { get; set; }

        [NotNull]
        public string Name
        {
            get { return name; }
        }

        public double Priority { get; set; }

        public override void Initialize([NotNull] Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            PanelManager.LoadType(type, this);
        }
    }
}
