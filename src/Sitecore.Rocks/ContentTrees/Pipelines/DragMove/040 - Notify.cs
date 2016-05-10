// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.ContentTrees.Pipelines.DragMove
{
    [Pipeline(typeof(DragMovePipeline), 5000)]
    public class Notify : PipelineProcessor<DragMovePipeline>
    {
        protected override void Process([NotNull] DragMovePipeline pipeline)
        {
            Assert.ArgumentNotNull(pipeline, nameof(pipeline));

            foreach (var movedItem in pipeline.MovedItems)
            {
                Notifications.RaiseItemMoved(this, pipeline.Target.ItemUri, new ItemVersionUri(movedItem.ItemUri, LanguageManager.CurrentLanguage, Version.Latest));
            }
        }
    }
}
