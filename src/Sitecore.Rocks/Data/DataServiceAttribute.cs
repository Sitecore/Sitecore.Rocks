// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.Data
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false), MeansImplicitUse]
    public class DataServiceAttribute : ExtensibilityAttribute
    {
        public DataServiceAttribute([NotNull, Localizable(false)] string typeName)
        {
            Assert.ArgumentNotNull(typeName, nameof(typeName));

            TypeName = typeName;
            Priority = 5000;
        }

        public double Priority { get; set; }

        [Localizable(false)]
        public string TypeName { get; set; }

        public override void Initialize([NotNull] Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            DataServiceManager.LoadType(type, this);
        }
    }
}
