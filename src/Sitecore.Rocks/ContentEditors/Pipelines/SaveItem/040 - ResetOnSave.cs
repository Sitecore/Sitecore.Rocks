// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.ContentEditors.Pipelines.SaveItem
{
    [Pipeline(typeof(SaveItemPipeline), 4000)]
    public class ResetOnSave : PipelineProcessor<SaveItemPipeline>
    {
        protected override void Process(SaveItemPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            foreach (var item in pipeline.ContentModel.Items)
            {
                foreach (var field in item.Fields)
                {
                    if (field == null)
                    {
                        continue;
                    }

                    field.ResetOnSave = false;

                    var fieldControl = field.Control;
                    if (fieldControl != null)
                    {
                        fieldControl.GetControl().IsEnabled = true;
                    }
                }
            }
        }
    }
}
