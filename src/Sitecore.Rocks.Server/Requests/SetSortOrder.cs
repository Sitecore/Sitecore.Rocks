// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Configuration;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests
{
    public class SetSortOrder
    {
        [NotNull]
        public string Execute([NotNull] string id, [NotNull] string databaseName, int sortOrder)
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

            item.Editing.BeginEdit();
            item.Appearance.Sortorder = sortOrder;
            item.Editing.EndEdit();

            return string.Empty;
        }
    }
}
