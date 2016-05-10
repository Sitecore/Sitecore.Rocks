// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions;
using Sitecore.Rocks.UI.Packages.PluginManagers.Galleries;

namespace Sitecore.Rocks.UI.Packages.PluginManagers.Dialogs.PluginGalleries
{
    public partial class PluginGalleryListBox
    {
        public PluginGalleryListBox()
        {
            InitializeComponent();

            RenderGallerySources();

            EnableButtons();
        }

        public void AddLocation()
        {
            var name = NameTextBox.Text ?? string.Empty;
            var location = LocationTextBox.Text ?? string.Empty;

            var repository = new GalleryDescriptor(name, location);

            var listBoxItem = GetListBoxItem(repository);

            List.SelectedIndex = List.Items.Add(listBoxItem);

            NameTextBox.Text = string.Empty;
            LocationTextBox.Text = string.Empty;

            EnableButtons();
        }

        public void Save()
        {
            var list = List.Items.OfType<ListBoxItem>().Select(i => i.Tag as GalleryDescriptor);

            AppHost.Plugins.SetPluginGalleries(list);
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

                if (string.IsNullOrEmpty(NameTextBox.Text))
                {
                    NameTextBox.Text = Path.GetFileName(d.SelectedPath);
                }
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
            AddButton.IsEnabled = !string.IsNullOrEmpty(NameTextBox.Text) && !string.IsNullOrEmpty(LocationTextBox.Text);
            RemoveButton.IsEnabled = List.SelectedItem != null;
            MoveUpButton.IsEnabled = List.SelectedIndex > 0;
            MoveDownButton.IsEnabled = List.SelectedIndex < List.Items.Count - 1 && List.SelectedIndex >= 0;

            // this.OkButton.IsEnabled = !(string.IsNullOrEmpty(this.NameTextBox.Text) ^ string.IsNullOrEmpty(this.LocationTextBox.Text));
        }

        private void EnableButtons([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            EnableButtons();
        }

        [NotNull]
        private ListBoxItem GetListBoxItem([NotNull] GalleryDescriptor descriptor)
        {
            Debug.ArgumentNotNull(descriptor, nameof(descriptor));

            var stackPanel = new StackPanel();

            stackPanel.Children.Add(new TextBlock
            {
                Text = descriptor.Name
            });

            stackPanel.Children.Add(new TextBlock(new Italic(new Run(descriptor.Location)))
            {
                Margin = new Thickness(16, 0, 0, 0)
            });

            var listBoxItem = new ListBoxItem
            {
                Content = stackPanel,
                Tag = descriptor
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

        private void RenderGallerySources()
        {
            List.Items.Clear();

            foreach (var descriptor in AppHost.Plugins.GetPluginGalleries())
            {
                var listBoxItem = GetListBoxItem(descriptor);

                List.Items.Add(listBoxItem);
            }
        }
    }
}
