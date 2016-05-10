// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.UI.UpdateServerComponents.Updates
{
    public class InstalledPluginInfo
    {
        public InstalledPluginInfo()
        {
            ServerFile = string.Empty;
            Name = string.Empty;
            Version = string.Empty;
            RuntimeVersion = RuntimeVersion.Empty;
        }

        [NotNull]
        public string Name { get; set; }

        [NotNull]
        public RuntimeVersion RuntimeVersion { get; set; }

        [NotNull]
        public string ServerFile { get; set; }

        [NotNull]
        public string Version { get; set; }
    }
}
