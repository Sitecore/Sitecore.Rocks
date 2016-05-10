// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Libraries.ContentTrees.VirtualItems
{
    public class LibraryTreeViewItem : BaseTreeViewItem
    {
        private const string RegistryPath = "ContentTree\\Libraries";

        private bool firstTime = true;

        public LibraryTreeViewItem([NotNull] LibraryBase library)
        {
            Library = library;

            Text = library.Name;
            Margin = new Thickness(0, 2, 0, 0);
            ToolTip = library.Name + " Library";
            Icon = library.Icon;

            Loaded += ControlLoaded;
            Expanded += SetExpanderState;
            Collapsed += SetExpanderState;

            library.Items.CollectionChanged += (sender, args) => Refresh();
            library.PropertyChanged += HandlePropertyChanged;
        }

        [NotNull]
        public LibraryBase Library { get; }

        public override bool GetChildren(GetChildrenDelegate callback, bool async)
        {
            Assert.ArgumentNotNull(callback, nameof(callback));

            var result = new List<BaseTreeViewItem>();

            foreach (var item in Library.Items)
            {
                var libraryItemTreeViewItem = new LibraryItemTreeViewItem(item);
                result.Add(libraryItemTreeViewItem);
            }

            callback(result);

            return true;
        }

        protected override void Expand(bool async)
        {
            var dynamicFolder = Library as IDynamicLibrary;
            if (dynamicFolder != null && firstTime)
            {
                firstTime = false;
                dynamicFolder.Refresh();
                return;
            }

            base.Expand(async);
        }

        protected override bool Renamed(string newName)
        {
            LibraryManager.Rename(Library, newName);
            return true;
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs routedEventArgs)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(routedEventArgs, nameof(routedEventArgs));

            Loaded -= ControlLoaded;

            if (Library is IDynamicLibrary)
            {
                return;
            }

            IsExpanded = GetExpanderState();
        }

        private bool GetExpanderState()
        {
            return (string)AppHost.Settings.Get(RegistryPath, Library.Name, @"1") == @"1";
        }

        private void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name")
            {
                Text = Library.Name;
            }
        }

        private void SetExpanderState([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (Library is IDynamicLibrary)
            {
                return;
            }

            AppHost.Settings.Set(RegistryPath, Library.Name, IsExpanded ? @"1" : @"0");
            e.Handled = true;
        }
    }
}
