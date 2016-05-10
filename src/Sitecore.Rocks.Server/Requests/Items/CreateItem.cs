// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Pipelines.SetFieldValue;

namespace Sitecore.Rocks.Server.Requests.Items
{
    public class CreateItem
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string parentPath, [NotNull] string newName, [NotNull] string templateName, [NotNull] string fields)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(parentPath, nameof(parentPath));
            Assert.ArgumentNotNull(newName, nameof(newName));
            Assert.ArgumentNotNull(templateName, nameof(templateName));
            Assert.ArgumentNotNull(fields, nameof(fields));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                return string.Empty;
            }

            TemplateItem templateItem = database.GetItem(templateName);
            if (templateItem == null)
            {
                return string.Empty;
            }

            Item item;
            if (ID.IsID(parentPath))
            {
                var parent = database.GetItem(parentPath);
                if (parent == null)
                {
                    return string.Empty;
                }

                item = parent.Add(newName, templateItem);
            }
            else
            {
                TemplateItem folderTemplateItem = database.GetItem(TemplateIDs.Folder);
                Assert.IsNotNull(folderTemplateItem, "folderTemplateItem");

                item = database.CreateItemPath(parentPath + "/" + newName, folderTemplateItem, templateItem);
            }

            if (item == null)
            {
                return string.Empty;
            }

            item.Editing.BeginEdit();

            var parts = fields.Split('|');
            for (var index = 0; index < parts.Length - 1; index += 2)
            {
                var fieldName = parts[index];
                var value = parts[index + 1];

                SetFieldValuePipeline.Run().WithParameters(item, fieldName, value);
            }

            item.Editing.EndEdit();

            return item.ID + "|" + item.ParentID;
        }
    }
}
