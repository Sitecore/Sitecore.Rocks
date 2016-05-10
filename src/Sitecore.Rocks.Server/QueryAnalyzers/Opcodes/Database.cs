// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Web.UI;
using Sitecore.Configuration;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class Database : Opcode
    {
        public Database([NotNull] string databaseName, Opcode opcode)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

            DatabaseName = databaseName;
            Opcode = opcode;
        }

        [NotNull]
        public string DatabaseName { get; }

        public Opcode Opcode { get; }

        [CanBeNull]
        public override object Evaluate([NotNull] Query query, [NotNull] QueryContext contextNode)
        {
            Assert.ArgumentNotNull(query, nameof(query));
            Assert.ArgumentNotNull(contextNode, nameof(contextNode));

            var database = Factory.GetDatabase(DatabaseName);
            contextNode = new QueryContext(database.DataManager, database.GetRootItem().ID);

            return Opcode.Evaluate(query, contextNode);
        }

        public override void Print([NotNull] HtmlTextWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            PrintIndent(output);
            PrintLine(output, GetType().Name);
        }
    }
}
