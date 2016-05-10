// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Data
{
    public class Version : IComparable
    {
        public Version(int number)
        {
            Number = number;
        }

        [NotNull]
        public static Version Empty { get; } = new Version(-1);

        [NotNull]
        public static Version Latest { get; } = new Version(0);

        public int Number { get; }

        public int CompareTo([CanBeNull] object obj)
        {
            var other = obj as Version;
            if (other != null)
            {
                return Number.CompareTo(other.Number);
            }

            return 0;
        }

        public bool Equals([CanBeNull] Version other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other.Number == Number;
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

            if (obj.GetType() != typeof(Version))
            {
                return false;
            }

            return Equals((Version)obj);
        }

        public override int GetHashCode()
        {
            return Number;
        }

        public static bool operator ==([NotNull] Version left, [NotNull] Version right)
        {
            return Equals(left, right);
        }

        public static bool operator !=([NotNull] Version left, [NotNull] Version right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return Number.ToString();
        }
    }
}
