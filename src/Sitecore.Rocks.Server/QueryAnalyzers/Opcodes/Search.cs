// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Web.UI;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Requests.Indexes;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class Search : Opcode
    {
        public Search([NotNull] string literal)
        {
            Assert.ArgumentNotNull(literal, nameof(literal));

            Literal = literal;
        }

        [NotNull]
        public string Literal { get; }

        [CanBeNull]
        public override object Evaluate([NotNull] Query query, [NotNull] QueryContext contextNode)
        {
            Assert.ArgumentNotNull(query, nameof(query));
            Assert.ArgumentNotNull(contextNode, nameof(contextNode));

            return LuceneRequest.Evaluate(query, contextNode, Literal);
        }

        public override void Print([NotNull] HtmlTextWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            PrintIndent(output);
            PrintLine(output, GetType().Name);
        }
    }
}
