// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Publishing;

namespace Sitecore.Rocks.Server.Requests.Publishing
{
    public class PublishItem
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string itemsList, bool deep, bool compareRevisions)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(itemsList, nameof(itemsList));

            var database = Factory.GetDatabase(databaseName);

            var publishingTargets = PublishManager.GetPublishingTargets(database);

            var targetDatabases = publishingTargets.Select(target => Factory.GetDatabase(target["Target database"])).ToArray();

            var languages = LanguageManager.GetLanguages(database).ToArray();

            var published = new List<string>();

            foreach (var id in itemsList.Split(','))
            {
                var item = database.GetItem(id);
                if (item == null)
                {
                    continue;
                }

                Publish(published, item, targetDatabases, languages, deep, compareRevisions);
            }

            return string.Empty;
        }

        protected virtual void Publish([NotNull] List<string> links, [NotNull] Item item, [NotNull] Database[] targetDatabases, [NotNull] Language[] languages, bool deep, bool compareRevisions)
        {
            Debug.ArgumentNotNull(links, nameof(links));
            Debug.ArgumentNotNull(item, nameof(item));
            Debug.ArgumentNotNull(targetDatabases, nameof(targetDatabases));
            Debug.ArgumentNotNull(languages, nameof(languages));

            var key = item.Database.Name + item.ID;
            if (links.Contains(key))
            {
                return;
            }

            links.Add(key);

            PublishManager.PublishItem(item, targetDatabases, languages, deep, compareRevisions);
        }
    }
}
