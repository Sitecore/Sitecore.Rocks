// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.Sites.Connections;

namespace Sitecore.Rocks.Controls
{
    public partial class SiteSelector
    {
        private bool _isLoaded;

        private Site _site;

        public SiteSelector()
        {
            InitializeComponent();

            Site = AppHost.Settings.ActiveDatabaseUri.Site;
        }

        [CanBeNull]
        public Site Site
        {
            get { return _site; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                if (value == _site)
                {
                    return;
                }

                _site = value;

                if (Site == Site.Empty)
                {
                    SiteTextBlock.Text = "<None>";
                }
                else
                {
                    SiteTextBlock.Text = _site.Name;
                }

                var selectionChanged = SelectionChanged;
                if (selectionChanged != null)
                {
                    selectionChanged(this, Site);
                }
            }
        }

        public event Notifications.SiteEventHandler SelectionChanged;

        private void GetChildren(object sender, BaseTreeViewItem baseTreeViewItem, List<BaseTreeViewItem> children)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(baseTreeViewItem, nameof(baseTreeViewItem));
            Debug.ArgumentNotNull(children, nameof(children));

            var siteTreeViewItem = baseTreeViewItem as SiteTreeViewItem;
            if (siteTreeViewItem != null)
            {
                children.Clear();
            }
        }

        private void HandleKeyDown([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.Key == Key.Escape)
            {
                Popup.IsOpen = false;
                e.Handled = true;
            }
        }

        private void LoadTreeView([NotNull] object sender, [NotNull] RoutedEventArgs routedEventArgs)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(routedEventArgs, nameof(routedEventArgs));

            TreeView.Loaded -= LoadTreeView;

            TreeView.Items.Clear();

            var folder = ConnectionManager.GetConnectionFolder();

            var item = new ConnectionFolderTreeViewItem(folder);

            TreeView.Items.Add(item);
            item.MakeExpandable();
            item.IsExpanded = true;

            LocateSite();

            _isLoaded = true;
        }

        private void LocateSite()
        {
            if (Site == null || Site == Site.Empty)
            {
                return;
            }

            var siteTreeViewItem = TreeView.FindSiteTreeViewItem(Site);
            if (siteTreeViewItem == null)
            {
                return;
            }

            siteTreeViewItem.BringIntoView();
            siteTreeViewItem.IsSelected = true;
            siteTreeViewItem.IsItemSelected = true;
            Keyboard.Focus(siteTreeViewItem);
        }

        private void OpenDropDown(object sender, RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (!_isLoaded)
            {
                TreeView.Loaded += LoadTreeView;
            }

            Popup.IsOpen = true;
        }

        private void SetSelection([NotNull] object sender, [NotNull] RoutedPropertyChangedEventArgs<object> e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var item = TreeView.SelectedItem as SiteTreeViewItem;
            if (item == null)
            {
                return;
            }

            Site = item.Site;
            Popup.IsOpen = false;
        }
    }
}
