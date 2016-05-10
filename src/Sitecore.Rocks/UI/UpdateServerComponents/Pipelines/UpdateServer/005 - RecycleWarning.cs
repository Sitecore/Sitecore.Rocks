// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.UI.UpdateServerComponents.Pipelines.UpdateServer
{
    [Pipeline(typeof(UpdateServerPipeline), 500)]
    public class RecycleWarning : PipelineProcessor<UpdateServerPipeline>
    {
        protected override void Process([NotNull] UpdateServerPipeline pipeline)
        {
            Assert.ArgumentNotNull(pipeline, nameof(pipeline));

            if (AppHost.Settings.Options.HideUpdateDialog)
            {
                return;
            }

            var d = new UpdateDialog
            {
                Caption = Resources.RecycleWarning_Process_Update_Server_Components,
                UpdateText = Resources.RecycleWarning_Process_
            };

            if (AppHost.Shell.ShowDialog(d) != true)
            {
                pipeline.Abort();
            }
        }
    }
}
