// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Publishing;

namespace Sitecore.Rocks.Server.Requests.Publishing
{
    public class GetAdvancedPublishingInformation
    {
        [NotNull]
        public string Execute([NotNull] string databaseName)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("publish");

            output.WriteStartElement("languages");

            foreach (var language in LanguageManager.GetLanguages(database))
            {
                output.WriteStartElement("language");
                output.WriteAttributeString("name", language.Name);
                output.WriteAttributeString("displayname", language.GetDisplayName());
                output.WriteEndElement();
            }

            output.WriteEndElement();

            output.WriteStartElement("targets");

            foreach (var target in PublishManager.GetPublishingTargets(database))
            {
                output.WriteStartElement("target");
                output.WriteAttributeString("name", target.Name);
                output.WriteAttributeString("database", target["Target database"]);
                output.WriteEndElement();
            }

            output.WriteEndElement();

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}
