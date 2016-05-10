// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Publishing
{
    public class GetPublishingRestrictions
    {
        private static readonly ID publishingTargetsId = new ID("{D9E44555-02A6-407A-B4FC-96B9026CAADD}");

        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string id, [NotNull] string languageName)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(id, nameof(id));
            Assert.ArgumentNotNull(languageName, nameof(languageName));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var item = database.GetItem(id, LanguageManager.GetLanguage(languageName));
            if (item == null)
            {
                throw new Exception("Item not found");
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("restrictions");
            output.WriteAttributeString("publishable", item.Publishing.NeverPublish ? "0" : "1");
            output.WriteAttributeString("publishfrom", DateUtil.ToIsoDate(item.Publishing.PublishDate));
            output.WriteAttributeString("publishto", DateUtil.ToIsoDate(item.Publishing.UnpublishDate));

            output.WriteStartElement("versions");

            foreach (var version in item.Versions.GetVersions())
            {
                output.WriteStartElement("version");

                output.WriteAttributeString("number", version.Version.ToString());
                output.WriteAttributeString("publishable", version.Publishing.HideVersion ? "0" : "1");
                output.WriteAttributeString("publishfrom", DateUtil.ToIsoDate(version.Publishing.ValidFrom));
                output.WriteAttributeString("publishto", DateUtil.ToIsoDate(version.Publishing.ValidTo));

                output.WriteEndElement();
            }

            output.WriteEndElement();

            var targets = item.Database.GetItem(publishingTargetsId);
            if (targets != null)
            {
                var publishingTargets = item[FieldIDs.PublishingTargets];

                output.WriteStartElement("targets");

                foreach (Item target in targets.Children)
                {
                    output.WriteStartElement("target");

                    output.WriteAttributeString("id", target.ID.ToString());
                    output.WriteAttributeString("name", target.Name);
                    output.WriteAttributeString("isselected", publishingTargets.Contains(target.ID.ToString()) ? "1" : "0");

                    output.WriteEndElement();
                }

                output.WriteEndElement();
            }

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}
