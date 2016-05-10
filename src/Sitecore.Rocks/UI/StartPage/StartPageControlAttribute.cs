// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.UI.StartPage
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false), BaseTypeRequired(typeof(IStartPageControl)), MeansImplicitUse]
    public class StartPageControlAttribute : ExtensibilityAttribute
    {
        public StartPageControlAttribute(double priority) : this("", priority)
        {
        }

        public StartPageControlAttribute([CanBeNull] string parentName, double priority)
        {
            ParentName = parentName;
            Priority = priority;
        }

        [CanBeNull]
        public string ParentName { get; private set; }

        public double Priority { get; private set; }

        public override void Initialize([NotNull] Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            StartPageManager.LoadType(type, this);
        }
    }
}
