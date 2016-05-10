// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Web.UI;
using Sitecore.Data.Items;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensions.QueryExtensions;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public abstract class ItemsOpcode : Opcode
    {
        protected ItemsOpcode([CanBeNull] Opcode from)
        {
            From = from;
        }

        [CanBeNull]
        protected Opcode From { get; set; }

        [NotNull]
        public override object Evaluate([NotNull] Query query, [NotNull] QueryContext contextNode)
        {
            Assert.ArgumentNotNull(query, nameof(query));
            Assert.ArgumentNotNull(contextNode, nameof(contextNode));

            return query.FormatItemsAffected(Execute(query, contextNode));
        }

        public override void Print([NotNull] HtmlTextWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            PrintIndent(output);
            PrintLine(output, GetType().Name);

            if (From != null)
            {
                output.Indent++;
                From.Print(output);
                output.Indent--;
            }
        }

        protected abstract void Execute([NotNull] Item item);

        private int Execute([NotNull] Query query, [NotNull] QueryContext contextNode)
        {
            Debug.ArgumentNotNull(query, nameof(query));
            Debug.ArgumentNotNull(contextNode, nameof(contextNode));

            object o = contextNode;

            var from = From;
            if (from != null)
            {
                o = query.Evaluate(from, contextNode);
                if (o == null)
                {
                    return 0;
                }
            }

            var items = query.GetItems(o).ToList();
            foreach (var item in items)
            {
                Execute(item);
            }

            return items.Count();
        }
    }
}
