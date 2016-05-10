// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentEditors
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false), MeansImplicitUse]
    public class FieldControlAttribute : ExtensibilityAttribute
    {
        public FieldControlAttribute([NotNull, Localizable(false)] string typeName)
        {
            Assert.ArgumentNotNull(typeName, nameof(typeName));

            TypeName = typeName;
        }

        [NotNull, Localizable(false)]
        public string TypeName { get; private set; }

        public override void Initialize(Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            FieldTypeManager.LoadType(type, this);
        }
    }
}
