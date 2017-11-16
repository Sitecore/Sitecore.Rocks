// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentEditors;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.ContentTrees.Gutters;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.ContentTrees.Pipelines.DefaultActions;
using Sitecore.Rocks.ContentTrees.Pipelines.GetChildren;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ContextMenuExtensions;
using Sitecore.Rocks.Extensions.DependencyObjectExtensions;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Controls
{
    public partial class ItemTreeView
    {
        public delegate void ExpandToDelegate([CanBeNull] BaseSiteTreeViewItem item);

        public delegate void FilterChildrenEventHandler([NotNull] object sender, [NotNull] BaseTreeViewItem baseTreeViewItem, List<BaseTreeViewItem> children);

        public delegate object GetContextDelegate(object source);

        private readonly Journal<ScopeDescriptor> _scopes = new Journal<ScopeDescriptor>();

        private bool _allowDrag;

        private string _filterText;

        public ItemTreeView()
        {
            AllowContextMenu = true;
            AllowRenameItem = true;
            AllowDrag = true;
            AllowDrop = true;
            AllowEmptyContextMenu = true;

            InitializeComponent();

            Loaded += ControlLoaded;

            TreeView = CreateTreeView();
            Content = TreeView;
            _scopes.Push(new ScopeDescriptor(TreeView, null));
        }

        public bool AllowContextMenu { get; set; }

        public bool AllowDrag
        {
            get { return _allowDrag; }

            set
            {
                _allowDrag = value;

                if (TreeView != null)
                {
                    TreeView.AllowDrag = value;
                }
            }
        }

        public bool AllowEmptyContextMenu { get; set; }

        public bool AllowRenameItem { get; set; }

        public bool CanGoBack
        {
            get { return _scopes.CanGoBack; }
        }

        public bool CanGoForward
        {
            get { return _scopes.CanGoForward; }
        }

        [NotNull]
        public string FilterText
        {
            get { return _filterText ?? string.Empty; }
        }

        [CanBeNull]
        public GetContextDelegate GetContext { get; set; }

        [NotNull]
        public ItemCollection Items
        {
            get { return TreeView.Items; }
        }

        [CanBeNull]
        public object SelectedItem
        {
            get { return TreeView.SelectedItem; }
        }

        [NotNull]
        public List<BaseTreeViewItem> SelectedItems
        {
            get { return TreeView.SelectedItems; }
        }

        public SelectionMode SelectionMode
        {
            get { return TreeView.SelectionMode; }

            set { TreeView.SelectionMode = value; }
        }

        public bool SubscribeToNotifications { get; set; }

        public bool SupportsVirtualItems { get; set; }

        [NotNull]
        protected IEnumerable<MultiSelectTreeView> TreeViews
        {
            get { return _scopes.GetHistory().Select(scope => scope.TreeView).ToList(); }
        }

        [CanBeNull]
        internal BaseTreeViewItem RootTreeViewItem { get; private set; }

        [NotNull]
        internal MultiSelectTreeView TreeView { get; private set; }

        public event MouseButtonEventHandler DoubleClick;

        [CanBeNull]
        public BaseSiteTreeViewItem ExpandTo([NotNull] ItemUri itemUri)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            if (itemUri == ItemUri.Empty)
            {
                return null;
            }

            var item = itemUri.Site.DataService.GetItemFields(new ItemVersionUri(itemUri, LanguageManager.CurrentLanguage, Data.Version.Latest));
            if (item == Item.Empty)
            {
                return null;
            }

            return ExpandTo(item);
        }

        public void ExpandTo([NotNull] DatabaseUri databaseUri, [NotNull] string itemPath)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(itemPath, nameof(itemPath));

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                Guid guid;
                if (!Guid.TryParse(response, out guid))
                {
                    return;
                }

                var itemUri = new ItemUri(databaseUri, new ItemId(guid));

                ExpandTo(itemUri);
            };

            databaseUri.Site.DataService.ExecuteAsync("Items.GetItemId", completed, itemPath, databaseUri.DatabaseName.ToString());
        }

        [CanBeNull]
        public BaseSiteTreeViewItem ExpandTo([NotNull] Item item)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            BaseSiteTreeViewItem parentNode;
            var index = -1;

            var databaseNode = FindDatabaseTreeViewItem(item.Uri.ItemUri);
            if (databaseNode != null)
            {
                parentNode = databaseNode;

                index = item.Path.Count - 1;
            }
            else
            {
                var databaseUri = item.Uri.ItemUri.DatabaseUri;
                var rootItem = TreeView.Items.OfType<ItemTreeViewItem>().FirstOrDefault(i => i.ItemUri.DatabaseUri == databaseUri);

                if (rootItem == null)
                {
                    return null;
                }

                for (var n = item.Path.Count - 1; n >= 0; n--)
                {
                    if (string.Compare(item.Path[n].Name, rootItem.Item.Name, StringComparison.InvariantCultureIgnoreCase) != 0)
                    {
                        continue;
                    }

                    index = n - 1;
                    break;
                }

                if (index == -1)
                {
                    return rootItem;
                }

                parentNode = rootItem;
            }

            for (var n = index; n >= 0; n--)
            {
                var itemPath = item.Path[n];

                var itemNode = FindItem(parentNode, itemPath);
                if (itemNode == null)
                {
                    parentNode.IsExpanded = false;
                    parentNode.Items.Clear();

                    parentNode.ExpandAndWait();

                    itemNode = FindItem(parentNode, itemPath);
                    if (itemNode == null)
                    {
                        return null;
                    }
                }

                parentNode = itemNode;
            }

            TreeView.Clear();
            parentNode.IsSelected = true;
            parentNode.IsItemSelected = true;

            var node = parentNode.Parent as BaseTreeViewItem;
            while (node != null)
            {
                node.IsExpanded = true;
                node = node.Parent as BaseTreeViewItem;
            }

            Keyboard.Focus(parentNode);

            return parentNode;
        }

        public void Filter([NotNull] string text)
        {
            Assert.ArgumentNotNull(text, nameof(text));

            _filterText = text;

            foreach (BaseTreeViewItem item in TreeView.Items)
            {
                if (item == null)
                {
                    continue;
                }

                Filter(item, text);
            }
        }

        public event FilterChildrenEventHandler FilterChildren;

        [CanBeNull]
        public T FindItem<T>([NotNull] ItemId itemId) where T : BaseTreeViewItem
        {
            Assert.ArgumentNotNull(itemId, nameof(itemId));

            return FindItem<T>(itemId, TreeView.Items);
        }

        [CanBeNull]
        public T FindItem<T>([NotNull] ItemUri itemUri) where T : BaseTreeViewItem
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            return FindItem<T>(itemUri, TreeView.Items);
        }

        public void FindItem([NotNull] string text)
        {
            Assert.ArgumentNotNull(text, nameof(text));

            DatabaseUri databaseUri = null;
            var itemId = ItemId.Empty;

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            foreach (var treeViewItem in SelectedItems)
            {
                var item = treeViewItem as IItemUri;
                if (item != null)
                {
                    databaseUri = item.ItemUri.DatabaseUri;
                    itemId = item.ItemUri.ItemId;
                    break;
                }

                var databaseItem = treeViewItem as DatabaseTreeViewItem;
                if (databaseItem != null)
                {
                    databaseUri = databaseItem.DatabaseUri;
                    break;
                }
            }

            if (databaseUri == null)
            {
                AppHost.MessageBox(Rocks.Resources.ContentTree_FindItem_NoOpenDatabase, Rocks.Resources.Information, MessageBoxButton.OK, MessageBoxImage.Information);
                TreeView.Focus();
                return;
            }

            ExecuteCompleted c = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                if (string.IsNullOrEmpty(response))
                {
                    AppHost.MessageBox(Rocks.Resources.ContentTree_FindItem_The_text_was_not_found_, Rocks.Resources.Information, MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var itemUri = new ItemUri(databaseUri, new ItemId(new Guid(response)));

                ExpandTo(itemUri);
            };

            databaseUri.Site.DataService.ExecuteAsync("ContentTrees.Find", c, text, itemId.ToString(), databaseUri.DatabaseName.Name);
        }

        [NotNull]
        public IEnumerable<BaseTreeViewItem> GetSelectedItems([NotNull] object source)
        {
            Assert.ArgumentNotNull(source, nameof(source));

            var baseTreeViewItem = TreeView.GetBaseTreeViewItem(source);

            return TreeView.GetSelectedItems(baseTreeViewItem);
        }

        public void PushScope([NotNull] BaseTreeViewItem treeViewItem)
        {
            Assert.ArgumentNotNull(treeViewItem, nameof(treeViewItem));

            TreeView = CreateTreeView();
            Content = TreeView;

            _scopes.Push(new ScopeDescriptor(TreeView, null));

            TreeView.Items.Add(treeViewItem);

            if (treeViewItem.Items.Count > 0)
            {
                treeViewItem.IsExpanded = true;
            }

            Keyboard.Focus(treeViewItem);

            RaiseScopeChanged();
        }

        public void ScopeBack()
        {
            var descriptor = _scopes.GoBack();
            if (descriptor != null)
            {
                SetScope(descriptor);
            }
        }

        public event EventHandler ScopeChanged;

        public void ScopeForward()
        {
            var descriptor = _scopes.GoForward();
            SetScope(descriptor);
        }

        public void ScopeHome()
        {
            var treeView = CreateTreeView();

            var items = new List<BaseTreeViewItem>();

            GetChildrenPipeline.Run().WithParameters(items, null, null);

            foreach (var baseTreeViewItem in items)
            {
                treeView.Items.Add(baseTreeViewItem);
            }

            Content = treeView;
            TreeView = treeView;

            _scopes.Push(new ScopeDescriptor(treeView, null));

            RaiseScopeChanged();
        }

        public event RoutedPropertyChangedEventHandler<object> SelectedItemsChanged;

        public void SetFocus()
        {
            var selectedItem = TreeView.SelectedItems.FirstOrDefault();
            if (selectedItem == null)
            {
                return;
            }

            selectedItem.BringIntoView();
            selectedItem.Focus();
            Keyboard.Focus(selectedItem);
        }

        [CanBeNull]
        internal DatabaseTreeViewItem FindDatabaseTreeViewItem([NotNull] ItemUri itemUri)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));

            return FindDatabaseTreeViewItem(TreeView.Items, itemUri);
        }

        [CanBeNull]
        internal SiteTreeViewItem FindSiteTreeViewItem([NotNull] Site site)
        {
            Debug.ArgumentNotNull(site, nameof(site));

            return FindSiteTreeViewItem(TreeView.Items, site);
        }

        internal void InternalFilterChildren([NotNull] BaseTreeViewItem item, [NotNull] List<BaseTreeViewItem> children)
        {
            Debug.ArgumentNotNull(item, nameof(item));
            Debug.ArgumentNotNull(children, nameof(children));

            var getChildren = FilterChildren;
            if (getChildren != null)
            {
                getChildren(this, item, children);
            }
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            Notifications.Unloaded += ControlUnloaded;

            if (!SubscribeToNotifications)
            {
                return;
            }

            Notifications.RegisterTemplateEvents(this, saved: TemplateSaved);
            Notifications.RegisterItemEvents(this, deleted: ItemDeleted);
        }

        private void ControlUnloaded([NotNull] object sender, [NotNull] object window)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(window, nameof(window));

            if (!this.IsContainedIn(window))
            {
                return;
            }

            var current = _scopes.Peek();

            foreach (var scope in _scopes.Entries)
            {
                if (scope == current)
                {
                    continue;
                }

                Content = scope.TreeView;
                Notifications.RaiseUnloaded(this, Content);
            }

            if (current != null)
            {
                Content = current.TreeView;
            }
        }

        [NotNull]
        private MultiSelectTreeView CreateTreeView()
        {
            var treeView = new MultiSelectTreeView
            {
                BorderThickness = new Thickness(0),
                AllowDrag = AllowDrag,
                AllowDrop = AllowDrop,
                SelectionMode = SelectionMode.Extended
            };

            treeView.ContextMenuOpening += OpenContextMenu;
            treeView.KeyDown += HandleKeyDown;
            treeView.MouseUp += HandleMouseUp;
            treeView.PreviewMouseLeftButtonDown += MouseLeftButton;
            treeView.SelectedItemsChanged += RaiseSelectedItemsChanged;

            return treeView;
        }

        private Visibility Filter([NotNull] BaseTreeViewItem item, [NotNull] string text)
        {
            Debug.ArgumentNotNull(item, nameof(item));
            Debug.ArgumentNotNull(text, nameof(text));

            var isVisible = false;

            foreach (var o in item.Items)
            {
                var child = o as BaseTreeViewItem;
                if (child == null)
                {
                    continue;
                }

                var v = Filter(child, text);

                if (v == Visibility.Visible)
                {
                    isVisible = true;
                }
            }

            item.Visibility = isVisible || item.Text.IsFilterMatch(text) ? Visibility.Visible : Visibility.Collapsed;

            return item.Visibility;
        }

        [CanBeNull]
        private DatabaseTreeViewItem FindDatabaseTreeViewItem([NotNull] ItemCollection collection, [NotNull] ItemUri itemUri)
        {
            Debug.ArgumentNotNull(collection, nameof(collection));
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));

            foreach (var item in collection)
            {
                var folderItem = item as ConnectionFolderTreeViewItem;
                var localFolderItem = item as LocalConnectionFolderTreeViewItem;
                if (folderItem == null && localFolderItem == null)
                {
                    continue;
                }

                var baseTreeViewItem = item as BaseTreeViewItem;
                if (!baseTreeViewItem.HasChildren)
                {
                    baseTreeViewItem.ExpandAndWait();
                    baseTreeViewItem.IsExpanded = false;
                }

                var result = FindDatabaseTreeViewItem(baseTreeViewItem.Items, itemUri);
                if (result != null)
                {
                    return result;
                }
            }

            foreach (var item in collection)
            {
                var siteItem = item as SiteTreeViewItem;
                if (siteItem == null)
                {
                    continue;
                }

                if (siteItem.Site != itemUri.Site)
                {
                    continue;
                }

                if (!siteItem.HasChildren)
                {
                    siteItem.ExpandAndWait();
                    siteItem.IsExpanded = false;
                }

                foreach (var i in siteItem.Items)
                {
                    var databaseItem = i as DatabaseTreeViewItem;
                    if (databaseItem == null)
                    {
                        continue;
                    }

                    if (databaseItem.DatabaseUri.DatabaseName == itemUri.DatabaseName)
                    {
                        return databaseItem;
                    }
                }
            }

            foreach (var i in collection)
            {
                var databaseItem = i as DatabaseTreeViewItem;
                if (databaseItem == null)
                {
                    continue;
                }

                if (databaseItem.DatabaseUri.DatabaseName == itemUri.DatabaseName)
                {
                    return databaseItem;
                }
            }

            return null;
        }

        [CanBeNull]
        private ItemTreeViewItem FindItem([NotNull] BaseSiteTreeViewItem parentNode, [NotNull] ItemPath itemPath)
        {
            Debug.ArgumentNotNull(parentNode, nameof(parentNode));
            Debug.ArgumentNotNull(itemPath, nameof(itemPath));

            foreach (var item in parentNode.Items)
            {
                var result = item as ItemTreeViewItem;
                if (result == null)
                {
                    continue;
                }

                if (result.ItemUri == itemPath.ItemUri)
                {
                    return result;
                }

                result = FindItem(result, itemPath);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        [CanBeNull]
        private T FindItem<T>([NotNull] ItemUri itemUri, [NotNull] ItemCollection items) where T : BaseTreeViewItem
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(items, nameof(items));

            foreach (var obj in items)
            {
                var baseTreeViewItem = obj as BaseTreeViewItem;
                if (baseTreeViewItem == null)
                {
                    continue;
                }

                var item = baseTreeViewItem as IItemUri;
                if (item != null && item.ItemUri == itemUri && item is T)
                {
                    return (T)baseTreeViewItem;
                }

                var result = FindItem<T>(itemUri, baseTreeViewItem.Items);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        [CanBeNull]
        private T FindItem<T>([NotNull] ItemId itemId, [NotNull] ItemCollection items) where T : BaseTreeViewItem
        {
            Debug.ArgumentNotNull(itemId, nameof(itemId));
            Debug.ArgumentNotNull(items, nameof(items));

            foreach (var o in items)
            {
                var baseItem = o as BaseTreeViewItem;
                if (baseItem == null)
                {
                    continue;
                }

                var item = baseItem as IItemUri;
                if (item == null)
                {
                    continue;
                }

                if (item.ItemUri.ItemId == itemId && item is T)
                {
                    return (T)baseItem;
                }

                var baseTreeViewItem = FindItem<T>(itemId, baseItem.Items);
                if (baseTreeViewItem != null)
                {
                    return baseTreeViewItem;
                }
            }

            return null;
        }

        [CanBeNull]
        private SiteTreeViewItem FindSiteTreeViewItem([NotNull] ItemCollection collection, [NotNull] Site site)
        {
            Debug.ArgumentNotNull(collection, nameof(collection));
            Debug.ArgumentNotNull(site, nameof(site));

            foreach (var item in collection)
            {
                var folderItem = item as ConnectionFolderTreeViewItem;
                var localFolderItem = item as LocalConnectionFolderTreeViewItem;
                if (folderItem == null && localFolderItem == null)
                {
                    continue;
                }

                var baseTreeViewItem = item as BaseTreeViewItem;
                if (!baseTreeViewItem.HasChildren)
                {
                    baseTreeViewItem.ExpandAndWait();
                    baseTreeViewItem.IsExpanded = false;
                }

                var result = FindSiteTreeViewItem(baseTreeViewItem.Items, site);
                if (result != null)
                {
                    return result;
                }
            }

            return collection.OfType<SiteTreeViewItem>().FirstOrDefault(siteItem => siteItem.Site == site);
        }

        private void HandleKeyDown([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.Key == Key.F2)
            {
                SetCurrentItemInEditMode(true);
                return;
            }

            if (e.Key != Key.Enter || Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt) || Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                return;
            }

            var context = new ContentTreeContext(this, TreeView.GetSelectedItems(null));
            var pipeline = DefaultActionPipeline.Run().WithParameters(context);
            e.Handled = pipeline.Handled;
        }

        private void HandleMouseUp([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.ButtonState != MouseButtonState.Released)
            {
                return;
            }

            if (!Keyboard.IsKeyDown(Key.LeftAlt) && !Keyboard.IsKeyDown(Key.RightAlt))
            {
                return;
            }

            var baseTreeViewItem = TreeView.GetBaseTreeViewItem(e.Source);
            if (baseTreeViewItem != null)
            {
                Commands.CommandManager.Execute(typeof(ContentTrees.Commands.Editing.EditItems), new ContentTreeContext(this, TreeView.GetSelectedItems(baseTreeViewItem)));
            }
        }

        private void ItemDeleted([NotNull] object sender, [NotNull] ItemUri itemUri)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));

            foreach (var treeView in TreeViews)
            {
                for (var index = treeView.SelectedItems.Count - 1; index >= 0; index--)
                {
                    var item = treeView.SelectedItems[index] as ItemTreeViewItem;
                    if (item == null)
                    {
                        continue;
                    }

                    if (item.ItemUri == itemUri)
                    {
                        treeView.SelectedItems.Remove(item);
                    }
                }
            }
        }

        private void MouseLeftButton([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.ClickCount <= 1)
            {
                return;
            }

            var frameworkElement = e.Source as FrameworkElement;
            while (frameworkElement != null)
            {
                if (frameworkElement is BaseTreeViewItem)
                {
                    break;
                }

                frameworkElement = frameworkElement.Parent as FrameworkElement;
            }

            if (frameworkElement == null)
            {
                return;
            }

            var doubleClick = DoubleClick;
            if (doubleClick == null)
            {
                return;
            }

            frameworkElement = e.OriginalSource as FrameworkElement;
            if (frameworkElement != null)
            {
                if (frameworkElement.GetAncestorOrSelf<ToggleButton>() != null)
                {
                    return;
                }
            }

            doubleClick(sender, e);
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (!AllowContextMenu)
            {
                return;
            }

            ContextMenu = null;

            object context;

            var point = ((UIElement)e.Source).TranslatePoint(new Point(e.CursorLeft, e.CursorTop), this);
            if (point.X < 20 && e.CursorLeft != -1 && e.CursorTop != -1)
            {
                var control = TreeView.GetBaseTreeViewItem(e.Source);
                if (control == null)
                {
                    e.Handled = true;
                    return;
                }

                context = new GutterContext(this, TreeView.GetSelectedItems(control));
            }
            else
            {
                var control = TreeView.GetBaseTreeViewItem(e.Source);
                if (control == null)
                {
                    if (!AllowEmptyContextMenu)
                    {
                        return;
                    }

                    context = new ContentTreeContext(this, Enumerable.Empty<BaseTreeViewItem>());
                }
                else if (GetContext != null)
                {
                    context = GetContext(e.Source);
                }
                else
                {
                    context = new ContentTreeContext(this, TreeView.GetSelectedItems(control));
                }
            }

            var commands = Commands.CommandManager.GetCommands(context).ToList();
            if (!commands.Any())
            {
                return;
            }

            var contextMenu = new ContextMenu();

            contextMenu.Build(commands, context);

            ContextMenu = contextMenu;
        }

        private void RaiseScopeChanged()
        {
            var changed = ScopeChanged;
            if (changed != null)
            {
                changed(this, EventArgs.Empty);
            }
        }

        private void RaiseSelectedItemsChanged([NotNull] object sender, [NotNull] RoutedPropertyChangedEventArgs<object> e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedItemsChanged = SelectedItemsChanged;
            if (selectedItemsChanged != null)
            {
                selectedItemsChanged(sender, e);
            }
        }

        private void SetCurrentItemInEditMode(bool editMode)
        {
            if (!AllowRenameItem)
            {
                return;
            }

            var items = TreeView.SelectedItems;
            if (items.Count != 1)
            {
                return;
            }

            var selectedItem = items[0] as TreeViewItem;
            if (selectedItem == null)
            {
                return;
            }

            var treeViewItemHeader = selectedItem.Header as BaseTreeViewItemHeader;
            if (treeViewItemHeader == null)
            {
                return;
            }

            if (treeViewItemHeader.IsEditable)
            {
                treeViewItemHeader.IsInEditMode = editMode;
            }
        }

        private void SetScope([NotNull] ScopeDescriptor descriptor)
        {
            Debug.ArgumentNotNull(descriptor, nameof(descriptor));

            TreeView = descriptor.TreeView;
            RootTreeViewItem = descriptor.Item;
            Content = TreeView;

            var input = TreeView.SelectedItem as IInputElement;
            if (input != null)
            {
                Keyboard.Focus(input);
            }

            RaiseScopeChanged();
        }

        private void TemplateSaved([NotNull] object sender, [NotNull] ItemUri templateUri, [NotNull] string templateName)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(templateUri, nameof(templateUri));
            Debug.ArgumentNotNull(templateName, nameof(templateName));

            foreach (var treeView in TreeViews)
            {
                var item = FindItem<ItemTreeViewItem>(templateUri, treeView.Items);
                if (item != null)
                {
                    item.Refresh();
                }
            }
        }

        public class ScopeDescriptor
        {
            public ScopeDescriptor([NotNull] MultiSelectTreeView treeView, [CanBeNull] BaseTreeViewItem item)
            {
                Assert.ArgumentNotNull(treeView, nameof(treeView));

                TreeView = treeView;
                Item = item;
            }

            [CanBeNull]
            public BaseTreeViewItem Item { get; }

            [NotNull]
            public MultiSelectTreeView TreeView { get; }
        }
    }
}
