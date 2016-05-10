// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.Dialogs.SelectFileDialogs;

namespace Sitecore.Rocks.UI.FolderSynchronization.Dialogs
{
    public partial class EditFolderSynchronizerDialog
    {
        public EditFolderSynchronizerDialog(Site site, string sourceFolder, string destinationFolder, FolderSynchronizationMode mode, string pattern)
        {
            InitializeComponent();
            this.InitializeDialog();

            Site = site;
            SourceFolder = sourceFolder;
            DestinationFolder = destinationFolder;
            Mode = mode;
            Pattern = pattern;

            SourceTextBox.Text = SourceFolder;
            DestinationTextBox.Text = DestinationFolder;
            PatternTextBox.Text = Pattern;

            switch (Mode)
            {
                case FolderSynchronizationMode.Mirror:
                    MirrorMode.IsSelected = true;
                    break;
                case FolderSynchronizationMode.Copy:
                    CopyMode.IsSelected = true;
                    break;
            }

            EnableButtons();
        }

        [NotNull]
        public string DestinationFolder { get; private set; }

        public FolderSynchronizationMode Mode { get; private set; }

        [NotNull]
        public string Pattern { get; set; }

        [NotNull]
        public string PatternFolder { get; private set; }

        [NotNull]
        public Site Site { get; }

        [NotNull]
        public string SourceFolder { get; private set; }

        private void BrowseDestinationFolder([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var dialog = new SelectFileDialog
            {
                Site = Site
            };

            if (!dialog.ShowDialog())
            {
                return;
            }

            DestinationTextBox.Text = dialog.SelectedFilePath.TrimStart('\\') + "\\";
        }

        private void BrowseSourceFolder([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            using (var d = new FolderBrowserDialog())
            {
                d.ShowNewFolderButton = true;
                d.Description = "Select source folder:";
                d.SelectedPath = Path.Combine(Site.WebRootPath, SourceFolder);

                if (d.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }

                var sourceFolder = d.SelectedPath;
                if (sourceFolder.StartsWith(Site.WebRootPath + "\\", StringComparison.OrdinalIgnoreCase))
                {
                    sourceFolder = sourceFolder.Mid(Site.WebRootPath.Length + 1);
                }

                SourceTextBox.Text = sourceFolder;
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
            OkButton.IsEnabled = !string.IsNullOrEmpty(SourceTextBox.Text) && !string.IsNullOrEmpty(DestinationTextBox.Text);
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            SourceFolder = SourceTextBox.Text;
            DestinationFolder = DestinationTextBox.Text;
            Pattern = PatternTextBox.Text;
            Mode = CopyMode.IsSelected ? FolderSynchronizationMode.Copy : FolderSynchronizationMode.Mirror;

            this.Close(true);
        }
    }
}
