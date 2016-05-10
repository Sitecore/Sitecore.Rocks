// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Templates
{
    public class GetSectionXml : GetTemplateBase
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string id, bool includeInheritedFields)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(id, nameof(id));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var item = database.GetItem(id);
            if (item == null)
            {
                throw new Exception("Item not found.");
            }

            var template = TemplateManager.GetTemplate(item.ParentID, item.Database);
            if (template == null)
            {
                throw new Exception("Template not found.");
            }

            var section = template.GetSection(new ID(id));
            if (section == null)
            {
                return string.Empty;
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("template");

            WriteSection(output, database, section, template, includeInheritedFields, true);

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}
