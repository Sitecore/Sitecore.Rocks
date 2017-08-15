// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Extensions.StringExtensions;
using Sitecore.Resources.Media;
using Sitecore.Rocks.Server.Extensions.XmlTextWriterExtensions;

namespace Sitecore.Rocks.Server.Requests.Media
{
    public class Upload
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string newItemPath, [NotNull] string fileStream)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(newItemPath, nameof(newItemPath));
            Assert.ArgumentNotNull(fileStream, nameof(fileStream));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var parentPath = newItemPath;
            var n = parentPath.LastIndexOf('/');
            if (n > 0)
            {
                parentPath = parentPath.Left(n);
            }

            var parent = database.GetItem(parentPath);
            if (parent == null)
            {
                throw new Exception("Item not found: " + parentPath);
            }

            var stream = new MemoryStream(System.Convert.FromBase64String(fileStream));

            var options = new MediaCreatorOptions
            {
                AlternateText = Path.GetFileNameWithoutExtension(newItemPath),
                Database = database,
                FileBased = false,
                IncludeExtensionInItemName = false,
                Language = LanguageManager.DefaultLanguage,
                Versioned = false
            };

            var item = MediaManager.Creator.CreateFromStream(stream, "/upload/" + Path.GetFileName(newItemPath), options);
            if (item == null)
            {
                throw new Exception("Failed to upload file");
            }

            item.MoveTo(parent);

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("upload");

            output.WriteItemHeader(item);

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}
