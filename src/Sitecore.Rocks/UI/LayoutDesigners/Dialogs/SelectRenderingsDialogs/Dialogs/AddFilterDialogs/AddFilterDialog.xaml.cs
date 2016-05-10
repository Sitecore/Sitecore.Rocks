// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.UI.Dialogs.SelectItemDialogs;

namespace Sitecore.Rocks.UI.LayoutDesigners.Dialogs.SelectRenderingsDialogs.Dialogs.AddFilterDialogs
{
    public partial class AddFilterDialog
    {
        public AddFilterDialog()
        {
            InitializeComponent();
            this.InitializeDialog();

            DataContext = this;
        }

        [NotNull]
        public DatabaseUri DatabaseUri { get; set; }

        [CanBeNull]
        public string FilterName { get; set; }

        [CanBeNull]
        public string FilterText { get; set; }

        [CanBeNull]
        public string RootPath { get; set; }

        public new bool ShowDialog()
        {
            Assert.IsNotNull(DatabaseUri, "DatabaseUri must be set");

            EnableButtons();

            return AppHost.Shell.ShowDialog(this) == true;
        }

        private void Browse([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var dialog = new SelectItemDialog
            {
                DatabaseUri = DatabaseUri
            };

            if (dialog.ShowDialog())
            {
                dialog.GetSelectedItemPath(path =>
                {
                    RootPathTextBox.Text = path;
                    RootPath = path;
                });
            }
        }

        private void EnableButtons([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            EnableButtons();
        }

        private void EnableButtons()
        {
            OkButton.IsEnabled = !string.IsNullOrEmpty(NameTextBox.Text);
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(true);
        }
    }
}
