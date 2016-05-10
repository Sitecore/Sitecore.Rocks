// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;

namespace Sitecore.Rocks.Server.QueryAnalyzers.WhereHandlers
{
    public class DatabaseWhereHandler : IWhereHandler
    {
        public DatabaseWhereHandler(string databaseName)
        {
            DatabaseName = databaseName;
        }

        [NotNull]
        public string DatabaseName { get; }

        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull(parser, nameof(parser));

            parser.Match();

            parser.Match(TokenType.Namespace, "\":\" expected");

            return new Database(DatabaseName, parser.GetQueries());
        }
    }
}
