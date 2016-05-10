// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.ContentTrees.Pipelines.DragCopy
{
    [Pipeline(typeof(DragCopyPipeline), 4000)]
    public class UpdateTreeView : PipelineProcessor<DragCopyPipeline>
    {
        protected override void Process(DragCopyPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (pipeline.NewItems.Count == 0)
            {
                return;
            }

            pipeline.Owner.RefreshAndExpand(false);

            pipeline.Owner.SelectChildItems(pipeline.NewItems.Select(i => i.NewItemUri));
        }
    }
}
