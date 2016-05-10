// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Text;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.IO;

namespace Sitecore.Rocks.Server.Requests.Files
{
    public class ReadFile
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string itemId)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(itemId, nameof(itemId));

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
                return string.Empty;
            }

            if (!FileUtil.Exists(path))
            {
                return string.Empty;
            }

            return File.ReadAllText(FileUtil.MapPath(path), Encoding.UTF8);
        }
    }
}
