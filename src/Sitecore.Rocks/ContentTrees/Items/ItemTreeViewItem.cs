// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Dialogs;
using Sitecore.Rocks.ContentTrees.Gutters;
using Sitecore.Rocks.ContentTrees.Items.SelectedObjects;
using Sitecore.Rocks.ContentTrees.Pipelines.DeleteItem;
using Sitecore.Rocks.ContentTrees.Pipelines.DragCopy;
using Sitecore.Rocks.ContentTrees.Pipelines.DragMove;
using Sitecore.Rocks.ContentTrees.Pipelines.DuplicateItem;
using Sitecore.Rocks.ContentTrees.Pipelines.RenameItem;
using Sitecore.Rocks.ContentTrees.StatusIcons;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Extensions.DataServiceExtensions;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Media;
using Sitecore.Rocks.UI.TransferItems;
using TaskDialogInterop;

namespace Sitecore.Rocks.ContentTrees.Items
{
    [DebuggerDisplay(@"{ItemUri}")]
    public class ItemTreeViewItem : BaseSiteTreeViewItem, ITemplatedItem, IItemData, ICanDelete, ICanRename, ICanRefresh, ICanRefreshItem, ICanDuplicate, ICanDrop, ICanDrag, IScopeable, ISelectable
    {
        public const string BaseTreeViewItemDragIdentifier = "SitecoreItem";

        private readonly ControlDragAdorner adorner;

        public ItemTreeViewItem([NotNull] ItemHeader item) : base(item.ItemUri.Site)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            Item = item;
            Text = item.Name;
            Icon = item.Icon;
            ToolTip = string.Empty;
            adorner = new ControlDragAdorner(ItemHeader, ControlDragAdornerPosition.All);

            RefreshStatusIcons();

            ToolTipOpening += OpenToolTip;
            Notifications.RegisterItemEvents(this, renamed: ItemRenamed, deleted: ItemDeleted, serialized: ItemSerialized);
            Notifications.RegisterFieldEvents(this, FieldChanged);
        }

        public bool IsTemplate
        {
            get { return IdManager.IsTemplate(Item.TemplateId, "template"); }
        }

        [NotNull]
        public ItemHeader Item { get; private set; }

        public ItemUri ItemUri
        {
            get { return Item.ItemUri; }
        }

        public ItemId TemplateId
        {
            get { return Item.TemplateId; }
        }

        public string TemplateName
        {
            get { return Item.TemplateName; }
        }

        string IItem.Name
        {
            get { return Item.Name; }
        }

        public void Delete(bool deleteFiles)
        {
            var pipeline = PipelineManager.GetPipeline<DeleteItemPipeline>();

            pipeline.ItemUri = ItemUri;
            pipeline.DeleteFiles = deleteFiles;

            pipeline.Start();
        }

        public void Duplicate()
        {
            var pipeline = PipelineManager.GetPipeline<DuplicateItemPipeline>();

            pipeline.TreeViewItem = this;
            pipeline.ItemUri = ItemUri;
            pipeline.NewName = Text;

            pipeline.Start();
        }

        public override bool GetChildren(GetChildrenDelegate callback, bool async)
        {
            Assert.ArgumentNotNull(callback, nameof(callback));

            bool[] busy =
            {
                true
            };

            GetItemsCompleted<ItemHeader> completed = delegate(IEnumerable<ItemHeader> children)
            {
                if (!busy[0])
                {
                    return;
                }

                Dispatcher.Invoke(new Action(() => GetChildren(children.ToList(), callback)));
                busy[0] = false;
            };

            Site.DataService.GetChildrenAsync(ItemUri, completed);

            if (!async)
            {
                if (AppHost.DoEvents(ref busy[0]))
                {
                    return false;
                }
            }

            return true;
        }

        public string GetDragIdentifier()
        {
            return BaseTreeViewItemDragIdentifier;
        }

        [NotNull]
        public string GetPath()
        {
            // return this.Item.Path;
            var result = Text;

            var item = Parent as ItemTreeViewItem;
            while (item != null)
            {
                result = item.Text + @"/" + result;
                item = item.Parent as ItemTreeViewItem;
            }

            return @"/" + result;
        }

        public void HandleDragOver(object sender, DragEventArgs e, BaseTreeViewItem treeViewItem)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));
            Assert.ArgumentNotNull(treeViewItem, nameof(treeViewItem));

            adorner.AllowedPositions = ControlDragAdornerPosition.None;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                HandleFileDragOver(e);
                return;
            }

            var target = treeViewItem as ItemTreeViewItem;
            if (target == null)
            {
                return;
            }

            if (e.Data.GetDataPresent(@"CF_VSSTGPROJECTITEMS") && e.Data.GetDataPresent(@"Text"))
            {
                var fileName = e.Data.GetData(@"Text") as string ?? string.Empty;
                if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
                {
                    var project = AppHost.Projects.FirstOrDefault(p => fileName.StartsWith(p.FolderName, StringComparison.InvariantCultureIgnoreCase));
                    if (project != null)
                    {
                        e.Effects = DragDropEffects.Link;
                    }
                }

                return;
            }

            if (e.Data.GetDataPresent(DragManager.DragIdentifier))
            {
                HandleItemDragOver(target, e);
                return;
            }

            Notifications.RaiseItemTreeViewDragOver(this, target, e);
            if (e.Effects != DragDropEffects.None)
            {
                adorner.AllowedPositions = ControlDragAdornerPosition.Over;
            }
        }

        public void HandleDrop(object sender, DragEventArgs e, BaseTreeViewItem treeViewItem)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));
            Assert.ArgumentNotNull(treeViewItem, nameof(treeViewItem));

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                HandleFileDrop(e);
                return;
            }

            var target = treeViewItem as ItemTreeViewItem;
            if (target == null)
            {
                Diagnostics.Trace.Expected(typeof(ItemTreeViewItem));
                return;
            }

            if (e.Data.GetDataPresent(@"CF_VSSTGPROJECTITEMS") && e.Data.GetDataPresent(@"Text"))
            {
                HandleSolutionExplorerDrop(target, e);
                return;
            }

            if (e.Data.GetDataPresent(DragManager.DragIdentifier))
            {
                HandleItemDrop(target, e);
                return;
            }

            Notifications.RaiseItemTreeViewDrop(this, target, e);
        }

        public void RefreshItem()
        {
            GetValueCompleted<ItemHeader> completed = delegate(ItemHeader itemHeader)
            {
                Item = itemHeader;

                ItemHeader.Text = itemHeader.Name;
                ItemHeader.Icon = itemHeader.Icon;
                Foreground = SystemColors.WindowTextBrush;
            };

            Site.DataService.GetItemHeader(Item.ItemUri, completed);
        }

        public void RefreshStatusIcons()
        {
            var baseTreeViewItemHeader = Header as BaseTreeViewItemHeader;
            if (baseTreeViewItemHeader == null)
            {
                return;
            }

            baseTreeViewItemHeader.StatusIcons.Children.Clear();
            foreach (var statusIcon in AppHost.Env.StatusIcons().StatusIcons)
            {
                var img = statusIcon.GetStatus(Item);
                if (img != null)
                {
                    baseTreeViewItemHeader.StatusIcons.Children.Add(img);
                }
            }
        }

        public void SelectChildItems([NotNull] IEnumerable<ItemUri> itemUris)
        {
            Assert.ArgumentNotNull(itemUris, nameof(itemUris));

            var treeView = GetItemTreeView();
            if (treeView == null)
            {
                return;
            }

            treeView.TreeView.Clear();

            foreach (var i in Items)
            {
                var item = i as ItemTreeViewItem;
                if (item == null)
                {
                    continue;
                }

                foreach (var itemUri in itemUris)
                {
                    if (item.ItemUri == itemUri)
                    {
                        treeView.TreeView.Select(item);
                    }
                }
            }
        }

        public void UpdateGutters()
        {
            GutterImage = null;
            GutterToolTip = string.Empty;

            var gutters = Item.Gutters;
            if (gutters.Count == 0)
            {
                return;
            }

            if (gutters.Count == 1)
            {
                GutterImage = gutters[0].Icon.GetSource();
                GutterToolTip = gutters[0].ToolTip;
                return;
            }

            var stackPanel = new StackPanel();

            foreach (var gutter in gutters)
            {
                var icon = new Image
                {
                    Source = gutter.Icon.GetSource(),
                    Width = 16,
                    Height = 16
                };

                var text = new TextBlock
                {
                    Text = gutter.ToolTip,
                    Margin = new Thickness(8, 0, 0, 0)
                };

                var line = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 4, 0, 0)
                };
                line.Children.Add(icon);
                line.Children.Add(text);

                stackPanel.Children.Add(line);
            }

            GutterImage = new Icon("Resources/16x16/gutteritems.png").GetSource();
            GutterToolTip = stackPanel;
        }

        [NotNull]
        protected virtual ItemTreeViewItem GetChildTreeViewItem([NotNull] ItemHeader child)
        {
            Diagnostics.Debug.ArgumentNotNull(child, nameof(child));

            return new ItemTreeViewItem(child);
        }

        protected override bool Renamed(string newName)
        {
            Diagnostics.Debug.ArgumentNotNull(newName, nameof(newName));

            var pipeline = PipelineManager.GetPipeline<RenameItemPipeline>();

            pipeline.NewName = newName;
            pipeline.ItemUri = ItemUri;

            pipeline.Start();

            if (pipeline.IsRenamed)
            {
                Item.Name = newName;

                var n = Item.Path.LastIndexOf('/');
                if (n >= 0)
                {
                    Item.Path = Item.Path.Left(n + 1) + newName;
                }

                /*
        var itemUri = this.ItemUri;
        var parent = this.Parent as BaseTreeViewItem;

        if (parent != null)
        {
          var treeView = parent.GetAncestor<ItemTreeView>();

          if (treeView != null)
          {
            parent.Refresh();
            parent.ExpandAndWait();
            treeView.ExpandTo(itemUri);
          }
        }
        */
            }

            return pipeline.IsRenamed;
        }

        internal void RefreshPreservingSelection()
        {
            var treeView = GetItemTreeView();
            if (treeView == null)
            {
                return;
            }

            var expandedItems = new List<ItemUri>();
            GetExpandedItems(expandedItems, Items);

            treeView.TreeView.Clear();

            Refresh();
            ExpandAndWait();

            foreach (var selectedItem in expandedItems)
            {
                var treeViewItem = treeView.ExpandTo(selectedItem);
                if (treeViewItem == null)
                {
                    continue;
                }

                treeViewItem.IsExpanded = true;
            }

            IsSelected = true;
            Focus();
            Keyboard.Focus(this);
        }

        private void FieldChanged([NotNull] object sender, [NotNull] FieldUri fieldUri, [CanBeNull] string newValue)
        {
            Diagnostics.Debug.ArgumentNotNull(sender, nameof(sender));
            Diagnostics.Debug.ArgumentNotNull(fieldUri, nameof(fieldUri));

            if (fieldUri.ItemVersionUri.ItemUri != ItemUri)
            {
                return;
            }

            if (Item.SerializationStatus == SerializationStatus.Serialized)
            {
                Item.SerializationStatus = SerializationStatus.Modified;
                RefreshStatusIcons();
            }

            if (fieldUri.FieldId == FieldIds.Icon && !string.IsNullOrEmpty(newValue))
            {
                var path = @"/sitecore/shell/~/icon/" + newValue;
                Icon = new Icon(fieldUri.Site, path);
            }

            if (fieldUri.FieldId == FieldIds.StandardValues)
            {
                var itemId = string.IsNullOrWhiteSpace(newValue) ? ItemId.Empty : new ItemId(new Guid(newValue));
                Item.StandardValuesId = itemId;
                Item.StandardValuesField = itemId;
            }
        }

        private void GetChildren([NotNull] IEnumerable<ItemHeader> children, [NotNull] GetChildrenDelegate callback)
        {
            Diagnostics.Debug.ArgumentNotNull(children, nameof(children));
            Diagnostics.Debug.ArgumentNotNull(callback, nameof(callback));

            var result = new List<BaseTreeViewItem>();

            var count = children.Count();
            if (count > 100)
            {
                count = LoadManyItemsDialog.Execute(count);
                if (count < 0)
                {
                    callback(result);
                    return;
                }
            }

            for (var n = 0; n < count; n++)
            {
                var child = children.ElementAt(n);

                var item = GetChildTreeViewItem(child);

                if (child.HasChildren)
                {
                    item.Add(DummyTreeViewItem.Instance);
                }

                result.Add(item);
            }

            callback(result);
        }

        string IItemData.GetData(string key)
        {
            Diagnostics.Debug.ArgumentNotNull(key, nameof(key));

            return ((IItemData)Item).GetData(key);
        }

        private DragOperation GetDragOperation([NotNull] DragEventArgs e, [NotNull] ItemTreeViewItem target, [NotNull] IEnumerable<IItem> items, bool isSameDatabase)
        {
            Diagnostics.Debug.ArgumentNotNull(e, nameof(e));
            Diagnostics.Debug.ArgumentNotNull(target, nameof(target));
            Diagnostics.Debug.ArgumentNotNull(items, nameof(items));

            if (!isSameDatabase)
            {
                return DragOperation.Copy;
            }

            if (items.Count() == 1 && (e.KeyStates & DragDropKeyStates.AltKey) == DragDropKeyStates.AltKey)
            {
                var item = items.First() as ItemTreeViewItem;

                if (item != null && item.IsTemplate)
                {
                    var path = target.GetPath();
                    if (!path.StartsWith(@"/sitecore/templates", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return DragOperation.CreateItem;
                    }
                }
            }

            if ((e.KeyStates & DragDropKeyStates.ControlKey) == DragDropKeyStates.ControlKey)
            {
                return DragOperation.Copy;
            }

            return DragOperation.Move;
        }

        private void GetExpandedItems([NotNull] List<ItemUri> expandedItems, [NotNull] ItemCollection items)
        {
            Diagnostics.Debug.ArgumentNotNull(expandedItems, nameof(expandedItems));
            Diagnostics.Debug.ArgumentNotNull(items, nameof(items));

            foreach (var item in items.OfType<ItemTreeViewItem>())
            {
                if (!item.IsExpanded)
                {
                    continue;
                }

                expandedItems.Add(item.ItemUri);
                GetExpandedItems(expandedItems, item.Items);
            }
        }

        BaseTreeViewItem IScopeable.GetScopedTreeViewItem()
        {
            var result = new ItemTreeViewItem(Item);
            if (Item.HasChildren)
            {
                result.MakeExpandable();
            }

            return result;
        }

        object ISelectable.GetSelectedObject()
        {
            return new ItemTreeViewItemSelectedObject(this);
        }

        private void HandleDropFiles([NotNull] string[] droppedFilePaths)
        {
            Diagnostics.Debug.ArgumentNotNull(droppedFilePaths, nameof(droppedFilePaths));

            var site = Item.ItemUri.Site;

            if (!site.DataService.CanExecuteAsync("Media.Upload"))
            {
                AppHost.MessageBox(string.Format(Rocks.Resources.MediaSkinListBox_HandleDropFiles_, site.DataServiceName), Rocks.Resources.Information, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            GetValueCompleted<ItemHeader> uploadCompleted = delegate(ItemHeader value)
            {
                if (droppedFilePaths.Length != 1)
                {
                    return;
                }

                var item = new ItemTreeViewItem(value);
                Items.Add(item);

                var itemVersionUri = new ItemVersionUri(value.ItemUri, LanguageManager.CurrentLanguage, Data.Version.Latest);

                AppHost.OpenContentEditor(itemVersionUri);
            };

            MediaManager.Upload(Item.ItemUri.DatabaseUri, Item.Path, droppedFilePaths, uploadCompleted);
        }

        private void HandleFileDragOver([NotNull] DragEventArgs e)
        {
            Diagnostics.Debug.ArgumentNotNull(e, nameof(e));

            e.Effects = DragDropEffects.None;

            if ((Item.ItemUri.Site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.Execute) == DataServiceFeatureCapabilities.Execute)
            {
                e.Effects = IsMediaItem() ? DragDropEffects.Copy : DragDropEffects.None;
            }
        }

        private void HandleFileDrop([NotNull] DragEventArgs e)
        {
            Diagnostics.Debug.ArgumentNotNull(e, nameof(e));

            if (!IsMediaItem())
            {
                return;
            }

            var droppedFilePaths = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            if (droppedFilePaths != null)
            {
                HandleDropFiles(droppedFilePaths);
            }
        }

        private void HandleItemDragOver([NotNull] ItemTreeViewItem target, [NotNull] DragEventArgs e)
        {
            Diagnostics.Debug.ArgumentNotNull(target, nameof(target));
            Diagnostics.Debug.ArgumentNotNull(e, nameof(e));

            var items = e.Data.GetData(DragManager.DragIdentifier) as IEnumerable<IItem>;
            if (items == null)
            {
                Diagnostics.Trace.Expected(typeof(IEnumerable<IItem>));
                return;
            }

            var isSameDatabase = true;

            foreach (var s in items)
            {
                if (s == null)
                {
                    Diagnostics.Trace.EnumerableContainsNull();
                    return;
                }

                if (s.ItemUri.DatabaseUri != target.ItemUri.DatabaseUri)
                {
                    isSameDatabase = false;
                }

                if (s.ItemUri == target.ItemUri)
                {
                    return;
                }
            }

            switch (GetDragOperation(e, target, items, isSameDatabase))
            {
                case DragOperation.Copy:
                    adorner.AllowedPositions = ControlDragAdornerPosition.All;
                    e.Effects = DragDropEffects.Copy;
                    break;
                case DragOperation.Move:
                    adorner.AllowedPositions = ControlDragAdornerPosition.All;
                    e.Effects = DragDropEffects.Move;
                    break;
                case DragOperation.CreateItem:
                    adorner.AllowedPositions = ControlDragAdornerPosition.Over;
                    e.Effects = DragDropEffects.Link;
                    break;
            }
        }

        private void HandleItemDrop([NotNull] ItemTreeViewItem target, [NotNull] DragEventArgs e)
        {
            Diagnostics.Debug.ArgumentNotNull(target, nameof(target));
            Diagnostics.Debug.ArgumentNotNull(e, nameof(e));

            var items = e.Data.GetData(DragManager.DragIdentifier) as IEnumerable<IItem>;
            if (items == null)
            {
                Diagnostics.Trace.Expected(typeof(IEnumerable<IItem>));
                return;
            }

            var isSameDatabase = items.All(i => i.ItemUri.DatabaseUri == target.ItemUri.DatabaseUri);
            if (!isSameDatabase)
            {
                TransferItems(target, items);
                return;
            }

            switch (GetDragOperation(e, target, items, true))
            {
                case DragOperation.Copy:
                    DragCopyPipeline.Run().WithParameters(target, items, adorner.LastPosition, e.KeyStates);
                    break;
                case DragOperation.Move:
                    DragMovePipeline.Run().WithParameters(target, items, adorner.LastPosition, e.KeyStates);
                    break;
            }
        }

        private void HandleSolutionExplorerDrop([NotNull] ItemTreeViewItem target, [NotNull] DragEventArgs e)
        {
            Diagnostics.Debug.ArgumentNotNull(target, nameof(target));
            Diagnostics.Debug.ArgumentNotNull(e, nameof(e));

            var droppedFileName = e.Data.GetData(DataFormats.Text, true) as string;
            if (droppedFileName == null)
            {
                return;
            }

            var project = AppHost.Projects.FirstOrDefault(p => droppedFileName.StartsWith(p.FolderName, StringComparison.InvariantCultureIgnoreCase));
            if (project == null)
            {
                return;
            }

            var relativeFileName = project.GetRelativeFileName(droppedFileName);

            if (AppHost.MessageBox(string.Format("Are you sure you want to bind:\n\n\\{0}\n\nto:\n\n{1}", relativeFileName, target.Item.Path), "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            project.LinkItemAndFile(target.ItemUri, relativeFileName);
        }

        private bool IsMediaItem()
        {
            var item = this;

            var mediaLibraryId = IdManager.GetItemId("/sitecore/media library");

            while (item != null)
            {
                if (item.Item.ItemId == mediaLibraryId)
                {
                    return true;
                }

                item = item.GetParentTreeViewItem() as ItemTreeViewItem;
            }

            return false;
        }

        private void ItemDeleted([NotNull] object sender, [NotNull] ItemUri deletedItemUri)
        {
            Diagnostics.Debug.ArgumentNotNull(sender, nameof(sender));
            Diagnostics.Debug.ArgumentNotNull(deletedItemUri, nameof(deletedItemUri));

            if (Item.ItemUri == deletedItemUri)
            {
                Remove();
            }

            if (Item.StandardValuesId == deletedItemUri.ItemId)
            {
                Item.StandardValuesId = ItemId.Empty;
                Item.StandardValuesField = ItemId.Empty;
            }
        }

        private void ItemRenamed([NotNull] object sender, [NotNull] ItemUri renamedItemUri, [NotNull] string newName)
        {
            Diagnostics.Debug.ArgumentNotNull(sender, nameof(sender));
            Diagnostics.Debug.ArgumentNotNull(renamedItemUri, nameof(renamedItemUri));
            Diagnostics.Debug.ArgumentNotNull(newName, nameof(newName));

            if (Item.ItemUri == renamedItemUri)
            {
                ItemHeader.Text = newName;
                Item.Name = newName;
                GutterManager.UpdateGutter(renamedItemUri);
            }
        }

        private void ItemSerialized([NotNull] object sender, [NotNull] ItemUri itemUri, SerializationOperation serializationOperation)
        {
            Diagnostics.Debug.ArgumentNotNull(sender, nameof(sender));
            Diagnostics.Debug.ArgumentNotNull(itemUri, nameof(itemUri));

            if (itemUri != ItemUri)
            {
                return;
            }

            Item.SerializationStatus = SerializationStatus.Serialized;
            RefreshStatusIcons();

            if (serializationOperation == SerializationOperation.Revert || serializationOperation == SerializationOperation.Serialize || serializationOperation == SerializationOperation.Update)
            {
                return;
            }

            SetSerialized(Items, serializationOperation);
        }

        private void OpenToolTip([NotNull] object sender, [NotNull] ToolTipEventArgs e)
        {
            Diagnostics.Debug.ArgumentNotNull(sender, nameof(sender));
            Diagnostics.Debug.ArgumentNotNull(e, nameof(e));

            ToolTip = ToolTipBuilder.BuildToolTip(Item);
        }

        private void SetSerialized([NotNull] ItemCollection items, SerializationOperation serializationOperation)
        {
            Diagnostics.Debug.ArgumentNotNull(items, nameof(items));

            foreach (var i in items)
            {
                var item = i as TreeViewItem;
                if (item == null)
                {
                    continue;
                }

                var itemTreeViewItem = item as ItemTreeViewItem;
                if (itemTreeViewItem != null)
                {
                    switch (serializationOperation)
                    {
                        case SerializationOperation.RevertTree:
                        case SerializationOperation.UpdateTree:
                            if (itemTreeViewItem.Item.SerializationStatus != SerializationStatus.NotSerialized)
                            {
                                Item.SerializationStatus = SerializationStatus.Serialized;
                                RefreshStatusIcons();
                            }

                            break;
                        case SerializationOperation.SerializeTree:
                            Item.SerializationStatus = SerializationStatus.Serialized;
                            RefreshStatusIcons();
                            break;
                    }
                }

                SetSerialized(item.Items, serializationOperation);
            }
        }

        private void TransferItems([NotNull] ItemTreeViewItem target, [NotNull] IEnumerable<IItem> items)
        {
            Diagnostics.Debug.ArgumentNotNull(target, nameof(target));
            Diagnostics.Debug.ArgumentNotNull(items, nameof(items));

            var options = new TaskDialogOptions
            {
                Owner = this.GetAncestorOrSelf<Window>(),
                Title = "Transfer Items",
                CommonButtons = TaskDialogCommonButtons.None,
                MainInstruction = "Are you sure you want to transfer these items to another database?",
                Content = "Any item dependencies will not be copied.",
                MainIcon = VistaTaskDialogIcon.Information,
                CommandButtons = new[]
                {
                    "Copy Items",
                    "Copy Items and SubItems",
                    "Copy Items - Change Item IDs",
                    "Copy Items and SubItems - Change Item IDs"
                },
                AllowDialogCancellation = true
            };

            var r = TaskDialog.Show(options).CommandButtonResult;
            if (r == null)
            {
                return;
            }

            var transfer = new TransferDialog(target, items, r == 1 || r == 3, r == 2 || r == 3);
            AppHost.Shell.ShowDialog(transfer);
        }
    }
}
