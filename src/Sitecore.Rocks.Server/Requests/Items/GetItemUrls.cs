// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Links;
using Sitecore.Sites;
using Sitecore.Text;

namespace Sitecore.Rocks.Server.Requests.Items
{
    public class GetItemUrls
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string itemId)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(itemId, nameof(itemId));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var item = database.GetItem(itemId);
            if (item == null)
            {
                throw new Exception("Item not found");
            }

            var devices = database.GetItem(ItemIDs.DevicesRoot);
            if (devices == null)
            {
                return string.Empty;
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("urls");

            foreach (Item child in devices.Children)
            {
                var options = new UrlOptions
                {
                    Site = SiteContext.GetSite("website"),
                    AddAspxExtension = true,
                    AlwaysIncludeServerUrl = true
                };

                var url = new UrlString(LinkManager.GetItemUrl(item, options));

                url["sc_device"] = child.Name;

                output.WriteStartElement("url");
                output.WriteAttributeString("url", url.ToString());
                output.WriteValue(child.Name);
                output.WriteEndElement();
            }

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}
