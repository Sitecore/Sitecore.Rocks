// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Configuration;
using Sitecore.Data.Items;

namespace Sitecore.Rocks.Server.Requests.Cloning
{
    public class Unclone
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, string itemId)
        {
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

            try
            {
                return UncloneItem(item);
            }
            catch (MissingMethodException)
            {
                throw new Exception("Item Cloning is not supported on this version of Sitecore");
            }
        }

        protected void UncloneItemRecursive(Item item)
        {
            if (item.IsClone)
            {
                var clone = new CloneItem(item);
                clone.Unclone();
            }

            foreach (Item child in item.Children)
            {
                UncloneItemRecursive(child);
            }
        }

        [NotNull]
        private string UncloneItem([NotNull] Item item)
        {
            if (!item.IsClone)
            {
                return "-1";
            }

            UncloneItemRecursive(item);

            return string.Empty;
        }
    }
}
