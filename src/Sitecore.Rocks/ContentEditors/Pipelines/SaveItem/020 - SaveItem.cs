// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.ContentEditors.Pipelines.SaveItem
{
    [Pipeline(typeof(SaveItemPipeline), 2000)]
    public class SaveItem : PipelineProcessor<SaveItemPipeline>
    {
        protected override void Process(SaveItemPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            var itemUri = pipeline.ContentModel.FirstItem.Uri.ItemUri;

            ExecuteCompleted callback = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                pipeline.Resume();
            };

            pipeline.Suspend();

            ItemModifier.Edit(itemUri.DatabaseUri, pipeline.ContentModel.Fields, true, callback);
        }
    }
}
