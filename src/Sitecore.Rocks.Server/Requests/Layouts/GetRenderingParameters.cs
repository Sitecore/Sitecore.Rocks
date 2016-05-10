// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Linq;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Resources;
using Sitecore.Rocks.Server.Layouts;
using Sitecore.Text;
using Sitecore.Web.UI;
using Sitecore.Web.UI.HtmlControls.Data;

namespace Sitecore.Rocks.Server.Requests.Layouts
{
    public class GetRenderingParameters
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string itemId)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(itemId, nameof(itemId));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new ArgumentException("Database not found");
            }

            var item = database.GetItem(itemId);
            if (item == null)
            {
                throw new ArgumentException("Item not found");
            }

            var speakCoreVersion = string.Empty;
            var speakCoreVersionId = string.Empty;

            var helper = new SpeakCoreVersionHelper();
            var versionItem = helper.GetSpeakCoreVersionItem(item);
            if (versionItem != null)
            {
                speakCoreVersion = versionItem.Name;
                speakCoreVersionId = versionItem.ID.ToString();
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer)
            {
                Formatting = Formatting.Indented
            };

            output.WriteStartElement("template");
            output.WriteAttributeString("name", item.Name);
            output.WriteAttributeString("icon", Images.GetThemedImageSource(item.Appearance.Icon, ImageDimension.id16x16));
            output.WriteAttributeString("parameters", item["Default Parameters"]);
            output.WriteAttributeString("placeholders", string.Join(",", PlaceHolderAnalyzer.Analyze(item).ToArray()));
            output.WriteAttributeString("dsl", item["Datasource Location"]);
            output.WriteAttributeString("dst", item["Datasource Template"]);
            output.WriteAttributeString("path", item["Path"]);
            output.WriteAttributeString("templateid", item["Parameters Template"]);
            output.WriteAttributeString("speakcoreversion", speakCoreVersion);
            output.WriteAttributeString("speakcoreversionid", speakCoreVersionId);

            var template = item["Parameters Template"];
            if (!string.IsNullOrEmpty(template))
            {
                var templateItem = item.Database.GetItem(template);
                if (templateItem != null)
                {
                    WriteParametersTemplate(output, templateItem);
                }
            }

            output.WriteEndElement();

            return writer.ToString();
        }

        private void WriteParametersTemplate([NotNull] XmlTextWriter output, [NotNull] Item templateItem)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(templateItem, nameof(templateItem));

            var template = TemplateManager.GetTemplate(templateItem.ID, templateItem.Database);

            foreach (var field in template.GetFields(true))
            {
                if (field.Template.BaseIDs.Length == 0)
                {
                    continue;
                }

                output.WriteStartElement("field");
                output.WriteAttributeString("name", field.Name);
                output.WriteAttributeString("type", field.Type);
                output.WriteAttributeString("category", field.Section.Name);
                output.WriteAttributeString("description", field.GetToolTip(Language.Current));

                var title = field.GetTitle(Language.Current);
                if (!string.IsNullOrEmpty(title))
                {
                    output.WriteAttributeString("displayname", title);
                }

                var bindingMode = BindingMode.ReadWrite;

                var source = new UrlString(field.Source);

                var bindMode = source.Parameters["bindmode"];
                if (!string.IsNullOrEmpty(bindMode))
                {
                    switch (bindMode.ToLowerInvariant())
                    {
                        case "readwrite":
                            bindingMode = BindingMode.ReadWrite;
                            break;
                        case "read":
                            bindingMode = BindingMode.Read;
                            break;
                        case "write":
                            bindingMode = BindingMode.Write;
                            break;
                        case "server":
                            bindingMode = BindingMode.Server;
                            break;
                        case "none":
                            bindingMode = BindingMode.None;
                            break;
                    }
                }

                output.WriteAttributeString("bindmode", bindingMode.ToString());

                var subtype = source.Parameters["subtype"];
                if (!string.IsNullOrEmpty(subtype))
                {
                    output.WriteAttributeString("subtype", subtype.ToLowerInvariant());
                }

                var editor = source.Parameters["editor"];
                if (!string.IsNullOrEmpty(editor))
                {
                    output.WriteAttributeString("editor", editor);
                }

                var valueLookup = string.Compare(field.Type, "valuelookup", StringComparison.InvariantCultureIgnoreCase) == 0 || string.Compare(field.Type, "droplist", StringComparison.InvariantCultureIgnoreCase) == 0;
                var lookup = string.Compare(field.Type, "lookup", StringComparison.InvariantCultureIgnoreCase) == 0 || string.Compare(field.Type, "droplink", StringComparison.InvariantCultureIgnoreCase) == 0;

                if (valueLookup || lookup)
                {
                    var items = LookupSources.GetItems(templateItem.Database.GetRootItem(), field.Source);
                    if (items.Length > 0)
                    {
                        output.WriteStartElement("values");

                        foreach (var item in items)
                        {
                            output.WriteStartElement("value");
                            output.WriteAttributeString("displayname", item.Name);
                            output.WriteValue(valueLookup ? item.Name : item.ID.ToString());
                            output.WriteEndElement();
                        }

                        output.WriteEndElement();
                    }
                }

                output.WriteEndElement();
            }
        }
    }
}
