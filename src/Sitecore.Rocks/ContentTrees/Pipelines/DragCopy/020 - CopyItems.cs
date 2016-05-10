// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Globalization;
using System.Linq;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.ContentTrees.Pipelines.DragMove;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.ContentTrees.Pipelines.DragCopy
{
    [Pipeline(typeof(DragCopyPipeline), 2000)]
    public class CopyItems : PipelineProcessor<DragCopyPipeline>
    {
        protected override void Process(DragCopyPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (pipeline.Position == ControlDragAdornerPosition.Over)
            {
                var item = pipeline.Items.FirstOrDefault();
                if (item != null && item.ItemUri.Site.DataService.CanExecuteAsync("Items.CopyItems"))
                {
                    CopyItemBatch(pipeline);
                }
                else
                {
                    CopyInto(pipeline);
                }
                return;
            }

            CopyNextTo(pipeline);
        }

        private void CopyInto([NotNull] DragCopyPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            foreach (var item in pipeline.Items)
            {
                var newName = string.Format(Resources.CopyItems_CopyInto_Copy_of__0_, item.Name);

                var newItemUri = item.ItemUri.Site.DataService.Copy(item.ItemUri, pipeline.Target.ItemUri.ItemId, newName);

                if (newItemUri == ItemUri.Empty)
                {
                    AppHost.MessageBox(string.Format(Resources.CopyItems_CopyInto_Failed_to_copy___0_, item.Name), Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    pipeline.Abort();
                    return;
                }

                var newItem = new DragCopyPipeline.NewItem
                {
                    Item = item,
                    NewName = newName,
                    NewItemUri = newItemUri
                };

                pipeline.NewItems.Add(newItem);
            }

            pipeline.Owner = pipeline.Target;
            pipeline.Anchor = null;
        }

        private void CopyItemBatch([NotNull] DragCopyPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            pipeline.Suspend();

            var sourceItemIds = string.Join(",", pipeline.Items.Select(i => i.ItemUri.ItemId.ToString()));

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    pipeline.Abort();
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    pipeline.Abort();
                    return;
                }

                foreach (var element in root.Elements())
                {
                    var itemHeader = ItemHeader.Parse(pipeline.Target.ItemUri.DatabaseUri, element);

                    var newItem = new DragCopyPipeline.NewItem
                    {
                        Item = itemHeader,
                        NewName = itemHeader.Name,
                        NewItemUri = itemHeader.ItemUri
                    };

                    pipeline.NewItems.Add(newItem);
                }

                pipeline.Owner = pipeline.Target;
                pipeline.Anchor = null;

                pipeline.Resume();
            };

            AppHost.Server.Items.CopyItems(sourceItemIds, pipeline.Target.ItemUri, completed);
        }

        private void CopyNextTo([NotNull] DragCopyPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            int sortOrder;
            int sortOrderDelta;
            ItemTreeViewItem parent;
            ItemTreeViewItem anchor;

            SortOrderHelper.GetSortOrder(pipeline.Target, pipeline.Position, pipeline.Items.Count(), out sortOrder, out sortOrderDelta, out parent, out anchor);
            if (parent == null)
            {
                AppHost.MessageBox(Resources.CopyItems_CopyNextTo_Parent_is_not_an_item, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                pipeline.Abort();
                return;
            }

            pipeline.Owner = parent;
            pipeline.Anchor = anchor;

            var fieldId = IdManager.GetFieldId("/sitecore/templates/System/Templates/Sections/Appearance/Appearance/__Sortorder");

            foreach (var item in pipeline.Items)
            {
                var newName = string.Format(Resources.CopyItems_CopyNextTo_Copy_of__0_, item.Name);

                var newItemUri = item.ItemUri.Site.DataService.Copy(item.ItemUri, parent.ItemUri.ItemId, newName);
                if (newItemUri == ItemUri.Empty)
                {
                    AppHost.MessageBox(string.Format(Resources.CopyItems_CopyNextTo_Failed_to_copy___0_, item.Name), Resources.Error, MessageBoxButton.OK, MessageBoxImage.Information);
                    pipeline.Abort();
                    return;
                }

                var newItem = new DragCopyPipeline.NewItem
                {
                    Item = item,
                    NewName = newName,
                    NewItemUri = newItemUri,
                    SortOrder = sortOrder
                };

                ItemModifier.Edit(newItemUri, fieldId, sortOrder.ToString(CultureInfo.InvariantCulture));

                sortOrder += sortOrderDelta;

                pipeline.NewItems.Add(newItem);
            }
        }
    }
}
