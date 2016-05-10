// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Web.UI;
using Sitecore.Configuration;
using Sitecore.Data.Managers;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using Sitecore.Rocks.Server.Extensions.QueryExtensions;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class Batches : Opcode
    {
        public Batches([NotNull] List<Opcode> batches)
        {
            BatchList = batches;
            Assert.ArgumentNotNull(batches, nameof(batches));
        }

        public List<Opcode> BatchList { get; set; }

        [NotNull]
        public override object Evaluate([NotNull] Query query, [NotNull] QueryContext contextNode)
        {
            Assert.ArgumentNotNull(query, nameof(query));
            Assert.ArgumentNotNull(contextNode, nameof(contextNode));

            var result = new List<object>();

            foreach (var opcode in BatchList)
            {
                var use = opcode as Use;
                if (use != null)
                {
                    var database = Factory.GetDatabase(use.DatabaseName);
                    contextNode = new QueryContext(database.DataManager, database.GetRootItem().ID);
                    continue;
                }

                var setContextNode = opcode as SetContextNode;
                if (setContextNode != null)
                {
                    var item = query.GetItem(contextNode, setContextNode.Expression);
                    if (item == null)
                    {
                        throw new QueryException("Context Node does not evaluate to a single item.");
                    }

                    contextNode = new QueryContext(item.Database.DataManager, item.ID);
                    continue;
                }

                var setLanguage = opcode as SetLanguage;
                if (setLanguage != null)
                {
                    var languageName = query.GetString(contextNode, setLanguage.Expression);
                    if (languageName == null)
                    {
                        throw new QueryException("Language does not evaluate to a string.");
                    }

                    var language = LanguageManager.GetLanguage(languageName);
                    if (language == null)
                    {
                        throw new QueryException(string.Format("Language \"{0}\" is not defined in the database.", languageName));
                    }

                    Context.Language = language;
                    continue;
                }

                var r = opcode.Evaluate(query, contextNode);

                result.Add(r);
            }

            return result;
        }

        public override void Print([NotNull] HtmlTextWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            PrintIndent(output);
            PrintLine(output, GetType().Name);

            foreach (var batch in BatchList)
            {
                output.Indent++;
                batch.Print(output);
                output.Indent--;
            }
        }
    }
}
