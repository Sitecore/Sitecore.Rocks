// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.UI.Packages.PackageManagers.Sources
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false), BaseTypeRequired(typeof(IPackageSource)), MeansImplicitUse]
    public class PackageSourceAttribute : ExtensibilityAttribute
    {
        public PackageSourceAttribute([Localizable(false), NotNull] string sectionName, [NotNull] string name, double priority)
        {
            Assert.ArgumentNotNull(sectionName, nameof(sectionName));
            Assert.ArgumentNotNull(name, nameof(name));

            SectionName = sectionName;
            Name = name;
            Priority = priority;
        }

        [NotNull]
        public string Name { get; set; }

        public double Priority { get; private set; }

        [NotNull]
        public string SectionName { get; set; }

        public override void Initialize([NotNull] Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            PackageSourceManager.LoadType(type, this);
        }
    }
}
