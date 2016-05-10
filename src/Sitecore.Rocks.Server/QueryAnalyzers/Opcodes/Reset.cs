// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Sitecore.Data.Items;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensions.QueryExtensions;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class Reset : Opcode
    {
        public Reset([NotNull] List<string> columns, [CanBeNull] Opcode from)
        {
            Assert.ArgumentNotNull(columns, nameof(columns));

            Columns = columns;
            From = @from;
        }

        [NotNull]
        public List<string> Columns { get; }

        [CanBeNull]
        public Opcode From { get; }

        [NotNull]
        public override object Evaluate([NotNull] Query query, [NotNull] QueryContext contextNode)
        {
            Assert.ArgumentNotNull(query, nameof(query));
            Assert.ArgumentNotNull(contextNode, nameof(contextNode));

            object o = contextNode;

            var from = From;
            if (@from != null)
            {
                o = query.Evaluate(@from, contextNode);
                if (o == null)
                {
                    return query.FormatItemsAffected(0);
                }
            }

            var items = query.GetItems(o);
            foreach (var item in items)
            {
                ResetItem(item);
            }

            return query.FormatItemsAffected(items.Count());
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

        private void ResetItem([NotNull] Item item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var isEditing = false;

            foreach (var column in Columns)
            {
                var field = item.Fields[column];
                if (field == null)
                {
                    continue;
                }

                if (!isEditing)
                {
                    item.Editing.BeginEdit();
                    isEditing = true;
                }

                field.Reset();
            }

            if (isEditing)
            {
                item.Editing.EndEdit();
            }
        }
    }
}
