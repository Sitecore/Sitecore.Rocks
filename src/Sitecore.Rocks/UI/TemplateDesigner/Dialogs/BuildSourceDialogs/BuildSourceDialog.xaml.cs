// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.UI.Dialogs.SelectItemDialogs;

namespace Sitecore.Rocks.UI.TemplateDesigner.Dialogs.BuildSourceDialogs
{
    public partial class BuildSourceDialog
    {
        public BuildSourceDialog([NotNull] DatabaseUri databaseUri, [NotNull] BuildSourceField field, [NotNull] string dataSource)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(field, nameof(field));
            Assert.ArgumentNotNull(dataSource, nameof(dataSource));

            InitializeComponent();
            this.InitializeDialog();

            DatabaseUri = databaseUri;
            Field = field;
            DataSourceField.Text = dataSource;

            Loaded += ControlLoaded;
        }

        [NotNull]
        public DatabaseUri DatabaseUri { get; }

        [NotNull]
        public string DataSource => DataSourceField.Text;

        [NotNull]
        public BuildSourceField Field { get; }

        private void Browse([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var itemId = new ItemId(DatabaseTreeViewItem.RootItemGuid);
            var databaseUri = DatabaseUri;

            var dataSource = new DataSourceString(DataSourceField.Text);

            var value = dataSource["datasource"];
            if (!string.IsNullOrEmpty(value))
            {
                Guid guid;
                if (Guid.TryParse(value, out guid))
                {
                    itemId = new ItemId(guid);
                }
            }

            var databaseName = dataSource["database"];
            if (string.IsNullOrEmpty(databaseName))
            {
                databaseName = dataSource["databasename"];
            }

            if (!string.IsNullOrEmpty(databaseName))
            {
                databaseUri = new DatabaseUri(DatabaseUri.Site, new DatabaseName(databaseName));
            }

            var itemUri = new ItemUri(databaseUri, itemId);

            var dialog = new SelectItemDialog();
            dialog.Initialize(Rocks.Resources.Browse, itemUri);

            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            dataSource.Path = string.Empty;
            dialog.GetSelectedItemPath(delegate(string itemPath)
            {
                dataSource.Remove(@"datasource");
                dataSource.Path = itemPath;
                DataSourceField.Text = dataSource.ToString();
            });
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            foreach (var descriptor in DataSourceParameterManager.DataSourceParameters)
            {
                if (!descriptor.Instance.CanExecute(DatabaseUri, Field))
                {
                    continue;
                }

                var listBoxItem = new ListBoxItem
                {
                    Content = descriptor.Header,
                    Tag = descriptor
                };

                Parameters.Items.Add(listBoxItem);
            }

            EnableButtons();
        }

        private void Edit([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Edit();
        }

        private void Edit()
        {
            var listBoxItem = Parameters.SelectedItem as ListBoxItem;
            if (listBoxItem == null)
            {
                return;
            }

            var descriptor = listBoxItem.Tag as DataSourceParameterManager.DataSourceParameterDescriptor;
            if (descriptor == null)
            {
                return;
            }

            var dataSource = new DataSourceString(DataSourceField.Text);

            var result = descriptor.Instance.Execute(DatabaseUri, Field, dataSource);
            if (result == null)
            {
                return;
            }

            DataSourceField.Text = result.ToString();
        }

        private void EnableButtons()
        {
            EditButton.IsEnabled = Parameters.SelectedItem != null;
        }

        private void EnableButtons([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            EnableButtons();
        }

        private void HandleDoubleClick([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Edit();
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(true);
        }
    }
}
