// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Workflows
{
    [Validation("Workflow should have an initial state", "Workflows")]
    public class WorkflowInitialState : ItemValidation
    {
        public override bool CanCheck(string contextName, Item item)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));
            Assert.ArgumentNotNull(item, nameof(item));

            return contextName == "Site";
        }

        public override void Check(ValidationWriter output, Item item)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(item, nameof(item));

            if (item.TemplateID != TemplateIDs.Workflow)
            {
                return;
            }

            if (StandardValuesManager.IsStandardValuesHolder(item))
            {
                return;
            }

            if (item.Name == "$name")
            {
                return;
            }

            var initialState = item["Initial state"];
            if (string.IsNullOrEmpty(initialState))
            {
                output.Write(SeverityLevel.Suggestion, "Workflow should have an initial state", string.Format("The workflow {0} has no initial state.", item.Appearance.DisplayName), "Open the workflow and set an initial state.");
                return;
            }

            var state = item.Database.GetItem(initialState);
            if (state != null)
            {
                return;
            }

            output.Write(SeverityLevel.Warning, "Workflow should have an initial state", string.Format("The workflow \"{0}\" has an initial state that does not exist.", item.Appearance.DisplayName), "Open the workflow and set an initial state.");
        }
    }
}
