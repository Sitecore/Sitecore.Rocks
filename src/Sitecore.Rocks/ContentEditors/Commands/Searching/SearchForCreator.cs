// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentEditors.Commands.Searching
{
    [Command(Submenu = "Search"), Feature(FeatureNames.AdvancedNavigation)]
    public class SearchForCreator : SearchCommand
    {
        public SearchForCreator()
        {
            Text = Resources.SearchForCreator_SearchForCreator_Creator;
            Group = "Statistics";
            SortingValue = 3100;

            FieldName = "__Created By";
            FieldPath = "/sitecore/templates/System/Templates/Sections/Statistics/Statistics/__Created by";
        }
    }
}
