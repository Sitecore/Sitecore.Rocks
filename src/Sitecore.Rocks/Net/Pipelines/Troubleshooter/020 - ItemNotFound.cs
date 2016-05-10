// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.Net.Pipelines.Troubleshooter
{
    [Pipeline(typeof(TroubleshooterPipeline), 2000)]
    public class ItemNotFound : PipelineProcessor<TroubleshooterPipeline>
    {
        protected override void Process(TroubleshooterPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (pipeline.Exception == null)
            {
                return;
            }

            var message = pipeline.Exception.Message;
            if (!message.Contains(@"System.Exception: Item not found"))
            {
                return;
            }

            if (!pipeline.Silent)
            {
                AppHost.MessageBox(Resources.ItemNotFound_Process_, Resources.Information, MessageBoxButton.OK, MessageBoxImage.Information);
            }

            pipeline.Cancelled = true;
            pipeline.Retry = false;

            pipeline.Abort();
        }
    }
}
