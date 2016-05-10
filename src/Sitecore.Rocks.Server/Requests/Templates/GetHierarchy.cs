// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Resources;
using Sitecore.Web.UI;

namespace Sitecore.Rocks.Server.Requests.Templates
{
    public class GetHierarchy
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string itemId, [NotNull] string hierarchy)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(itemId, nameof(itemId));
            Assert.ArgumentNotNull(hierarchy, nameof(hierarchy));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var item = database.GetItem(itemId);
            if (item == null)
            {
                throw new Exception("Item not found");
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            if (hierarchy == "SuperTemplates")
            {
                WriteSuperTemplates(output, item);
            }
            else
            {
                WriteSubTemplates(output, item);
            }

            return writer.ToString();
        }

        private void WriteSubTemplates([NotNull] XmlTextWriter output, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(item, nameof(item));

            output.WriteStartElement("item");
            output.WriteAttributeString("id", item.ID.ToString());
            output.WriteAttributeString("name", item.Name);
            output.WriteAttributeString("icon", Images.GetThemedImageSource(item.Appearance.Icon, ImageDimension.id16x16));

            var template = TemplateManager.GetTemplate(item.ID, item.Database);

            foreach (var baseTemplate in template.GetDescendants())
            {
                var i = item.Database.GetItem(baseTemplate.ID);
                WriteSubTemplates(output, i);
            }

            output.WriteEndElement();
        }

        private void WriteSuperTemplates([NotNull] XmlTextWriter output, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(item, nameof(item));

            output.WriteStartElement("item");
            output.WriteAttributeString("id", item.ID.ToString());
            output.WriteAttributeString("name", item.Name);
            output.WriteAttributeString("icon", Images.GetThemedImageSource(item.Appearance.Icon, ImageDimension.id16x16));

            var innerField = item.Fields["__base template"];
            if (innerField != null)
            {
                var multiList = new MultilistField(innerField);
                foreach (var baseTemplate in multiList.GetItems())
                {
                    WriteSuperTemplates(output, baseTemplate);
                }
            }

            output.WriteEndElement();
        }
    }
}
