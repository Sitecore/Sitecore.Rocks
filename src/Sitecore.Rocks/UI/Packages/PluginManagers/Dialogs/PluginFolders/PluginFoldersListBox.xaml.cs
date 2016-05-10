// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions;
using Sitecore.Rocks.UI.Packages.PluginManagers.Galleries;

namespace Sitecore.Rocks.UI.Packages.PluginManagers.Dialogs.PluginFolders
{
    public partial class PluginFoldersListBox
    {
        public PluginFoldersListBox()
        {
            InitializeComponent();

            RenderFolders();

            EnableButtons();
        }

        public void AddLocation()
        {
            var location = LocationTextBox.Text ?? string.Empty;

            var folder = new FolderDescriptor(location);

            var listBoxItem = GetListBoxItem(folder);

            List.SelectedIndex = List.Items.Add(listBoxItem);

            LocationTextBox.Text = string.Empty;

            EnableButtons();
        }

        public void Save()
        {
            var list = List.Items.OfType<ListBoxItem>().Select(i => i.Tag as FolderDescriptor);

            AppHost.Plugins.SetPluginFolders(list);
        }

        private void Add([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AddLocation();
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

                if (d.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                LocationTextBox.Text = d.SelectedPath;
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
            AddButton.IsEnabled = !string.IsNullOrEmpty(LocationTextBox.Text);
            RemoveButton.IsEnabled = List.SelectedItem != null;
        }

        private void EnableButtons([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            EnableButtons();
        }

        [NotNull]
        private ListBoxItem GetListBoxItem([NotNull] FolderDescriptor descriptor)
        {
            Debug.ArgumentNotNull(descriptor, nameof(descriptor));

            var stackPanel = new StackPanel();

            stackPanel.Children.Add(new TextBlock
            {
                Text = descriptor.Location
            });

            var listBoxItem = new ListBoxItem
            {
                Content = stackPanel,
                Tag = descriptor
            };

            return listBoxItem;
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

        private void RenderFolders()
        {
            List.Items.Clear();

            foreach (var descriptor in AppHost.Plugins.GetPluginFolders())
            {
                var listBoxItem = GetListBoxItem(descriptor);

                List.Items.Add(listBoxItem);
            }
        }
    }
}
