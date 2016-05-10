// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Libraries.ContentTrees.VirtualItems
{
    public class LibrariesRootTreeViewItem : BaseTreeViewItem
    {
        private const string RegistryKey = "RootExpanded";

        private const string RegistryPath = "ContentTree\\Libraries";

        public LibrariesRootTreeViewItem()
        {
            Text = "Libraries";
            Margin = new Thickness(0, 16, 0, 0);
            Icon = new Icon("Resources/16x16/Folder-Copy.png");
            ToolTip = "Libraries";

            Visibility = AppHost.Settings.GetBool("SitecoreExplorer", "Libraries", true) ? Visibility.Visible : Visibility.Collapsed;

            Notifications.RegisterSystemEvent(this, HandleSettingsChanged);

            Loaded += ControlLoaded;
            Expanded += SetExpanderState;
            Collapsed += SetExpanderState;
        }

        public override bool GetChildren(GetChildrenDelegate callback, bool async)
        {
            Assert.ArgumentNotNull(callback, nameof(callback));

            var result = new List<BaseTreeViewItem>();

            var libraries = LibraryManager.Libraries;

            foreach (var library in libraries)
            {
                var libraryTreeViewItem = new LibraryTreeViewItem(library);
                libraryTreeViewItem.MakeExpandable();

                result.Add(libraryTreeViewItem);
            }

            callback(result);

            return true;
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs routedEventArgs)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(routedEventArgs, nameof(routedEventArgs));

            Loaded -= ControlLoaded;

            IsExpanded = GetExpanderState();

            var collection = LibraryManager.Libraries as ObservableCollection<LibraryBase>;
            if (collection != null)
            {
                collection.CollectionChanged += (s, args) => Refresh();
            }
        }

        private static bool GetExpanderState()
        {
            return (string)AppHost.Settings.Get(RegistryPath, RegistryKey, @"1") == @"1";
        }

        private void HandleSettingsChanged(string path, string key)
        {
            if (path == "SitecoreExplorer" && key == "Libraries")
            {
                Visibility = AppHost.Settings.GetBool("SitecoreExplorer", "Libraries", true) ? Visibility.Visible : Visibility.Collapsed;
            }
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
