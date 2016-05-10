// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Workflows
{
    public class SetWorkflowState
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string itemList, [NotNull] string stateId)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(itemList, nameof(itemList));
            Assert.ArgumentNotNull(stateId, nameof(stateId));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                return string.Empty;
            }

            var items = GetItems(database, itemList);

            var workflowState = database.GetItem(stateId);
            var workflow = workflowState.Parent;

            foreach (var item in items)
            {
                item.Editing.BeginEdit();
                item[FieldIDs.Workflow] = workflow.ID.ToString();
                item[FieldIDs.WorkflowState] = workflowState.ID.ToString();
                item.Editing.EndEdit();
            }

            return workflow.ID.ToString();
        }

        [NotNull]
        private List<Item> GetItems([NotNull] Database database, [NotNull] string itemList)
        {
            Debug.ArgumentNotNull(database, nameof(database));
            Debug.ArgumentNotNull(itemList, nameof(itemList));

            var items = new List<Item>();

            var l = itemList.Split(',');

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
