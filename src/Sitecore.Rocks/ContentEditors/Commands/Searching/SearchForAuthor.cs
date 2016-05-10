// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentEditors.Commands.Searching
{
    [Command(Submenu = "Search"), Feature(FeatureNames.AdvancedNavigation)]
    public class SearchForAuthor : SearchCommand
    {
        public SearchForAuthor()
        {
            Text = Resources.SearchForAuthor_SearchForAuthor_Author;
            Group = "Statistics";
            SortingValue = 3000;

            FieldName = "__updated By";
            FieldPath = "/sitecore/templates/System/Templates/Sections/Statistics/Statistics/__Updated By";
        }
    }
}
