// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Fields;

namespace Sitecore.Rocks.Server.QueryAnalyzers
{
    public class SelectField
    {
        [NotNull]
        public string ColumnName { get; set; }

        [CanBeNull]
        public Field Field { get; set; }

        [NotNull]
        public string Value { get; set; }
    }
}
