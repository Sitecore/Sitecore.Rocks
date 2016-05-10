// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentEditors;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI
{
    public enum Focused
    {
        None,

        ContentTree,

        ContentEditor
    }

    public static class ActiveContext
    {
        private static ContentEditor activeContentEditor;

        private static ContentTree activeContentTree;

        private static Focused focused;

        [CanBeNull]
        public static ContentEditor ActiveContentEditor
        {
            get { return activeContentEditor; }

            set
            {
                if (value == activeContentEditor)
                {
                    return;
                }

                activeContentEditor = value;

                RaiseActiveContentEditorChanged();
                RaiseContentModelChanged();
            }
        }

        [CanBeNull]
        public static ContentTree ActiveContentTree
        {
            get { return activeContentTree; }

            set
            {
                if (value == activeContentTree)
                {
                    return;
                }

                activeContentTree = value;

                RaiseActiveContentTreeChanged();
                RaiseSelectedItemsChanged();
            }
        }

        [NotNull]
        public static ContentModel ContentModel
        {
            get
            {
                if (ActiveContentEditor == null)
                {
                    throw Exceptions.InvalidOperation(Resources.ActiveContext_ContentModel_Active_Content_Editor_is_null_);
                }

                return ActiveContentEditor.ContentModel;
            }
        }

        public static Focused Focused
        {
            get { return focused; }

            set
            {
                if (focused == value)
                {
                    return;
                }

                focused = value;
                RaiseFocusedChanged();
            }
        }

        [NotNull]
        public static List<BaseTreeViewItem> SelectedItems
        {
            get
            {
                if (ActiveContentTree == null)
                {
                    throw Exceptions.InvalidOperation(Resources.ActiveContext_SelectedItems_Active_Content_Tree_is_null_);
                }

                return ActiveContentTree.ItemTreeView.SelectedItems;
            }
        }

        public static event EventHandler ActiveContentEditorChanged;

        public static event EventHandler ActiveContentTreeChanged;

        public static event EventHandler ContentModelChanged;

        public static event EventHandler FocusedChanged;

        [CanBeNull]
        public static DatabaseUri GetActiveDatabase()
        {
            switch (Focused)
            {
                case Focused.ContentTree:
                    return GetContentTreeDatabase();

                case Focused.ContentEditor:
                    return GetContentEditorDatabase();

                default:
                    return null;
            }
        }

        [NotNull]
        public static IEnumerable<ItemUri> GetActiveItems()
        {
            switch (Focused)
            {
                case Focused.ContentTree:
                    return GetContentTreeItems();

                case Focused.ContentEditor:
                    return GetContentEditorItems();

                default:
                    return Enumerable.Empty<ItemUri>();
            }
        }

        [CanBeNull]
        public static Site GetActiveSite()
        {
            switch (Focused)
            {
                case Focused.ContentTree:
                    return GetContentTreeSite();

                case Focused.ContentEditor:
                    return GetContentEditorSite();

                default:
                    return null;
            }
        }

        public static void RaiseContentModelChanged()
        {
            var changed = ContentModelChanged;
            if (changed != null)
            {
                changed(null, EventArgs.Empty);
            }
        }

        public static void RaiseSelectedItemsChanged()
        {
            var changed = SelectedItemsChanged;
            if (changed != null)
            {
                changed(null, EventArgs.Empty);
            }
        }

        public static event EventHandler SelectedItemsChanged;

        [CanBeNull]
        private static DatabaseUri GetContentEditorDatabase()
        {
            var contentEditor = ActiveContentEditor;
            if (contentEditor == null)
            {
                return null;
            }

            var contentModel = contentEditor.ContentModel;
            if (contentModel.IsEmpty)
            {
                return null;
            }

            return contentModel.FirstItem.Uri.ItemUri.DatabaseUri;
        }

        [NotNull]
        private static IEnumerable<ItemUri> GetContentEditorItems()
        {
            if (ActiveContentEditor == null)
            {
                yield break;
            }

            foreach (var item in ContentModel.Items)
            {
                yield return item.Uri.ItemUri;
            }
        }

        [CanBeNull]
        private static Site GetContentEditorSite()
        {
            var contentEditor = ActiveContentEditor;
            if (contentEditor == null)
            {
                return null;
            }

            var contentModel = contentEditor.ContentModel;
            if (contentModel.IsEmpty)
            {
                return null;
            }

            return contentModel.FirstItem.Uri.Site;
        }

        [CanBeNull]
        private static DatabaseUri GetContentTreeDatabase()
        {
            var contentTree = ActiveContentTree;
            if (contentTree == null)
            {
                return null;
            }

            var items = SelectedItems;
            foreach (var item in items)
            {
                var baseSiteTreeViewItem = item as IItemUri;
                if (baseSiteTreeViewItem != null)
                {
                    return baseSiteTreeViewItem.ItemUri.DatabaseUri;
                }
            }

            return null;
        }

        [NotNull]
        private static IEnumerable<ItemUri> GetContentTreeItems()
        {
            if (ActiveContentTree == null)
            {
                return Enumerable.Empty<ItemUri>();
            }

            var result = new List<ItemUri>();

            foreach (var baseTreeViewItem in SelectedItems)
            {
                var item = baseTreeViewItem as ItemTreeViewItem;
                if (item == null)
                {
                    return Enumerable.Empty<ItemUri>();
                }

                result.Add(item.ItemUri);
            }

            return result;
        }

        [CanBeNull]
        private static Site GetContentTreeSite()
        {
            var contentTree = ActiveContentTree;
            if (contentTree == null)
            {
                return null;
            }

            var items = SelectedItems;
            foreach (var item in items)
            {
                var baseSiteTreeViewItem = item as BaseSiteTreeViewItem;
                if (baseSiteTreeViewItem != null)
                {
                    return baseSiteTreeViewItem.Site;
                }
            }

            return null;
        }

        private static void RaiseActiveContentEditorChanged()
        {
            var changed = ActiveContentEditorChanged;
            if (changed != null)
            {
                changed(null, EventArgs.Empty);
            }
        }

        private static void RaiseActiveContentTreeChanged()
        {
            var changed = ActiveContentTreeChanged;
            if (changed != null)
            {
                changed(null, EventArgs.Empty);
            }
        }

        private static void RaiseFocusedChanged()
        {
            var changed = FocusedChanged;
            if (changed != null)
            {
                changed(null, EventArgs.Empty);
            }
        }
    }
}
