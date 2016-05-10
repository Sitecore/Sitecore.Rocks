// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Controls.QueryBuilders
{
    public partial class QueryBuilder
    {
        private readonly List<ResultItem> _items = new List<ResultItem>();

        private DatabaseUri _databaseUri;

        public QueryBuilder()
        {
            InitializeComponent();
        }

        [CanBeNull]
        public DatabaseUri DatabaseUri
        {
            get { return _databaseUri; }

            set
            {
                _databaseUri = value;
                Site = _databaseUri == null ? null : _databaseUri.Site;
            }
        }

        [CanBeNull]
        public Site Site { get; set; }

        [NotNull]
        public string Text
        {
            get { return Editor.Text; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                Editor.Text = value;
            }
        }

        public CustomValidationType Type { get; set; }

        public event EventHandler TextChanged;

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

        private void ParseResult([NotNull] XElement root)
        {
            Debug.ArgumentNotNull(root, nameof(root));

            _items.Clear();

            foreach (var element in root.Elements())
            {
                var item = new ResultItem
                {
                    Path = element.Value
                };

                _items.Add(item);
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
            Site site;
            DatabaseName databaseName;

            if (Type == CustomValidationType.ExpandedWebConfig || Type == CustomValidationType.WebConfig)
            {
                site = Site;
                if (site == null || site == Site.Empty)
                {
                    AppHost.MessageBox("Please a site first.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                databaseName = DatabaseName.Empty;
            }
            else
            {
                var dbUri = DatabaseUri;
                if (dbUri == null || dbUri == DatabaseUri.Empty)
                {
                    AppHost.MessageBox("Please a database first.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                site = dbUri.Site;
                databaseName = dbUri.DatabaseName;
            }

            ExecuteCompleted c = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    Loading.HideLoading(ResultList);
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    Loading.HideLoading(ResultList);
                    return;
                }

                ParseResult(root);
                RenderResult();
            };

            Loading.ShowLoading(ResultList);

            site.DataService.ExecuteAsync("Validations.TestQuery", c, databaseName.ToString(), Editor.Text, (int)Type);
        }

        private void RenderResult()
        {
            ResultList.ItemsSource = null;
            ResultList.ItemsSource = _items;

            var count = _items.Count;
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

        public class ResultItem
        {
            [CanBeNull, UsedImplicitly]
            public string Path { get; set; }
        }
    }
}
