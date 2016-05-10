// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Query;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class Comment : Opcode
    {
        public Comment(string comment)
        {
            CommentText = comment;
        }

        public string CommentText { get; }

        [NotNull]
        public override object Evaluate([NotNull] Query query, [NotNull] QueryContext contextNode)
        {
            Assert.ArgumentNotNull(query, nameof(query));
            Assert.ArgumentNotNull(contextNode, nameof(contextNode));

            return CommentText;
        }
    }
}
