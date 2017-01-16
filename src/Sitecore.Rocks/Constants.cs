// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;

namespace Sitecore.Rocks
{
    public static class Constants
    {
        public const string IsoDateTimeUtcMarker = "Z";

        public const string SitecoreRocks = "Sitecore.Rocks";

        public const string SitecoreRocksServer = "Sitecore.Rocks.Server";

        public const string SitecoreRocksVisualStudio = "Sitecore.Rocks.VisualStudio";

        public static class DatabaseNames
        {
            public static readonly string Core = "core";
        }

        public static class TemplateIds
        {
            public static readonly Guid ViewRenderingId = new Guid("{99F8905D-4A87-4EB8-9F8B-A9BEBFB3ADD6}");
        }

        public static class Versions
        {
            public static readonly Version Version7 = new Version(7, 0);

            public static readonly Version Version72 = new Version(7, 2);

            public static readonly Version Version80 = new Version(8, 0);

            public static readonly Version Version82 = new Version(8, 2);
        }
    }
}
