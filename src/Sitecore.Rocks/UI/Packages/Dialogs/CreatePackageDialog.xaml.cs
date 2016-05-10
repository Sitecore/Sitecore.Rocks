// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.UI.Packages.Dialogs
{
    public partial class CreatePackageDialog
    {
        public CreatePackageDialog()
        {
            InitializeComponent();
            this.InitializeDialog();

            FileName.Text = AppHost.Settings.GetString("Packages", "LastLocation", "package.zip");
        }

        [NotNull]
        public IEnumerable<ItemUri> SelectedItems
        {
            get { return ItemDependencies.SelectedItems; }
        }

        [NotNull]
        protected IEnumerable<IItem> Items { get; set; }

        public void Initialize([NotNull] IEnumerable<IItem> items)
        {
            Assert.ArgumentNotNull(items, nameof(items));

            Items = items;

            ItemDependencies.ItemsSource = items.Select(i => i.ItemUri);
        }

        private void Browse([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var fileName = FileName.Text;

            var dialog = new SaveFileDialog
            {
                Title = "Package File Name",
                CheckPathExists = true,
                OverwritePrompt = true,
                FileName = Path.GetFileName(fileName),
                InitialDirectory = Path.GetDirectoryName(fileName),
                Filter = "Packages (*.zip)|*.zip|All Files|*.*"
            };

            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            FileName.Text = dialog.FileName;
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AppHost.Settings.Set("Packages", "LastLocation", FileName.Text);

            this.Close(true);
        }
    }
}
