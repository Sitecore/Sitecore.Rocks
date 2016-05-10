// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.LayoutDesigners.Items;
using Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Models;
using Sitecore.Rocks.UI.LayoutDesigners.Properties.PlaceHolders;

namespace Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Items
{
    public class DeviceTreeViewItem : LayoutTreeViewItemBase
    {
        public DeviceTreeViewItem([NotNull] PageTreeViewItem pageTreeViewItem, [NotNull] DeviceModel device)
        {
            Assert.ArgumentNotNull(pageTreeViewItem, nameof(pageTreeViewItem));
            Assert.ArgumentNotNull(device, nameof(device));

            PageTreeViewItem = pageTreeViewItem;
            Device = device;
            Text = device.DeviceName + " : Device";
            Icon = device.Icon;
            DataContext = this;

            Device.PropertyChanged += HandlePropertyChanged;

            Loaded += ControlLoaded;
        }

        [NotNull]
        public DeviceModel Device { get; }

        [NotNull]
        public PageTreeViewItem PageTreeViewItem { get; private set; }

        [NotNull]
        public RenderingTreeViewItem AddRendering([NotNull] PlaceHolderTreeViewItem placeHolderTreeViewItem, [NotNull] IItem item, int treeViewIndex, int renderingIndex)
        {
            Assert.ArgumentNotNull(placeHolderTreeViewItem, nameof(placeHolderTreeViewItem));
            Assert.ArgumentNotNull(item, nameof(item));

            var rendering = new RenderingItem(Device, item)
            {
                PlaceholderKey = new PlaceHolderKey(placeHolderTreeViewItem.PlaceHolderName)
            };

            return AddRendering(placeHolderTreeViewItem, rendering, treeViewIndex, renderingIndex);
        }

        [NotNull]
        public RenderingTreeViewItem AddRendering([NotNull] PlaceHolderTreeViewItem placeHolderTreeViewItem, [NotNull] RenderingItem rendering, int treeViewIndex, int renderingIndex)
        {
            Assert.ArgumentNotNull(placeHolderTreeViewItem, nameof(placeHolderTreeViewItem));
            Assert.ArgumentNotNull(rendering, nameof(rendering));

            var renderingTreeViewItem = new RenderingTreeViewItem(this, rendering);

            return AddRendering(placeHolderTreeViewItem, renderingTreeViewItem, treeViewIndex, renderingIndex);
        }

        [NotNull]
        public RenderingTreeViewItem AddRendering([NotNull] PlaceHolderTreeViewItem placeHolderTreeViewItem, [NotNull] RenderingTreeViewItem renderingTreeViewItem, int treeViewIndex, int renderingIndex)
        {
            Assert.ArgumentNotNull(placeHolderTreeViewItem, nameof(placeHolderTreeViewItem));
            Assert.ArgumentNotNull(renderingTreeViewItem, nameof(renderingTreeViewItem));

            Action completed = delegate
            {
                foreach (var placeHolder in renderingTreeViewItem.Rendering.GetPlaceHolderNames())
                {
                    var p = new PlaceHolderTreeViewItem(this, placeHolder)
                    {
                        IsExpanded = AppHost.Settings.GetBool(LayoutTreeView.LayoutDesignerTreeviewPlaceholders, placeHolder, true)
                    };

                    renderingTreeViewItem.Items.Add(p);
                }
            };

            renderingTreeViewItem.Rendering.GetRenderingParameters(renderingTreeViewItem.Rendering.ItemUri, completed);
            renderingTreeViewItem.Rendering.Modified += (sender, args) => Device.PageModel.RaiseModified();

            if (treeViewIndex < 0 || treeViewIndex >= placeHolderTreeViewItem.Items.Count)
            {
                placeHolderTreeViewItem.Items.Add(renderingTreeViewItem);
            }
            else
            {
                // weird WPF workd-around - renderingTreeViewItem should never have a parent
                if (renderingTreeViewItem.Parent != null)
                {
                    renderingTreeViewItem.Remove();
                }

                placeHolderTreeViewItem.Items.Insert(treeViewIndex, renderingTreeViewItem);
            }

            if (renderingIndex < 0 || treeViewIndex >= Device.Renderings.Count)
            {
                Device.Renderings.Add(renderingTreeViewItem.Rendering);
            }
            else
            {
                Device.Renderings.Insert(renderingIndex, renderingTreeViewItem.Rendering);
            }

            return renderingTreeViewItem;
        }

        [CanBeNull]
        public PlaceHolderTreeViewItem FindPlaceHolderTreeViewItem([NotNull] string placeHolderName)
        {
            Assert.ArgumentNotNull(placeHolderName, nameof(placeHolderName));

            return FindItem<PlaceHolderTreeViewItem>(Items, i => i.PlaceHolderName == placeHolderName);
        }

        public override bool GetChildren(GetChildrenDelegate callback, bool async)
        {
            Assert.ArgumentNotNull(callback, nameof(callback));

            callback(Enumerable.Empty<BaseTreeViewItem>());

            return true;
        }

        public void RenderRenderings()
        {
            Clear();

            var placeHolders = Device.LayoutPlaceHolders.Split(',');
            var usedRenderings = new List<RenderingItem>();

            foreach (var name in placeHolders)
            {
                var placeHolderName = name.Trim();
                if (string.IsNullOrEmpty(placeHolderName))
                {
                    continue;
                }

                var placeHolderTreeViewItem = new PlaceHolderTreeViewItem(this, placeHolderName);
                placeHolderTreeViewItem.IsExpanded = AppHost.Settings.GetBool(LayoutTreeView.LayoutDesignerTreeviewPlaceholders, placeHolderTreeViewItem.Text, placeHolderName == "Page.Body");

                Items.Add(placeHolderTreeViewItem);

                var placeHolderPath = "/" + placeHolderName;

                RenderRenderings(placeHolderTreeViewItem.Items, placeHolderPath, placeHolderName, usedRenderings);
            }

            var unusedRenderings = Device.Renderings.Where(rendering => !usedRenderings.Contains(rendering)).ToList();
            RenderUnusedRenderings(unusedRenderings);
        }

        public void RenderUnusedRenderings([NotNull] List<RenderingItem> unusedRenderings)
        {
            Assert.ArgumentNotNull(unusedRenderings, nameof(unusedRenderings));

            foreach (var rendering in unusedRenderings)
            {
                var placeHolderName = rendering.PlaceholderKey.ToString();

                var placeHolderTreeViewItem = FindPlaceHolderTreeViewItem(placeHolderName) as BaseTreeViewItem;
                if (placeHolderTreeViewItem == null)
                {
                    placeHolderTreeViewItem = new TempPlaceHolderTreeViewItem(this, placeHolderName)
                    {
                        IsExpanded = true
                    };
                    Items.Add(placeHolderTreeViewItem);
                }

                var renderingTreeViewItem = new RenderingTreeViewItem(this, rendering);

                placeHolderTreeViewItem.Items.Add(renderingTreeViewItem);
            }
        }

        public override void Unload()
        {
            Device.PropertyChanged -= HandlePropertyChanged;
        }

        protected override void OnCollapsed(RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));

            base.OnCollapsed(e);

            AppHost.Settings.SetBool(LayoutTreeView.LayoutDesignerTreeviewDevices, Text, false);
        }

        protected override void OnExpanded(RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));

            base.OnExpanded(e);

            AppHost.Settings.SetBool(LayoutTreeView.LayoutDesignerTreeviewDevices, Text, true);
        }

        private void Clear()
        {
            Unload(Items);
            Items.Clear();
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            var value = TryFindResource(@"DeviceTreeViewItem") as Style;
            if (value != null)
            {
                Style = value;
            }
        }

        [CanBeNull]
        private T FindItem<T>([NotNull] ItemCollection items, [NotNull] Func<T, bool> predicate) where T : BaseTreeViewItem
        {
            Debug.ArgumentNotNull(items, nameof(items));
            Debug.ArgumentNotNull(predicate, nameof(predicate));

            foreach (var baseTreeViewItem in items.OfType<BaseTreeViewItem>())
            {
                var t = baseTreeViewItem as T;
                if (t != null)
                {
                    if (predicate(t))
                    {
                        return baseTreeViewItem as T;
                    }
                }

                var result = FindItem(baseTreeViewItem.Items, predicate);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private void HandlePropertyChanged([NotNull] object sender, [NotNull] PropertyChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.PropertyName == "LayoutId")
            {
                TrackLayout();
            }
        }

        private void RenderRenderings([NotNull] ItemCollection items, [NotNull] string placeHolderPath, [NotNull] string placeHolderName, [NotNull] List<RenderingItem> usedRenderings)
        {
            Debug.ArgumentNotNull(items, nameof(items));
            Debug.ArgumentNotNull(placeHolderPath, nameof(placeHolderPath));
            Debug.ArgumentNotNull(placeHolderName, nameof(placeHolderName));
            Debug.ArgumentNotNull(usedRenderings, nameof(usedRenderings));

            foreach (var rendering in Device.Renderings)
            {
                var key = rendering.PlaceholderKey.Key;
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                if (string.Compare(key, placeHolderName, StringComparison.InvariantCultureIgnoreCase) != 0 && string.Compare(key, placeHolderPath, StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    continue;
                }

                var renderingTreeViewItem = new RenderingTreeViewItem(this, rendering);
                renderingTreeViewItem.IsExpanded = AppHost.Settings.GetBool(LayoutTreeView.LayoutDesignerTreeviewRenderings, renderingTreeViewItem.Rendering.ItemId, true);

                items.Add(renderingTreeViewItem);

                usedRenderings.Add(rendering);

                foreach (var placeHolder in rendering.GetPlaceHolderNames())
                {
                    var placeHolderTreeViewItem = new PlaceHolderTreeViewItem(this, placeHolder);
                    placeHolderTreeViewItem.IsExpanded = AppHost.Settings.GetBool(LayoutTreeView.LayoutDesignerTreeviewPlaceholders, placeHolderTreeViewItem.Text, true);

                    renderingTreeViewItem.Items.Add(placeHolderTreeViewItem);

                    RenderRenderings(placeHolderTreeViewItem.Items, placeHolderPath + "/" + placeHolder, placeHolder, usedRenderings);
                }
            }
        }

        private void TrackLayout()
        {
            Site.RequestCompleted completed = delegate(string response)
            {
                Device.LayoutPlaceHolders = response;
                RenderRenderings();
            };

            Device.PageModel.DatabaseUri.Site.Execute("Layouts.GetLayoutPlaceHolders", completed, Device.PageModel.DatabaseUri.DatabaseName.ToString(), Device.LayoutId);
        }

        private void Unload([NotNull] ItemCollection items)
        {
            Debug.ArgumentNotNull(items, nameof(items));

            foreach (var item in items.OfType<BaseTreeViewItem>())
            {
                var appBuilderTreeViewItem = item as LayoutTreeViewItemBase;
                if (appBuilderTreeViewItem != null)
                {
                    appBuilderTreeViewItem.Unload();
                }

                Unload(item.Items);
            }
        }
    }
}
