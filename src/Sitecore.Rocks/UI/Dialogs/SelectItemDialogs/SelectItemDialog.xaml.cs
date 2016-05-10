// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;
using Sitecore.Rocks.Extensions.DataServiceExtensions;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.UI.Dialogs.SelectItemDialogs.Panes;

namespace Sitecore.Rocks.UI.Dialogs.SelectItemDialogs
{
    public partial class SelectItemDialog
    {
        private DatabaseUri databaseUri;

        private bool isLoaded;

        private bool isLoading;

        private string selectedItemName;

        private List<string> selectedItemNames;

        private List<ITemplatedItem> selectedItems;

        private ItemId selectedItemTemplateId;

        private ItemUri selectedItemUri;

        public SelectItemDialog()
        {
            InitializeComponent();
            this.InitializeDialog();

            Panes = new List<ISelectDialogPane>();
            Title = "Select Item";
        }

        public bool AllowMultipleSelection { get; set; }

        [NotNull]
        public DatabaseUri DatabaseUri
        {
            get { return databaseUri; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));
                databaseUri = value;
            }
        }

        [NotNull]
        public string InitialItemPath { get; set; }

        [Obsolete("Use InitialItemPath instead"), NotNull]
        public string ItemPath
        {
            get { return InitialItemPath; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));
                InitialItemPath = value;
            }
        }

        [NotNull, Obsolete("Use SelectedItemUri instead")]
        public ItemUri SelectedItem
        {
            get { return SelectedItemUri; }
        }

        [NotNull]
        public string SelectedItemName
        {
            get { return selectedItemName; }
        }

        [NotNull]
        public IEnumerable<string> SelectedItemNames
        {
            get { return selectedItemNames; }
        }

        [CanBeNull]
        public ItemId SelectedItemTemplateId
        {
            get { return selectedItemTemplateId; }
        }

        [NotNull]
        public ItemUri SelectedItemUri
        {
            get { return selectedItemUri ?? ItemUri.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                selectedItemUri = value;

                DatabaseUri = selectedItemUri == ItemUri.Empty ? DatabaseUri.Empty : selectedItemUri.DatabaseUri;
            }
        }

        [CanBeNull]
        public string SettingsKey { get; set; }

        public bool ShowAllDatabases { get; set; }

        [NotNull, ImportMany(typeof(ISelectDialogPane))]
        protected List<ISelectDialogPane> Panes { get; set; }

        public void EnableButtons()
        {
            if (selectedItems == null)
            {
                OkButton.IsEnabled = false;
                return;
            }

            if (!AllowMultipleSelection && selectedItems.Count != 1)
            {
                OkButton.IsEnabled = false;
                return;
            }

            OkButton.IsEnabled = true;
        }

        public void GetSelectedItemPath([NotNull] Action<string> completed)
        {
            Assert.ArgumentNotNull(completed, nameof(completed));

            if (SelectedItemUri == ItemUri.Empty)
            {
                completed(string.Empty);
                return;
            }

            SelectedItemUri.Site.DataService.GetItemHeader(SelectedItemUri, itemHeader => completed(itemHeader.Path));
        }

        public void Initialize([NotNull] string title, [NotNull] ItemUri selectedItem)
        {
            Assert.ArgumentNotNull(title, nameof(title));
            Assert.ArgumentNotNull(selectedItem, nameof(selectedItem));

            Title = title;
            SelectedItemUri = selectedItem;
            DatabaseUri = selectedItem.DatabaseUri;
            InitialItemPath = string.Empty;

            Load();
        }

        public void Initialize([NotNull] string title, [NotNull] DatabaseUri databaseUri, [NotNull] string initialItemPath)
        {
            Assert.ArgumentNotNull(title, nameof(title));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(initialItemPath, nameof(initialItemPath));

            Title = title;
            SelectedItemUri = ItemUri.Empty;
            DatabaseUri = databaseUri;
            InitialItemPath = initialItemPath;

            Load();
        }

        public void Ok()
        {
            if (!string.IsNullOrEmpty(SettingsKey) && SelectedItemUri != ItemUri.Empty && DatabaseUri != DatabaseUri.Empty)
            {
                AppHost.Settings.SetString("SelectItemDialog\\" + SettingsKey, DatabaseUri.ToString(), SelectedItemUri.ItemId.ToString());
            }

            foreach (var item in Tabs.Items)
            {
                var tabItem = item as TabItem;
                if (tabItem == null)
                {
                    continue;
                }

                var pane = tabItem.Content as ISelectDialogPane;
                if (pane != null)
                {
                    pane.Close();
                }
            }

            this.Close(true);
        }

        public void SetDatabaseUri([NotNull] DatabaseUri databaseUri)
        {
            DatabaseUri = databaseUri;
            Load();
        }

        public void SetSelectedItems([NotNull] IEnumerable<ITemplatedItem> items)
        {
            Assert.ArgumentNotNull(items, nameof(items));

            selectedItems = items.ToList();
            selectedItemNames = selectedItems.Select(i => i.Name).ToList();

            var item = selectedItems.FirstOrDefault();
            if (item == null)
            {
                selectedItemName = string.Empty;
                selectedItemTemplateId = null;
                SelectedItemUri = ItemUri.Empty;
            }
            else
            {
                selectedItemName = item.Name;
                selectedItemTemplateId = item.TemplateId;
                SelectedItemUri = item.ItemUri;
            }

            EnableButtons();
        }

        public new bool ShowDialog()
        {
            if (!isLoaded)
            {
                Load();
            }

            return AppHost.Shell.ShowDialog(this) == true;
        }

        private void Load()
        {
            isLoaded = true;

            Panes.Clear();
            Tabs.Items.Clear();

            var selectedTab = AppHost.Settings.GetString("SelectItemDialog", "SelectedTab", string.Empty);
            AppHost.Extensibility.ComposeParts(this);

            isLoading = true;

            foreach (var pane in Panes)
            {
                pane.Initialize(this);

                var tabItem = new TabItem
                {
                    Header = pane.Header,
                    Content = pane
                };

                Tabs.Items.Add(tabItem);

                if (selectedTab == pane.GetType().FullName)
                {
                    Tabs.SelectedItem = tabItem;
                }
            }

            if (Tabs.SelectedItem == null)
            {
                Tabs.SelectedIndex = 0;
            }

            isLoading = false;

            SetActiveTab();
            EnableButtons();
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Ok();
        }

        private void SetActiveTab([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (!isLoading)
            {
                SetActiveTab();
            }
        }

        private void SetActiveTab()
        {
            var tabItem = Tabs.SelectedItem as TabItem;
            if (tabItem == null)
            {
                return;
            }

            var pane = tabItem.Content as ISelectDialogPane;
            if (pane == null)
            {
                return;
            }

            pane.SetActive();

            AppHost.Settings.Set("SelectItemDialog", "SelectedTab", pane.GetType().FullName);
        }
    }
}
