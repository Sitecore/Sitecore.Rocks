// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.ContentTrees.VirtualItems;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.ContentTrees.Pipelines.GetChildren
{
    [Pipeline(typeof(GetChildrenPipeline), 1000)]
    public class GetVirtualItems : PipelineProcessor<GetChildrenPipeline>
    {
        protected override void Process(GetChildrenPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            foreach (var baseTreeViewItem in VirtualItemManager.GetChildren(pipeline.ParentItem))
            {
                pipeline.Items.Add(baseTreeViewItem);
            }
        }
    }
}
