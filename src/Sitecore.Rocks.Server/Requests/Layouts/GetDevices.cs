// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Resources;
using Sitecore.Web.UI;

namespace Sitecore.Rocks.Server.Requests.Layouts
{
    public class GetDevices
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

            output.WriteStartElement("devices");

            foreach (var device in database.Resources.Devices.GetAll())
            {
                output.WriteStartElement("device");
                output.WriteAttributeString("id", device.ID.ToString());
                output.WriteAttributeString("name", device.Name);
                output.WriteAttributeString("displayname", device.DisplayName);
                output.WriteAttributeString("icon", Images.GetThemedImageSource(device.Icon, ImageDimension.id32x32));
                output.WriteEndElement();
            }

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}
