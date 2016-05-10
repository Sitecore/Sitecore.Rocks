// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.UI.Repositories.Dialogs
{
    public partial class EditRepositoriesDialog
    {
        public EditRepositoriesDialog([NotNull] Repository repository)
        {
            Assert.ArgumentNotNull(repository, nameof(repository));

            InitializeComponent();
            this.InitializeDialog();

            Repository = repository;

            RenderRepositories();

            EnableButtons();
        }

        [NotNull]
        public Repository Repository { get; }

        public void SaveRepositories()
        {
            Repository.Entries.Clear();

            foreach (var item in List.Items)
            {
                var listBoxItem = item as ListBoxItem;
                if (listBoxItem == null)
                {
                    continue;
                }

                var repository = listBoxItem.Tag as RepositoryEntry;
                if (repository == null)
                {
                    continue;
                }

                Repository.Entries.Add(repository);
            }

            Repository.Save();
        }

        private void Add([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AddLocation();
        }

        private void AddLocation()
        {
            var name = NameTextBox.Text ?? string.Empty;
            var location = LocationTextBox.Text ?? string.Empty;

            var repository = new RepositoryEntry(name, location);

            var listBoxItem = GetListBoxItem(repository);

            List.SelectedIndex = List.Items.Add(listBoxItem);

            NameTextBox.Text = string.Empty;
            LocationTextBox.Text = string.Empty;

            EnableButtons();
        }

        private void Browse([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            using (var d = new FolderBrowserDialog())
            {
                d.ShowNewFolderButton = true;
                d.Description = Rocks.Resources.PackageRepositoryDialog_Browse_Select_a_location_;
                d.SelectedPath = LocationTextBox.Text;

                if (d.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }

                LocationTextBox.Text = d.SelectedPath;

                if (string.IsNullOrEmpty(NameTextBox.Text))
                {
                    NameTextBox.Text = Path.GetFileName(d.SelectedPath);
                }
            }
        }

        private void CloseDialog([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (!string.IsNullOrEmpty(NameTextBox.Text) && !string.IsNullOrEmpty(LocationTextBox.Text))
            {
                AddLocation();
            }

            SaveRepositories();

            this.Close(true);
        }

        private void EnableButtons([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            EnableButtons();
        }

        private void EnableButtons()
        {
            AddButton.IsEnabled = !string.IsNullOrEmpty(NameTextBox.Text) && !string.IsNullOrEmpty(LocationTextBox.Text);
            RemoveButton.IsEnabled = List.SelectedItem != null;
            OpenButton.IsEnabled = List.SelectedItem != null;
            MoveUpButton.IsEnabled = List.SelectedIndex > 0;
            MoveDownButton.IsEnabled = List.SelectedIndex < List.Items.Count - 1 && List.SelectedIndex >= 0;

            OkButton.IsEnabled = !(string.IsNullOrEmpty(NameTextBox.Text) ^ string.IsNullOrEmpty(LocationTextBox.Text));
        }

        private void EnableButtons([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            EnableButtons();
        }

        [NotNull]
        private ListBoxItem GetListBoxItem([NotNull] RepositoryEntry repositoryEntry)
        {
            Debug.ArgumentNotNull(repositoryEntry, nameof(repositoryEntry));

            var stackPanel = new StackPanel();

            stackPanel.Children.Add(new TextBlock
            {
                Text = repositoryEntry.Name
            });

            stackPanel.Children.Add(new TextBlock(new Italic(new Run(repositoryEntry.Location)))
            {
                Margin = new Thickness(16, 0, 0, 0)
            });

            var listBoxItem = new ListBoxItem
            {
                Content = stackPanel,
                Tag = repositoryEntry
            };

            return listBoxItem;
        }

        private void MoveDown([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedItem = List.SelectedItem;
            if (selectedItem == null)
            {
                return;
            }

            var selectedIndex = List.SelectedIndex;

            if (selectedIndex >= List.Items.Count - 1)
            {
                return;
            }

            List.Items.Remove(selectedItem);
            List.Items.Insert(selectedIndex + 1, selectedItem);

            List.SelectedIndex = selectedIndex + 1;
        }

        private void MoveUp([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedItem = List.SelectedItem;
            if (selectedItem == null)
            {
                return;
            }

            var selectedIndex = List.SelectedIndex;

            if (selectedIndex <= 0)
            {
                return;
            }

            List.Items.Remove(selectedItem);
            List.Items.Insert(selectedIndex - 1, selectedItem);

            List.SelectedIndex = selectedIndex - 1;
        }

        private void Open([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedItem = List.SelectedItem as ListBoxItem;
            if (selectedItem == null)
            {
                return;
            }

            var entry = selectedItem.Tag as RepositoryEntry;
            if (entry == null)
            {
                return;
            }

            IO.File.OpenInWindowsExplorer(entry.Path);
        }

        private void Remove([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedItem = List.SelectedItem;
            if (selectedItem == null)
            {
                return;
            }

            List.Items.Remove(selectedItem);

            EnableButtons();
        }

        private void RenderRepositories()
        {
            List.Items.Clear();

            foreach (var repository in Repository.Entries)
            {
                var listBoxItem = GetListBoxItem(repository);

                List.Items.Add(listBoxItem);
            }
        }
    }
}
