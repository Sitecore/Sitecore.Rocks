// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.CodeGeneration
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true), MeansImplicitUse, UsedImplicitly]
    public class CodeGeneratorAttribute : ExtensibilityAttribute
    {
        [Localizable(false)]
        public CodeGeneratorAttribute([NotNull] string name)
        {
            Assert.ArgumentNotNull(name, nameof(name));

            Name = name;
        }

        [NotNull]
        public string Name { get; }

        public override void Initialize(Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            CodeGenerationManager.LoadType(Name, type);
        }
    }
}
