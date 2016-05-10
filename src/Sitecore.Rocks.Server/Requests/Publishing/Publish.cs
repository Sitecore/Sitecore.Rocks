// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Configuration;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Publishing;

namespace Sitecore.Rocks.Server.Requests.Publishing
{
    public class Publish
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, int mode)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

            var database = Factory.GetDatabase(databaseName);

            var publishingTargets = PublishManager.GetPublishingTargets(database);

            var targetDatabases = publishingTargets.Select(target => Factory.GetDatabase(target["Target database"])).ToArray();

            var languages = LanguageManager.GetLanguages(database).ToArray();

            switch (mode)
            {
                case 0:
                    PublishManager.Republish(database, targetDatabases, languages);
                    break;

                case 1:
                    PublishManager.PublishIncremental(database, targetDatabases, languages);
                    break;

                case 2:
                    PublishManager.PublishSmart(database, targetDatabases, languages);
                    break;

                case 3:
                    PublishManager.RebuildDatabase(database, targetDatabases);
                    break;
            }

            return string.Empty;
        }
    }
}
