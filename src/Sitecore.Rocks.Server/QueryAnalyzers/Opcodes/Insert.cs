// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Data.Items;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using Sitecore.Rocks.Server.Extensions.QueryExtensions;
using Sitecore.Rocks.Server.Pipelines.SetFieldValue;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class Insert : Opcode
    {
        public Insert([NotNull] List<string> columns, [NotNull] List<Opcode> values)
        {
            Assert.ArgumentNotNull(columns, nameof(columns));
            Assert.ArgumentNotNull(values, nameof(values));

            Columns = columns;
            Values = values;
        }

        [NotNull]
        public List<string> Columns { get; set; }

        [NotNull]
        public List<Opcode> Values { get; set; }

        [NotNull]
        public override object Evaluate([NotNull] Query query, [NotNull] QueryContext contextNode)
        {
            Assert.ArgumentNotNull(query, nameof(query));
            Assert.ArgumentNotNull(contextNode, nameof(contextNode));

            TemplateItem templateItem = null;
            Item path = null;
            string itemName = null;

            for (var index = Columns.Count - 1; index >= 0; index--)
            {
                var column = Columns[index];
                var value = Values[index];

                if (column == "@templateitem")
                {
                    var templateContextNode = new QueryContext(contextNode.Queryable, contextNode.GetQueryContextItem().Database.GetRootItem().ID);

                    templateItem = query.GetItem(templateContextNode, value);

                    if (templateItem == null)
                    {
                        throw new QueryException("@@templateitem column does not evaluate to a single item");
                    }

                    Columns.Remove(column);
                    Values.Remove(value);

                    continue;
                }

                if (column == "@itemname")
                {
                    itemName = query.GetString(contextNode, value);
                    if (itemName == null)
                    {
                        throw new QueryException("@@itemname column does not evaluate to a string");
                    }

                    Columns.Remove(column);
                    Values.Remove(value);

                    continue;
                }

                if (column == "@path")
                {
                    var pathContextNode = new QueryContext(contextNode.Queryable, contextNode.GetQueryContextItem().Database.GetRootItem().ID);

                    path = query.GetItem(pathContextNode, value);

                    if (path == null)
                    {
                        throw new QueryException("@@path column does not evaluate to a single item");
                    }

                    Columns.Remove(column);
                    Values.Remove(value);
                }
            }

            if (templateItem == null)
            {
                throw new QueryException("@@templateitem column missing");
            }

            if (itemName == null)
            {
                throw new QueryException("@@itemname column missing");
            }

            if (path == null)
            {
                throw new QueryException("@@path column missing");
            }

            InsertItem(query, templateItem, itemName, path);

            return query.FormatItemsAffected(1);
        }

        private void InsertItem(Query query, [NotNull] TemplateItem templateItem, [NotNull] string itemName, [NotNull] Item parent)
        {
            Debug.ArgumentNotNull(templateItem, nameof(templateItem));
            Debug.ArgumentNotNull(itemName, nameof(itemName));
            Debug.ArgumentNotNull(parent, nameof(parent));

            var isEditing = false;

            var item = parent.Add(itemName, templateItem);

            for (var index = 0; index < Values.Count; index++)
            {
                var columnName = Columns[index];
                var valueExpression = Values[index];

                var obj = query.EvaluateSubQuery(valueExpression, item);

                var value = obj != null ? obj.ToString() : string.Empty;
                if (value == null)
                {
                    continue;
                }

                if (!isEditing)
                {
                    item.Editing.BeginEdit();
                    isEditing = true;
                }

                SetFieldValuePipeline.Run().WithParameters(item, columnName, value);
            }

            if (isEditing)
            {
                item.Editing.EndEdit();
            }
        }
    }
}
