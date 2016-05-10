// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Documentation
{
    public class Export
    {
        public static readonly ID ViewRenderingId = new ID(new Guid("{99F8905D-4A87-4EB8-9F8B-A9BEBFB3ADD6}"));

        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string itemId)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(itemId, nameof(itemId));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new InvalidOperationException("Database not found");
            }

            var item = database.GetItem(itemId);
            if (item == null)
            {
                throw new InvalidOperationException("Item not found");
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer)
            {
                Formatting = Formatting.Indented
            };

            output.WriteStartElement("items");

            Write(output, item);

            output.WriteEndElement();

            return writer.ToString();
        }

        private void Write([NotNull] XmlTextWriter output, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(item, nameof(item));

            if (item.TemplateID == ViewRenderingId)
            {
                WriteViewRendering(output, item);
            }
            else if (item.TemplateID == TemplateIDs.TemplateField)
            {
                WriteTemplateField(output, item);
            }
            else if (string.Compare(item.TemplateName, "Speak-Documentation-Text", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                WriteDocumentationText(output, item);
            }

            foreach (Item child in item.Children)
            {
                Write(output, child);
            }
        }

        private void WriteDocumentationText([NotNull] XmlTextWriter output, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(item, nameof(item));

            var hasElements = false;

            foreach (var language in item.Languages)
            {
                var i = item.Database.GetItem(item.ID, language);

                var title = i["Title"];
                var subtitle = i["Subtitle"];
                var text = i["Text"];
                if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(subtitle) && string.IsNullOrEmpty(text))
                {
                    continue;
                }

                if (!hasElements)
                {
                    output.WriteStartElement("text");
                    output.WriteAttributeString("id", item.ID.ToString());

                    hasElements = true;
                }

                output.WriteStartElement("lang");
                output.WriteAttributeString("name", language.Name);

                if (!string.IsNullOrEmpty(title))
                {
                    output.WriteStartElement("title");
                    output.WriteCData(title);
                    output.WriteEndElement();
                }

                if (!string.IsNullOrEmpty(subtitle))
                {
                    output.WriteStartElement("subtitle");
                    output.WriteCData(subtitle);
                    output.WriteEndElement();
                }

                if (!string.IsNullOrEmpty(text))
                {
                    output.WriteStartElement("text");
                    output.WriteCData(text);
                    output.WriteEndElement();
                }

                output.WriteEndElement();
            }

            if (hasElements)
            {
                output.WriteEndElement();
            }
        }

        private void WriteTemplateField([NotNull] XmlTextWriter output, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(item, nameof(item));

            var hasElements = false;

            foreach (var language in item.Languages)
            {
                var i = item.Database.GetItem(item.ID, language);

                var shortHelp = i.Help.ToolTip;
                if (string.IsNullOrEmpty(shortHelp))
                {
                    continue;
                }

                if (!hasElements)
                {
                    output.WriteStartElement("templatefield");
                    output.WriteAttributeString("id", item.ID.ToString());
                    hasElements = true;
                }

                output.WriteStartElement("lang");
                output.WriteAttributeString("name", language.Name);

                if (!string.IsNullOrEmpty(shortHelp))
                {
                    output.WriteStartElement("short");
                    output.WriteCData(shortHelp);
                    output.WriteEndElement();
                }

                output.WriteEndElement();
            }

            if (hasElements)
            {
                output.WriteEndElement();
            }
        }

        private void WriteViewRendering([NotNull] XmlTextWriter output, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(item, nameof(item));

            var hasElements = false;

            foreach (var language in item.Languages)
            {
                var i = item.Database.GetItem(item.ID, language);

                var shortHelp = i.Help.ToolTip;
                var longHelp = i.Help.Text;
                var displayName = i[FieldIDs.DisplayName];

                if (string.IsNullOrEmpty(shortHelp) && string.IsNullOrEmpty(longHelp) && string.IsNullOrEmpty(displayName))
                {
                    continue;
                }

                if (!hasElements)
                {
                    output.WriteStartElement("viewrendering");
                    output.WriteAttributeString("id", item.ID.ToString());
                    hasElements = true;
                }

                output.WriteStartElement("lang");
                output.WriteAttributeString("name", language.Name);

                if (!string.IsNullOrEmpty(shortHelp))
                {
                    output.WriteStartElement("short");
                    output.WriteCData(shortHelp);
                    output.WriteEndElement();
                }

                if (!string.IsNullOrEmpty(longHelp))
                {
                    output.WriteStartElement("long");
                    output.WriteCData(longHelp);
                    output.WriteEndElement();
                }

                if (!string.IsNullOrEmpty(displayName))
                {
                    output.WriteStartElement("displayname");
                    output.WriteCData(displayName);
                    output.WriteEndElement();
                }

                output.WriteEndElement();
            }

            if (hasElements)
            {
                output.WriteEndElement();
            }
        }
    }
}
