// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;

namespace Sitecore.Rocks.Server.Requests.LayoutBuilders
{
    public class CreateDataSource
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string itemId, [NotNull] string name, [NotNull] string location, [NotNull] string templateId)
        {
            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new InvalidOperationException("Database not found");
            }

            var owner = database.GetItem(itemId);
            if (owner == null)
            {
                throw new InvalidOperationException("Item not found");
            }

            var templateItem = database.GetItem(templateId);
            if (owner == null)
            {
                throw new InvalidOperationException("Template item not found");
            }

            Item parentItem;

            if (ID.IsID(location))
            {
                parentItem = database.GetItem(location);
            }
            else
            {
                string path;
                if (string.IsNullOrEmpty(location))
                {
                    path = owner.Paths.Path;
                }
                else
                {
                    path = location.StartsWith("/") ? location : owner.Paths.Path + "/" + location;
                }

                parentItem = database.GetItem(path);
                if (parentItem == null)
                {
                    var folderTemplate = new TemplateItem(database.GetItem(TemplateIDs.Folder));
                    parentItem = database.CreateItemPath(path, folderTemplate);
                }
            }

            if (parentItem == null)
            {
                throw new InvalidOperationException("Location item not found");
            }

            var newItemName = name + " Data Source";
            var index = 2;

            while (parentItem.Children[newItemName] != null)
            {
                newItemName = name + " Data Source " + index;
                index++;
            }

            var newItem = ItemManager.AddFromTemplate(newItemName, templateItem.ID, parentItem);
            if (newItem == null)
            {
                throw new InvalidOperationException("Could not create data source item");
            }

            return newItem.ID + "," + parentItem.ID;
        }
    }
}
