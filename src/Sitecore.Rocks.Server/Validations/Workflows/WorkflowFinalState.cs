// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Configuration;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Workflows
{
    [Validation("Workflow should have a final state", "Workflows")]
    public class WorkflowFinalState : Validation
    {
        public override bool CanCheck(string contextName)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));

            return contextName == "Site";
        }

        public override void Check(ValidationWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            var database = Factory.GetDatabase("master");

            var workflowProvider = database.WorkflowProvider;
            if (workflowProvider == null)
            {
                return;
            }

            var workflows = workflowProvider.GetWorkflows();
            if (workflows == null)
            {
                return;
            }

            foreach (var workflow in workflows)
            {
                var hasFinalState = false;

                foreach (var workflowState in workflow.GetStates())
                {
                    if (workflowState.FinalState)
                    {
                        hasFinalState = true;
                        break;
                    }
                }

                if (!hasFinalState)
                {
                    output.Write(SeverityLevel.Suggestion, "Workflow should have a final state", string.Format("The workflow \"{0}\" has no final state.", workflow.Appearance.DisplayName), "One or more of the workflow state should be a final state. Open the final state item and check the \"Final State\" checkbox.");
                }
            }
        }
    }
}
