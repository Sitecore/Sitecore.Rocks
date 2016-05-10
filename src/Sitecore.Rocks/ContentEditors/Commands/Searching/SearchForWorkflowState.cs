// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentEditors.Commands.Searching
{
    [Command(Submenu = "Search"), Feature(FeatureNames.AdvancedNavigation)]
    public class SearchForWorkflowState : SearchCommand
    {
        public SearchForWorkflowState()
        {
            Text = Resources.SearchForWorkflowState_SearchForWorkflowState_Items_in_Same_Workflow_State;
            Group = "Workflows";
            SortingValue = 5100;

            FieldName = "__Workflow State";
            FieldPath = "/sitecore/templates/System/Templates/Sections/Workflow/Workflow/__Workflow state";
        }
    }
}
