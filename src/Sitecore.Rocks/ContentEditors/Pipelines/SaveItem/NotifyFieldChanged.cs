// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.ContentEditors.Pipelines.SaveItem
{
    [Pipeline(typeof(SaveItemPipeline), 7000)]
    public class NotifyFieldChanged : PipelineProcessor<SaveItemPipeline>
    {
        protected override void Process(SaveItemPipeline pipeline)
        {
            Assert.ArgumentNotNull(pipeline, nameof(pipeline));

            foreach (var field in pipeline.ContentModel.Fields)
            {
                foreach (var fieldUri in field.FieldUris)
                {
                    Notifications.RaiseFieldChanged(pipeline.Editor, fieldUri, field.Value);
                }
            }
        }
    }
}
