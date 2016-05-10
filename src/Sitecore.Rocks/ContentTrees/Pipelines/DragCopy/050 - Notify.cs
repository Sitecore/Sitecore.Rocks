// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.ContentTrees.Pipelines.DragCopy
{
    [Pipeline(typeof(DragCopyPipeline), 5000)]
    public class Notify : PipelineProcessor<DragCopyPipeline>
    {
        protected override void Process(DragCopyPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            foreach (var newItem in pipeline.NewItems)
            {
                Notifications.RaiseItemAdded(this, new ItemVersionUri(newItem.NewItemUri, LanguageManager.CurrentLanguage, Version.Latest), pipeline.Target.ItemUri);
            }
        }
    }
}
