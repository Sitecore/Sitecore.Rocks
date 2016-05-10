// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Reflection;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Extensions.AssemblyNameExtensions;
using Sitecore.Rocks.Shell.Pipelines.Upgrade;

namespace Sitecore.Rocks.Shell.Pipelines.Initialization
{
    [Pipeline(typeof(InitializationPipeline), 500)]
    public class CheckVersion : PipelineProcessor<InitializationPipeline>
    {
        protected override void Process(InitializationPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (!pipeline.IsStartUp)
            {
                return;
            }

            var lastVersion = AppHost.Settings.Get("System", "Version", string.Empty) as string ?? string.Empty;
            var currentVersion = Assembly.GetExecutingAssembly().GetFileVersion().ToString();

            if (lastVersion == currentVersion)
            {
                return;
            }

            UpgradePipeline.Run().WithParameters(currentVersion, lastVersion);

            AppHost.Settings.Set("System", "Version", currentVersion);
        }
    }
}
