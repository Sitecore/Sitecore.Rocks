// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Searching;

namespace Sitecore.Rocks.ContentTrees.VirtualItems.Searches
{
    public class SearchTreeViewItem : BaseTreeViewItem, ICanDelete, ICanDrag, ICanDrop
    {
        private const string DragIdentifier = "SitecoreSavedSearches";

        private readonly ControlDragAdorner adorner;

        public SearchTreeViewItem()
        {
            Icon = new Icon("Resources/16x16/search.png");

            MouseDoubleClick += HandleDoubleClick;

            adorner = new ControlDragAdorner(ItemHeader, ControlDragAdornerPosition.All);
        }

        [NotNull]
        public SavedSearch SavedSearch { get; private set; }

        public void Delete(bool deleteFiles)
        {
            SearchManager.Delete(SavedSearch);
            SearchManager.Save();

            var parent = Parent as TreeViewItem;
            if (parent != null)
            {
                parent.Items.Remove(this);
            }
        }

        public override bool GetChildren(GetChildrenDelegate callback, bool async)
        {
            Assert.ArgumentNotNull(callback, nameof(callback));

            callback(Enumerable.Empty<BaseTreeViewItem>());

            return true;
        }

        public void Initialize([NotNull] SavedSearch savedSearch)
        {
            Assert.ArgumentNotNull(savedSearch, nameof(savedSearch));

            SavedSearch = savedSearch;
            Text = savedSearch.Name;

            ToolTip = savedSearch.QueryText;
        }

        private void DropSavedSearches([NotNull] List<BaseTreeViewItem> items)
        {
            Debug.ArgumentNotNull(items, nameof(items));

            var searches = SearchManager.SavedSearches;
            var itemList = new List<BaseTreeViewItem>(items);

            var inserts = new List<SavedSearch>();
            foreach (var item in itemList)
            {
                var searchTreeViewItem = (SearchTreeViewItem)item;

                inserts.Add(searchTreeViewItem.SavedSearch);

                searches.Remove(searchTreeViewItem.SavedSearch);
            }

            inserts.Reverse();

            var index = searches.IndexOf(SavedSearch);
            if (adorner.LastPosition == ControlDragAdornerPosition.Bottom)
            {
                index++;
            }

            foreach (var search in inserts)
            {
                searches.Insert(index, search);
            }

            SearchManager.Save();
        }

        string ICanDrag.GetDragIdentifier()
        {
            return DragIdentifier;
        }

        private void HandleDoubleClick([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var searchViewer = AppHost.Windows.Factory.OpenSearchViewer(SavedSearch.Site);
            if (searchViewer == null)
            {
                return;
            }

            searchViewer.Search(SavedSearch.Field, SavedSearch.QueryText);

            e.Handled = true;
        }

        void ICanDrop.HandleDragOver(object sender, DragEventArgs e, BaseTreeViewItem treeViewItem)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));
            Debug.ArgumentNotNull(treeViewItem, nameof(treeViewItem));

            adorner.AllowedPositions = ControlDragAdornerPosition.None;

            if (!e.Data.GetDataPresent(DragIdentifier))
            {
                return;
            }

            e.Effects = DragDropEffects.Move;
            adorner.AllowedPositions = ControlDragAdornerPosition.Top | ControlDragAdornerPosition.Bottom;
        }

        void ICanDrop.HandleDrop(object sender, DragEventArgs e, BaseTreeViewItem treeViewItem)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));
            Debug.ArgumentNotNull(treeViewItem, nameof(treeViewItem));

            if (e.Data.GetDataPresent(DragIdentifier))
            {
                var items = (List<BaseTreeViewItem>)e.Data.GetData(DragIdentifier);
                DropSavedSearches(items);
            }

            e.Handled = true;
        }
    }
}
