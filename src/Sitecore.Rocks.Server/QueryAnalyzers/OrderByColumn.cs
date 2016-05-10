// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.QueryAnalyzers
{
    public class OrderByColumn
    {
        public OrderByColumn([NotNull] string columnName, int direction)
        {
            Assert.ArgumentNotNull(columnName, nameof(columnName));

            ColumnName = columnName;
            Direction = direction;
        }

        [NotNull]
        public string ColumnName { get; private set; }

        public int Direction { get; private set; }
    }
}
