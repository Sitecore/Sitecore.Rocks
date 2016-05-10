// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Web.UI;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class ItemVersion : Opcode
    {
        [NotNull]
        public override object Evaluate([NotNull] Query query, [NotNull] QueryContext contextNode)
        {
            Assert.ArgumentNotNull(query, nameof(query));
            Assert.ArgumentNotNull(contextNode, nameof(contextNode));

            var item = contextNode.GetQueryContextItem();
            if (item == null)
            {
                return string.Empty;
            }

            return item.Version.Number;
        }

        public override void Print([NotNull] HtmlTextWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            PrintIndent(output);
            PrintLine(output, GetType().Name);
        }
    }
}
