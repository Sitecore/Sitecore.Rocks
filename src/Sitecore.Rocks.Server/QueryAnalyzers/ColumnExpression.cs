// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Query;

namespace Sitecore.Rocks.Server.QueryAnalyzers
{
    public class ColumnExpression
    {
        [CanBeNull]
        public string ColumnName { get; set; }

        [CanBeNull]
        public Opcode Expression { get; set; }

        [CanBeNull]
        public string FieldName { get; set; }
    }
}
