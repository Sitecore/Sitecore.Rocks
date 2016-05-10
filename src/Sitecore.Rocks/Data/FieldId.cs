// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Data
{
    public class FieldId
    {
        private readonly Guid _fieldId;

        public FieldId()
        {
        }

        public FieldId(Guid fieldId)
        {
            _fieldId = fieldId;
        }

        [NotNull]
        public static FieldId Empty { get; } = new FieldId(Guid.Empty);

        public bool Equals([CanBeNull] FieldId other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other._fieldId.Equals(_fieldId);
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

            if (obj.GetType() != typeof(FieldId))
            {
                return false;
            }

            return Equals((FieldId)obj);
        }

        public override int GetHashCode()
        {
            return _fieldId.GetHashCode();
        }

        [NotNull]
        public FieldId NewItemId()
        {
            return new FieldId(Guid.NewGuid());
        }

        public static bool operator ==([CanBeNull] FieldId left, [CanBeNull] FieldId right)
        {
            return Equals(left, right);
        }

        public static bool operator !=([CanBeNull] FieldId left, [CanBeNull] FieldId right)
        {
            return !Equals(left, right);
        }

        public Guid ToGuid()
        {
            return _fieldId;
        }

        [NotNull]
        public string ToShortId()
        {
            var guid = ToString();
            return guid.Substring(1, 8) + guid.Substring(10, 4) + guid.Substring(15, 4) + guid.Substring(20, 4) + guid.Substring(25, 12);
        }

        public override string ToString()
        {
            return _fieldId.ToString("B").ToUpperInvariant();
        }
    }
}
