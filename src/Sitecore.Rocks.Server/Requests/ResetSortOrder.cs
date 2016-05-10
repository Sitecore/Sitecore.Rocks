// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Configuration;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests
{
    public class ResetSortOrder
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

            var field = item.Fields[FieldIDs.Sortorder];
            if (field != null)
            {
                item.Editing.BeginEdit();
                field.Value = string.Empty;
                field.Reset();
                item.Editing.EndEdit();
            }

            return string.Empty;
        }
    }
}
