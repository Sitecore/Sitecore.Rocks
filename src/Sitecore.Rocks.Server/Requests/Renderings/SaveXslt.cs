// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Text;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.IO;

namespace Sitecore.Rocks.Server.Requests.Renderings
{
    public class SaveXslt
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string itemId, [NotNull] string fileContents)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(itemId, nameof(itemId));
            Assert.ArgumentNotNull(fileContents, nameof(fileContents));

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

            var path = item["Path"];
            if (string.IsNullOrEmpty(path))
            {
                path = "/xsl/" + item.Name + ".xsl";

                item.Editing.BeginEdit();
                item["Path"] = path;
                item.Editing.EndEdit();
            }

            File.WriteAllText(FileUtil.MapPath(path), fileContents, Encoding.UTF8);

            return string.Empty;
        }
    }
}
