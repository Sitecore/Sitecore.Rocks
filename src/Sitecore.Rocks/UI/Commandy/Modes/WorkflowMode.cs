// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.UI.Commandy.Modes
{
    [Export(typeof(IMode))]
    public class WorkflowMode : ModeBase
    {
        private readonly List<WorkflowCommand> workflowCommands = new List<WorkflowCommand>();

        public WorkflowMode([NotNull] Commandy commandy) : base(commandy)
        {
            Assert.ArgumentNotNull(commandy, nameof(commandy));

            Name = "Workflow Command";
            Alias = "w";

            var context = commandy.Parameter as IItemSelectionContext;
            if (context == null)
            {
                IsReady = true;
                return;
            }

            if (!context.Items.Any())
            {
                IsReady = true;
                return;
            }

            var list = string.Join("|", context.Items.Select(i => i.ItemUri.ItemId.ToString()));
            var databaseUri = context.Items.First().ItemUri.DatabaseUri;

            databaseUri.Site.DataService.ExecuteAsync("Workflows.GetWorkflowCommands", LoadWorkflowCommands, databaseUri.DatabaseName.ToString(), list);
        }

        public override string Watermark
        {
            get { return "Workflow Command"; }
        }

        [NotNull]
        public IEnumerable<WorkflowCommand> WorkflowCommands
        {
            get { return workflowCommands; }
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            return context.Items.Any();
        }

        public override void Execute(Hit hit, object parameter)
        {
            Assert.ArgumentNotNull(hit, nameof(hit));
            Assert.ArgumentNotNull(parameter, nameof(parameter));

            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            var workflowCommand = hit.Tag as WorkflowCommand;
            if (workflowCommand == null)
            {
                return;
            }

            var comment = AppHost.Prompt(string.Format(Resources.Workflow_ExecuteWorkflowCommand_Comment_for___0___, workflowCommand.Name), workflowCommand.Name);
            if (comment == null)
            {
                return;
            }

            var list = string.Join("|", context.Items.Select(i => i.ItemUri.ItemId.ToString()));
            var databaseUri = context.Items.First().ItemUri.DatabaseUri;

            ExecuteCompleted callback = (response, executeResult) => DataService.HandleExecute(response, executeResult);

            databaseUri.Site.DataService.ExecuteAsync("Workflows.ExecuteWorkflowCommand", callback, databaseUri.DatabaseName.ToString(), list, workflowCommand.Id, comment);
        }

        private void LoadWorkflowCommands([NotNull] string response, [NotNull] ExecuteResult executeResult)
        {
            Debug.ArgumentNotNull(response, nameof(response));
            Debug.ArgumentNotNull(executeResult, nameof(executeResult));

            if (!DataService.HandleExecute(response, executeResult))
            {
                return;
            }

            workflowCommands.Clear();
            IsReady = true;

            var root = response.ToXElement();
            if (root == null)
            {
                return;
            }

            foreach (var element in root.Elements())
            {
                var name = element.GetAttributeValue("name");
                var id = element.GetAttributeValue("id");

                workflowCommands.Add(new WorkflowCommand(name, id));
            }
        }

        public class WorkflowCommand
        {
            public WorkflowCommand([NotNull] string name, [NotNull] string id)
            {
                Assert.ArgumentNotNull(name, nameof(name));
                Assert.ArgumentNotNull(id, nameof(id));

                Name = name;
                Id = id;
            }

            [NotNull]
            public string Id { get; }

            [NotNull]
            public string Name { get; }
        }
    }
}
