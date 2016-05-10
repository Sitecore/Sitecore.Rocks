// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data.Managers;

namespace Sitecore.Rocks.Server.Requests.Sites
{
    public class GetDatabasesAndLanguages
    {
        [NotNull]
        public string Execute()
        {
            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("databases");

            foreach (var database in Factory.GetDatabases())
            {
                if (database.Name == "filesystem")
                {
                    continue;
                }

                output.WriteStartElement("database");
                output.WriteAttributeString("name", database.Name);

                foreach (var language in LanguageManager.GetLanguages(database))
                {
                    output.WriteStartElement("language");
                    output.WriteAttributeString("name", language.Name);
                    output.WriteAttributeString("displayname", language.GetDisplayName());
                    output.WriteEndElement();
                }

                output.WriteEndElement();
            }

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}
