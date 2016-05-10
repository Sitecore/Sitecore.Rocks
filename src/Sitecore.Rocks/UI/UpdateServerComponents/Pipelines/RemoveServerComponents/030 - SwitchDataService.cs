// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Sites.Connections;

namespace Sitecore.Rocks.UI.UpdateServerComponents.Pipelines.RemoveServerComponents
{
    [Pipeline(typeof(RemoveServerComponentsPipeline), 3000)]
    public class SwitchDataService : PipelineProcessor<RemoveServerComponentsPipeline>
    {
        protected override void Process(RemoveServerComponentsPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (pipeline.Site == null)
            {
                return;
            }

            if (pipeline.DataService.GetType() == typeof(OldWebService))
            {
                return;
            }

            pipeline.Site.SetDataServiceName("Good Old Web Service");
            pipeline.DataService = pipeline.Site.DataService;

            ConnectionManager.Save();
        }
    }
}
