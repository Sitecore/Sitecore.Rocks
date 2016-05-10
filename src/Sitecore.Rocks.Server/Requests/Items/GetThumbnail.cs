// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Links;
using Sitecore.Shell.Applications.ContentManager.Dialogs.SetThumbnail;
using Sitecore.Sites;
using Sitecore.Text;
using Sitecore.Web;

namespace Sitecore.Rocks.Server.Requests.Items
{
    public class GetThumbnail
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string itemId)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(itemId, nameof(itemId));

            var url = itemId;

            var database = Factory.GetDatabase(databaseName);
            Assert.IsNotNull(database, typeof(Database));

            Item item;
            try
            {
                item = database.GetItem(itemId);
            }
            catch
            {
                item = null;
            }

            if (item != null)
            {
                url = GetItemUrl(databaseName, itemId);
                if (string.IsNullOrEmpty(url))
                {
                    return string.Empty;
                }
            }

            var filename = TempFolder.GetFilename("thumbnail.png");

            var htmlCapture = new HtmlCapture
            {
                Url = url,
                FileName = filename
            };

            if (htmlCapture.Capture())
            {
                return filename;
            }

            return string.Empty;
        }

        [NotNull]
        private string GetItemUrl([NotNull] string databaseName, [NotNull] string id)
        {
            Debug.ArgumentNotNull(databaseName, nameof(databaseName));
            Debug.ArgumentNotNull(id, nameof(id));

            var database = Factory.GetDatabase(databaseName);
            Assert.IsNotNull(database, typeof(Database));

            var item = database.GetItem(id);
            Assert.IsNotNull(item, typeof(Item));

            var site = Factory.GetSite(Settings.Preview.DefaultSite);
            if (site == null)
            {
                return string.Empty;
            }

            return GetItemUrl(item, site);
        }

        [NotNull]
        private string GetItemUrl([NotNull] Item item, [NotNull] SiteContext site)
        {
            Debug.ArgumentNotNull(item, nameof(item));
            Debug.ArgumentNotNull(site, nameof(site));

            var options = UrlOptions.DefaultOptions;
            options.Site = site;

            var itemUrl = LinkManager.GetItemUrl(item, options);

            var url = new UrlString(WebUtil.GetServerUrl() + itemUrl);

            url["sc_site"] = site.Name;
            url["sc_mode"] = "normal";
            url["sc_duration"] = "temporary";

            return url.ToString();
        }
    }
}
