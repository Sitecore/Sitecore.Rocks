// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentEditors.Commands.Searching
{
    [Command(Submenu = "Search"), Feature(FeatureNames.AdvancedNavigation)]
    public class SearchForWorkflow : SearchCommand
    {
        public SearchForWorkflow()
        {
            Text = Resources.SearchForWorkflow_SearchForWorkflow_Items_in_Same_Workflow;
            Group = "Workflows";
            SortingValue = 5000;

            FieldName = "__Workflow";
            FieldPath = "/sitecore/templates/System/Templates/Sections/Workflow/Workflow/__Workflow";
        }
    }
}
