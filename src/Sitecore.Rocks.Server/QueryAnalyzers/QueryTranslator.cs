// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Text;
using Sitecore.Collections;
using Sitecore.Data;
using Sitecore.Data.DataProviders.Sql;
using Sitecore.Data.DataProviders.Sql.FastQuery;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.QueryAnalyzers
{
    public class QueryTranslator : QueryToSqlTranslator
    {
        public QueryTranslator([NotNull] SqlDataApi api) : base(api)
        {
            Assert.ArgumentNotNull(api, nameof(api));
        }

        [CanBeNull]
        public IDList QueryFast([NotNull] Database database, [NotNull] Opcode opcode)
        {
            Assert.ArgumentNotNull(database, nameof(database));
            Assert.ArgumentNotNull(opcode, nameof(opcode));

            var parameters = new ParametersList();

            var sql = TranslateQuery(opcode, database, parameters);
            if (sql == null)
            {
                return null;
            }

            using (var reader = _api.CreateReader(sql, parameters.ToArray()))
            {
                var list = new IDList();
                while (reader.Read())
                {
                    list.Add(_api.GetId(0, reader));
                }

                return list;
            }
        }

        [CanBeNull]
        protected virtual string TranslateQuery([NotNull] Opcode opcode, [NotNull] Database database, [NotNull] ParametersList parameters)
        {
            Assert.ArgumentNotNull(opcode, nameof(opcode));
            Assert.ArgumentNotNull(database, nameof(database));
            Assert.ArgumentNotNull(parameters, nameof(parameters));

            var step = opcode as Step;
            if (step == null || !(step is Root))
            {
                return null;
            }

            step = step.NextStep;
            var sql = string.Empty;

            while (step != null)
            {
                if (!(step is Children) && !(step is Descendant) && !(step is Ancestor) && !(step is Parent))
                {
                    throw new Exception("Can navigate only child, parent, descendant, and ancestor axes.");
                }

                ITranslationContext context = new BasicTranslationContext(_factory, _api, database, parameters);
                var predicate = GetPredicate(step);
                var name = GetName(step);

                // Dirty hack to overcome problems with numbers in Query
                if (name.StartsWith("_."))
                {
                    name = name.Substring(2);
                }

                Opcode expression = null;
                if (predicate != null)
                {
                    expression = predicate.Expression;
                }

                var where = string.Empty;
                if (expression != null)
                {
                    where = context.Factory.GetTranslator(expression).Translate(expression, context);
                }

                var builder = new StringBuilder();

                AddInitialStatement(builder);
                AddFieldJoins(context, builder);

                if (!string.IsNullOrEmpty(sql))
                {
                    AddNestedQuery(step, sql, builder);
                }

                AddExtraJoins(context, builder);

                // Add WHERE statement
                where = where.Trim();
                var whereAppended = false;
                if (where.Length > 0)
                {
                    whereAppended = AddConditionJoint(false, builder);
                    builder.Append(where);
                }

                // Add Name filter
                if (name.Length > 0 && name != "*")
                {
                    whereAppended = AddConditionJoint(whereAppended, builder);
                    AddNameFilter(name, builder);
                }

                if (step is Children && string.IsNullOrEmpty(sql))
                {
                    AddConditionJoint(whereAppended, builder);
                    AddRootItemFilter(parameters, builder);
                }

                sql = builder.ToString();
                step = step.NextStep;
            }

            return sql;
        }
    }
}
