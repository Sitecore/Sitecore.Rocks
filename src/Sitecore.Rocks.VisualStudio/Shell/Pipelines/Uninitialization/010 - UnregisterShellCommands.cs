// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.Shell.Pipelines.Uninitialization
{
    [Pipeline(typeof(UninitializationPipeline), 1000)]
    public class UnregisterShellCommands : PipelineProcessor<UninitializationPipeline>
    {
        protected override void Process(UninitializationPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            VsShell.UnregisterShellMenuCommands(SitecorePackage.Instance);
        }
    }
}
