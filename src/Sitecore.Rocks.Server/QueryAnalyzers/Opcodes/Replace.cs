// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Web.UI;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensions.QueryExtensions;
using Sitecore.Rocks.Server.Pipelines.GetFieldValue;
using Sitecore.Rocks.Server.Pipelines.SetFieldValue;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class Replace : Opcode
    {
        public Replace([NotNull] Opcode find, [NotNull] Opcode with, [CanBeNull] string field, [CanBeNull] Opcode from)
        {
            Assert.ArgumentNotNull(find, nameof(find));
            Assert.ArgumentNotNull(with, nameof(with));

            Find = find;
            With = with;
            Field = field;
            From = from;
        }

        [CanBeNull]
        public string Field { get; set; }

        [NotNull]
        public Opcode Find { get; set; }

        [CanBeNull]
        public Opcode From { get; set; }

        [NotNull]
        public Opcode With { get; set; }

        [NotNull]
        public override object Evaluate([NotNull] Query query, [NotNull] QueryContext contextNode)
        {
            Assert.ArgumentNotNull(query, nameof(query));
            Assert.ArgumentNotNull(contextNode, nameof(contextNode));

            object o = contextNode;

            var from = From;
            if (from != null)
            {
                o = query.Evaluate(from, contextNode);
                if (o == null)
                {
                    return query.FormatItemsAffected(0);
                }
            }

            var items = query.GetItems(o);

            var count = 0;
            foreach (var item in items)
            {
                if (UpdateItem(query, item))
                {
                    count++;
                }
            }

            return query.FormatItemsAffected(count);
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

        [NotNull]
        private string GetStringValue(Query query, [NotNull] Opcode expression, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(expression, nameof(expression));
            Debug.ArgumentNotNull(item, nameof(item));

            var obj = query.EvaluateSubQuery(expression, item);

            return obj != null ? obj.ToString() : string.Empty;
        }

        private bool UpdateItem(Query query, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var find = GetStringValue(query, Find, item);
            var with = GetStringValue(query, With, item);

            var result = false;

            item.Editing.BeginEdit();

            if (Field != null)
            {
                var value = GetFieldValuePipeline.Run().WithParameters(item, Field).Value;

                value = value.Replace(find, with);

                if (value != item[Field])
                {
                    SetFieldValuePipeline.Run().WithParameters(item, Field, value);
                    result = true;
                }
            }
            else
            {
                foreach (Field field in item.Fields)
                {
                    var value = GetFieldValuePipeline.Run().WithParameters(field).Value;

                    value = value.Replace(find, with);

                    if (value == field.Value)
                    {
                        continue;
                    }

                    SetFieldValuePipeline.Run().WithParameters(field, value);
                    result = true;
                }
            }

            item.Editing.EndEdit();

            return result;
        }
    }
}
