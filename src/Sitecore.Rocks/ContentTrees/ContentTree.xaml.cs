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
using Sitecore.Rocks.ContentTrees.Gutters;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.ContentTrees.Items.SelectedObjects;
using Sitecore.Rocks.ContentTrees.Pipelines.DefaultActions;
using Sitecore.Rocks.ContentTrees.Pipelines.GetChildren;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ContextMenuExtensions;
using Sitecore.Rocks.Extensions.DependencyObjectExtensions;
using Sitecore.Rocks.Extensions.UserControlExtensions;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI;

namespace Sitecore.Rocks.ContentTrees
{
    public partial class ContentTree : IContextProvider, ISelectionTracking
    {
        public const string ContentTreeQuickViewRegistryKey = "ContentTree\\QuickView";

        public ContentTree()
        {
            InitializeComponent();

            Notifications.RegisterItemEvents(this, saved: ItemSaved);
            Notifications.RegisterSiteEvents(this, deleted: DeleteSite);

            GotFocus += FocusControl;
            Loaded += ControlLoaded;
            Notifications.Unloaded += ControlUnloaded;
            Notifications.FeaturesChanged += HandleChangedFeatures;

            ItemTreeView.SelectedItemsChanged += SelectedItemsChanged;

            Initialize();
        }

        [NotNull]
        public ItemTreeView ContentTreeView => ItemTreeView;

        [NotNull]
        public IPane Pane { get; set; }

        [NotNull]
        public IEnumerable<TreeViewItem> TreeViewItems
        {
            get
            {
                foreach (var item in GetTreeViewItems(ItemTreeView.Items))
                {
                    yield return item;
                }
            }
        }

        public void Activate()
        {
            var activatable = Pane as IActivatable;
            if (activatable != null)
            {
                activatable.Activate();
            }
        }

        [NotNull]
        public object GetContext()
        {
            return new ContentTreeContext(ItemTreeView, ItemTreeView.TreeView.GetSelectedItems(null));
        }

        public void Initialize()
        {
            ItemTreeView.TreeView.Items.Clear();

            var items = new List<BaseTreeViewItem>();

            GetChildrenPipeline.Run().WithParameters(items, null, null);

            foreach (var baseTreeViewItem in items)
            {
                ItemTreeView.TreeView.Items.Add(baseTreeViewItem);
            }

            UpdateScopeButtons();
            UpdateScope();
        }

        public void Locate([NotNull] ItemUri itemUri)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            Activate();

            ItemTreeView.TreeView.Clear();
            var treeViewItem = ItemTreeView.ExpandTo(itemUri);
            if (treeViewItem != null)
            {
                treeViewItem.BringIntoView();
                treeViewItem.IsSelected = true;
                Keyboard.Focus(treeViewItem);
            }
            else
            {
                Keyboard.Focus(ItemTreeView.TreeView);
            }
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            ActiveContext.ActiveContentTree = this;
        }

        private void ControlUnloaded([NotNull] object sender, [NotNull] object window)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(window, nameof(window));

            if (!this.IsContainedIn(window))
            {
                return;
            }

            Notifications.Unloaded -= ControlUnloaded;

            if (Equals(ActiveContext.ActiveContentTree, this))
            {
                ActiveContext.ActiveContentTree = null;
                if (ActiveContext.Focused == Focused.ContentTree)
                {
                    ActiveContext.Focused = Focused.None;
                }
            }
        }

        private void DeleteSite([NotNull] object sender, [NotNull] Site site)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(site, nameof(site));

            var sites = TreeViewItems.OfType<SiteTreeViewItem>().Where(i => i.Site == site).ToList();

            foreach (var siteTreeViewItem in sites)
            {
                var itemsControl = siteTreeViewItem.Parent as ItemsControl;
                if (itemsControl != null)
                {
                    itemsControl.Items.Remove(siteTreeViewItem);
                }
            }
        }

        private void DoubleClick([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var pipeline = DefaultActionPipeline.Run().WithParameters(GetContext());

            e.Handled = pipeline.Handled;
        }

        private void FilterChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ItemTreeView.Filter(Filter.Text);
        }

        private void FindKeyDown([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.Key != Key.Enter)
            {
                return;
            }

            ItemTreeView.FindItem(Find.Text);
        }

        private void FocusControl([NotNull] object sender, [NotNull] RoutedEventArgs args)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(args, nameof(args));

            ActiveContext.ActiveContentTree = this;
            ActiveContext.Focused = Focused.ContentTree;
        }

        IEnumerable<object> ISelectionTracking.GetSelectedObjects()
        {
            Func<BaseTreeViewItem, object> getDescriptor = delegate(BaseTreeViewItem item)
            {
                var tracker = item as ISelectable;
                if (tracker != null)
                {
                    return tracker.GetSelectedObject();
                }

                return new GenericSelectedObject(item);
            };

            return ItemTreeView.SelectedItems.Select(getDescriptor);
        }

        [NotNull]
        private IEnumerable<TreeViewItem> GetTreeViewItems([NotNull] ItemCollection items)
        {
            Debug.ArgumentNotNull(items, nameof(items));
            foreach (var item in items)
            {
                if (item == DummyTreeViewItem.Instance)
                {
                    continue;
                }

                var treeViewItem = item as TreeViewItem;
                if (treeViewItem == null)
                {
                    continue;
                }

                yield return treeViewItem;

                foreach (var child in GetTreeViewItems(treeViewItem.Items))
                {
                    yield return child;
                }
            }
        }

        private void GoClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ItemTreeView.FindItem(Find.Text);
        }

        private void HandleChangedFeatures([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            UpdateScopeButtons();
        }

        private void HandleNewConnectionClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            SiteManager.NewConnection();
        }

        private void ItemSaved([NotNull] object sender, [NotNull] ContentModel contentModel)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(contentModel, nameof(contentModel));

            var list = new List<ItemUri>();

            foreach (var field in contentModel.Fields)
            {
                foreach (var fieldUri in field.FieldUris)
                {
                    if (list.Contains(fieldUri.ItemVersionUri.ItemUri))
                    {
                        list.Add(fieldUri.ItemVersionUri.ItemUri);
                    }
                }
            }

            GutterManager.UpdateGutter(list);
        }

        private void OpenMenu([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = GetContext();

            var contextMenu = ContextMenuExtensions.GetContextMenu(context);
            if (contextMenu == null)
            {
                return;
            }

            contextMenu.Placement = PlacementMode.Bottom;
            contextMenu.PlacementTarget = Menu;
            contextMenu.IsOpen = true;
        }

        private void ScopeBack([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (ItemTreeView.CanGoBack)
            {
                ItemTreeView.ScopeBack();
            }
        }

        private void ScopeForward([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (ItemTreeView.CanGoForward)
            {
                ItemTreeView.ScopeForward();
            }
        }

        private void ScopeHome([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ItemTreeView.ScopeHome();
        }

        private void SelectedItemsChanged([NotNull] object sender, [NotNull] RoutedPropertyChangedEventArgs<object> routedPropertyChangedEventArgs)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(routedPropertyChangedEventArgs, nameof(routedPropertyChangedEventArgs));

            AppHost.Selection.Track(Pane, ((ISelectionTracking)this).GetSelectedObjects());

            ActiveContext.RaiseSelectedItemsChanged();
        }

        private void ToolBarLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.InitializeToolBar(sender);
        }

        private void UpdateScope([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            UpdateScope();
        }

        private void UpdateScope()
        {
            GoBack.IsEnabled = ItemTreeView.CanGoBack;
            GoForward.IsEnabled = ItemTreeView.CanGoForward;

            // this.Home.IsEnabled = this.ItemTreeView.CanGoBack;
        }

        private void UpdateScopeButtons()
        {
            var visibility = AppHost.Features.IsEnabled("Scopes") ? Visibility.Visible : Visibility.Collapsed;

            Home.Visibility = visibility;
            GoBack.Visibility = visibility;
            GoForward.Visibility = visibility;
        }
    }
}
