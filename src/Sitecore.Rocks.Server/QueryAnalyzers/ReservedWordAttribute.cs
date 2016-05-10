// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensibility;

namespace Sitecore.Rocks.Server.QueryAnalyzers
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ReservedWordAttribute : ExtensibilityAttribute
    {
        public ReservedWordAttribute([NotNull] string word, int tokenType)
        {
            Assert.ArgumentNotNull(word, nameof(word));

            Word = word;
            TokenType = tokenType;
        }

        public int TokenType { get; set; }

        public string Word { get; set; }

        public override void Initialize([NotNull] Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            QueryAnalyzerManager.LoadReservedWord(this);
        }
    }
}
