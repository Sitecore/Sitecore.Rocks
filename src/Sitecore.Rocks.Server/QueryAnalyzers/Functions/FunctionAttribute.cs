// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensibility;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Functions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true), BaseTypeRequired(typeof(IFunction))]
    public class FunctionAttribute : ExtensibilityAttribute
    {
        public FunctionAttribute([NotNull] string functionName)
        {
            Assert.ArgumentNotNull(functionName, nameof(functionName));

            FunctionName = functionName;
        }

        [CanBeNull]
        public string Example { get; set; }

        [NotNull]
        public string FunctionName { get; private set; }

        [CanBeNull]
        public string LongHelp { get; set; }

        [CanBeNull]
        public string ShortHelp { get; set; }

        public override void Initialize([NotNull] Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            QueryAnalyzerManager.LoadType(type, this);
        }
    }
}
