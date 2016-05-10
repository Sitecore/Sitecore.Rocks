// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Workflows;

namespace Sitecore.Rocks.Server.Requests.Workflows
{
    public class GetWorkflowCommands
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string itemList)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(itemList, nameof(itemList));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                return string.Empty;
            }

            var items = GetItems(database, itemList);

            string workflowId = null;
            string workflowStateId = null;

            foreach (var item in items)
            {
                var w = item[FieldIDs.Workflow];
                var s = item[FieldIDs.WorkflowState];

                if (workflowId == null && workflowStateId == null)
                {
                    workflowId = w;
                    workflowStateId = s;
                }
                else if (workflowId != w || workflowStateId != s)
                {
                    return string.Empty;
                }
            }

            if (string.IsNullOrEmpty(workflowId) || string.IsNullOrEmpty(workflowStateId))
            {
                return string.Empty;
            }

            var provider = database.WorkflowProvider;
            if (provider == null)
            {
                return string.Empty;
            }

            var workflow = provider.GetWorkflow(workflowId);
            if (workflow == null)
            {
                return string.Empty;
            }

            var workflowCommands = workflow.GetCommands(workflowStateId);
            if (workflowCommands == null)
            {
                return string.Empty;
            }

            return FormatCommands(workflowCommands);
        }

        [NotNull]
        private string FormatCommands([NotNull] WorkflowCommand[] workflowCommands)
        {
            Debug.ArgumentNotNull(workflowCommands, nameof(workflowCommands));

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("commands");

            foreach (var workflowCommand in workflowCommands)
            {
                var displayName = workflowCommand.DisplayName;
                if (displayName.StartsWith("_"))
                {
                    continue;
                }

                output.WriteStartElement("command");

                output.WriteAttributeString("name", displayName);
                output.WriteAttributeString("id", workflowCommand.CommandID);

                output.WriteEndElement();
            }

            output.WriteEndElement();

            return writer.ToString();
        }

        [NotNull]
        private List<Item> GetItems([NotNull] Database database, [NotNull] string itemList)
        {
            Debug.ArgumentNotNull(database, nameof(database));
            Debug.ArgumentNotNull(itemList, nameof(itemList));

            var items = new List<Item>();

            var l = itemList.Split('|');

            foreach (var id in l)
            {
                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }

                var item = database.GetItem(id);
                if (item == null)
                {
                    continue;
                }

                items.Add(item);
            }

            return items;
        }
    }
}
