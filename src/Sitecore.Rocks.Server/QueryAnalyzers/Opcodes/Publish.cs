// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Sitecore.Configuration;
using Sitecore.Data.Managers;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using Sitecore.Publishing;
using Sitecore.Rocks.Server.Extensions.QueryExtensions;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class Publish : Opcode
    {
        public Publish([CanBeNull] Opcode @from, [NotNull] List<string> targets)
        {
            Assert.ArgumentNotNull(targets, nameof(targets));

            From = from;
            Targets = targets;
        }

        [CanBeNull]
        protected Opcode From { get; set; }

        [NotNull]
        protected List<string> Targets { get; set; }

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

            if (From == null)
            {
                return;
            }

            output.Indent++;
            From.Print(output);
            output.Indent--;
        }

        private int Execute([NotNull] Query query, [NotNull] QueryContext contextNode)
        {
            Debug.ArgumentNotNull(query, nameof(query));
            Debug.ArgumentNotNull(contextNode, nameof(contextNode));

            var database = contextNode.GetQueryContextItem().Database;

            var publishingTargets = PublishManager.GetPublishingTargets(database);

            foreach (var target in Targets)
            {
                if (!publishingTargets.Any(t => string.Compare(t.Name, target, StringComparison.InvariantCultureIgnoreCase) == 0))
                {
                    throw new QueryException(string.Format("Publishing target \"{0}\" not found", target));
                }
            }

            var targetDatabases = publishingTargets.Where(i => !Targets.Any() || Targets.Any(t => string.Compare(t, i.Name, StringComparison.InvariantCultureIgnoreCase) == 0)).Select(target => Factory.GetDatabase(target["Target database"])).ToArray();

            var languages = LanguageManager.GetLanguages(database).ToArray();

            object o = contextNode;

            var where = From;
            if (where != null)
            {
                o = query.Evaluate(where, contextNode);
                if (o == null)
                {
                    return 0;
                }
            }

            var items = query.GetItems(o).ToList();
            foreach (var item in items)
            {
                PublishManager.PublishItem(item, targetDatabases, languages, true, true);
            }

            return items.Count();
        }
    }
}
