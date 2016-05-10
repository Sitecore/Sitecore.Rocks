// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Searching
{
    [Command(Submenu = "Search"), CommandId(CommandIds.SitecoreExplorer.SearchMyItems, typeof(ContentTreeContext), Text = "Search My Items"), Feature(FeatureNames.AdvancedNavigation)]
    public class SearchMyItems : SearchCommand
    {
        public SearchMyItems()
        {
            Text = Resources.SearchMyItems_SearchMyItems_My_Items;
            Group = "My Items";
            SortingValue = 1000;

            FieldName = "Author";
        }

        protected override string GetValue(IItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            return item.ItemUri.Site.Credentials.UserName;
        }
    }
}
