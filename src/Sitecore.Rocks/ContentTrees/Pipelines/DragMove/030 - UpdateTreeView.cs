// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows.Controls;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;

namespace Sitecore.Rocks.ContentTrees.Pipelines.DragMove
{
    [Pipeline(typeof(DragMovePipeline), 4000)]
    public class UpdateTreeView : PipelineProcessor<DragMovePipeline>
    {
        protected override void Process(DragMovePipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (pipeline.MovedItems.Count == 0)
            {
                return;
            }

            pipeline.Owner.Items.Clear();
            pipeline.Owner.Add(DummyTreeViewItem.Instance);
            pipeline.Owner.Refresh();

            foreach (var movedItem in pipeline.MovedItems)
            {
                var item = movedItem as ItemTreeViewItem;
                if (item == null)
                {
                    continue;
                }

                var parent = item.GetAncestor<ItemsControl>();
                if (parent != null)
                {
                    parent.Items.Remove(item);
                }
            }

            pipeline.Owner.SelectChildItems(pipeline.MovedItems.Select(i => i.ItemUri));
        }
    }
}
