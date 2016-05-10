// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml.Linq;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensions.XElementExtensions;

namespace Sitecore.Rocks.Server.Requests.Documentation
{
    public class Import
    {
        public static readonly ID ViewRenderingId = new ID(new Guid("{99F8905D-4A87-4EB8-9F8B-A9BEBFB3ADD6}"));

        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string xml)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(xml, nameof(xml));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new InvalidOperationException("Database not found");
            }

            var doc = XDocument.Parse(xml);
            var root = doc.Root;
            if (root == null)
            {
                return "File has no elements.";
            }

            var output = new StringWriter();

            foreach (var element in root.Elements())
            {
                var id = element.GetAttributeValue("id");
                var item = database.GetItem(id);
                if (item == null)
                {
                    continue;
                }

                switch (element.Name.ToString())
                {
                    case "text":
                        UpdateDocumentationText(output, item, element);
                        break;
                    case "templatefield":
                        UpdateTemplateField(output, item, element);
                        break;
                    case "viewrendering":
                        UpdateViewRendering(output, item, element);
                        break;
                    default:
                        output.WriteLine("Unknown key: " + element.Name);
                        break;
                }
            }

            return output.ToString();
        }

        private void UpdateDocumentationText([NotNull] StringWriter output, [NotNull] Item item, [NotNull] XElement element)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(item, nameof(item));
            Debug.ArgumentNotNull(element, nameof(element));

            foreach (var langElement in element.Elements())
            {
                var languageName = langElement.GetAttributeValue("name");
                if (string.IsNullOrEmpty(languageName))
                {
                    output.WriteLine("Language not found: " + languageName);
                    continue;
                }

                var i = item.Database.GetItem(item.ID, LanguageManager.GetLanguage(languageName));
                if (i == null)
                {
                    output.WriteLine("Item not found: " + item.ID);
                    continue;
                }

                var title = langElement.GetElementValue("title");
                var subtitle = langElement.GetElementValue("subtitle");
                var text = langElement.GetElementValue("text");

                i.Editing.BeginEdit();

                if (!string.IsNullOrEmpty(title))
                {
                    i["Title"] = title;
                }

                if (!string.IsNullOrEmpty(subtitle))
                {
                    i["Subtitle"] = title;
                }

                if (!string.IsNullOrEmpty(text))
                {
                    i["Text"] = title;
                }

                i.Editing.EndEdit();
            }
        }

        private void UpdateTemplateField([NotNull] StringWriter output, [NotNull] Item item, [NotNull] XElement element)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(item, nameof(item));
            Debug.ArgumentNotNull(element, nameof(element));

            foreach (var langElement in element.Elements())
            {
                var languageName = langElement.GetAttributeValue("name");
                if (string.IsNullOrEmpty(languageName))
                {
                    output.WriteLine("Language not found: " + languageName);
                    continue;
                }

                var i = item.Database.GetItem(item.ID, LanguageManager.GetLanguage(languageName));
                if (i == null)
                {
                    output.WriteLine("Item not found: " + item.ID);
                    continue;
                }

                var shortHelp = langElement.GetElementValue("short");

                i.Editing.BeginEdit();

                i.Help.ToolTip = shortHelp;

                i.Editing.EndEdit();
            }
        }

        private void UpdateViewRendering([NotNull] StringWriter output, [NotNull] Item item, [NotNull] XElement element)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(item, nameof(item));
            Debug.ArgumentNotNull(element, nameof(element));

            foreach (var langElement in element.Elements())
            {
                var languageName = langElement.GetAttributeValue("name");
                if (string.IsNullOrEmpty(languageName))
                {
                    output.WriteLine("Language not found: " + languageName);
                    continue;
                }

                var i = item.Database.GetItem(item.ID, LanguageManager.GetLanguage(languageName));
                if (i == null)
                {
                    output.WriteLine("Item not found: " + item.ID);
                    continue;
                }

                var shortHelp = langElement.GetElementValue("short");
                var longHelp = langElement.GetElementValue("long");
                var displayName = langElement.GetElementValue("displayname");

                i.Editing.BeginEdit();

                if (!string.IsNullOrEmpty(shortHelp))
                {
                    i.Help.ToolTip = shortHelp;
                }

                if (!string.IsNullOrEmpty(longHelp))
                {
                    i.Help.Text = longHelp;
                }

                if (!string.IsNullOrEmpty(displayName))
                {
                    i.Appearance.DisplayName = displayName;
                }

                i.Editing.EndEdit();
            }
        }
    }
}
