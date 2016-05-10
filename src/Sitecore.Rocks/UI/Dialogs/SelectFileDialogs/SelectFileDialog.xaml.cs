// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.Dialogs.SelectFileDialogs
{
    public partial class SelectFileDialog
    {
        private bool isLoaded;

        private Site site;

        public SelectFileDialog()
        {
            InitializeComponent();
            this.InitializeDialog();

            SelectedFilePath = string.Empty;
        }

        [NotNull]
        public string SelectedFilePath { get; private set; }

        [NotNull]
        public Site Site
        {
            get { return site ?? Site.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                site = value;
            }
        }

        [Obsolete]
        public void Initialize([NotNull] Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            Site = site;

            Load();
        }

        public new bool ShowDialog()
        {
            Load();

            return AppHost.Shell.ShowDialog(this) == true;
        }

        private void Load()
        {
            if (isLoaded)
            {
                return;
            }

            isLoaded = true;

            Assert.IsNotNull(Site, "Site property must be set");

            var fileUri = new FileUri(Site, @"\", FileUriBaseFolder.Web, true);

            var fileItem = new RootFileTreeViewItem(fileUri)
            {
                Text = fileUri.Site.Name
            };

            fileItem.MakeExpandable();
            Files.Items.Add(fileItem);

            fileItem.IsExpanded = true;
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedItem = Files.SelectedItem as FileTreeViewItem;
            if (selectedItem != null)
            {
                SelectedFilePath = selectedItem.FileUri.FileName;
            }

            if (string.IsNullOrEmpty(SelectedFilePath))
            {
                AppHost.MessageBox("Please select a file first.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            this.Close(true);
        }
    }
}
