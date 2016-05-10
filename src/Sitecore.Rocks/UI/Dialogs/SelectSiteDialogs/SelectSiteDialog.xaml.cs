// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.Sites.Connections;

namespace Sitecore.Rocks.UI.Dialogs.SelectSiteDialogs
{
    public partial class SelectSiteDialog
    {
        public SelectSiteDialog()
        {
            InitializeComponent();
            this.InitializeDialog();

            PreselectedHostName = string.Empty;
            PreselectedUserName = string.Empty;

            Loaded += ControlLoaded;
        }

        [NotNull]
        public string LabelText
        {
            get { return Label.Content as string ?? string.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                Label.Content = value;
            }
        }

        public bool ShowNewConnectionButton
        {
            get { return NewConnectionButton.Visibility == Visibility.Visible; }

            set { NewConnectionButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
        }

        [CanBeNull]
        public Site Site { get; set; }

        [NotNull]
        protected string PreselectedHostName { get; set; }

        [NotNull]
        protected string PreselectedUserName { get; set; }

        public void Select([NotNull] string hostName, [NotNull] string userName)
        {
            Assert.ArgumentNotNull(hostName, nameof(hostName));
            Assert.ArgumentNotNull(userName, nameof(userName));

            /*
      if (string.IsNullOrEmpty(userName))
      {
        userName = @"sitecore\admin";
      }

      this.PreselectedHostName = hostName;
      this.PreselectedUserName = userName;

      if (!string.IsNullOrEmpty(hostName))
      {
        this.Title = VisualStudio.Resources.SitePicker_Select_Select_Site + @" - " + hostName;
      }

      ListBoxItem l = null;

      foreach (var item in this.Sites.Items)
      {
        var listBoxItem = item as ListBoxItem;
        if (listBoxItem == null)
        {
          continue;
        }

        var site = listBoxItem.Tag as Site;
        if (site == null)
        {
          continue;
        }

        if (string.Compare(site.HostName, hostName, StringComparison.InvariantCultureIgnoreCase) == 0 && string.Compare(site.UserName, userName, StringComparison.InvariantCultureIgnoreCase) == 0)
        {
          listBoxItem.IsSelected = true;
          return;
        }

        if (string.Compare(site.HostName, hostName, StringComparison.InvariantCultureIgnoreCase) == 0)
        {
          l = listBoxItem;
        }
      }

      if (l != null)
      {
        l.IsSelected = true;
      }
      */
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            LoadSites();
            EnableButtons();
        }

        private void EnableButtons()
        {
            OkButton.IsEnabled = TreeView.SelectedItem as SiteTreeViewItem != null;
        }

        private void EnableButtons([NotNull] object sender, [NotNull] RoutedPropertyChangedEventArgs<object> e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            EnableButtons();
        }

        private void GetChildren([NotNull] object sender, [NotNull] BaseTreeViewItem baseTreeViewItem, [NotNull] List<BaseTreeViewItem> children)
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
                e.Handled = true;
                this.Close(false);
                return;
            }

            if (e.Key == Key.Enter)
            {
                OkClicked(sender, e);
                e.Handled = true;
            }
        }

        private void HandleMouseDoubleClick([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var siteTreeViewItem = TreeView.SelectedItem as SiteTreeViewItem;
            if (siteTreeViewItem != null)
            {
                OkClicked(sender, e);
                e.Handled = true;
            }
        }

        private void LoadSites()
        {
            TreeView.Items.Clear();

            var folder = ConnectionManager.GetConnectionFolder();

            var item = new ConnectionFolderTreeViewItem(folder);

            item.MakeExpandable();

            TreeView.Items.Add(item);

            item.ExpandAndWait();

            LocateSite();
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

        private void NewConnection([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var site = SiteManager.NewConnection(PreselectedHostName, PreselectedUserName);
            if (site == null)
            {
                return;
            }

            Site = site;

            LoadSites();

            LocateSite();
        }

        private void OkClicked([NotNull] object sender, [NotNull] RoutedEventArgs routedEventArgs)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(routedEventArgs, nameof(routedEventArgs));

            var siteTreeViewItem = TreeView.SelectedItem as SiteTreeViewItem;
            if (siteTreeViewItem == null)
            {
                AppHost.MessageBox("Please select a site.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Site = siteTreeViewItem.Site;

            this.Close(true);
        }
    }
}
