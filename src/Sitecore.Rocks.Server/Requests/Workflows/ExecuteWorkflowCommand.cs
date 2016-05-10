// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Workflows
{
    public class ExecuteWorkflowCommand
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string itemList, [NotNull] string commandId, [NotNull] string comment)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(itemList, nameof(itemList));
            Assert.ArgumentNotNull(commandId, nameof(commandId));
            Assert.ArgumentNotNull(comment, nameof(comment));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                return string.Empty;
            }

            var items = GetItems(database, itemList);

            string workflowId = null;

            foreach (var item in items)
            {
                var w = item[FieldIDs.Workflow];

                if (workflowId == null)
                {
                    workflowId = w;
                }
                else if (workflowId != w)
                {
                    return string.Empty;
                }
            }

            if (string.IsNullOrEmpty(workflowId))
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

            foreach (var item in items)
            {
                try
                {
                    workflow.Execute(commandId, item, comment, false);
                }
                catch (Exception ex)
                {
                    Log.Error("Failed to execute workflow step", ex, GetType());
                }
            }

            return string.Empty;
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
