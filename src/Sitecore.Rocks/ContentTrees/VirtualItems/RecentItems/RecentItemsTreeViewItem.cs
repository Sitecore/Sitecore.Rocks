// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.VirtualItems.RecentItems
{
    public class RecentItemsTreeViewItem : BaseTreeViewItem
    {
        private const string RegistryKey = "Expanded";

        private const string RegistryPath = "ContentTree\\Items";

        public RecentItemsTreeViewItem()
        {
            Text = Rocks.Resources.RecentItemsTreeViewItem_RecentItemsTreeViewItem_Items;
            Margin = new Thickness(0, 16, 0, 16);
            ToolTip = "Recently opened items";

            Loaded += ControlLoaded;
            Expanded += SetExpanderState;
            Collapsed += SetExpanderState;
        }

        public override bool GetChildren(GetChildrenDelegate callback, bool async)
        {
            Assert.ArgumentNotNull(callback, nameof(callback));

            callback(Enumerable.Empty<BaseTreeViewItem>());

            return true;
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs routedEventArgs)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(routedEventArgs, nameof(routedEventArgs));

            Loaded -= ControlLoaded;

            Icon = new Icon("Resources/16x16/recent.png");
            IsExpanded = GetExpanderState();
        }

        private static bool GetExpanderState()
        {
            return (string)AppHost.Settings.Get(RegistryPath, RegistryKey, @"1") == @"1";
        }

        private void SetExpanderState([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AppHost.Settings.Set(RegistryPath, RegistryKey, IsExpanded ? @"1" : @"0");
            e.Handled = true;
        }
    }
}
