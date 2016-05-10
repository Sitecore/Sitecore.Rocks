// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensions.XmlTextWriterExtensions;
using Sitecore.Rocks.Server.Requests.Layouts;
using Sitecore.Text;

namespace Sitecore.Rocks.Server.Requests.LayoutBuilders
{
    public class GetRenderingsInPlaceHolder
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string placeHolderName, [NotNull] string placeHolderPath)
        {
            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var items = new List<Item>();

            foreach (var path in GetRenderings.RootPaths)
            {
                var item = database.GetItem(path);
                if (item != null)
                {
                    ProcessRenderings(items, placeHolderName, item);
                }
            }

            var placeHolderSettingsRoot = database.GetItem(ItemIDs.PlaceholderSettingsRoot);
            if (placeHolderSettingsRoot != null)
            {
                ProcessPlaceHolderSettings(items, database, placeHolderSettingsRoot.Paths.Path + "/" + placeHolderName);
                ProcessPlaceHolderSettings(items, database, placeHolderSettingsRoot.Paths.Path + placeHolderPath);
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("renderings");

            foreach (var item in items)
            {
                output.WriteItemHeader(item);
            }

            output.WriteEndElement();

            return writer.ToString();
        }

        private void ProcessPlaceHolderSettings(List<Item> items, Database database, string path)
        {
            var item = database.GetItem(path);
            if (item == null)
            {
                return;
            }

            var allowedControls = new MultilistField(item.Fields["Allowed Controls"]);

            foreach (var rendering in allowedControls.GetItems().Where(r => items.All(i => i.ID != r.ID)))
            {
                items.Add(rendering);
            }
        }

        private void ProcessRenderings(List<Item> items, string placeHolderName, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            foreach (Item child in item.Children)
            {
                var template = TemplateManager.GetTemplate(child);
                if (template == null)
                {
                    continue;
                }

                if (template.DescendsFrom(GetRenderings.RenderingOptionsId))
                {
                    var defaultParameters = child["Default Parameters"];
                    if (!string.IsNullOrEmpty(defaultParameters))
                    {
                        var url = new UrlString(defaultParameters);

                        if (string.Compare(url["Placeholder"], placeHolderName, StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            items.Add(item);
                        }
                    }
                }

                ProcessRenderings(items, placeHolderName, child);
            }
        }
    }
}
