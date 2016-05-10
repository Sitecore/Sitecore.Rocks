// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.CodeGeneration
{
    [ExtensibilityInitialization(PreInit = "Clear")]
    public static class CodeGenerationManager
    {
        static CodeGenerationManager()
        {
            Generators = new Dictionary<string, Type>();
        }

        [NotNull]
        private static Dictionary<string, Type> Generators { get; }

        [UsedImplicitly]
        public static void Clear()
        {
            Generators.Clear();
        }

        [CanBeNull]
        public static ICodeGenerator GetGenerator([NotNull] string name)
        {
            Assert.ArgumentNotNull(name, nameof(name));

            Type type;

            if (!Generators.TryGetValue(name, out type))
            {
                return null;
            }

            var constructorInfo = type.GetConstructor(Type.EmptyTypes);
            if (constructorInfo == null)
            {
                return null;
            }

            return constructorInfo.Invoke(null) as ICodeGenerator;
        }

        public static void LoadType([NotNull] string name, [NotNull] Type type)
        {
            Assert.ArgumentNotNull(name, nameof(name));
            Assert.ArgumentNotNull(type, nameof(type));

            Generators[name] = type;
        }
    }
}
