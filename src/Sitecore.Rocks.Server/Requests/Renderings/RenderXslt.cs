// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Web.UI.WebControls;

namespace Sitecore.Rocks.Server.Requests.Renderings
{
    public class RenderXslt
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string itemId, [NotNull] string contextId)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(itemId, nameof(itemId));
            Assert.ArgumentNotNull(contextId, nameof(contextId));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var item = database.GetItem(itemId);
            if (item == null)
            {
                return string.Empty;
            }

            var contextItem = database.GetItem(contextId);
            if (contextItem == null)
            {
                return "Context item does not exist";
            }

            var path = item["Path"];
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            var xslFile = new XslFile
            {
                Path = path
            };

            using (new DatabaseSwitcher(contextItem.Database))
            {
                using (new ContextItemSwitcher(contextItem))
                {
                    return xslFile.Transform(contextItem);
                }
            }
        }
    }
}
