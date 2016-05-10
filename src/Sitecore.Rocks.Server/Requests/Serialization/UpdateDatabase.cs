// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Configuration;
using Sitecore.Data.Serialization;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Serialization
{
    public class UpdateDatabase
    {
        protected bool ForceUpdate { get; set; }

        [NotNull]
        public string Execute([NotNull] string databaseName)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var item = database.GetRootItem();
            if (item == null)
            {
                throw new Exception("Item not found");
            }

            var options = new LoadOptions
            {
                ForceUpdate = ForceUpdate
            };

            var directory = PathUtils.GetDirectoryPath(new ItemReference(item).ToString());

            Manager.LoadTree(directory, options);

            return string.Empty;
        }
    }
}
