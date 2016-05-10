// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Web.UI;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensions.QueryExtensions;
using Sitecore.Rocks.Server.Packages;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class Package : Opcode
    {
        public Package([NotNull] string fileName, [CanBeNull] Opcode from)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            FileName = fileName;
            From = from;
        }

        public string FileName { get; set; }

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

            var package = new ZipPackageBuilder(FileName);

            var items = query.GetItems(o);
            foreach (var item in items)
            {
                package.Items.Add(item);
            }

            package.Build();

            return items.Count();
        }
    }
}
