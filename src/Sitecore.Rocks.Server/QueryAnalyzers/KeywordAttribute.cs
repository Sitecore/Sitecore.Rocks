// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensibility;

namespace Sitecore.Rocks.Server.QueryAnalyzers
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false), MeansImplicitUse]
    public class KeywordAttribute : ExtensibilityAttribute
    {
        public KeywordAttribute([NotNull] string keyword, int tokenType)
        {
            Assert.ArgumentNotNull(keyword, nameof(keyword));

            Keyword = keyword;
            TokenType = tokenType;
        }

        public string Example { get; set; }

        public string Keyword { get; set; }

        public string LongHelp { get; set; }

        public string ShortHelp { get; set; }

        public string Syntax { get; set; }

        public int TokenType { get; set; }

        public override void Initialize(Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            QueryAnalyzerManager.LoadKeyword(type, this);
        }
    }
}
