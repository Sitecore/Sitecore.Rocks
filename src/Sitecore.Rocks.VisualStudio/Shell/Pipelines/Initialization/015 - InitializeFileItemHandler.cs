// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Projects.FileItems;

namespace Sitecore.Rocks.Shell.Pipelines.Initialization
{
    [Pipeline(typeof(InitializationPipeline), 1500)]
    public class InitializeFileItemHandler : PipelineProcessor<InitializationPipeline>
    {
        protected override void Process(InitializationPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (pipeline.IsStartUp)
            {
                FileItemDropManager.Initialize();
            }
        }
    }
}
