// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.CodeGeneration.CodeFirstTemplates.FieldTypes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true), BaseTypeRequired(typeof(IFieldTypeHandler))]
    public class FieldTypeHandlerAttribute : ExtensibilityAttribute
    {
        public override void Initialize([NotNull] Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            FieldTypeHandlerManager.LoadType(type, this);
        }
    }
}
