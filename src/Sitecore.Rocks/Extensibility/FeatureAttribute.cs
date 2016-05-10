// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Extensibility
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class FeatureAttribute : Attribute
    {
        public FeatureAttribute([NotNull] string featureName)
        {
            Assert.ArgumentNotNull(featureName, nameof(featureName));

            FeatureName = featureName;
        }

        [NotNull]
        public string FeatureName { get; private set; }
    }
}
