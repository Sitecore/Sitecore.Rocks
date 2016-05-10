// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.Shell.Pipelines.Initialization
{
    [Pipeline(typeof(InitializationPipeline), 750)]
    public class RegisterShellMenuCommands : PipelineProcessor<InitializationPipeline>
    {
        protected override void Process(InitializationPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            VsShell.RegisterShellMenuCommands(SitecorePackage.Instance);
        }
    }
}
