// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.Shell.Pipelines.LoggedIn
{
    [Pipeline(typeof(LoggedInPipeline), 1000)]
    public class SitecoreRocksVersion : PipelineProcessor<LoggedInPipeline>
    {
        protected override void Process(LoggedInPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            var sitecorerocksElement = pipeline.RootElement.Element("sitecorerocks");
            if (sitecorerocksElement == null)
            {
                return;
            }

            pipeline.WebDataService.WebServiceVersion = sitecorerocksElement.GetAttributeValue("rocks");
            pipeline.WebDataService.SitecoreVersionString = sitecorerocksElement.GetAttributeValue("sitecore");
            pipeline.WebDataService.SitecoreVersion = RuntimeVersion.Parse(pipeline.WebDataService.SitecoreVersionString);
        }
    }
}
