// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.ContentTrees.Pipelines.DuplicateItem
{
    [Pipeline(typeof(DuplicateItemPipeline), 4000)]
    public class UpdateTreeView : PipelineProcessor<DuplicateItemPipeline>
    {
        protected override void Process([NotNull] DuplicateItemPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            var parent = pipeline.TreeViewItem.Parent as BaseTreeViewItem;
            if (parent == null)
            {
                return;
            }

            if (pipeline.NewItemUri == ItemUri.Empty)
            {
                parent.Refresh();
            }
            else if (!parent.HasChildren)
            {
                parent.Refresh();

                foreach (var item in parent.Items)
                {
                    var i = item as ItemTreeViewItem;
                    if (i == null)
                    {
                        continue;
                    }

                    if (i.ItemUri == pipeline.NewItemUri)
                    {
                        i.IsSelected = true;
                        Keyboard.Focus(i);
                        break;
                    }
                }
            }
            else
            {
                var itemHeader = pipeline.TreeViewItem.Item.Clone();
                itemHeader.Name = pipeline.NewName;
                itemHeader.ItemUri = new ItemUri(new DatabaseUri(pipeline.NewItemUri.Site, pipeline.NewItemUri.DatabaseName), pipeline.NewItemUri.ItemId);

                var n = itemHeader.Path.LastIndexOf('/');
                if (n >= 0)
                {
                    itemHeader.Path = itemHeader.Path.Left(n + 1) + pipeline.NewName;
                }

                var item = new ItemTreeViewItem(itemHeader);

                if (pipeline.TreeViewItem.HasItems)
                {
                    item.Add(DummyTreeViewItem.Instance);
                }

                parent.Add(item);

                item.IsSelected = true;
                Keyboard.Focus(item);
            }
        }
    }
}
