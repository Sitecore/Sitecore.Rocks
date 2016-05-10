// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Text;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Extensions.IItemSelectionContextExtensions
{
    public static class ItemSelectionContextExtensions
    {
        [NotNull]
        public static DatabaseName GetDatabaseName([NotNull] this IItemSelectionContext itemSelectionContext)
        {
            Assert.ArgumentNotNull(itemSelectionContext, nameof(itemSelectionContext));

            if (!itemSelectionContext.Items.Any())
            {
                return DatabaseName.Empty;
            }

            if (!IsSameDatabase(itemSelectionContext))
            {
                return DatabaseName.Empty;
            }

            return itemSelectionContext.Items.First().ItemUri.DatabaseName;
        }

        [NotNull]
        public static string GetItemIdList([NotNull] this IItemSelectionContext itemSelectionContext)
        {
            Assert.ArgumentNotNull(itemSelectionContext, nameof(itemSelectionContext));

            var sb = new StringBuilder();

            var first = true;
            foreach (var item in itemSelectionContext.Items)
            {
                if (!first)
                {
                    sb.Append(',');
                }

                first = false;

                sb.Append(item.ItemUri.ItemId);
            }

            return sb.ToString();
        }

        [NotNull]
        public static Site GetSite([NotNull] this IItemSelectionContext itemSelectionContext)
        {
            Assert.ArgumentNotNull(itemSelectionContext, nameof(itemSelectionContext));

            if (!itemSelectionContext.Items.Any())
            {
                return Site.Empty;
            }

            if (!IsSameDatabase(itemSelectionContext))
            {
                return Site.Empty;
            }

            return itemSelectionContext.Items.First().ItemUri.Site;
        }

        public static bool IsSameDatabase([NotNull] this IItemSelectionContext itemSelectionContext)
        {
            Assert.ArgumentNotNull(itemSelectionContext, nameof(itemSelectionContext));

            DatabaseUri databaseUri = null;

            foreach (var item in itemSelectionContext.Items)
            {
                if (databaseUri == null)
                {
                    databaseUri = item.ItemUri.DatabaseUri;
                }
                else if (item.ItemUri.DatabaseUri != databaseUri)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsSameSite([NotNull] this IItemSelectionContext itemSelectionContext)
        {
            Assert.ArgumentNotNull(itemSelectionContext, nameof(itemSelectionContext));

            Site site = null;

            foreach (var item in itemSelectionContext.Items)
            {
                if (site == null)
                {
                    site = item.ItemUri.Site;
                }
                else if (item.ItemUri.Site != site)
                {
                    return false;
                }
            }

            return true;
        }

        [NotNull]
        public static System.Version SitecoreVersion([NotNull] this IItemSelectionContext itemSelectionContext)
        {
            Assert.ArgumentNotNull(itemSelectionContext, nameof(itemSelectionContext));

            if (!itemSelectionContext.Items.Any())
            {
                return Sites.SitecoreVersion.EmptyVersion;
            }

            if (!IsSameSite(itemSelectionContext))
            {
                return Sites.SitecoreVersion.EmptyVersion;
            }

            return itemSelectionContext.Items.First().ItemUri.Site.SitecoreVersion;
        }

        public static bool SitecoreVersion([NotNull] this IItemSelectionContext itemSelectionContext, [NotNull] System.Version minVersion, bool trueIfEmptyVersion = true)
        {
            Assert.ArgumentNotNull(itemSelectionContext, nameof(itemSelectionContext));
            Assert.ArgumentNotNull(minVersion, nameof(minVersion));

            if (!itemSelectionContext.Items.Any())
            {
                return false;
            }

            if (!IsSameSite(itemSelectionContext))
            {
                return false;
            }

            var version = itemSelectionContext.Items.First().ItemUri.Site.SitecoreVersion;

            if (version == Sites.SitecoreVersion.EmptyVersion)
            {
                return trueIfEmptyVersion;
            }

            if (version < minVersion)
            {
                return false;
            }

            return true;
        }

        public static bool SitecoreVersion([NotNull] this IItemSelectionContext itemSelectionContext, [NotNull] System.Version minVersion, [NotNull] System.Version maxVersion, bool trueIfEmptyVersion = true)
        {
            Assert.ArgumentNotNull(itemSelectionContext, nameof(itemSelectionContext));
            Assert.ArgumentNotNull(minVersion, nameof(minVersion));
            Assert.ArgumentNotNull(maxVersion, nameof(maxVersion));

            if (!itemSelectionContext.Items.Any())
            {
                return false;
            }

            if (!IsSameSite(itemSelectionContext))
            {
                return false;
            }

            var version = itemSelectionContext.Items.First().ItemUri.Site.SitecoreVersion;

            if (version == Sites.SitecoreVersion.EmptyVersion)
            {
                return trueIfEmptyVersion;
            }

            if (version < minVersion || version > maxVersion)
            {
                return false;
            }

            return true;
        }
    }
}
