// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DataServiceExtensions;

namespace Sitecore.Rocks.UI.Libraries.Dialogs
{
    public partial class SearchBuilder
    {
        public delegate void SelectionChangedEventHandler(object sender, IEnumerable<ItemHeader> items);

        private readonly List<ResultItem> items = new List<ResultItem>();

        public SearchBuilder()
        {
            InitializeComponent();

            Loaded += ControlLoaded;
        }

        [CanBeNull]
        public DatabaseUri DatabaseUri { get; set; }

        [NotNull]
        public IEnumerable<ITemplatedItem> SelectedItems
        {
            get { return ResultList.SelectedItems.OfType<ResultItem>().Select(i => i.ItemHeader); }
        }

        public SelectionMode SelectionMode
        {
            get { return ResultList.SelectionMode; }

            set { ResultList.SelectionMode = value; }
        }

        [CanBeNull]
        public string SettingsKey { get; set; }

        [CanBeNull]
        public string Text
        {
            get { return Code.Text; }

            set
            {
                if (!DesignerProperties.GetIsInDesignMode(this))
                {
                    Code.Text = value ?? string.Empty;
                }
            }
        }

        public event SelectionChangedEventHandler SelectionChanged;

        public event EventHandler TextChanged;

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            var settingsKey = SettingsKey;
            if (!string.IsNullOrEmpty(settingsKey))
            {
                Code.Text = AppHost.Settings.GetString("SearchBuilder", settingsKey, string.Empty);
            }
        }

        private void Evaluate([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Refresh();
        }

        private void HandleKeyDown([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.Key == Key.E && Keyboard.Modifiers == ModifierKeys.Control)
            {
                Refresh();
            }
        }

        private void RaiseSelectedItemsChanged()
        {
            var handler = SelectionChanged;
            if (handler != null)
            {
                handler(this, ResultList.SelectedItems.OfType<ResultItem>().Select(i => i.ItemHeader));
            }
        }

        private void RaiseTextChanged([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var textChanged = TextChanged;
            if (textChanged != null)
            {
                textChanged(this, e);
            }
        }

        private void Refresh()
        {
            var databaseUri = DatabaseUri;
            if (databaseUri == null || databaseUri == DatabaseUri.Empty)
            {
                AppHost.MessageBox("Please a database first.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var queryText = Text ?? string.Empty;
            if (string.IsNullOrEmpty(queryText))
            {
                return;
            }

            var settingsKey = SettingsKey;
            if (!string.IsNullOrEmpty(settingsKey))
            {
                AppHost.Settings.SetString("SearchBuilder", settingsKey, Code.Text);
            }

            GetItemsCompleted<ItemHeader> completed = delegate(IEnumerable<ItemHeader> itemHeaders)
            {
                items.Clear();

                foreach (var itemHeader in itemHeaders)
                {
                    items.Add(new ResultItem(itemHeader));
                }

                RenderResult();
            };

            Loading.ShowLoading(ResultList);

            databaseUri.Site.DataService.Search(queryText, databaseUri, string.Empty, string.Empty, ItemUri.Empty, 0, completed);
        }

        private void RenderResult()
        {
            ResultList.ItemsSource = null;
            ResultList.ItemsSource = items;

            var count = items.Count;
            SelectCount.Text = "Found " + count + (count == 1 ? " item" : " items");

            ResizeGridViewColumn(PathColumn);

            Loading.HideLoading(ResultList);
        }

        private void ResizeGridViewColumn([NotNull] GridViewColumn column)
        {
            Debug.ArgumentNotNull(column, nameof(column));

            if (double.IsNaN(column.Width))
            {
                column.Width = column.ActualWidth;
            }

            column.Width = double.NaN;
        }

        private void SetSelectedItems([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            RaiseSelectedItemsChanged();
        }

        public class ResultItem
        {
            public ResultItem([NotNull] ItemHeader itemHeader)
            {
                Assert.ArgumentNotNull(itemHeader, nameof(itemHeader));

                ItemHeader = itemHeader;
                Path = itemHeader.Path;
            }

            [NotNull]
            public ItemHeader ItemHeader { get; }

            [NotNull, UsedImplicitly]
            public string Path { get; private set; }
        }
    }
}
