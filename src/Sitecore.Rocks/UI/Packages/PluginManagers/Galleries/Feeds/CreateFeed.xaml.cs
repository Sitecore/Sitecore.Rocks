// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml;
using NuGet;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ListViewExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.UserControlExtensions;
using Sitecore.Rocks.UI.Packages.PluginManagers.Descriptors;

namespace Sitecore.Rocks.UI.Packages.PluginManagers.Galleries.Feeds
{
    public partial class CreatePlugin : IFeed
    {
        private readonly ListViewSorter fileListSorter;

        private readonly List<PluginFile> pluginFiles = new List<PluginFile>();

        public CreatePlugin([NotNull] PluginManagerDialog pluginDialogDialog)
        {
            Assert.ArgumentNotNull(pluginDialogDialog, nameof(pluginDialogDialog));

            InitializeComponent();

            FeedName = "Create";
            PluginManagerDialog = pluginDialogDialog;

            fileListSorter = new ListViewSorter(FileList);

            NoFiles.DragOver += HandleFileDragOver;
            NoFiles.Drop += HandleFileDrop;
            FileList.DragOver += HandleFileDragOver;
            FileList.Drop += HandleFileDrop;
        }

        [NotNull]
        public string FeedName { get; }

        [CanBeNull]
        public string FileName { get; set; }

        [NotNull]
        public ICollection<PluginFile> Files
        {
            get { return pluginFiles; }
        }

        public bool Modified { get; set; }

        public PluginManagerDialog PluginManagerDialog { get; }

        protected bool IsLoading { get; set; }

        public void Build()
        {
            if (string.IsNullOrEmpty(FileName))
            {
                var fileName = GetFileName();
                if (string.IsNullOrEmpty(fileName))
                {
                    return;
                }

                FileName = fileName;
                if (!Save())
                {
                    return;
                }
            }

            if (Modified)
            {
                if (!Save())
                {
                    return;
                }
            }

            var nuspecFileName = FileName;
            var nupkgFileName = Path.ChangeExtension(nuspecFileName, string.Empty) + VersionTextBox.Text + ".nupkg";

            try
            {
                using (var nuspec = new FileStream(nuspecFileName, FileMode.Open, FileAccess.Read))
                {
                    var packageBuilder = new PackageBuilder(nuspec, Path.GetDirectoryName(nupkgFileName));

                    using (var nupkg = new FileStream(nupkgFileName, FileMode.Create))
                    {
                        packageBuilder.Save(nupkg);
                    }
                }
            }
            catch (Exception ex)
            {
                if (AppHost.MessageBox(string.Format("Failed to create the NuGet plugin: {0}\n\nDo you want to report this error?\n\n{1}", nupkgFileName, ex.Message), "Information", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                {
                    AppHost.Shell.HandleException(ex);
                }

                return;
            }

            if (AppHost.MessageBox(string.Format("The plugin has been built and is located at:\n\n{0}\n\nDo you want to open it in Windows Explorer?\n\nWhen it works as expected, consider sharing it on MyGet.org. You can upload the plugin using the Upload tab in this dialog.", nupkgFileName), "Information", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                AppHost.Files.OpenInWindowsExplorer(nupkgFileName);
            }
        }

        public void ClearControl()
        {
        }

        public FrameworkElement GetControl()
        {
            return this;
        }

        public void Refresh()
        {
        }

        public bool Save()
        {
            if (string.IsNullOrEmpty(TitleTextBox.Text))
            {
                AppHost.MessageBox("Plugin must have a title.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            if (string.IsNullOrEmpty(AuthorsTextBox.Text))
            {
                AppHost.MessageBox("The Authors field must have a value.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            if (string.IsNullOrEmpty(VersionTextBox.Text))
            {
                AppHost.MessageBox("The Version field must have a value.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            SemanticVersion semanticVersion;
            if (!SemanticVersion.TryParse(VersionTextBox.Text, out semanticVersion))
            {
                AppHost.MessageBox("The Version field is not a valid Semantic Version.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            if (string.IsNullOrEmpty(DescriptionTextBox.Text))
            {
                AppHost.MessageBox("The Description field must have a value.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            if (string.IsNullOrEmpty(FileName))
            {
                var fileName = GetFileName();
                if (string.IsNullOrEmpty(fileName))
                {
                    return false;
                }

                FileName = fileName;
            }

            var packageId = Regex.Replace(TitleTextBox.Text, "\\W", string.Empty);

            using (var output = new XmlTextWriter(FileName, Encoding.UTF8))
            {
                output.Formatting = Formatting.Indented;
                output.WriteStartElement("package");

                output.WriteStartElement("metadata");
                output.WriteElementString("id", packageId);
                output.WriteElementString("version", VersionTextBox.Text);
                output.WriteElementString("title", TitleTextBox.Text);
                output.WriteElementString("authors", AuthorsTextBox.Text);
                output.WriteElementString("owners", OwnersTextBox.Text);
                output.WriteElementString("description", DescriptionTextBox.Text);
                output.WriteElementString("summary", SummaryTextBox.Text);
                output.WriteElementString("releaseNotes", ReleaseNotesTextBox.Text);
                output.WriteElementString("copyright", CopyrightTextBox.Text);
                output.WriteElementString("tags", TagsTextBox.Text);

                output.WriteEndElement();

                output.WriteStartElement("files");

                foreach (var fileName in Files)
                {
                    if (Directory.Exists(fileName.FileName))
                    {
                        foreach (var file in Directory.GetFiles(fileName.FileName, "*", SearchOption.AllDirectories))
                        {
                            var fileInfo = new FileInfo(file);
                            if ((fileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                            {
                                continue;
                            }

                            if ((fileInfo.Attributes & FileAttributes.System) == FileAttributes.System)
                            {
                                continue;
                            }

                            output.WriteStartElement("file");

                            output.WriteAttributeString("src", file);
                            output.WriteAttributeString("target", "vAny\\" + Path.GetFileName(file));

                            output.WriteEndElement();
                        }
                    }
                    else
                    {
                        output.WriteStartElement("file");

                        output.WriteAttributeString("src", fileName.FileName);
                        output.WriteAttributeString("target", "vAny\\" + Path.GetFileName(fileName.FileName));

                        output.WriteEndElement();
                    }
                }

                output.WriteEndElement();
                output.WriteEndElement();
            }

            Modified = false;

            return true;
        }

        public void SetInstalledPlugins(IEnumerable<BasePluginDescriptor> installedPlugins)
        {
            Assert.ArgumentNotNull(installedPlugins, nameof(installedPlugins));
        }

        public void SetPage(int pageIndex)
        {
        }

        public void ShowFileList()
        {
            FileList.ItemsSource = null;
            FileList.ItemsSource = Files;

            fileListSorter.Resort();

            NoFiles.Visibility = Visibility.Collapsed;
            FileListPanel.Visibility = Visibility.Visible;

            FileList.ResizeColumn(FileNameColumn);
            FileList.ResizeColumn(FolderPathColumn);
        }

        internal void InternalAddFiles([NotNull] IEnumerable<PluginFile> files)
        {
            foreach (var pluginFile in files)
            {
                if (Files.All(i => i != pluginFile))
                {
                    Files.Add(pluginFile);
                }
            }
        }

        private void AddFolder([NotNull] List<PluginFile> files, [NotNull] string folder)
        {
            Debug.ArgumentNotNull(files, nameof(files));
            Debug.ArgumentNotNull(folder, nameof(folder));

            foreach (var fileName in AppHost.Files.GetFiles(folder))
            {
                files.Add(new PluginFile(fileName));
            }

            foreach (var subfolder in AppHost.Files.GetDirectories(folder))
            {
                AddFolder(files, subfolder);
            }
        }

        private void Build([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Build();
        }

        private void FileListHeaderClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            fileListSorter.HeaderClick(sender, e);
        }

        [NotNull]
        private string GetFileName()
        {
            var fileName = "MyPlugin.nuspec";
            if (!string.IsNullOrEmpty(TitleTextBox.Text))
            {
                fileName = TitleTextBox.Text + ".nuspec";
            }

            var dialog = new SaveFileDialog
            {
                Title = "Save As",
                CheckPathExists = true,
                OverwritePrompt = true,
                FileName = fileName,
                Filter = "NuSpec (*.nuspec)|*.nuspec|All files|*.*"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return string.Empty;
            }

            return dialog.FileName;
        }

        private void HandleFileDragOver([NotNull] object sender, [NotNull] System.Windows.DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Effects = System.Windows.DragDropEffects.None;
            e.Handled = true;

            if (e.Data.GetDataPresent("CF_VSSTGPROJECTITEMS") && e.Data.GetDataPresent(@"Text"))
            {
                e.Effects = System.Windows.DragDropEffects.Copy;
                return;
            }

            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop, true))
            {
                var droppedFilePaths = e.Data.GetData(System.Windows.DataFormats.FileDrop, true) as string[];
                if (droppedFilePaths == null)
                {
                    return;
                }

                e.Effects = System.Windows.DragDropEffects.Copy;
                return;
            }

            if (e.Data.GetDataPresent(FileTreeViewItem.FileItemDragIdentifier))
            {
                e.Effects = System.Windows.DragDropEffects.Copy;
            }
        }

        private void HandleFileDrop([NotNull] object sender, [NotNull] System.Windows.DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Handled = true;
            var files = new List<PluginFile>();

            if (e.Data.GetDataPresent("CF_VSSTGPROJECTITEMS") && e.Data.GetDataPresent(@"Text"))
            {
                var fileName = e.Data.GetData("Text") as string ?? string.Empty;

                if (fileName.EndsWith("\\"))
                {
                    fileName = fileName.Left(fileName.Length - 1);
                }

                if (Directory.Exists(fileName))
                {
                    AddFolder(files, fileName);
                }
                else
                {
                    files.Add(new PluginFile(fileName));
                }
            }

            if (!files.Any() && e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop, true))
            {
                var droppedFilePaths = e.Data.GetData(System.Windows.DataFormats.FileDrop, true) as string[];
                if (droppedFilePaths == null)
                {
                    return;
                }

                foreach (var fileName in droppedFilePaths)
                {
                    if (Directory.Exists(fileName))
                    {
                        AddFolder(files, fileName);
                    }
                    else
                    {
                        files.Add(new PluginFile(fileName));
                    }
                }
            }

            if (!files.Any() && e.Data.GetDataPresent(FileTreeViewItem.FileItemDragIdentifier))
            {
                var items = ((IEnumerable<BaseTreeViewItem>)e.Data.GetData(FileTreeViewItem.FileItemDragIdentifier)).ToList();
                if (!items.Any())
                {
                    return;
                }

                files.AddRange(items.OfType<FileTreeViewItem>().Select(i => new PluginFile(i.FileUri.FileName)));
            }

            if (!files.Any())
            {
                return;
            }

            InternalAddFiles(files);

            Modified = true;
            ShowFileList();

            Keyboard.Focus(FileList);
        }

        private void HandleFileListKeyDown([NotNull] object sender, [NotNull] System.Windows.Input.KeyEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.Key == Key.Delete)
            {
                RemoveItems(FileList, pluginFiles);
            }
        }

        private void OpenFolder([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (string.IsNullOrEmpty(FileName))
            {
                if (AppHost.MessageBox("The plugin has not yet been saved.\n\nDo you want to save it now?", "Information", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                {
                    return;
                }

                var fileName = GetFileName();
                if (string.IsNullOrEmpty(fileName))
                {
                    return;
                }

                FileName = fileName;
                if (!Save())
                {
                    return;
                }
            }

            var targetFile = Path.ChangeExtension(FileName, @".zip");

            IO.File.OpenInWindowsExplorer(targetFile);
        }

        private void RemoveItems<T>([NotNull] System.Windows.Controls.ListView listView, [NotNull] List<T> list) where T : class
        {
            Debug.ArgumentNotNull(listView, nameof(listView));
            Debug.ArgumentNotNull(list, nameof(list));

            var selectedItems = new List<object>();
            foreach (var selectedItem in listView.SelectedItems)
            {
                selectedItems.Add(selectedItem);
            }

            if (selectedItems.Count == 0)
            {
                return;
            }

            var isModified = false;
            var index = -1;

            foreach (var selectedItem in selectedItems)
            {
                var item = selectedItem as T;
                if (item == null)
                {
                    continue;
                }

                var n = list.IndexOf(item);

                if (index < 0)
                {
                    index = n;
                }
                else if (index > n)
                {
                    index--;
                }

                list.Remove(item);
                isModified = true;
            }

            if (isModified)
            {
                listView.ItemsSource = null;
                listView.ItemsSource = list;
            }

            if (index < 0)
            {
                index = 0;
            }

            if (index >= listView.Items.Count)
            {
                index = listView.Items.Count - 1;
            }

            if (index >= 0 && index < listView.Items.Count)
            {
                listView.SelectedIndex = index;
            }

            if (isModified)
            {
                Modified = true;
            }
        }

        private void SetModified([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (IsLoading)
            {
                return;
            }

            Modified = true;
        }

        private void ToolBarLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));
            Debug.ArgumentNotNull(sender, nameof(sender));

            this.InitializeToolBar(sender);
        }
    }
}
