// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.Shell.Pipelines.Upgrade
{
    public class UpgradePipeline : Pipeline<UpgradePipeline>
    {
        [NotNull]
        public string CurrentVersion { get; set; }

        [NotNull]
        public string LastVersion { get; set; }

        [NotNull]
        public UpgradePipeline WithParameters([NotNull] string currentVersion, [NotNull] string lastVersion)
        {
            Assert.ArgumentNotNull(currentVersion, nameof(currentVersion));
            Assert.ArgumentNotNull(lastVersion, nameof(lastVersion));

            LastVersion = lastVersion;
            CurrentVersion = currentVersion;

            Start();

            return this;
        }
    }
}
