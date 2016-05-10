// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Publishing;

namespace Sitecore.Rocks.Server.Requests.Publishing
{
    public class ClearPublishingQueue
    {
        [NotNull]
        public string Execute([NotNull] string databaseName)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            PublishManager.ClearPublishQueue(database);

            return string.Empty;
        }
    }
}
