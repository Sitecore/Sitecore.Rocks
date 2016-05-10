// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Searching
{
    [Command(Submenu = "Search"), CommandId(CommandIds.SitecoreExplorer.SearchLastYear, typeof(ContentTreeContext), Text = "Search Last Year"), Feature(FeatureNames.AdvancedNavigation)]
    public class SearchLastYear : SearchCommand
    {
        public SearchLastYear()
        {
            Text = Resources.SearchLastYear_SearchLastYear_Items_Changed_Last_Year;
            Group = "Recent";
            SortingValue = 2200;

            FieldName = "__updated";

            var to = DateTime.Now.AddDays(1).ToString(@"yyyyMMdd");
            var from = DateTime.Now.AddYears(-1).ToString(@"yyyyMMdd");

            Value = string.Format(@"[{0} TO {1}]", from, to);
        }
    }
}
