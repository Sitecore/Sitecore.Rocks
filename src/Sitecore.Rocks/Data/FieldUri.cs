// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Data
{
    public class FieldUri
    {
        public FieldUri([NotNull] ItemVersionUri itemVersionUri, [NotNull] FieldId fieldId)
        {
            Assert.ArgumentNotNull(itemVersionUri, nameof(itemVersionUri));
            Assert.ArgumentNotNull(fieldId, nameof(fieldId));

            ItemVersionUri = itemVersionUri;
            FieldId = fieldId;
        }

        [NotNull]
        public DatabaseName DatabaseName => ItemVersionUri.DatabaseName;

        [NotNull]
        public DatabaseUri DatabaseUri => ItemVersionUri.DatabaseUri;

        public FieldId FieldId { get; }

        [NotNull]
        public ItemId ItemId => ItemVersionUri.ItemId;

        public ItemVersionUri ItemVersionUri { get; }

        [NotNull]
        public Language Language => ItemVersionUri.Language;

        [NotNull]
        public Site Site => ItemVersionUri.Site;

        [NotNull]
        public Version Version => ItemVersionUri.Version;

        [NotNull]
        public FieldUri Clone()
        {
            var databaseUri = new DatabaseUri(ItemVersionUri.Site, ItemVersionUri.DatabaseName);
            var itemUri = new ItemUri(databaseUri, ItemVersionUri.ItemId);
            var itemVersionUri = new ItemVersionUri(itemUri, ItemVersionUri.Language, ItemVersionUri.Version);

            return new FieldUri(itemVersionUri, FieldId);
        }

        public bool Equals([CanBeNull] FieldUri other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other.FieldId.Equals(FieldId) && Equals(other.ItemVersionUri, ItemVersionUri);
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

            if (obj.GetType() != typeof(FieldUri))
            {
                return false;
            }

            return Equals((FieldUri)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (FieldId.GetHashCode() * 397) ^ (ItemVersionUri != null ? ItemVersionUri.GetHashCode() : 0);
            }
        }

        public static bool operator ==([CanBeNull] FieldUri left, [CanBeNull] FieldUri right)
        {
            return Equals(left, right);
        }

        public static bool operator !=([CanBeNull] FieldUri left, [CanBeNull] FieldUri right)
        {
            return !Equals(left, right);
        }
    }
}
