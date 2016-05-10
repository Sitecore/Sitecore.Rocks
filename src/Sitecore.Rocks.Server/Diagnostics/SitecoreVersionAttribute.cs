// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Diagnostics
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class SitecoreVersionAttribute : Attribute
    {
        public SitecoreVersionAttribute([NotNull] string minVersion, [NotNull] string maxVersion = "")
        {
            Assert.ArgumentNotNull(minVersion, nameof(minVersion));
            Assert.ArgumentNotNull(maxVersion, nameof(maxVersion));

            MinVersion = minVersion;
            MaxVersion = maxVersion;
        }

        [NotNull]
        public string MaxVersion { get; private set; }

        [NotNull]
        public string MinVersion { get; private set; }
    }
}
