// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Resources;
using Sitecore.Web.UI;

namespace Sitecore.Rocks.Server.Requests.Templates
{
    public class GetTemplateFieldSorterFields
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string itemId)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(itemId, nameof(itemId));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            TemplateItem item = database.GetItem(itemId);
            if (item == null)
            {
                throw new Exception("Template item not found");
            }

            var sections = new Dictionary<string, SectionDefinition>();

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("template");
            output.WriteAttributeString("name", item.Name);
            output.WriteAttributeString("icon", Images.GetThemedImageSource(item.InnerItem.Appearance.Icon, ImageDimension.id32x32));
            output.WriteAttributeString("path", item.InnerItem.Paths.Path);

            var template = TemplateManager.GetTemplate(new ID(itemId), database);
            if (item == null)
            {
                throw new Exception("Template item not found");
            }

            foreach (var field in item.Fields)
            {
                var templateItem = field.InnerItem.Parent.Parent;
                var templateField = template.GetField(field.ID);

                SectionDefinition section;
                if (!sections.TryGetValue(templateField.Section.Name, out section))
                {
                    section = new SectionDefinition();
                    sections[templateField.Section.Name] = section;

                    section.SectionSortOrder = templateField.Section.Sortorder.ToString();
                    section.SectionItemId = templateField.Section.ID.ToString();
                }

                output.WriteStartElement("field");

                output.WriteAttributeString("id", field.ID.ToString());
                output.WriteAttributeString("name", field.Name);
                output.WriteAttributeString("type", field.Type);
                output.WriteAttributeString("sortorder", field.Sortorder.ToString());
                output.WriteAttributeString("sectionsortorder", section.SectionSortOrder);
                output.WriteAttributeString("sectionname", templateField.Section.Name);
                output.WriteAttributeString("sectionid", section.SectionItemId);
                output.WriteAttributeString("templatename", templateItem.Name);
                output.WriteAttributeString("templateicon", Images.GetThemedImageSource(templateItem.Appearance.Icon, ImageDimension.id16x16));
                output.WriteAttributeString("isinherited", templateItem.ID != item.ID ? "true" : "false");

                output.WriteEndElement();
            }

            output.WriteEndElement();

            return writer.ToString();
        }

        public class SectionDefinition
        {
            public string SectionItemId { get; set; }

            public string SectionSortOrder { get; set; }
        }
    }
}
