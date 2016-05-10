// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sitecore.Collections;
using Sitecore.Data;
using Sitecore.Data.DataProviders.Sql;
using Sitecore.Data.Items;
using Sitecore.Data.Query;
using Sitecore.Data.SqlServer;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers;

namespace Sitecore.Rocks.Server.Extensions.QueryExtensions
{
    public static class QueryExtensions
    {
        [CanBeNull]
        public static object Evaluate([NotNull] this Query query, [NotNull] Opcode opcode, [NotNull] Item item)
        {
            Assert.ArgumentNotNull(query, nameof(query));
            Assert.ArgumentNotNull(opcode, nameof(opcode));
            Assert.ArgumentNotNull(item, nameof(item));

            var queryContext = new QueryContext(item.Database.DataManager, item.ID);
            return Evaluate(query, opcode, queryContext);
        }

        [CanBeNull]
        public static object Evaluate([NotNull] this Query query, [NotNull] Opcode opcode, [NotNull] QueryContext contextNode)
        {
            Assert.ArgumentNotNull(query, nameof(query));
            Assert.ArgumentNotNull(opcode, nameof(opcode));
            Assert.ArgumentNotNull(contextNode, nameof(contextNode));

            var item = contextNode.GetQueryContextItem();
            if (item == null)
            {
                return opcode.Evaluate(query, contextNode);
            }

            var queryTranslator = GetQueryTranslator(contextNode);
            if (queryTranslator == null)
            {
                return opcode.Evaluate(query, contextNode);
            }

            IDList idList;
            try
            {
                idList = queryTranslator.QueryFast(item.Database, opcode);
            }
            catch
            {
                idList = null;
            }

            if (idList == null)
            {
                return opcode.Evaluate(query, contextNode);
            }

            if (idList.Count == 1)
            {
                return new QueryContext(item.Database.DataManager, idList[0]);
            }

            var result = new List<QueryContext>();
            foreach (ID id in idList)
            {
                result.Add(new QueryContext(item.Database.DataManager, id));
            }

            return result.ToArray();
        }

        [CanBeNull]
        public static object EvaluateSubQuery([NotNull] this Query query, Opcode opcode, Item item)
        {
            var subquery = new QueryAnalyzerQuery(opcode);
            subquery.Max = query.Max;

            var q = query as QueryAnalyzerQuery;
            if (q != null)
            {
                subquery.Counter = q.Counter;
            }

            subquery.Function += QueryAnalyzer.FunctionCall;

            var result = subquery.Evaluate(opcode, item);

            if (q != null)
            {
                q.Counter = subquery.Counter;
            }

            return result;
        }

        [NotNull]
        public static string FormatItemsAffected([NotNull] this Query query, int count)
        {
            Assert.ArgumentNotNull(query, nameof(query));

            if (count == 1)
            {
                return "(1 item affected.)";
            }

            return string.Format("({0} items affected.)", count);
        }

        [CanBeNull]
        public static Item GetItem([NotNull] this Query query, [NotNull] QueryContext contextNode, [NotNull] Opcode expression)
        {
            Assert.ArgumentNotNull(query, nameof(query));
            Assert.ArgumentNotNull(contextNode, nameof(contextNode));
            Assert.ArgumentNotNull(expression, nameof(expression));

            var result = expression.Evaluate(query, contextNode);

            var instance = result as QueryContext;
            if (instance == null)
            {
                return null;
            }

            return instance.GetQueryContextItem();
        }

        [NotNull]
        public static IEnumerable<Item> GetItems([NotNull] this Query query, [CanBeNull] object result)
        {
            Assert.ArgumentNotNull(query, nameof(query));

            if (result == null)
            {
                yield break;
            }

            var list = result as QueryContext[];
            if (list != null)
            {
                foreach (var context in list)
                {
                    var item = context.GetQueryContextItem();
                    if (item != null)
                    {
                        yield return item;
                    }
                }
            }

            var instance = result as QueryContext;
            if (instance != null)
            {
                var item = instance.GetQueryContextItem();
                if (item != null)
                {
                    yield return item;
                }
            }
        }

        [CanBeNull]
        public static string GetString([NotNull] this Query query, [NotNull] QueryContext contextNode, [NotNull] Opcode expression)
        {
            Assert.ArgumentNotNull(query, nameof(query));
            Assert.ArgumentNotNull(contextNode, nameof(contextNode));
            Assert.ArgumentNotNull(expression, nameof(expression));

            return expression.Evaluate(query, contextNode) as string;
        }

        [CanBeNull]
        private static QueryTranslator GetQueryTranslator([NotNull] QueryContext contextNode)
        {
            Assert.ArgumentNotNull(contextNode, nameof(contextNode));

            var item = contextNode.GetQueryContextItem();
            var dataProviders = item.Database.GetDataProviders();

            var dataProvider = dataProviders.FirstOrDefault(provider => provider is SqlServerDataProvider);
            if (dataProvider == null)
            {
                return null;
            }

            var field = typeof(SqlDataProvider).GetProperty("Api", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                return null;
            }

            var api = field.GetValue(dataProvider, null) as SqlDataApi;
            if (api == null)
            {
                return null;
            }

            return new QueryTranslator(api);
        }
    }
}
