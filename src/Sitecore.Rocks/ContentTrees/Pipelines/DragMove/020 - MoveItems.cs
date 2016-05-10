// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.ContentTrees.Pipelines.DragMove
{
    [Pipeline(typeof(DragMovePipeline), 2000)]
    public class MoveItems : PipelineProcessor<DragMovePipeline>
    {
        protected override void Process(DragMovePipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (pipeline.Position == ControlDragAdornerPosition.Over)
            {
                MoveInto(pipeline);
                return;
            }

            MoveNextTo(pipeline);
        }

        private bool Move([NotNull] ItemUri itemUri, [NotNull] ItemUri targetItemUri)
        {
            Debug.ArgumentNotNull(targetItemUri, nameof(targetItemUri));
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));

            if (!itemUri.Site.CanExecute)
            {
                return itemUri.Site.DataService.Move(itemUri, targetItemUri.ItemId);
            }

            itemUri.Site.Execute("Items.Move", response => { }, itemUri.DatabaseName.ToString(), itemUri.ItemId.ToString(), targetItemUri.ItemId.ToString());
            return true;
        }

        private void MoveInto([NotNull] DragMovePipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            foreach (var item in pipeline.Items)
            {
                if (!Move(item.ItemUri, pipeline.Target.ItemUri))
                {
                    AppHost.MessageBox(string.Format(Resources.MoveItems_MoveInto_Failed_to_move___0_, item.Name), Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                }

                pipeline.MovedItems.Add(item);
            }

            pipeline.Owner = pipeline.Target;
            pipeline.Anchor = null;
        }

        private void MoveNextTo([NotNull] DragMovePipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            int sortOrder;
            int sortOrderDelta;

            ItemTreeViewItem parent;
            ItemTreeViewItem anchor;

            SortOrderHelper.GetSortOrder(pipeline.Target, pipeline.Position, pipeline.Items.Count(), out sortOrder, out sortOrderDelta, out parent, out anchor);
            if (parent == null)
            {
                AppHost.MessageBox(Resources.MoveItems_MoveNextTo_Parent_is_not_an_item, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                pipeline.Abort();
                return;
            }

            pipeline.Owner = parent;
            pipeline.Anchor = anchor;

            var fieldId = IdManager.GetFieldId("/sitecore/templates/System/Templates/Sections/Appearance/Appearance/__Sortorder");

            foreach (var item in pipeline.Items)
            {
                if (!Move(item.ItemUri, parent.ItemUri))
                {
                    AppHost.MessageBox(string.Format(Resources.MoveItems_MoveNextTo_Failed_to_move___0_, item.Name), Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                }

                ItemModifier.Edit(item.ItemUri, fieldId, sortOrder.ToString());

                var itemTreeViewItem = item as ItemTreeViewItem;
                if (itemTreeViewItem != null)
                {
                    itemTreeViewItem.Item.SortOrder = sortOrder;
                }

                sortOrder += sortOrderDelta;

                pipeline.MovedItems.Add(item);
            }
        }
    }
}
