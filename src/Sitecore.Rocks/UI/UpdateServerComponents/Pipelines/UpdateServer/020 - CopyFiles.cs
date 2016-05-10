// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.UI.UpdateServerComponents.Pipelines.UpdateServer
{
    [Pipeline(typeof(UpdateServerPipeline), 2000)]
    public class CopyFiles : PipelineProcessor<UpdateServerPipeline>
    {
        protected override void Process(UpdateServerPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            UpdateServerComponentsManager.CopyFiles(pipeline.Updates, pipeline.WebRootPath);
        }
    }
}
