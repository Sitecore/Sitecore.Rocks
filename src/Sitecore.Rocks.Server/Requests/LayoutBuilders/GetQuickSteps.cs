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
using Sitecore.Layouts;
using Sitecore.Rocks.Server.Extensions.XmlTextWriterExtensions;
using Sitecore.Rocks.Server.Requests.Layouts;
using Sitecore.Text;

namespace Sitecore.Rocks.Server.Requests.LayoutBuilders
{
    public class GetQuickSteps
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string layout)
        {
            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("steps");

            var layoutDefinition = LayoutDefinition.Parse(layout);

            var placeHolders = GetPlaceHolders(database, layoutDefinition);

            foreach (var path in GetRenderings.RootPaths)
            {
                var item = database.GetItem(path);
                if (item != null)
                {
                    ProcessRenderings(output, placeHolders, item);
                }
            }

            output.WriteEndElement();

            return writer.ToString();
        }

        [NotNull]
        private List<string> GetPlaceHolders([NotNull] Database database, [NotNull] LayoutDefinition layoutDefinition)
        {
            var result = new List<string>();

            result.Add("Page.Code");
            result.Add("Page.Body");

            var devices = layoutDefinition.Devices;
            if (devices != null)
            {
                foreach (DeviceDefinition device in devices)
                {
                    GetPlaceHolders(database, result, device);
                    break;
                }
            }

            return result;
        }

        private void GetPlaceHolders(Database database, List<string> placeHolders, DeviceDefinition device)
        {
            var renderings = device.Renderings;
            if (renderings == null)
            {
                return;
            }

            foreach (RenderingDefinition rendering in renderings)
            {
                var item = database.GetItem(rendering.ItemID);
                if (item == null)
                {
                    continue;
                }

                var p = item["Place Holders"];
                if (string.IsNullOrEmpty(p))
                {
                    continue;
                }

                foreach (var n in p.Split(','))
                {
                    var url = rendering.Parameters;
                    if (string.IsNullOrEmpty(url))
                    {
                        continue;
                    }

                    var parameters = new UrlString(url);
                    var id = parameters.Parameters["id"];

                    var name = n.Replace("$Id", id).Trim();

                    if (!placeHolders.Contains(name))
                    {
                        placeHolders.Add(name);
                    }
                }
            }
        }

        private void ProcessRenderings([NotNull] XmlTextWriter output, List<string> placeHolderNames, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(item, nameof(item));

            foreach (Item child in item.Children)
            {
                var template = TemplateManager.GetTemplate(child);

                if (template.DescendsFrom(GetRenderings.RenderingOptionsId))
                {
                    var defaultParameters = child["Default Parameters"];
                    if (!string.IsNullOrEmpty(defaultParameters))
                    {
                        var url = new UrlString(defaultParameters);

                        var name = url["Placeholder"];
                        if (placeHolderNames.Contains(name))
                        {
                            output.WriteStartElement("add");
                            output.WriteAttributeString("placeholder", name);
                            output.WriteItemHeader(child);
                            output.WriteEndElement();
                        }
                    }
                }

                ProcessRenderings(output, placeHolderNames, child);
            }
        }
    }
}
