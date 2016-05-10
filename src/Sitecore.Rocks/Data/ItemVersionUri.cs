// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Data
{
    public class ItemVersionUri
    {
        public ItemVersionUri([NotNull] ItemUri itemUri, [NotNull] Language language, [NotNull] Version version)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(language, nameof(language));
            Assert.ArgumentNotNull(version, nameof(version));

            ItemUri = itemUri;
            Language = language;
            Version = version;
        }

        protected ItemVersionUri()
        {
            ItemUri = ItemUri.Empty;
            Language = Language.Empty;
            Version = Version.Empty;
        }

        [NotNull]
        public DatabaseName DatabaseName => ItemUri.DatabaseName;

        [NotNull]
        public DatabaseUri DatabaseUri => ItemUri.DatabaseUri;

        [NotNull]
        public static ItemVersionUri Empty { get; } = new ItemVersionUri();

        [NotNull]
        public ItemId ItemId => ItemUri.ItemId;

        public ItemUri ItemUri { get; }

        public Language Language { get; }

        [NotNull]
        public Site Site => ItemUri.Site;

        public Version Version { get; }

        public bool Equals([CanBeNull] ItemVersionUri other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(other.ItemUri, ItemUri) && Equals(other.Language, Language) && Equals(other.Version, Version);
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

            if (obj.GetType() != typeof(ItemVersionUri))
            {
                return false;
            }

            return Equals((ItemVersionUri)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = ItemUri != null ? ItemUri.GetHashCode() : 0;
                result = (result * 397) ^ (Language != null ? Language.GetHashCode() : 0);
                result = (result * 397) ^ (Version != null ? Version.GetHashCode() : 0);
                return result;
            }
        }

        public static bool operator ==([CanBeNull] ItemVersionUri left, [CanBeNull] ItemVersionUri right)
        {
            return Equals(left, right);
        }

        public static bool operator !=([CanBeNull] ItemVersionUri left, [CanBeNull] ItemVersionUri right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return ItemUri + @"/" + Language.Name + @"/" + Version;
        }
    }
}
