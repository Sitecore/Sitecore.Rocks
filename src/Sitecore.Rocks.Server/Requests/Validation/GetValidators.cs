// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Resources;
using Sitecore.Web.UI;

namespace Sitecore.Rocks.Server.Requests.Validation
{
    public class GetValidators
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

            var item = database.GetItem("/sitecore/system/settings/Validation Rules/Item Rules");

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("validators");

            WriteValidator(output, item);

            output.WriteEndElement();

            return writer.ToString();
        }

        private void WriteValidator([NotNull] XmlTextWriter output, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(item, nameof(item));

            if (item.TemplateID != TemplateIDs.Folder)
            {
                var section = item.Parent.Name;
                var path = item.Paths.Path;

                output.WriteStartElement("validator");

                output.WriteAttributeString("id", item.ID.ToString());
                output.WriteAttributeString("icon", Images.GetThemedImageSource(item.Appearance.Icon, ImageDimension.id16x16));
                output.WriteAttributeString("section", section);
                output.WriteAttributeString("path", path);

                output.WriteValue(item.Name);

                output.WriteEndElement();
            }

            foreach (Item child in item.Children)
            {
                WriteValidator(output, child);
            }
        }
    }
}
