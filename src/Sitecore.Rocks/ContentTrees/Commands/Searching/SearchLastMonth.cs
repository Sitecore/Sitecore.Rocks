// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Searching
{
    [Command(Submenu = "Search"), CommandId(CommandIds.SitecoreExplorer.SearchLastMonth, typeof(ContentTreeContext), Text = "Search Last Month"), Feature(FeatureNames.AdvancedNavigation)]
    public class SearchLastMonth : SearchCommand
    {
        public SearchLastMonth()
        {
            Text = Resources.SearchLastMonth_SearchLastMonth_Items_Changed_Last_Month;
            Group = "Recent";
            SortingValue = 2100;

            FieldName = "__updated";

            var to = DateTime.Now.AddDays(1).ToString(@"yyyyMMdd");
            var from = DateTime.Now.AddMonths(-1).ToString(@"yyyyMMdd");

            Value = string.Format(@"[{0} TO {1}]", from, to);
        }
    }
}
