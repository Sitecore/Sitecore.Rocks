// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;
using Sitecore.Rocks.Extensions.ListBoxExtensions;
using Sitecore.Rocks.Projects;
using Sitecore.Rocks.Shell.ProjectOptions;
using Sitecore.Rocks.UI.FolderSynchronization.Dialogs;

namespace Sitecore.Rocks.UI.FolderSynchronization
{
    [Export(typeof(IPropertyPage), Priority = 2000)]
    public partial class FolderSynchronizationPropertyPage : IPropertyPage
    {
        public FolderSynchronizationPropertyPage()
        {
            InitializeComponent();

            Header = "Folder Sync.";
            VisualStudioProject = null;

            FolderSynchronizationManager = AppHost.Container.Get<FolderSynchronizationManager>();

            RefreshLog();
        }

        [NotNull]
        public FolderSynchronizationManager FolderSynchronizationManager { get; }

        public string Header { get; }

        [CanBeNull]
        public ProjectBase Project { get; private set; }

        [CanBeNull]
        public object VisualStudioProject { get; private set; }

        public void LoadProject(ProjectBase project, object visualStudioProject)
        {
            Assert.ArgumentNotNull(visualStudioProject, nameof(visualStudioProject));

            AddButton.IsEnabled = false;
            FolderListBox.Items.Clear();
            LogListBox.Items.Clear();

            if (project == null)
            {
                return;
            }

            Project = project;

            Refresh();

            AddButton.IsEnabled = true;
        }

        private void Add([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var project = Project as Project;
            if (project == null)
            {
                return;
            }

            var dialog = new EditFolderSynchronizerDialog(project.Site, string.Empty, string.Empty, FolderSynchronizationMode.Mirror, string.Empty);
            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            var folderSynchronizationManager = AppHost.Container.Get<FolderSynchronizationManager>();

            folderSynchronizationManager.Add(project, dialog.SourceFolder, dialog.DestinationFolder, dialog.Mode, dialog.Pattern);

            Refresh();
        }

        private void ClearLog([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            lock (FolderSynchronizationManager.LogSyncObject)
            {
                FolderSynchronizationManager.Log.Clear();
            }

            RefreshLog();
        }

        private void Edit([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var project = Project as Project;
            if (project == null)
            {
                return;
            }

            var folderSynchronizer = FolderListBox.SelectedItem as FolderSynchronizer;
            if (folderSynchronizer == null)
            {
                return;
            }

            var dialog = new EditFolderSynchronizerDialog(project.Site, folderSynchronizer.SourceFolder, folderSynchronizer.DestinationFolder, folderSynchronizer.Mode, folderSynchronizer.Pattern);
            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            var folderSynchronizationManager = AppHost.Container.Get<FolderSynchronizationManager>();

            folderSynchronizationManager.Remove(Project, folderSynchronizer);
            folderSynchronizationManager.Add(project, dialog.SourceFolder, dialog.DestinationFolder, dialog.Mode, dialog.Pattern);

            Refresh();
        }

        private void Refresh()
        {
            if (Project == null)
            {
                return;
            }

            var selectedIndex = FolderListBox.SelectedIndex;

            FolderListBox.Items.Clear();

            SynchronizeOutputCheckBox.IsChecked = FolderSynchronizationManager.GetSynchronizeOutputFolderOnBuild(Project);

            foreach (var source in FolderSynchronizationManager.Folders.Where(f => f.Project == Project))
            {
                FolderListBox.Items.Add(source);
            }

            if (selectedIndex >= FolderListBox.Items.Count)
            {
                selectedIndex = FolderListBox.Items.Count - 1;
            }

            FolderListBox.SelectedIndex = selectedIndex;
        }

        private void Refresh([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Refresh();
        }

        private void RefreshLog()
        {
            LogListBox.Items.Clear();

            lock (FolderSynchronizationManager.LogSyncObject)
            {
                foreach (var message in FolderSynchronizationManager.Log.OfType<string>().Reverse())
                {
                    LogListBox.Items.Add(message);
                }
            }
        }

        private void RefreshLog([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            RefreshLog();
        }

        private void Remove([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedItem = FolderListBox.RemoveSelectedItem() as FolderSynchronizer;
            if (selectedItem == null)
            {
                return;
            }

            FolderSynchronizationManager.Remove(Project, selectedItem);
        }

        private void SetProjectSynchronization([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (Project == null)
            {
                return;
            }

            FolderSynchronizationManager.SetSynchronizeOutputFolderOnBuild(Project, SynchronizeOutputCheckBox.IsChecked == true);
        }

        private void Sync([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var folder = FolderListBox.SelectedItem as FolderSynchronizer;
            if (folder == null)
            {
                return;
            }

            if (AppHost.MessageBox("Are you sure you want to sync these folders?", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            folder.Sync();
        }
    }
}
