// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.ContentTrees.Pipelines.DefaultActions
{
    [Pipeline(typeof(DefaultActionPipeline), 9000)]
    public class EditItems : PipelineProcessor<DefaultActionPipeline>
    {
        protected override void Process(DefaultActionPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (pipeline.Handled)
            {
                return;
            }

            var command = new Commands.Editing.EditItems();

            if (!command.CanExecute(pipeline.Context))
            {
                return;
            }

            AppHost.Usage.ReportCommand(command, pipeline.Context);
            command.Execute(pipeline.Context);
            pipeline.Handled = true;
        }
    }
}
