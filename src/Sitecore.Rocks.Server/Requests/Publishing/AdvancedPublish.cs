// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Publishing;

namespace Sitecore.Rocks.Server.Requests.Publishing
{
    public class AdvancedPublish
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string itemId, int mode, int source, [NotNull] string languages, [NotNull] string targets, bool relatedItems)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(itemId, nameof(itemId));
            Assert.ArgumentNotNull(languages, nameof(languages));
            Assert.ArgumentNotNull(targets, nameof(targets));
            Assert.ArgumentNotNull(relatedItems, nameof(relatedItems));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var languageList = GetLanguages(database, languages);
            var targetList = GetTargets(database, targets);

            var instance = ServerHost.AppVersion.Get(this, new System.Version(7, 2));

            switch (source)
            {
                case 0:
                    PublishDatabase(database, mode, languageList, targetList);
                    break;

                case 1:
                    instance.PublishItem(database, itemId, false, languageList, targetList, relatedItems);
                    break;

                case 2:
                    instance.PublishItem(database, itemId, true, languageList, targetList, relatedItems);
                    break;
            }

            return string.Empty;
        }

        protected virtual void PublishItem([NotNull] Database database, [NotNull] string itemId, bool deep, [NotNull] List<Language> languageList, [NotNull] List<Database> targetDatabases, bool publishRelatedItems)
        {
            Debug.ArgumentNotNull(database, nameof(database));
            Debug.ArgumentNotNull(itemId, nameof(itemId));
            Debug.ArgumentNotNull(languageList, nameof(languageList));
            Debug.ArgumentNotNull(targetDatabases, nameof(targetDatabases));

            if (string.IsNullOrEmpty(itemId))
            {
                return;
            }

            var item = database.GetItem(itemId);
            if (item == null)
            {
                return;
            }

            PublishManager.PublishItem(item, targetDatabases.ToArray(), languageList.ToArray(), deep, false, publishRelatedItems);
        }

        [NotNull]
        private List<Language> GetLanguages([NotNull] Database database, [NotNull] string languages)
        {
            Debug.ArgumentNotNull(database, nameof(database));
            Debug.ArgumentNotNull(languages, nameof(languages));

            if (string.IsNullOrEmpty(languages))
            {
                return LanguageManager.GetLanguages(database).ToList();
            }

            var languageList = new List<Language>();
            foreach (var languageName in languages.Split('|'))
            {
                var language = LanguageManager.GetLanguage(languageName, database);
                if (language != null)
                {
                    languageList.Add(language);
                }
            }

            return languageList;
        }

        [NotNull]
        private List<Database> GetTargets([NotNull] Database database, [NotNull] string targets)
        {
            Debug.ArgumentNotNull(database, nameof(database));
            Debug.ArgumentNotNull(targets, nameof(targets));

            if (string.IsNullOrEmpty(targets))
            {
                var publishingTargets = PublishManager.GetPublishingTargets(database);

                return publishingTargets.Select(target => Factory.GetDatabase(target["Target database"])).ToList();
            }

            var targetNames = targets.Split('|');
            var targetList = new List<Database>();
            foreach (var item in PublishManager.GetPublishingTargets(database))
            {
                if (!targetNames.Contains(item.Name))
                {
                    continue;
                }

                var targetDatabaseName = item["Target database"];
                if (string.IsNullOrEmpty(targetDatabaseName))
                {
                    continue;
                }

                var targetDatabase = Factory.GetDatabase(targetDatabaseName);
                if (targetDatabase != null)
                {
                    targetList.Add(targetDatabase);
                }
            }

            return targetList;
        }

        private void PublishDatabase([NotNull] Database database, int mode, [NotNull] List<Language> languageList, [NotNull] List<Database> targetList)
        {
            Debug.ArgumentNotNull(database, nameof(database));
            Debug.ArgumentNotNull(languageList, nameof(languageList));
            Debug.ArgumentNotNull(targetList, nameof(targetList));

            switch (mode)
            {
                case 0:
                    PublishManager.PublishIncremental(database, targetList.ToArray(), languageList.ToArray());
                    break;

                case 1:
                    PublishManager.PublishSmart(database, targetList.ToArray(), languageList.ToArray());
                    break;

                case 2:
                    PublishManager.Republish(database, targetList.ToArray(), languageList.ToArray());
                    break;

                case 3:
                    PublishManager.RebuildDatabase(database, targetList.ToArray());
                    break;
            }
        }
    }
}
