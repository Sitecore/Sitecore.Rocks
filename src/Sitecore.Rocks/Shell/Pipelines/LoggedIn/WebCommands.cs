// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands.WebCommands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.Shell.Pipelines.LoggedIn
{
    [Pipeline(typeof(LoggedInPipeline), 3000)]
    public class WebCommands : PipelineProcessor<LoggedInPipeline>
    {
        protected override void Process(LoggedInPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            var webcommandsElement = pipeline.RootElement.Element("webcommands");
            if (webcommandsElement == null)
            {
                return;
            }

            WebCommandManager.Load(pipeline.WebDataService, webcommandsElement);
        }
    }
}
