// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Configuration;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Links
{
    public class GetTemplateInstances
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string id)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(id, nameof(id));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var item = database.GetItem(id);
            if (item == null)
            {
                throw new Exception("Item not found");
            }

            return Globals.LinkDatabase.GetReferrerCount(item).ToString();
        }
    }
}
