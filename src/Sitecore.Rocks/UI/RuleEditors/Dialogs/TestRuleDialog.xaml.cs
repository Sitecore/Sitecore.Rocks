// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.UI.Dialogs.SelectItemDialogs;

namespace Sitecore.Rocks.UI.RuleEditors.Dialogs
{
    public partial class TestRuleDialog
    {
        public TestRuleDialog()
        {
            InitializeComponent();
            this.InitializeDialog();
        }

        [NotNull]
        protected DatabaseUri DatabaseUri { get; set; }

        [CanBeNull]
        protected ItemUri LastSelectedItemUri { get; set; }

        [NotNull]
        protected RuleModel RuleModel { get; set; }

        public void Initialize([NotNull] DatabaseUri databaseUri, [NotNull] RuleModel ruleModel)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(ruleModel, nameof(ruleModel));

            DatabaseUri = databaseUri;
            RuleModel = ruleModel;

            Path.Text = AppHost.Settings.GetString("RuleTester", "ItemPath", @"/sitecore/content/Home");

            EnableButtons();
            Refresh();
        }

        private void Browse([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var dialog = new SelectItemDialog();

            ItemUri itemUri;
            if (LastSelectedItemUri != null)
            {
                itemUri = LastSelectedItemUri;
            }
            else
            {
                itemUri = new ItemUri(DatabaseUri, new ItemId(DatabaseTreeViewItem.RootItemGuid));
            }

            dialog.Initialize(Rocks.Resources.TestRuleDialog_Browse_Item, itemUri);

            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            LastSelectedItemUri = dialog.SelectedItemUri;

            dialog.GetSelectedItemPath(delegate(string itemPath)
            {
                Path.Text = itemPath;

                AppHost.Settings.Set("RuleTester", "ItemPath", itemPath);

                Refresh();
            });
        }

        private void EnableButtons([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            EnableButtons();
        }

        private void EnableButtons()
        {
            TestButton.IsEnabled = !string.IsNullOrEmpty(Path.Text);
        }

        private void HandleKeyDown([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.Key == Key.Enter)
            {
                Refresh();
            }
        }

        private void LoadResult([NotNull] string response, [NotNull] ExecuteResult executeresult)
        {
            Debug.ArgumentNotNull(response, nameof(response));
            Debug.ArgumentNotNull(executeresult, nameof(executeresult));

            Loading.HideLoading(Log);
            Log.Items.Clear();

            if (!DataService.HandleExecute(response, executeresult))
            {
                return;
            }

            var root = response.ToXElement();
            if (root == null)
            {
                return;
            }

            foreach (var element in root.Elements())
            {
                var text = element.Value;

                var listBoxItem = new ListBoxItem();

                Log.Items.Add(listBoxItem);

                listBoxItem.Content = text;
            }
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(true);
        }

        private void Refresh()
        {
            if (string.IsNullOrEmpty(Path.Text))
            {
                Log.Items.Clear();
                Loading.HideLoading(Log);
                return;
            }

            Loading.ShowLoading(Log);

            DatabaseUri.Site.DataService.ExecuteAsync("Rules.TestRule", LoadResult, DatabaseUri.DatabaseName.ToString(), Path.Text, RuleModel.ToString());
        }

        private void Run([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Refresh();
        }
    }
}
