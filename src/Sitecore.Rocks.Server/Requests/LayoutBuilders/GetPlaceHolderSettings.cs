// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Rocks.Server.Extensions.XmlTextWriterExtensions;

namespace Sitecore.Rocks.Server.Requests.LayoutBuilders
{
    public class GetPlaceHolderSettings
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string placeHolderName, [NotNull] string placeHolderPath)
        {
            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var placeHolderSettingsRoot = database.GetItem(ItemIDs.PlaceholderSettingsRoot);
            if (placeHolderSettingsRoot == null)
            {
                return string.Empty;
            }

            var item = database.GetItem(placeHolderSettingsRoot.Paths.Path + placeHolderPath) ?? database.GetItem(placeHolderSettingsRoot.Paths.Path + "/" + placeHolderName);
            if (item == null)
            {
                return string.Empty;
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteItemHeader(item);

            return writer.ToString();
        }
    }
}
