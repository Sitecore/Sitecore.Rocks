// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Repositories;

namespace Sitecore.Rocks.Controls
{
    public class RepositoryComboBox : ComboBox
    {
        private string _repositoryListName;

        [NotNull]
        public string RepositoryName
        {
            get { return _repositoryListName ?? string.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                _repositoryListName = value;

                RenderItems();
            }
        }

        [CanBeNull]
        public RepositoryEntry SelectedRepositoryEntry
        {
            get
            {
                var comboxBoxItem = SelectedItem as ComboBoxItem;
                if (comboxBoxItem == null)
                {
                    return null;
                }

                return comboxBoxItem.Tag as RepositoryEntry;
            }
        }

        private bool IsLoading { get; set; }

        [CanBeNull]
        private RepositoryEntry LastSelection { get; set; }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));

            var comboxBoxItem = SelectedItem as ComboBoxItem;
            if (comboxBoxItem != null)
            {
                if (comboxBoxItem.Content as string == "<Repositories>")
                {
                    var repositoryList = RepositoryManager.GetRepository(RepositoryName);

                    if (repositoryList.Edit("Repositories"))
                    {
                        RenderItems();
                    }

                    if (LastSelection == null)
                    {
                        SelectedItem = null;
                    }
                    else
                    {
                        SelectedItem = Items.OfType<ComboBoxItem>().FirstOrDefault(i => IsLastSelected(i.Tag as RepositoryEntry));
                    }

                    e.Handled = true;
                    return;
                }
            }

            if (!IsLoading)
            {
                LastSelection = SelectedRepositoryEntry;
            }

            base.OnSelectionChanged(e);
        }

        private void AddRepositoriesItems()
        {
            var comboBoxItem = new ComboBoxItem
            {
                Content = "<Repositories>",
                ToolTip = "Manage Repositories",
            };

            Items.Add(comboBoxItem);
        }

        private bool IsLastSelected([CanBeNull] RepositoryEntry repositoryEntry)
        {
            if (repositoryEntry == null)
            {
                return false;
            }

            if (LastSelection == null)
            {
                return false;
            }

            return repositoryEntry.Name == LastSelection.Name && repositoryEntry.Location == LastSelection.Location;
        }

        private void RenderItems()
        {
            IsLoading = true;
            try
            {
                var selectedRepository = SelectedRepositoryEntry;

                Items.Clear();

                var repositoryList = RepositoryManager.GetRepository(RepositoryName);

                ComboBoxItem selectedComboBoxItem = null;
                foreach (var repository in repositoryList.Entries)
                {
                    var comboBoxItem = new ComboBoxItem
                    {
                        Content = repository.Name,
                        ToolTip = repository.Location,
                        Tag = repository
                    };

                    Items.Add(comboBoxItem);

                    if (selectedComboBoxItem == null)
                    {
                        selectedComboBoxItem = comboBoxItem;
                    }

                    if (selectedRepository != null && repository.Name == selectedRepository.Name && repository.Location == selectedRepository.Location)
                    {
                        selectedComboBoxItem = comboBoxItem;
                    }
                }

                AddRepositoriesItems();

                if (selectedComboBoxItem != null)
                {
                    selectedComboBoxItem.IsSelected = true;
                }
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
