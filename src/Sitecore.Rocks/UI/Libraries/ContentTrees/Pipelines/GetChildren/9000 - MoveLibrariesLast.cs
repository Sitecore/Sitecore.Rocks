// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.ContentTrees.Pipelines.GetChildren;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.UI.Libraries.ContentTrees.VirtualItems;

namespace Sitecore.Rocks.UI.Libraries.ContentTrees.Pipelines.GetChildren
{
    [Pipeline(typeof(GetChildrenPipeline), 9000)]
    public class MoveLibrariesLast : PipelineProcessor<GetChildrenPipeline>
    {
        protected override void Process(GetChildrenPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            var items = pipeline.Items.Where(i => i is LibrariesRootTreeViewItem).ToList();
            if (!items.Any())
            {
                return;
            }

            foreach (var item in items)
            {
                pipeline.Items.Remove(item);
            }

            foreach (var item in items)
            {
                pipeline.Items.Add(item);
            }
        }
    }
}
