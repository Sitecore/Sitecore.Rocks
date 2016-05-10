// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Data
{
    public class ItemId : IComparable
    {
        private readonly Guid _itemId;

        public ItemId()
        {
        }

        public ItemId(Guid itemId)
        {
            _itemId = itemId;
        }

        [NotNull]
        public static ItemId Empty { get; } = new ItemId(Guid.Empty);

        public int CompareTo([CanBeNull] object obj)
        {
            var other = obj as ItemId;
            if (other != null)
            {
                return _itemId.CompareTo(other._itemId);
            }

            return 0;
        }

        public bool Equals([CanBeNull] ItemId other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other._itemId.Equals(_itemId);
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

            if (obj.GetType() != typeof(ItemId))
            {
                return false;
            }

            return Equals((ItemId)obj);
        }

        public override int GetHashCode()
        {
            return _itemId.GetHashCode();
        }

        [NotNull]
        public ItemId NewItemId()
        {
            return new ItemId(Guid.NewGuid());
        }

        public static bool operator ==([CanBeNull] ItemId left, [CanBeNull] ItemId right)
        {
            return Equals(left, right);
        }

        public static bool operator !=([CanBeNull] ItemId left, [CanBeNull] ItemId right)
        {
            return !Equals(left, right);
        }

        public Guid ToGuid()
        {
            return _itemId;
        }

        [NotNull]
        public string ToShortId()
        {
            var guid = ToString();
            return guid.Substring(1, 8) + guid.Substring(10, 4) + guid.Substring(15, 4) + guid.Substring(20, 4) + guid.Substring(25, 12);
        }

        public override string ToString()
        {
            return _itemId.ToString("B").ToUpperInvariant();
        }
    }
}
