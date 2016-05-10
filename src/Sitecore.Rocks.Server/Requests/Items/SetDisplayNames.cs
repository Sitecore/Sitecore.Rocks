// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Items
{
    public class SetDisplayNames
    {
        [NotNull]
        public string Execute([NotNull] string id, [NotNull] string databaseName, [NotNull] string names)
        {
            Assert.ArgumentNotNull(id, nameof(id));
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(names, nameof(names));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer)
            {
                Formatting = Formatting.Indented
            };

            output.WriteStartElement("items");

            foreach (var pair in names.Split('^'))
            {
                var parts = pair.Split('|');

                var languageName = parts[0];
                var displayName = parts[1];

                var language = LanguageManager.GetLanguage(languageName, database);
                if (language == null)
                {
                    continue;
                }

                var item = database.GetItem(id, language);
                if (item == null)
                {
                    continue;
                }

                item.Editing.BeginEdit();
                item.Appearance.DisplayName = displayName;
                item.Editing.EndEdit();

                output.WriteStartElement("item");
                output.WriteAttributeString("id", item.ID.ToString());
                output.WriteAttributeString("l", item.Language.ToString());
                output.WriteAttributeString("v", item.Version.ToString());
                output.WriteAttributeString("f", FieldIDs.DisplayName.ToString());
                output.WriteValue(displayName);
                output.WriteEndElement();
            }

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}
