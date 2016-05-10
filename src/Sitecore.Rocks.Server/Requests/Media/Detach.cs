// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Resources.Media;

namespace Sitecore.Rocks.Server.Requests.Media
{
    public class Detach
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

            MediaItem mediaItem = item;

            var uri = MediaUri.Parse(mediaItem);

            var media = MediaManager.GetMedia(uri);
            media.ReleaseStream();

            return string.Empty;
        }
    }
}
