// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.ImageLib;
using Sitecore.IO;
using Sitecore.Resources.Media;
using Sitecore.SecurityModel;

namespace Sitecore.Rocks.Server.Requests.Items
{
    public class SetThumbnail
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string id, [NotNull] string fieldId, [NotNull] string fileName, [NotNull] string zoom, int x, int y, bool updateItem)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(id, nameof(id));

            double z;
            if (!double.TryParse(zoom, out z))
            {
                z = 1;
            }

            var database = Factory.GetDatabase(databaseName);
            Assert.IsNotNull(database, typeof(Database));

            var item = database.GetItem(id);
            Assert.IsNotNull(item, typeof(Item));

            var thumbnailFileName = GenerateThumbnail(fileName, z, new Rectangle(x, y, 128, 128));
            if (string.IsNullOrEmpty(thumbnailFileName))
            {
                throw new Exception("Thumbnail not created");
            }

            var mediaItem = Upload(item, fieldId, thumbnailFileName);

            return UpdateItem(item, fieldId, mediaItem, updateItem);
        }

        [NotNull]
        private string GenerateMediaFolderPath([NotNull] ID itemId)
        {
            Debug.ArgumentNotNull(itemId, nameof(itemId));

            var systemFolder = string.Format("{0}/{1}", FileDropAreaField.SystemFolderLocation.TrimEnd('/'), FileDropAreaField.SystemFolderName);
            var key = itemId.ToShortID().ToString();

            var result = new StringBuilder();
            var chars = key.ToCharArray();

            result.Append(systemFolder);
            for (var n = 0; n < 4; n++)
            {
                if (n >= key.Length)
                {
                    break;
                }

                result.Append('/');
                result.Append(chars[n]);
            }

            result.Append("/thumbnail_");
            result.Append(itemId.ToShortID().ToString());

            return result.ToString();
        }

        [NotNull]
        private string GenerateThumbnail([NotNull] string filename, double zoom, Rectangle crop)
        {
            Debug.ArgumentNotNull(filename, nameof(filename));

            using (var screenshot = Image.FromFile(FileUtil.MapPath(filename)) as Bitmap)
            {
                if (screenshot == null)
                {
                    return string.Empty;
                }

                var resizer = new Resizer();

                var resizeOptions = new ResizeOptions
                {
                    AllowStretch = true,
                    BackgroundColor = Color.White,
                    Format = screenshot.RawFormat,
                    Size = new Size((int)(screenshot.Width * zoom), (int)(screenshot.Height * zoom))
                };

                using (var resized = resizer.Resize(screenshot, resizeOptions, screenshot.RawFormat, InterpolationMode.Bilinear))
                {
                    using (var cropped = new Bitmap(resized, 128, 128))
                    {
                        using (var surface = Graphics.FromImage(cropped))
                        {
                            surface.Clear(Color.White);
                            surface.DrawImage(resized, 0, 0, crop, GraphicsUnit.Pixel);
                        }

                        var thumbnailFileName = GetFileName(filename, 128);

                        cropped.Save(FileUtil.MapPath(thumbnailFileName));

                        return thumbnailFileName;
                    }
                }
            }
        }

        [NotNull]
        private string GetFileName([NotNull] string filename, int size)
        {
            Debug.ArgumentNotNull(filename, nameof(filename));

            var folder = FileUtil.NormalizeWebPath(Path.GetDirectoryName(filename));
            var file = Path.GetFileNameWithoutExtension(filename);
            var ext = Path.GetExtension(filename);

            return string.Format("{0}/{1}{2}x{2}{3}", folder, file, size, ext);
        }

        [NotNull]
        private string UpdateItem([NotNull] Item item, [CanBeNull] string fieldId, [NotNull] MediaItem mediaItem, bool updateItem)
        {
            var src = MediaManager.GetMediaUrl(item, new MediaUrlOptions());

            var value = "<image />";
            if (fieldId != null)
            {
                value = item[fieldId];
            }

            XDocument doc;
            try
            {
                doc = XDocument.Parse(value);
            }
            catch
            {
                doc = null;
            }

            if (doc == null)
            {
                doc = XDocument.Parse("<image />");
            }

            var root = doc.Root;
            if (root == null)
            {
                return string.Empty;
            }

            root.SetAttributeValue("mediaid", mediaItem.ID);
            root.SetAttributeValue("mediapath", mediaItem.MediaPath);
            root.SetAttributeValue("src", src);

            if (updateItem)
            {
                item.Editing.BeginEdit();
                item[fieldId] = doc.ToString();
                item.Editing.EndEdit();
            }

            return doc.ToString();
        }

        [NotNull]
        private MediaItem Upload([NotNull] Item item, [NotNull] string fieldId, [NotNull] string filename)
        {
            Debug.ArgumentNotNull(filename, nameof(filename));

            MediaItem mediaItem = null;

            using (new SecurityDisabler())
            {
                ThumbnailField field = item.Fields[fieldId];
                if (field != null)
                {
                    mediaItem = field.MediaItem;
                }

                if (mediaItem == null)
                {
                    var path = GenerateMediaFolderPath(item.ID);

                    var options = new MediaCreatorOptions
                    {
                        Database = Client.ContentDatabase,
                        Destination = path,
                        KeepExisting = false,
                        Versioned = false,
                        AlternateText = "Thumbnail for " + item.Paths.Path
                    };

                    var notificationContext = Context.Notifications;
                    if (notificationContext != null)
                    {
                        notificationContext.Disabled = true;
                    }

                    mediaItem = MediaManager.Creator.CreateFromFile(filename, options);

                    if (notificationContext != null)
                    {
                        notificationContext.Disabled = false;
                    }
                }
                else
                {
                    using (var fileStream = new FileStream(FileUtil.MapPath(filename), FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var media = MediaManager.GetMedia(mediaItem);

                        media.SetStream(fileStream, "png");
                    }
                }

                return mediaItem;
            }
        }
    }
}
