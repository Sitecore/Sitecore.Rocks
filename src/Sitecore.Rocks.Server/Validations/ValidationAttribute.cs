// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensibility;

namespace Sitecore.Rocks.Server.Validations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true), MeansImplicitUse]
    public class ValidationAttribute : ExtensibilityAttribute
    {
        public ValidationAttribute([NotNull] string name, [NotNull] string category)
        {
            Assert.ArgumentNotNull(name, nameof(name));
            Assert.ArgumentNotNull(category, nameof(category));

            Name = name;
            Category = category;
        }

        public string Category { get; private set; }

        public bool ExecutePerLanguage { get; set; }

        public string Name { get; private set; }

        public override void Initialize([NotNull] Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            ValidationManager.LoadType(type, this);
        }
    }
}
