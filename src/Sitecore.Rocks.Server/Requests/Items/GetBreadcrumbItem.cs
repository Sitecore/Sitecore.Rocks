// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Resources;
using Sitecore.Rocks.Server.Pipelines.BreadcrumbDropDown;
using Sitecore.Web.UI;

namespace Sitecore.Rocks.Server.Requests.Items
{
    public class GetBreadcrumbItem
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string path)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(path, nameof(path));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer)
            {
                Formatting = Formatting.Indented
            };

            var p = path.Replace("\\", "/");
            if (p.StartsWith("sitecore/", StringComparison.InvariantCultureIgnoreCase))
            {
                p = "/" + p;
            }

            var item = database.GetItem(p);
            if (item != null)
            {
                WriteBreadcrumb(output, item);
            }
            else
            {
                WriteDropDown(output, database, path);
            }

            return writer.ToString();
        }

        private void WriteBreadcrumb(XmlTextWriter output, Item item)
        {
            output.WriteStartElement("breadcrumb");
            output.WriteAttributeString("path", item.Paths.Path);

            GetItemFields.GetBreadcrumbItems(output, item, item.Database.GetRootItem());

            output.WriteEndElement();
        }

        private void WriteDropDown(XmlTextWriter output, Database database, string path)
        {
            var pipeline = BreadcrumbDropDownPipeline.Run().WithParameters(database, path);

            output.WriteStartElement("dropdown");

            foreach (var item in pipeline.Items)
            {
                output.WriteStartElement("item");
                output.WriteAttributeString("id", item.ID.ToString());
                output.WriteAttributeString("name", item.Name);
                output.WriteAttributeString("path", item.Paths.Path);
                output.WriteAttributeString("icon", Images.GetThemedImageSource(item.Appearance.Icon, ImageDimension.id16x16));
                output.WriteEndElement();
            }

            output.WriteEndElement();
        }
    }
}
