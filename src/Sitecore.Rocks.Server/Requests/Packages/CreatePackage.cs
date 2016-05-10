// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Rocks.Server.Packages;
using Sitecore.Web;

namespace Sitecore.Rocks.Server.Requests.Packages
{
    public class CreatePackage
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string itemList)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(itemList, nameof(itemList));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var fileName = TempFolder.GetFilename("package.zip");

            var items = GetItems(database, itemList);
            var package = new ZipPackageBuilder(fileName);

            foreach (var item in items)
            {
                package.Items.Add(item);
            }

            var result = package.Build();

            return WebUtil.GetServerUrl() + result;
        }

        [NotNull]
        private IEnumerable<Item> GetItems([NotNull] Database database, [NotNull] string itemList)
        {
            Debug.ArgumentNotNull(database, nameof(database));
            Debug.ArgumentNotNull(itemList, nameof(itemList));

            var l = itemList.Split('|');

            foreach (var id in l)
            {
                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }

                var item = database.GetItem(id);
                if (item == null)
                {
                    continue;
                }

                yield return item;
            }
        }
    }
}
