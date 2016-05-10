// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensions.QueryExtensions;
using Sitecore.Rocks.Server.QueryAnalyzers.WhereHandlers;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class Import : Opcode
    {
        public Import([CanBeNull] IImportSource importSource)
        {
            ImportSource = importSource;
        }

        protected IImportSource ImportSource { get; set; }

        public override object Evaluate([NotNull] Query query, [NotNull] QueryContext contextNode)
        {
            Assert.ArgumentNotNull(query, nameof(query));
            Assert.ArgumentNotNull(contextNode, nameof(contextNode));

            var items = ImportSource.Execute(contextNode.GetQueryContextItem());

            return query.FormatItemsAffected(items);
        }
    }
}
