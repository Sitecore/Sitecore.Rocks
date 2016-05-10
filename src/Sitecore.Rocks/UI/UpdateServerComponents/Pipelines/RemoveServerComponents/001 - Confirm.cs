// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.UI.UpdateServerComponents.Pipelines.RemoveServerComponents
{
    [Pipeline(typeof(RemoveServerComponentsPipeline), 1)]
    public class Confirm : PipelineProcessor<RemoveServerComponentsPipeline>
    {
        protected override void Process([NotNull] RemoveServerComponentsPipeline pipeline)
        {
            Assert.ArgumentNotNull(pipeline, nameof(pipeline));

            if (AppHost.MessageBox("Are you sure you want to remove server components from this site?", "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                pipeline.Abort();
            }
        }
    }
}
