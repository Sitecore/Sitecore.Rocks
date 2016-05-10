// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Resources.Media;

namespace Sitecore.Rocks.Server.Requests.Media
{
    public class DownloadAttachment
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
                throw new Exception("Item not found");
            }

            // TODO: potential memory overflow
            var media = MediaManager.GetMedia(item);

            Assert.IsNotNull(media, typeof(Resources.Media.Media));

            var mediaStream = media.GetStream();
            var memory = new MemoryStream();

            mediaStream.CopyTo(memory);

            return System.Convert.ToBase64String(memory.ToArray());
        }
    }
}
