// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensions.XmlTextWriterExtensions;

namespace Sitecore.Rocks.Server.Requests.Layouts
{
    public class GetRenderings
    {
        public static readonly ID RenderingOptionsId = new ID("{D1592226-3898-4CE2-B190-090FD5F84A4C}");

        public static readonly string[] RootPaths = new[]
        {
            "/sitecore/client",
            "/sitecore/layout"
        };

        [NotNull]
        public string Execute([NotNull] string databaseName)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("renderings");

            foreach (var path in RootPaths)
            {
                var item = database.GetItem(path);
                if (item != null)
                {
                    ProcessRenderings(output, item);
                }
            }

            output.WriteEndElement();

            return writer.ToString();
        }

        private void ProcessRenderings([NotNull] XmlTextWriter output, [NotNull] Item item)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(item, nameof(item));

            foreach (Item child in item.Children)
            {
                try
                {
                    var template = TemplateManager.GetTemplate(child);

                    if (template.DescendsFrom(RenderingOptionsId))
                    {
                        output.WriteItemHeader(child);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Get Renderings", ex, GetType());
                }

                ProcessRenderings(output, child);
            }
        }
    }
}
