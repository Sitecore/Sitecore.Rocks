// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Managers;
using Sitecore.Data.Templates;
using Sitecore.Diagnostics;
using Sitecore.Resources;

namespace Sitecore.Rocks.Server.Requests.Templates
{
    public class GetTemplateXml : GetTemplateBase
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

            var template = TemplateManager.GetTemplate(item.ID, item.Database);
            if (template == null)
            {
                throw new Exception("Template not found.");
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("template");
            output.WriteAttributeString("name", template.Name);
            output.WriteAttributeString("id", template.ID.ToString());
            output.WriteAttributeString("basetemplates", GetBaseTemplates(template, database));
            output.WriteAttributeString("icon", ImageBuilder.ResizeImageSrc(item.Appearance.Icon, 32, 32));
            output.WriteAttributeString("standardvaluesitemid", item[FieldIDs.StandardValues]);
            output.WriteAttributeString("path", item.Paths.Path);

            List<TemplateSection> sections;
            if (includeInheritedFields)
            {
                var fields = template.GetFields(true);
                sections = fields.Select(f => f.Section).Distinct().ToList();
            }
            else
            {
                sections = new List<TemplateSection>(template.GetSections());
            }

            sections.Sort(this);

            foreach (var section in sections)
            {
                WriteSection(output, database, section, template, includeInheritedFields, false);
            }

            output.WriteEndElement();

            return writer.ToString();
        }

        [NotNull]
        private string GetBaseTemplates([NotNull] Template template, [NotNull] Database database)
        {
            Debug.ArgumentNotNull(template, nameof(template));
            Debug.ArgumentNotNull(database, nameof(database));

            var result = string.Empty;
            foreach (var id in template.BaseIDs)
            {
                var t = TemplateManager.GetTemplate(id, database);
                if (t == null)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(result))
                {
                    result += "|";
                }

                result += t.ID.ToString();
            }

            return result;
        }
    }
}
