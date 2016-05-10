// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Web.UI;
using Sitecore.Collections;
using Sitecore.Data.Items;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensions.QueryExtensions;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class Select : Opcode
    {
        public Select([NotNull] List<ColumnExpression> columnExpressions, [CanBeNull] Opcode @from, [CanBeNull] IEnumerable<OrderByColumn> orderBy, bool isDistinct)
        {
            Assert.ArgumentNotNull(columnExpressions, nameof(columnExpressions));

            ColumnExpressions = columnExpressions;
            From = from;
            OrderBy = orderBy ?? Enumerable.Empty<OrderByColumn>();
            IsDistinct = isDistinct;
        }

        [NotNull]
        public List<ColumnExpression> ColumnExpressions { get; set; }

        [CanBeNull]
        public Opcode From { get; set; }

        public bool IsDistinct { get; set; }

        [NotNull]
        public IEnumerable<OrderByColumn> OrderBy { get; set; }

        [NotNull]
        public override object Evaluate([NotNull] Query query, [NotNull] QueryContext contextNode)
        {
            Assert.ArgumentNotNull(query, nameof(query));
            Assert.ArgumentNotNull(contextNode, nameof(contextNode));

            var result = new SelectDataTable();

            BuildColumns(result);

            object o = contextNode;

            var from = From;
            if (from != null)
            {
                o = query.Evaluate(from, contextNode);
                if (o == null)
                {
                    return result;
                }
            }

            var list = o as QueryContext[];
            if (list != null)
            {
                foreach (var context in list)
                {
                    if (!AddBatchScriptItem(query, result, context.GetQueryContextItem(), list.Count()))
                    {
                        break;
                    }
                }

                if (IsDistinct)
                {
                    MakeDistinct(result);
                }

                if (OrderBy.Any())
                {
                    Sort(result);
                }

                return result;
            }

            var instance = o as QueryContext;
            if (instance != null)
            {
                AddBatchScriptItem(query, result, instance.GetQueryContextItem(), 1);
            }

            return result;
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

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int StrCmpLogicalW(string strA, string strB);

        private bool AddBatchScriptItem([NotNull] Query query, [NotNull] SelectDataTable dataTable, [NotNull] Item item, int count)
        {
            Debug.ArgumentNotNull(dataTable, nameof(dataTable));
            Debug.ArgumentNotNull(item, nameof(item));

            var result = true;

            var selectItem = new SelectItem
            {
                Item = item
            };

            var columnIndex = 0;
            foreach (var columnExpression in ColumnExpressions)
            {
                var selectField = new SelectField();

                if (columnExpression.FieldName != null)
                {
                    selectField.Value = item[columnExpression.FieldName];
                    selectField.Field = item.Fields[columnExpression.FieldName];
                }
                else if (columnExpression.Expression != null)
                {
                    var value = query.EvaluateSubQuery(columnExpression.Expression, item);

                    if (value != null)
                    {
                        selectField.Value = value.ToString();
                    }
                    else
                    {
                        var function = columnExpression.Expression as Function;
                        if (function != null && string.Compare(function.Name, "count", StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            selectField.Value = count.ToString();
                            result = false;
                        }
                        else
                        {
                            selectField.Value = "<null>";
                        }
                    }
                }

                var columnName = columnExpression.ColumnName;
                if (string.IsNullOrEmpty(columnName))
                {
                    columnName = "Column " + columnIndex;
                    columnIndex++;
                }

                selectField.ColumnName = columnName;

                selectItem.Fields.Add(selectField);
            }

            dataTable.Items.Add(selectItem);

            return result;
        }

        private void BuildColumns([NotNull] SelectDataTable result)
        {
            Debug.ArgumentNotNull(result, nameof(result));

            var columnIndex = 0;
            foreach (var columnExpression in ColumnExpressions)
            {
                var columnName = columnExpression.ColumnName;
                if (string.IsNullOrEmpty(columnName))
                {
                    columnName = "Column " + columnIndex;
                    columnIndex++;
                }

                var column = new SelectColumn
                {
                    Header = columnName,
                    IsReadOnly = string.IsNullOrEmpty(columnExpression.FieldName)
                };

                result.Columns.Add(column);
            }
        }

        private int Compare([NotNull] SelectItem x, [NotNull] SelectItem y)
        {
            Debug.ArgumentNotNull(x, nameof(x));
            Debug.ArgumentNotNull(y, nameof(y));

            foreach (var column in OrderBy)
            {
                var value1 = GetOrderByValue(x, column.ColumnName);
                var value2 = GetOrderByValue(y, column.ColumnName);

                if (value1 == null && value2 == null)
                {
                    return 0;
                }

                if (value1 == null)
                {
                    return -1 * column.Direction;
                }

                if (value2 == null)
                {
                    return column.Direction;
                }

                var result = StrCmpLogicalW(value1, value2);
                if (result != 0)
                {
                    return result * column.Direction;
                }
            }

            return 0;
        }

        private int GetHashValue(SelectItem item)
        {
            var value = new StringBuilder();

            foreach (var selectField in item.Fields)
            {
                value.Append("|");
                value.Append(selectField.ColumnName);
                value.Append(":");
                value.Append(selectField.Value);
            }

            return value.ToString().GetHashCode();
        }

        [CanBeNull]
        private string GetOrderByValue([NotNull] SelectItem item, [NotNull] string columnName)
        {
            Debug.ArgumentNotNull(item, nameof(item));
            Debug.ArgumentNotNull(columnName, nameof(columnName));

            /*
      switch (columnName.ToUpperInvariant())
      {
        case "@ID":
          return item.ID.ToString();
        case "@NAME":
          return item.Name;
        case "@TEMPLATENAME":
          return item.TemplateName;
        case "@TEMPLATEID":
          return item.TemplateID.ToString();
        case "@PATH":
          return item.Paths.Path;
      }
      */
            var field = item.Fields.FirstOrDefault(f => f.ColumnName == columnName);
            if (field == null)
            {
                return null;
            }

            return field.Value;
        }

        private void MakeDistinct([NotNull] SelectDataTable dataTable)
        {
            var hash = new List<Pair<int, SelectItem>>();

            foreach (var selectItem in dataTable.Items)
            {
                var hashValue = GetHashValue(selectItem);
                var pair = new Pair<int, SelectItem>(hashValue, selectItem);

                hash.Add(pair);
            }

            hash.Sort((p0, p1) => p0.Part1 == p1.Part1 ? 0 : p0.Part1 > p1.Part1 ? 1 : -1);

            for (var index = hash.Count - 2; index >= 0; index--)
            {
                if (hash[index].Part1 == hash[index + 1].Part1)
                {
                    hash.RemoveAt(index + 1);
                }
            }

            dataTable.Items.Clear();
            dataTable.Items.AddRange(hash.Select(p => p.Part2));
        }

        private void Sort([NotNull] SelectDataTable dataTable)
        {
            dataTable.Items.Sort(Compare);
        }
    }
}
