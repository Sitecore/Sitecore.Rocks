// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Data
{
    public class Language : IComparable
    {
        private static readonly Language empty = new Language();

        private readonly string language;

        public Language([NotNull] string language)
        {
            Assert.ArgumentNotNull(language, nameof(language));

            this.language = language;
        }

        protected Language()
        {
            language = string.Empty;
        }

        [NotNull]
        public static Language Current => AppHost.Globals.CurrentLanguage;

        [NotNull]
        public static Language Empty => empty;

        [NotNull]
        public string Name => language;

        public int CompareTo([CanBeNull] object obj)
        {
            var other = obj as Language;
            if (other != null)
            {
                return language.CompareTo(other.language);
            }

            return 0;
        }

        public bool Equals([CanBeNull] Language other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(other.language, language);
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

            if (obj.GetType() != typeof(Language))
            {
                return false;
            }

            return Equals((Language)obj);
        }

        public override int GetHashCode()
        {
            return language != null ? language.GetHashCode() : 0;
        }

        public static bool operator ==([NotNull] Language left, [NotNull] Language right)
        {
            return Equals(left, right);
        }

        public static bool operator !=([NotNull] Language left, [NotNull] Language right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return language;
        }
    }
}
