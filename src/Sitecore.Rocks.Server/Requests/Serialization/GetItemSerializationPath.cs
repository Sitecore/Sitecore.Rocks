// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Configuration;
using Sitecore.Data.Serialization;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Serialization
{
    public class GetItemSerializationPath
    {
        [NotNull]
        public string Execute([NotNull] string id, [NotNull] string databaseName)
        {
            Assert.ArgumentNotNull(id, nameof(id));
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

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

            var pathResolver = VersionSpecific.Services.ItemPathResolver;
            return pathResolver.GetPath(item.Uri.ToString());
        }
    }
}
