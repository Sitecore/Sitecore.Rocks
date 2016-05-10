// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensions.XmlTextWriterExtensions;

namespace Sitecore.Rocks.Server.Requests.Layouts
{
    public class CreatePlaceHolderSettings
    {
        public static readonly ID PlaceHolderSettingsFolderTemplateId = new ID("{C3B037A0-46E5-4B67-AC7A-A144B962A56F}");

        public static readonly ID PlaceHolderSettingsTemplateId = new ID("{5C547D4E-7111-4995-95B0-6B561751BF2E}");

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

            var root = database.GetItem(ItemIDs.PlaceholderSettingsRoot);
            if (root == null)
            {
                return string.Empty;
            }

            var folderTemplate = database.GetItem(PlaceHolderSettingsFolderTemplateId);
            if (folderTemplate == null)
            {
                return string.Empty;
            }

            var placeHolderSettingsTemplate = database.GetItem(PlaceHolderSettingsTemplateId);
            if (placeHolderSettingsTemplate == null)
            {
                return string.Empty;
            }

            var p = root.Paths.Path + (path.StartsWith("/") ? string.Empty : "/") + path;

            var item = database.CreateItemPath(p, new TemplateItem(folderTemplate), new TemplateItem(placeHolderSettingsTemplate));
            if (item == null)
            {
                return string.Empty;
            }

            item.Editing.BeginEdit();
            item["Placeholder Key"] = path;
            item.Editing.EndEdit();

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteItemHeader(item);

            return writer.ToString();
        }
    }
}
