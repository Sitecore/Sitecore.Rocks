// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Searching;

namespace Sitecore.Rocks.ContentTrees.VirtualItems.Searches
{
    public class SearchRootTreeViewItem : BaseTreeViewItem
    {
        private const string RegistryKey = "Expanded";

        private const string RegistryPath = "ContentTree\\Searches";

        public SearchRootTreeViewItem()
        {
            Text = Rocks.Resources.SearchRootTreeViewItem_SearchRootTreeViewItem_Searches;
            Icon = new Icon("Resources/16x16/search.png");
            ToolTip = "Searches";

            SearchManager.SavedSearches.CollectionChanged += SavedSearchesChanged;

            Loaded += ControlLoaded;
            Expanded += SetExpanderState;
            Collapsed += SetExpanderState;
        }

        public void Closing()
        {
            SearchManager.SavedSearches.CollectionChanged -= SavedSearchesChanged;
        }

        public override bool GetChildren([NotNull] GetChildrenDelegate callback, bool async)
        {
            Assert.ArgumentNotNull(callback, nameof(callback));

            var result = new List<BaseTreeViewItem>();

            var savedSearches = SearchManager.SavedSearches;

            foreach (var savedSearch in savedSearches)
            {
                var item = new SearchTreeViewItem();
                item.Initialize(savedSearch);

                result.Add(item);
            }

            callback(result);

            return true;
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            IsExpanded = GetExpanderState();
        }

        private static bool GetExpanderState()
        {
            return (string)AppHost.Settings.Get(RegistryPath, RegistryKey, @"1") == @"1";
        }

        private void SavedSearchesChanged([NotNull] object sender, [NotNull] NotifyCollectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Refresh();
        }

        private void SetExpanderState([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            AppHost.Settings.Set(RegistryPath, RegistryKey, IsExpanded ? @"1" : @"0");
            e.Handled = true;
        }
    }
}
