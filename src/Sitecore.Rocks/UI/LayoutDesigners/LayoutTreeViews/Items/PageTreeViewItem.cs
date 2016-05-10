// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Models;

namespace Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Items
{
    public class PageTreeViewItem : LayoutTreeViewItemBase
    {
        public PageTreeViewItem([NotNull] LayoutTreeView layoutTreeView, [NotNull] PageModel page)
        {
            Assert.ArgumentNotNull(layoutTreeView, nameof(layoutTreeView));
            Assert.ArgumentNotNull(page, nameof(page));

            LayoutTreeView = layoutTreeView;
            PageModel = page;
            Text = "Layout";
            Icon = new Icon("/Resources/16x16/View-Web-Layout.png");

            Loaded += ControlLoaded;
            PageModel.PropertyChanged += HandlePropertyChanged;

            RenderLayout();

            IsExpanded = true;
        }

        [NotNull]
        public LayoutTreeView LayoutTreeView { get; private set; }

        [NotNull]
        public PageModel PageModel { get; }

        public override bool GetChildren(GetChildrenDelegate callback, bool async)
        {
            Assert.ArgumentNotNull(callback, nameof(callback));

            return false;
        }

        public override void Unload()
        {
            PageModel.PropertyChanged -= HandlePropertyChanged;
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            var value = TryFindResource(@"PageTreeViewItem") as Style;
            if (value != null)
            {
                Style = value;
            }
        }

        private void HandlePropertyChanged([NotNull] object sender, [NotNull] PropertyChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.PropertyName == "Text")
            {
                Text = "Layout";
            }
        }

        private void RenderLayout()
        {
            Items.Clear();

            foreach (var device in PageModel.Devices)
            {
                var deviceTreeViewItem = new DeviceTreeViewItem(this, device);
                deviceTreeViewItem.IsExpanded = AppHost.Settings.GetBool(LayoutTreeView.LayoutDesignerTreeviewDevices, deviceTreeViewItem.Text, deviceTreeViewItem.Text == "Default");
                Items.Add(deviceTreeViewItem);

                deviceTreeViewItem.RenderRenderings();
            }
        }
    }
}
