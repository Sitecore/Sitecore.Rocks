// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Configuration;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Items
{
    public class GetItemId
    {
        [NotNull]
        public string Execute([NotNull] string itemPath, [NotNull] string databaseName)
        {
            Assert.ArgumentNotNull(itemPath, nameof(itemPath));
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                return string.Empty;
            }

            var item = database.GetItem(itemPath);

            return item == null ? string.Empty : item.ID.ToString();
        }
    }
}
