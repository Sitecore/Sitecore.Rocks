// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.Net.Pipelines.Troubleshooter
{
    [Pipeline(typeof(TroubleshooterPipeline), 500)]
    public class SetDataServiceStatus : PipelineProcessor<TroubleshooterPipeline>
    {
        protected override void Process(TroubleshooterPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            pipeline.DataService.Status = DataServiceStatus.Failed;
        }
    }
}
