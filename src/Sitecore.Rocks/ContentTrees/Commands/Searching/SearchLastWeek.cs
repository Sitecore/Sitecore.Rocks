// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Searching
{
    [Command(Submenu = "Search"), CommandId(CommandIds.SitecoreExplorer.SearchLastWeek, typeof(ContentTreeContext), Text = "Search Last Week"), Feature(FeatureNames.AdvancedNavigation)]
    public class SearchLastWeek : SearchCommand
    {
        public SearchLastWeek()
        {
            Text = Resources.SearchLastWeek_SearchLastWeek_Items_Changed_Last_Week;
            Group = "Recent";
            SortingValue = 2000;

            FieldName = "__updated";

            var to = DateTime.Now.AddDays(1).ToString(@"yyyyMMdd");
            var from = DateTime.Now.AddDays(-7).ToString(@"yyyyMMdd");

            Value = string.Format(@"[{0} TO {1}]", from, to);
        }
    }
}
