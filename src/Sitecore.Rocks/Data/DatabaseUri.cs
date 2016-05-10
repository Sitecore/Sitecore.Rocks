// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Data
{
    public class DatabaseUri
    {
        public DatabaseUri([NotNull] Site site, [NotNull] DatabaseName databaseName)
        {
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

            DatabaseName = databaseName;
            Site = site;
        }

        [NotNull]
        public DatabaseName DatabaseName { get; }

        [NotNull]
        public static DatabaseUri Empty { get; } = new DatabaseUri(Site.Empty, DatabaseName.Empty);

        [NotNull]
        public Site Site { get; }

        public bool Equals([CanBeNull] DatabaseUri other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(other.DatabaseName, DatabaseName) && Equals(other.Site, Site);
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

            if (obj.GetType() != typeof(DatabaseUri))
            {
                return false;
            }

            return Equals((DatabaseUri)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = DatabaseName != null ? DatabaseName.GetHashCode() : 0;
                result = (result * 397) ^ (Site != null ? Site.GetHashCode() : 0);
                return result;
            }
        }

        public static bool operator ==([CanBeNull] DatabaseUri left, [CanBeNull] DatabaseUri right)
        {
            return Equals(left, right);
        }

        public static bool operator !=([CanBeNull] DatabaseUri left, [CanBeNull] DatabaseUri right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return @"/" + Site.Name + @"/" + DatabaseName;
        }

        public static bool TryParse([NotNull] string text, [NotNull] out DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(text, nameof(text));

            databaseUri = Empty;

            var parts = text.Split('/');

            if (parts.Length != 3)
            {
                return false;
            }

            var siteName = parts[1];
            var databaseName = parts[2];

            var site = SiteManager.GetSite(siteName);
            if (site == null)
            {
                return false;
            }

            databaseUri = new DatabaseUri(site, new DatabaseName(databaseName));

            return true;
        }
    }
}
