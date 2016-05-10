// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Resources;
using Sitecore.Web.UI;

namespace Sitecore.Rocks.Server.Requests.Layouts
{
    public class GetLayouts
    {
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

            output.WriteStartElement("layouts");

            WriteLayouts(output, database.SelectItems("fast://*[@@templateid='" + TemplateIDs.Layout + "']"));
            WriteLayouts(output, database.SelectItems("fast://*[@@templateid='" + TemplateIDs.XMLLayout + "']"));

            output.WriteEndElement();

            return writer.ToString();
        }

        private static void WriteLayouts(XmlTextWriter output, Item[] layouts)
        {
            foreach (var item in layouts)
            {
                if (StandardValuesManager.IsStandardValuesHolder(item))
                {
                    continue;
                }

                if (item.Paths.Path.StartsWith("/sitecore/templates/branches", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                var section = item.Parent.Name;
                var path = item.Paths.Path;

                output.WriteStartElement("layout");

                output.WriteAttributeString("id", item.ID.ToString());
                output.WriteAttributeString("icon", Images.GetThemedImageSource(item.Appearance.Icon, ImageDimension.id16x16));
                output.WriteAttributeString("section", section);
                output.WriteAttributeString("path", path);

                output.WriteValue(item.Name);

                output.WriteEndElement();
            }
        }
    }
}
