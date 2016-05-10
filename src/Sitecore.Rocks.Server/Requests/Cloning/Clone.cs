// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Configuration;
using Sitecore.Data.Items;

namespace Sitecore.Rocks.Server.Requests.Cloning
{
    public class Clone
    {
        [NotNull]
        public string Execute([NotNull] string sourceDatabaseName, string sourceItemId, [NotNull] string targetDatabaseName, string targetItemId, string deep)
        {
            var isDeep = deep == "1";

            var sourceDatabase = Factory.GetDatabase(sourceDatabaseName);
            if (sourceDatabase == null)
            {
                throw new Exception("Database not found");
            }

            var sourceItem = sourceDatabase.GetItem(sourceItemId);
            if (sourceItem == null)
            {
                throw new Exception("Item not found");
            }

            var targetDatabase = Factory.GetDatabase(targetDatabaseName);
            if (targetDatabase == null)
            {
                throw new Exception("Database not found");
            }

            var targetItem = sourceDatabase.GetItem(targetItemId);
            if (targetItem == null)
            {
                throw new Exception("Item not found");
            }

            try
            {
                return CloneItem(sourceItem, targetItem, isDeep);
            }
            catch (MissingMethodException)
            {
                throw new Exception("Item Cloning is not supported on this version of Sitecore");
            }
        }

        [NotNull]
        private string CloneItem([NotNull] Item sourceItem, [NotNull] Item targetItem, bool isDeep)
        {
            var clone = sourceItem.CloneTo(targetItem, isDeep);

            return clone.ID.ToString();
        }
    }
}
