// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentEditors.Commands.Searching
{
    [Command(Submenu = "Search"), Feature(FeatureNames.AdvancedNavigation)]
    public class SearchForOwner : SearchCommand
    {
        public SearchForOwner()
        {
            Text = Resources.SearchForOwner_SearchForOwner_Items_with_Same_Owner;
            Group = "Statistics";
            SortingValue = 3200;

            FieldName = "__Owner";
            FieldPath = "/sitecore/templates/System/Templates/Sections/Security/Security/__Owner";
        }
    }
}
