// © 2015-2017 by Jakob Christensen. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Extensions.StringExtensions;
using Sitecore.Rocks.Server.Extensions.QueryExtensions;
using Sitecore.Rocks.Server.Pipelines.SetFieldValue;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class Update : Opcode
    {
        public Update([NotNull] List<ColumnExpression> columnExpressions, [CanBeNull] Opcode from)
        {
            Assert.ArgumentNotNull(columnExpressions, nameof(columnExpressions));

            ColumnExpressions = columnExpressions;
            From = from;
        }

        public List<ColumnExpression> ColumnExpressions { get; set; }

        [CanBeNull]
        public Opcode From { get; set; }

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
            foreach (var item in items)
            {
                UpdateItem(query, item);
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

        private void ChangeTemplateById(Item item, string value)
        {
            var templateItem = item.Database.GetItem(value);
            Assert.IsNotNull(templateItem, "Template \"" + value + "\" not found");

            item.ChangeTemplate(templateItem);
        }

        private void ChangeTemplateByName(Item item, string value)
        {
            var template = TemplateManager.GetTemplate(value, item.Database);
            Assert.IsNotNull(template, "Template \"" + value + "\" not found");

            var templateItem = item.Database.GetItem(template.ID);
            Assert.IsNotNull(templateItem, "Template \"" + value + "\" not found");

            item.ChangeTemplate(templateItem);
        }

        private void MoveItem(Item item, string newPath)
        {
            var n = newPath.LastIndexOf('/');
            if (n < 0)
            {
                return;
            }

            var newParentPath = newPath.Left(n);
            var newItemName = newPath.Mid(n + 1);

            var parentItem = item.Database.CreateItemPath(newParentPath);
            if (parentItem == null)
            {
                return;
            }

            item.MoveTo(parentItem);

            using (new EditContext(item))
            {
                item.Name = newItemName;
            }
        }

        private void UpdateItem([NotNull] Query query, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var isEditing = false;
            var newPath = string.Empty;

            foreach (var columnExpression in ColumnExpressions)
            {
                string value = null;

                var columnName = columnExpression.ColumnName;
                if (string.IsNullOrEmpty(columnName))
                {
                    continue;
                }

                if (columnExpression.FieldName != null)
                {
                    value = item[columnExpression.FieldName];
                }
                else if (columnExpression.Expression != null)
                {
                    var obj = query.EvaluateSubQuery(columnExpression.Expression, item);
                    value = obj != null ? obj.ToString() : string.Empty;
                }

                if (value == null)
                {
                    continue;
                }

                if (!isEditing)
                {
                    item.Editing.BeginEdit();
                    isEditing = true;
                }

                switch (columnName.ToLowerInvariant())
                {
                    case "@name":
                        item.Name = value;
                        break;
                    case "@templatename":
                        ChangeTemplateByName(item, value);
                        break;
                    case "@templateid":
                        ChangeTemplateById(item, value);
                        break;
                    case "@path":
                        newPath = value;
                        break;
                    default:
                        SetFieldValuePipeline.Run().WithParameters(item, columnName, value);
                        break;
                }
            }

            if (isEditing)
            {
                item.Editing.EndEdit();
            }

            if (!string.IsNullOrEmpty(newPath))
            {
                MoveItem(item, newPath);
            }
        }
    }
}
