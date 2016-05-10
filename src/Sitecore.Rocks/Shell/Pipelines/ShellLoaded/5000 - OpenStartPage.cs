// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.UI.StartPage;

namespace Sitecore.Rocks.Shell.Pipelines.ShellLoaded
{
    [Pipeline(typeof(ShellLoadedPipeline), 5000)]
    public class OpenStartPage : PipelineProcessor<ShellLoadedPipeline>
    {
        protected override void Process(ShellLoadedPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (!AppHost.Settings.Options.ShowStartPageOnStartup)
            {
                return;
            }

            StartPageViewer.Open();
        }
    }
}
