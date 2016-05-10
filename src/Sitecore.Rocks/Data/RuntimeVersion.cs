// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.Data
{
    public class RuntimeVersion
    {
        public static readonly RuntimeVersion Empty = new RuntimeVersion(-1, -1, -1);

        public static readonly RuntimeVersion MaxValue = new RuntimeVersion(100, -1, -1);

        public RuntimeVersion(int major, int minor, int revision)
        {
            Major = major;
            Minor = minor;
            Revision = revision;
        }

        public int Major { get; }

        public int Minor { get; }

        public int Revision { get; }

        public static bool operator >(RuntimeVersion left, RuntimeVersion right)
        {
            return right < left;
        }

        public static bool operator <([CanBeNull] RuntimeVersion left, [CanBeNull] RuntimeVersion right)
        {
            if (left == null || right == null)
            {
                return false;
            }

            if (left.Major < right.Major)
            {
                return true;
            }

            if (left.Major > right.Major)
            {
                return false;
            }

            if (left.Minor < right.Minor)
            {
                return true;
            }

            if (left.Minor > right.Minor)
            {
                return false;
            }

            return left.Revision < right.Revision;
        }

        [NotNull]
        public static RuntimeVersion Parse([NotNull] string s)
        {
            Assert.ArgumentNotNull(s, nameof(s));

            if (s.StartsWith("v"))
            {
                s = s.Mid(1);
            }

            int major;
            var minor = 0;
            var revision = 0;

            var parts = s.Split('.');

            if (!int.TryParse(parts[0], out major))
            {
                return Empty;
            }

            if (parts.Length > 1)
            {
                if (!int.TryParse(parts[1], out minor))
                {
                    return Empty;
                }
            }

            if (parts.Length > 2)
            {
                int.TryParse(parts[2], out revision);
            }

            return new RuntimeVersion(major, minor, revision);
        }

        public override string ToString()
        {
            return string.Format("v{0}.{1}.{2}", Major, Minor, Revision);
        }
    }
}
