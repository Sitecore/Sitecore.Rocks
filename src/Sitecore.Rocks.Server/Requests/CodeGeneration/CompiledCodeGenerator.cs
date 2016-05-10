// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Specialized;
using System.Reflection;
using Sitecore.CodeDom.Compiler;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.CodeGeneration
{
    public class CompiledCodeGenerator
    {
        [NotNull]
        public string Execute([NotNull] string code)
        {
            Assert.ArgumentNotNull(code, nameof(code));

            var compiler = new CSharpCompiler();

            var assembly = compiler.SourceToMemory(code, new StringCollection());
            if (assembly == null)
            {
                return string.Empty;
            }

            var type = assembly.GetType("Sitecore.Generator");
            if (type == null)
            {
                return string.Empty;
            }

            var constructorInfo = type.GetConstructor(Type.EmptyTypes);
            if (constructorInfo == null)
            {
                return string.Empty;
            }

            var instance = constructorInfo.Invoke(null);
            if (instance == null)
            {
                return string.Empty;
            }

            var method = type.GetMethod("Execute", BindingFlags.Instance | BindingFlags.Public);
            if (method == null)
            {
                return string.Empty;
            }

            var value = method.Invoke(instance, null);
            if (value == null)
            {
                return string.Empty;
            }

            return value.ToString();
        }
    }
}
