// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.Commands.Serialization
{
    public static class DataServiceExtensions
    {
        public static void RevertDatabase([NotNull] this DataService dataService, [NotNull] DatabaseUri databaseUri, [NotNull] ExecuteCompleted callback)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(callback, nameof(callback));

            databaseUri.Site.DataService.ExecuteAsync("Serialization.RevertDatabase", callback, databaseUri.DatabaseName.Name);
        }

        public static void RevertItem([NotNull] this DataService dataService, [NotNull] ItemUri itemUri, [NotNull] ExecuteCompleted callback)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(callback, nameof(callback));

            itemUri.Site.DataService.ExecuteAsync("Serialization.RevertItem", callback, itemUri.ItemId.ToString(), itemUri.DatabaseName.Name);
        }

        public static void RevertTree([NotNull] this DataService dataService, [NotNull] ItemUri itemUri, [NotNull] ExecuteCompleted callback)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(callback, nameof(callback));

            itemUri.Site.DataService.ExecuteAsync("Serialization.RevertTree", callback, itemUri.ItemId.ToString(), itemUri.DatabaseName.Name);
        }

        public static void SerializeDatabase([NotNull] this DataService dataService, [NotNull] DatabaseUri databaseUri, [NotNull] ExecuteCompleted callback)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(callback, nameof(callback));

            databaseUri.Site.DataService.ExecuteAsync("Serialization.SerializeDatabase", callback, databaseUri.DatabaseName.Name);
        }

        public static void SerializeItem([NotNull] this DataService dataService, [NotNull] ItemUri itemUri, [NotNull] ExecuteCompleted callback)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(callback, nameof(callback));

            itemUri.Site.DataService.ExecuteAsync("Serialization.SerializeItem", callback, itemUri.ItemId.ToString(), itemUri.DatabaseName.Name);
        }

        public static void SerializeTree([NotNull] this DataService dataService, [NotNull] ItemUri itemUri, [NotNull] ExecuteCompleted callback)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(callback, nameof(callback));

            itemUri.Site.DataService.ExecuteAsync("Serialization.SerializeTree", callback, itemUri.ItemId.ToString(), itemUri.DatabaseName.Name);
        }

        public static void UpdateDatabase([NotNull] this DataService dataService, [NotNull] DatabaseUri databaseUri, [NotNull] ExecuteCompleted callback)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(callback, nameof(callback));

            databaseUri.Site.DataService.ExecuteAsync("Serialization.UpdateDatabase", callback, databaseUri.DatabaseName.Name);
        }

        public static void UpdateItem([NotNull] this DataService dataService, [NotNull] ItemUri itemUri, [NotNull] ExecuteCompleted callback)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(callback, nameof(callback));

            itemUri.Site.DataService.ExecuteAsync("Serialization.UpdateItem", callback, itemUri.ItemId.ToString(), itemUri.DatabaseName.Name);
        }

        public static void UpdateTree([NotNull] this DataService dataService, [NotNull] ItemUri itemUri, [NotNull] ExecuteCompleted callback)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(callback, nameof(callback));

            itemUri.Site.DataService.ExecuteAsync("Serialization.UpdateTree", callback, itemUri.ItemId.ToString(), itemUri.DatabaseName.Name);
        }
    }
}
