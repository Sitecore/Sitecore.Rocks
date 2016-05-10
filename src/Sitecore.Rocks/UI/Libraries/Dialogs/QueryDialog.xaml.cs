// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Controls.QueryBuilders;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.UI.Libraries.Dialogs
{
    public partial class QueryDialog
    {
        private bool isQueryNameVisible;

        public QueryDialog([NotNull] string query, [NotNull] DatabaseUri databaseUri, [NotNull] string queryName)
        {
            Assert.ArgumentNotNull(query, nameof(query));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(queryName, nameof(queryName));

            InitializeComponent();
            this.InitializeDialog();

            Loaded += ControlLoaded;

            if (databaseUri == DatabaseUri.Empty)
            {
                databaseUri = AppHost.Settings.ActiveDatabaseUri;
            }

            Query = query;
            DatabaseUri = databaseUri;
            QueryName = queryName;
            QueryBuilder.Type = CustomValidationType.Query;

            DatabaseSelector.SelectionChanged += SetDatabaseUri;

            EnableButtons();
        }

        [CanBeNull]
        public DatabaseUri DatabaseUri
        {
            get { return QueryBuilder.DatabaseUri; }

            set
            {
                DatabaseSelector.DatabaseUri = value;
                QueryBuilder.DatabaseUri = value;
                EnableButtons();
            }
        }

        public bool IsQueryNameVisible
        {
            get { return isQueryNameVisible; }

            set
            {
                isQueryNameVisible = value;
                FolderNameLabel.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                FolderNameTextBox.Visibility = FolderNameLabel.Visibility;
            }
        }

        [NotNull]
        public string Query
        {
            get { return QueryBuilder.Text; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                QueryBuilder.Text = value;
            }
        }

        [NotNull]
        public string QueryName
        {
            get { return FolderNameTextBox.Text; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                FolderNameTextBox.Text = value;
            }
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            var text = AppHost.Settings.GetString("Folders\\Builder", "Database", string.Empty);

            DatabaseUri databaseUri;
            if (DatabaseUri.TryParse(text, out databaseUri))
            {
                DatabaseUri = databaseUri;
            }
        }

        private void EnableButtons()
        {
            OkButton.IsEnabled = DatabaseUri != null && DatabaseUri != DatabaseUri.Empty && !string.IsNullOrEmpty(Query) && !string.IsNullOrEmpty(QueryName);
        }

        private void EnableButtons([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            EnableButtons();
        }

        private void EnableButtons2([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            EnableButtons();
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var databaseUri = DatabaseSelector.DatabaseUri;
            if (databaseUri != null && databaseUri != DatabaseUri.Empty)
            {
                AppHost.Settings.Set("Folders\\Builder", "Database", databaseUri.ToString());
            }

            this.Close(true);
        }

        private void SetDatabaseUri([NotNull] object sender, [NotNull] DatabaseUri databaseUri)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(databaseUri, nameof(databaseUri));

            DatabaseUri = databaseUri;
        }
    }
}
