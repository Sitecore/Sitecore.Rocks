// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class FastQuery : Opcode
    {
        public FastQuery([NotNull] string literal)
        {
            Literal = literal;
            Assert.ArgumentNotNull(literal, nameof(literal));
        }

        public string Literal { get; set; }

        [CanBeNull]
        public override object Evaluate([NotNull] Query query, [NotNull] QueryContext contextNode)
        {
            Assert.ArgumentNotNull(query, nameof(query));
            Assert.ArgumentNotNull(contextNode, nameof(contextNode));

            var contextItem = contextNode.GetQueryContextItem();

            var items = contextItem.Database.SelectItems(Literal);
            if (items == null)
            {
                return null;
            }

            var result = new List<QueryContext>();

            foreach (var item in items)
            {
                var queryContext = new QueryContext(item.Database.DataManager, item.ID);

                result.Add(queryContext);
            }

            return result;
        }
    }
}
