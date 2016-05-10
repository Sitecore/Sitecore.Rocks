// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Data
{
    public class ItemUri
    {
        public ItemUri([NotNull] DatabaseUri databaseUri, [NotNull] ItemId itemId)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(itemId, nameof(itemId));

            DatabaseUri = databaseUri;
            ItemId = itemId;
        }

        [NotNull]
        public DatabaseName DatabaseName => DatabaseUri.DatabaseName;

        [NotNull]
        public DatabaseUri DatabaseUri { get; }

        [NotNull]
        public static ItemUri Empty { get; } = new ItemUri(DatabaseUri.Empty, ItemId.Empty);

        [NotNull]
        public ItemId ItemId { get; }

        [NotNull]
        public Site Site => DatabaseUri.Site;

        public bool Equals([CanBeNull] ItemUri other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(other.DatabaseUri, DatabaseUri) && Equals(other.ItemId, ItemId);
        }

        public override bool Equals([CanBeNull] object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != typeof(ItemUri))
            {
                return false;
            }

            return Equals((ItemUri)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((DatabaseUri != null ? DatabaseUri.GetHashCode() : 0) * 397) ^ (ItemId != null ? ItemId.GetHashCode() : 0);
            }
        }

        public static bool operator ==([CanBeNull] ItemUri left, [CanBeNull] ItemUri right)
        {
            return Equals(left, right);
        }

        public static bool operator !=([CanBeNull] ItemUri left, [CanBeNull] ItemUri right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return @"/" + Site.Name + @"/" + DatabaseName + @"/" + ItemId;
        }

        public static bool TryParse([NotNull] string s, [NotNull] out ItemUri itemUri)
        {
            Assert.ArgumentNotNull(s, nameof(s));

            itemUri = Empty;

            var parts = s.Split('/');
            if (parts.Length != 4)
            {
                return false;
            }

            var siteName = parts[1];
            var databaseName = parts[2];
            var id = parts[3];

            var site = SiteManager.GetSite(siteName);
            if (site == null)
            {
                return false;
            }

            Guid guid;
            if (!Guid.TryParse(id, out guid))
            {
                return false;
            }

            itemUri = new ItemUri(new DatabaseUri(site, new DatabaseName(databaseName)), new ItemId(guid));

            return true;
        }
    }
}
