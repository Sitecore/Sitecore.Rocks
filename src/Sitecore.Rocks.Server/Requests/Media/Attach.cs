// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Resources.Media;
using Sitecore.SecurityModel;

namespace Sitecore.Rocks.Server.Requests.Media
{
    public class Attach
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string itemId, [NotNull] string fileName, [NotNull] string fileData)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(itemId, nameof(itemId));
            Assert.ArgumentNotNull(fileName, nameof(fileName));
            Assert.ArgumentNotNull(fileData, nameof(fileData));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var item = database.GetItem(itemId);
            if (item == null)
            {
                throw new Exception("Item not found");
            }

            var extension = FileUtil.GetExtension(fileName);

            var media = MediaManager.GetMedia(item);
            Assert.IsNotNull(media, typeof(Resources.Media.Media));

            var memoryStream = new MemoryStream(System.Convert.FromBase64String(fileData));

            media.SetStream(memoryStream, extension);

            using (new EditContext(item, SecurityCheck.Disable))
            {
                item["extension"] = extension;
            }

            return string.Empty;
        }
    }
}
