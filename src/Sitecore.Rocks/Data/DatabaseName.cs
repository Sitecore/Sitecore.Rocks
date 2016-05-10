// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Data
{
    public class DatabaseName
    {
        private static readonly DatabaseName empty = new DatabaseName();

        private static readonly DatabaseName master = new DatabaseName("master");

        private readonly string name = string.Empty;

        public DatabaseName([NotNull, Localizable(false)] string name)
        {
            Assert.ArgumentNotNull(name, nameof(name));

            this.name = name;
        }

        protected DatabaseName()
        {
        }

        [NotNull]
        public static DatabaseName Empty
        {
            get { return empty; }
        }

        [NotNull]
        public static DatabaseName Master
        {
            get { return master; }
        }

        [NotNull]
        public string Name
        {
            get { return name ?? string.Empty; }
        }

        public bool Equals([CanBeNull] DatabaseName other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(other.name, name);
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

            if (obj.GetType() != typeof(DatabaseName))
            {
                return false;
            }

            return Equals((DatabaseName)obj);
        }

        public override int GetHashCode()
        {
            return name != null ? name.GetHashCode() : 0;
        }

        public static bool operator ==([NotNull] DatabaseName left, [NotNull] DatabaseName right)
        {
            return Equals(left, right);
        }

        public static bool operator !=([NotNull] DatabaseName left, [NotNull] DatabaseName right)
        {
            return !Equals(left, right);
        }

        [NotNull]
        public override string ToString()
        {
            return Name;
        }
    }
}
