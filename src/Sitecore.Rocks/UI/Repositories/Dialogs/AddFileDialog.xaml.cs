// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.UI.Repositories.Dialogs
{
    public partial class AddFileDialog
    {
        public AddFileDialog([NotNull] string repositoryName)
        {
            Assert.ArgumentNotNull(repositoryName, nameof(repositoryName));

            InitializeComponent();
            this.InitializeDialog();

            SelectedFileName = string.Empty;
            RepositoryComboBox.RepositoryName = repositoryName;

            EnableButtons();
        }

        [NotNull]
        public string SelectedFileName { get; private set; }

        [CanBeNull]
        public RepositoryEntry SelectedRepositoryEntry
        {
            get { return RepositoryComboBox.SelectedRepositoryEntry; }
        }

        [NotNull]
        protected string FileName
        {
            get { return FileNameTextBox.Text ?? string.Empty; }
        }

        private void Browse([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var dialog = new OpenFileDialog
            {
                Title = "Add File",
                CheckFileExists = true,
                DefaultExt = @".ps1",
                Filter = @"All files|*.*",
            };

            if (dialog.ShowDialog() == true)
            {
                FileNameTextBox.Text = dialog.FileName;
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
            var fileExists = !string.IsNullOrEmpty(FileNameTextBox.Text) && File.Exists(FileNameTextBox.Text);

            OkButton.IsEnabled = fileExists && SelectedRepositoryEntry != null;
        }

        private void EnableButtons([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            EnableButtons();
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (!File.Exists(FileName))
            {
                AppHost.MessageBox("The file does not exists.", Rocks.Resources.Information, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var repository = SelectedRepositoryEntry;
            if (repository == null)
            {
                AppHost.MessageBox("Please select a repository.", Rocks.Resources.Information, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var source = FileName;
            var target = Path.Combine(repository.Path, Path.GetFileName(source) ?? string.Empty);

            Directory.CreateDirectory(Path.GetDirectoryName(target) ?? string.Empty);

            try
            {
                AppHost.Files.Copy(source, target, true);
            }
            catch (UnauthorizedAccessException ex)
            {
                AppHost.MessageBox(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SelectedFileName = target;

            this.Close(true);
        }
    }
}
