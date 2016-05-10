// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Templates
{
    public class ChangeTemplates
    {
        [NotNull]
        public string Execute([NotNull] string itemList, [NotNull] string newTemplateId, [NotNull] string databaseName)
        {
            Assert.ArgumentNotNull(itemList, nameof(itemList));
            Assert.ArgumentNotNull(newTemplateId, nameof(newTemplateId));
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            TemplateItem template = database.GetItem(newTemplateId);
            if (template == null)
            {
                throw new Exception("Template not found");
            }

            foreach (var id in itemList.Split('|'))
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

                item.ChangeTemplate(template);
            }

            return string.Empty;
        }
    }
}
