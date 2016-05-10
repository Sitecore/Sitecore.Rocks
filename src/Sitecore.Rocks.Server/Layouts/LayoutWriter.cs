// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Layouts;
using Sitecore.Resources;
using Sitecore.Rocks.Server.Extensions.XElementExtensions;
using Sitecore.Text;
using Sitecore.Web.UI;
using Sitecore.Web.UI.HtmlControls.Data;

namespace Sitecore.Rocks.Server.Layouts
{
    public class LayoutWriter
    {
        public void Write([NotNull] XmlTextWriter output, [NotNull] Database database, [NotNull] string layout)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(database, nameof(database));
            Assert.ArgumentNotNull(layout, nameof(layout));

            output.WriteStartElement("layout");

            WriteDevices(output, database);

            if (!string.IsNullOrEmpty(layout))
            {
                var layoutDefinition = LayoutDefinition.Parse(layout);

                var doc = XDocument.Parse(layout);
                var layoutRoot = doc.Root;
                if (layoutRoot == null)
                {
                    throw new InvalidOperationException("Invalid layout definition");
                }

                WriteLayout(output, database, layoutDefinition, layoutRoot);
            }

            output.WriteEndElement();
        }

        private void WriteDevice([NotNull] XmlTextWriter output, [NotNull] Database database, [NotNull] DeviceDefinition device, [NotNull] XElement deviceElement)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(database, nameof(database));
            Assert.ArgumentNotNull(device, nameof(device));
            Assert.ArgumentNotNull(deviceElement, nameof(deviceElement));

            var item = database.GetItem(device.ID);
            if (item == null)
            {
                return;
            }

            var icon = Images.GetThemedImageSource(item.Appearance.Icon, ImageDimension.id16x16);

            output.WriteStartElement("d");
            output.WriteAttributeString("id", device.ID);
            output.WriteAttributeString("ic", icon);
            output.WriteAttributeString("l", device.Layout);

            var deviceItem = database.GetItem(device.ID);
            if (deviceItem != null)
            {
                output.WriteAttributeString("n", deviceItem.Name);
            }

            if (!string.IsNullOrEmpty(device.Layout))
            {
                var layout = database.GetItem(device.Layout);
                if (layout != null)
                {
                    output.WriteAttributeString("ln", layout.Name);
                    output.WriteAttributeString("lp", layout.Paths.Path);

                    var placeHolders = PlaceHolderAnalyzer.Analyze(layout).ToArray();
                    if (placeHolders.Any())
                    {
                        output.WriteAttributeString("ph", string.Join(",", placeHolders));
                    }
                }
            }

            var renderings = device.Renderings;
            if (renderings != null)
            {
                foreach (RenderingDefinition rendering in renderings)
                {
                    var renderingElement = deviceElement.Elements("r").FirstOrDefault(e => e.GetAttributeValue("uid") == rendering.UniqueId);
                    WriteRendering(output, database, rendering, renderingElement);
                }
            }

            var placeholders = device.Placeholders;
            if (placeholders != null)
            {
                foreach (PlaceholderDefinition placeholder in placeholders)
                {
                    WritePlaceholder(output, placeholder);
                }
            }

            output.WriteEndElement();
        }

        private void WriteDevices([NotNull] XmlTextWriter output, [NotNull] Database database)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(database, nameof(database));

            var devices = database.GetItem(ItemIDs.DevicesRoot);
            if (devices == null)
            {
                return;
            }

            output.WriteStartElement("devices");

            foreach (Item child in devices.Children)
            {
                var icon = Images.GetThemedImageSource(child.Appearance.Icon, ImageDimension.id16x16);

                output.WriteStartElement("d");

                output.WriteAttributeString("id", child.ID.ToString());
                output.WriteAttributeString("ic", icon);
                output.WriteValue(child.Name);

                output.WriteEndElement();
            }

            output.WriteEndElement();
        }

        private void WriteLayout([NotNull] XmlTextWriter output, [NotNull] Database database, [NotNull] LayoutDefinition layoutDefinition, [NotNull] XElement layoutElement)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(database, nameof(database));
            Assert.ArgumentNotNull(layoutDefinition, nameof(layoutDefinition));
            Assert.ArgumentNotNull(layoutElement, nameof(layoutElement));

            output.WriteStartElement("layout");

            var devices = layoutDefinition.Devices;
            if (devices != null)
            {
                foreach (DeviceDefinition device in devices)
                {
                    var deviceElement = layoutElement.Elements("d").FirstOrDefault(e => e.GetAttributeValue("id") == device.ID);
                    if (deviceElement == null)
                    {
                        throw new InvalidOperationException(string.Format("Device \"{0}\" not found", device.ID));
                    }

                    WriteDevice(output, database, device, deviceElement);
                }
            }

            output.WriteEndElement();
        }

        private void WriteParametersTemplate([NotNull] XmlTextWriter output, [NotNull] Item templateItem)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(templateItem, nameof(templateItem));

            var template = TemplateManager.GetTemplate(templateItem.ID, templateItem.Database);

            output.WriteStartElement("template");

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

            output.WriteEndElement();
        }

        private void WritePlaceholder([NotNull] XmlTextWriter output, [NotNull] PlaceholderDefinition placeholder)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(placeholder, nameof(placeholder));

            output.WriteStartElement("p");

            output.WriteAttributeString("key", placeholder.Key ?? string.Empty);
            output.WriteAttributeString("md", placeholder.MetaDataItemId ?? string.Empty);
            output.WriteAttributeString("uid", placeholder.UniqueId ?? string.Empty);
            output.WriteAttributeString("ic", Images.GetThemedImageSource("Applications/16x16/selection.png", ImageDimension.id16x16));

            output.WriteEndElement();
        }

        private void WriteRendering([NotNull] XmlTextWriter output, [NotNull] Database database, [NotNull] RenderingDefinition rendering, [CanBeNull] XElement renderingElement)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(database, nameof(database));
            Assert.ArgumentNotNull(rendering, nameof(rendering));

            if (rendering.ItemID == null)
            {
                return;
            }

            var item = database.GetItem(rendering.ItemID);

            output.WriteStartElement("r");

            var itemName = "<Unknown Rendering>";
            var dataSourceLocation = string.Empty;
            var dataSourceTemplate = string.Empty;
            var path = string.Empty;
            var parametersTemplate = string.Empty;
            var icon = "Core2/16x16/question_mark.png";
            var placeHolders = string.Empty;
            var speakCoreVersion = string.Empty;
            var speakCoreVersionId = string.Empty;

            if (item != null)
            {
                itemName = item.Name;
                dataSourceLocation = item["Datasource Location"];
                dataSourceTemplate = item["Datasource Template"];
                path = item["Path"];
                parametersTemplate = item["Parameters Template"];
                icon = item.Appearance.Icon;
                placeHolders = string.Join(",", PlaceHolderAnalyzer.Analyze(item).ToArray());

                var helper = new SpeakCoreVersionHelper();
                var versionItem = helper.GetSpeakCoreVersionItem(item);
                if (versionItem != null)
                {
                    speakCoreVersion = versionItem.Name;
                    speakCoreVersionId = versionItem.ID.ToString();
                }
            }

            output.WriteAttributeString("name", itemName);
            output.WriteAttributeString("cac", rendering.Cachable ?? "0");
            output.WriteAttributeString("cnd", rendering.Conditions ?? string.Empty);
            output.WriteAttributeString("ds", rendering.Datasource ?? string.Empty);
            output.WriteAttributeString("id", rendering.ItemID);
            output.WriteAttributeString("mvt", rendering.MultiVariateTest ?? string.Empty);
            output.WriteAttributeString("par", rendering.Parameters ?? string.Empty);
            output.WriteAttributeString("ph", rendering.Placeholder ?? string.Empty);
            output.WriteAttributeString("uid", rendering.UniqueId ?? string.Empty);
            output.WriteAttributeString("vbd", rendering.VaryByData ?? "0");
            output.WriteAttributeString("vbdev", rendering.VaryByDevice ?? "0");
            output.WriteAttributeString("vbl", rendering.VaryByLogin ?? "0");
            output.WriteAttributeString("vbp", rendering.VaryByParameters ?? "0");
            output.WriteAttributeString("vbqs", rendering.VaryByQueryString ?? "0");
            output.WriteAttributeString("vbu", rendering.VaryByUser ?? "0");
            output.WriteAttributeString("ic", Images.GetThemedImageSource(icon, ImageDimension.id16x16));

            output.WriteAttributeString("placeholders", placeHolders);
            output.WriteAttributeString("dsl", dataSourceLocation);
            output.WriteAttributeString("dst", dataSourceTemplate);
            output.WriteAttributeString("path", path);
            output.WriteAttributeString("templateid", parametersTemplate);
            output.WriteAttributeString("speakcoreversion", speakCoreVersion);
            output.WriteAttributeString("speakcoreversionid", speakCoreVersionId);

            if (renderingElement != null)
            {
                if (renderingElement.HasElements)
                {
                    var rlsElement = renderingElement.Element("rls");
                    if (rlsElement != null)
                    {
                        var ruleset = rlsElement.InnerText();
                        output.WriteAttributeString("rls", ruleset);
                    }
                }
            }

            if (item != null && !string.IsNullOrEmpty(parametersTemplate))
            {
                var templateItem = item.Database.GetItem(parametersTemplate);
                if (templateItem != null)
                {
                    WriteParametersTemplate(output, templateItem);
                }
            }

            output.WriteEndElement();
        }
    }
}
