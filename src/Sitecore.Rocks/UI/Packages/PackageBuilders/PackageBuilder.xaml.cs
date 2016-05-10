// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;
using Sitecore.Rocks.Extensions.ListViewExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.UserControlExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.Dialogs.SelectSiteDialogs;
using Sitecore.Rocks.UI.Packages.PackageBuilders.Builders;
using Sitecore.Rocks.UI.Packages.PackageBuilders.Pipelines.LoadPackage;
using Sitecore.Rocks.UI.Packages.PackageBuilders.Pipelines.SavePackage;
using Sitecore.Rocks.UI.Packages.PackageManagers;

namespace Sitecore.Rocks.UI.Packages.PackageBuilders
{
    public partial class PackageBuilder : IContextProvider, IHasEditorPane, ISavable
    {
        private readonly ListViewSorter fileListSorter;

        private readonly ListViewSorter itemListSorter;

        private readonly List<PackageFile> packageFiles = new List<PackageFile>();

        private readonly List<PackageItem> packageItems = new List<PackageItem>();

        private bool modified;

        public PackageBuilder()
        {
            InitializeComponent();

            AppHost.Extensibility.ComposeParts(this);

            itemListSorter = new ListViewSorter(ItemList);
            fileListSorter = new ListViewSorter(FileList);

            NoItems.DragOver += HandleItemDragOver;
            NoItems.Drop += HandleItemDrop;
            ItemList.DragOver += HandleItemDragOver;
            ItemList.Drop += HandleItemDrop;

            NoFiles.DragOver += HandleFileDragOver;
            NoFiles.Drop += HandleFileDrop;
            FileList.DragOver += HandleFileDragOver;
            FileList.Drop += HandleFileDrop;

            RenderPackageFormats();
        }

        [CanBeNull]
        public string FileName { get; set; }

        [NotNull]
        public ICollection<PackageFile> Files
        {
            get { return packageFiles; }
        }

        [NotNull]
        public ICollection<PackageItem> Items
        {
            get { return packageItems; }
        }

        public bool Modified
        {
            get { return modified; }

            set
            {
                if (modified == value)
                {
                    return;
                }

                modified = value;

                Pane.SetModifiedFlag(value);
            }
        }

        [NotNull, ImportMany(typeof(IPackageBuilder))]
        public IEnumerable<IPackageBuilder> PackageBuilders { get; protected set; }

        public IEditorPane Pane { get; set; }

        [NotNull]
        public Site Site { get; set; }

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
                Save();
            }

            if (Modified)
            {
                Save();
            }

            var comboBoxItem = PackageFormatComboBox.SelectedItem as ComboBoxItem;
            if (comboBoxItem == null)
            {
                AppHost.MessageBox("Package Format must be set before building,", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var builder = comboBoxItem.Tag as IPackageBuilder;
            if (builder == null)
            {
                AppHost.MessageBox("Package Format must be set before building,", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            builder.Author = AuthorTextBox.Text;
            builder.Comment = CommentTextBox.Text;
            builder.FileName = FileName ?? string.Empty;
            builder.Files = FileList.Items.OfType<PackageFile>();
            builder.Items = ItemList.Items.OfType<PackageItem>();
            builder.License = LicenseEditor.Text;
            builder.PackageName = PackageNameTextBox.Text;
            builder.Publisher = PublisherTextBox.Text;
            builder.Readme = ReadmeTextBox.Text;
            builder.Site = Site;
            builder.Version = VersionTextBox.Text;
            builder.TargetFileFolder = TargetComboBox.SelectedIndex == 0 ? "wwwroot" : "content";

            if (!builder.IsValid())
            {
                return;
            }

            Action<string> completed = delegate(string targetFileName)
            {
                if (string.IsNullOrEmpty(targetFileName))
                {
                    BuildButton.IsEnabled = true;
                    return;
                }

                if (AppHost.MessageBox(string.Format("The package has been built and is located at:\n\n{0}\n\nDo you want to open it in Windows Explorer?", targetFileName), "Information", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                {
                    AppHost.Files.OpenInWindowsExplorer(targetFileName);
                }

                BuildButton.IsEnabled = true;
            };

            BuildButton.IsEnabled = false;

            builder.Build(completed);
        }

        public void Disable()
        {
            Designer.Visibility = Visibility.Collapsed;
            NotLoaded.Visibility = Visibility.Visible;
        }

        public object GetContext()
        {
            return new PackageBuilderContext(this, null);
        }

        public static bool Load([NotNull] string fileName, [CanBeNull] PackageBuilder packageBuilder, [CanBeNull] Site site)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            var text = AppHost.Files.ReadAllText(fileName);

            var root = text.ToXElement();
            if (root == null)
            {
                AppHost.MessageBox(Rocks.Resources.PackageBuilder_Load_File_is_not_a_valid_Package_file_, Rocks.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            var siteName = root.GetAttributeValue("site");
            if (!string.IsNullOrEmpty(siteName))
            {
                site = SiteManager.GetSite(siteName);
            }

            if (site == null)
            {
                var d = new SelectSiteDialog
                {
                    Title = "Package Connection",
                    LabelText = "Select the connection where the items are located."
                };

                if (AppHost.Shell.ShowDialog(d) != true)
                {
                    return false;
                }

                site = d.Site;
                if (site == null)
                {
                    return false;
                }
            }

            if (packageBuilder == null)
            {
                packageBuilder = AppHost.OpenDocumentWindow<PackageBuilder>(Path.GetFileName(fileName));
                if (packageBuilder == null)
                {
                    return false;
                }
            }

            packageBuilder.Site = site;
            packageBuilder.OpenPackage(fileName, root);

            return true;
        }

        public static bool Load([NotNull] IEnumerable<IItem> items)
        {
            Assert.ArgumentNotNull(items, nameof(items));

            var item = items.FirstOrDefault();
            if (item == null)
            {
                return false;
            }

            var packageBuilder = AppHost.OpenDocumentWindow<PackageBuilder>("Package");
            if (packageBuilder == null)
            {
                return false;
            }

            packageBuilder.Site = item.ItemUri.Site;
            packageBuilder.AddItems(items);

            return true;
        }

        public void OpenPackage([NotNull] string fileName, [NotNull] XElement root)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));
            Assert.ArgumentNotNull(root, nameof(root));

            IsLoading = true;
            try
            {
                FileName = fileName;

                LoadPackagePipeline.Run().WithParameters(this, Site, root);

                Keyboard.Focus(PackageNameTextBox);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void RemoveSelectedFiles()
        {
            RemoveItems(FileList, packageFiles);
        }

        public void RemoveSelectedItems()
        {
            RemoveItems(ItemList, packageItems);
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(FileName))
            {
                var fileName = GetFileName();
                if (string.IsNullOrEmpty(fileName))
                {
                    return;
                }

                FileName = fileName;
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer)
            {
                Formatting = Formatting.Indented
            };

            SavePackagePipeline.Run().WithParameters(this, output);

            File.WriteAllText(FileName, writer.ToString());

            Modified = false;
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

        public void ShowItemList()
        {
            ItemList.ItemsSource = null;
            ItemList.ItemsSource = Items;

            itemListSorter.Resort();

            NoItems.Visibility = Visibility.Collapsed;
            ItemListPanel.Visibility = Visibility.Visible;

            ItemList.ResizeColumn(ItemNameColumn);
            ItemList.ResizeColumn(ItemPathColumn);
            ItemList.ResizeColumn(ItemTemplateColumn);
            ItemList.ResizeColumn(ItemDatabaseColumn);
        }

        internal void InternalAddFiles([NotNull] IEnumerable<FileUri> files)
        {
            Debug.ArgumentNotNull(files, nameof(files));

            foreach (var fileUri in files)
            {
                var f = fileUri;

                if (Files.Any(i => i.FileUri == f))
                {
                    continue;
                }

                var packageFile = new PackageFile(fileUri);

                Files.Add(packageFile);
            }
        }

        internal bool InternalAddItems([NotNull] string response, [NotNull] ExecuteResult executeresult)
        {
            Debug.ArgumentNotNull(response, nameof(response));
            Debug.ArgumentNotNull(executeresult, nameof(executeresult));

            if (!DataService.HandleExecute(response, executeresult))
            {
                return false;
            }

            var root = response.ToXElement();
            if (root == null)
            {
                return false;
            }

            foreach (var element in root.Elements())
            {
                var databaseName = element.GetAttributeValue("database");

                var databaseUri = new DatabaseUri(Site, new DatabaseName(databaseName));

                var itemHeader = ItemHeader.Parse(databaseUri, element);

                if (Items.Any(i => i.ItemUri == itemHeader.ItemUri))
                {
                    continue;
                }

                var packageItem = new PackageItem(itemHeader);

                Items.Add(packageItem);
            }

            return true;
        }

        private void AddFolder([NotNull] List<FileUri> files, [NotNull] string folder)
        {
            Debug.ArgumentNotNull(files, nameof(files));
            Debug.ArgumentNotNull(folder, nameof(folder));

            foreach (var fileName in AppHost.Files.GetFiles(folder))
            {
                var f = fileName.Mid(Site.WebRootPath.Length);
                files.Add(new FileUri(Site, f, false));
            }

            foreach (var subfolder in AppHost.Files.GetDirectories(folder))
            {
                AddFolder(files, subfolder);
            }
        }

        private void AddItems([NotNull] string response, [NotNull] ExecuteResult executeresult)
        {
            Debug.ArgumentNotNull(response, nameof(response));
            Debug.ArgumentNotNull(executeresult, nameof(executeresult));

            if (!InternalAddItems(response, executeresult))
            {
                return;
            }

            Modified = true;
            ShowItemList();
            Keyboard.Focus(ItemList);
        }

        private void AddItems([NotNull] IEnumerable<IItem> items)
        {
            Debug.ArgumentNotNull(items, nameof(items));

            var sb = new StringBuilder();

            foreach (var item in items)
            {
                if (sb.Length > 0)
                {
                    sb.Append('|');
                }

                sb.Append(item.ItemUri.DatabaseName);
                sb.Append(',');
                sb.Append(item.ItemUri.ItemId);
            }

            var itemList = sb.ToString();
            if (string.IsNullOrEmpty(itemList))
            {
                return;
            }

            items.First().ItemUri.Site.DataService.ExecuteAsync("Items.GetItemHeaders", AddItems, itemList);
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
            var fileName = "NewPackage.package";
            if (!string.IsNullOrEmpty(PackageNameTextBox.Text))
            {
                fileName = PackageNameTextBox.Text + ".package";
            }

            var dialog = new SaveFileDialog
            {
                Title = "Save As",
                CheckPathExists = true,
                OverwritePrompt = true,
                FileName = fileName,
                Filter = "Packages (*.package)|*.package|All files|*.*"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return string.Empty;
            }

            Pane.Caption = Path.GetFileName(dialog.FileName);

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
                var fileName = e.Data.GetData("Text") as string ?? string.Empty;
                if (!string.IsNullOrEmpty(Site.WebRootPath) && fileName.StartsWith(Site.WebRootPath, StringComparison.InvariantCultureIgnoreCase))
                {
                    e.Effects = System.Windows.DragDropEffects.Copy;
                }

                return;
            }

            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop, true))
            {
                var droppedFilePaths = e.Data.GetData(System.Windows.DataFormats.FileDrop, true) as string[];
                if (droppedFilePaths == null)
                {
                    return;
                }

                if (!string.IsNullOrEmpty(Site.WebRootPath) && droppedFilePaths.All(p => p.StartsWith(Site.WebRootPath, StringComparison.InvariantCultureIgnoreCase)))
                {
                    e.Effects = System.Windows.DragDropEffects.Copy;
                }

                return;
            }

            if (e.Data.GetDataPresent(DragManager.DragIdentifier))
            {
                ItemsTab.IsSelected = true;
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

            var files = new List<FileUri>();

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
                    var f = fileName.Mid(Site.WebRootPath.Length);

                    files.Add(new FileUri(Site, f, Directory.Exists(fileName)));
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
                        var f = fileName.Mid(Site.WebRootPath.Length);

                        files.Add(new FileUri(Site, f, Directory.Exists(fileName)));
                    }
                }
            }

            if (!files.Any() && e.Data.GetDataPresent(FileTreeViewItem.FileItemDragIdentifier))
            {
                var items = (IEnumerable<BaseTreeViewItem>)e.Data.GetData(FileTreeViewItem.FileItemDragIdentifier);
                if (!items.Any())
                {
                    return;
                }

                files.AddRange(items.OfType<FileTreeViewItem>().Select(i => i.FileUri));
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
                RemoveItems(FileList, packageFiles);
            }
        }

        private void HandleItemDragOver([NotNull] object sender, [NotNull] System.Windows.DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Effects = System.Windows.DragDropEffects.None;
            e.Handled = true;

            if (e.Data.GetDataPresent(FileTreeViewItem.FileItemDragIdentifier))
            {
                FilesTab.IsSelected = true;
                return;
            }

            if (!e.Data.GetDataPresent(DragManager.DragIdentifier))
            {
                return;
            }

            e.Effects = System.Windows.DragDropEffects.Copy;
        }

        private void HandleItemDrop([NotNull] object sender, [NotNull] System.Windows.DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Handled = true;

            if (!e.Data.GetDataPresent(DragManager.DragIdentifier))
            {
                return;
            }

            var items = (IEnumerable<IItem>)e.Data.GetData(DragManager.DragIdentifier);
            if (!items.Any())
            {
                return;
            }

            if (items.Any(item => item.ItemUri.Site != Site))
            {
                AppHost.MessageBox(string.Format(Rocks.Resources.PackageBuilder_HandleItemDrop_, Site.Name), Rocks.Resources.Information, MessageBoxButton.OK, MessageBoxImage.Information);
            }

            AddItems(items);
        }

        private void HandleItemListKeyDown([NotNull] object sender, [NotNull] System.Windows.Input.KeyEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.Key == Key.Delete)
            {
                RemoveItems(ItemList, packageItems);
            }
        }

        private void ItemListHeaderClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            itemListSorter.HeaderClick(sender, e);
        }

        private void Manage([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var d = new PackageManagerDialog();

            d.Initialize(Site);

            AppHost.Shell.ShowDialog(d);
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = new PackageBuilderContext(this, sender);

            ContextMenu = AppHost.ContextMenus.Build(context, e);
        }

        private void OpenFolder([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (string.IsNullOrEmpty(FileName))
            {
                if (AppHost.MessageBox("The package has not yet been saved.\n\nDo you want to save it now?", "Information", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                {
                    return;
                }

                var fileName = GetFileName();
                if (string.IsNullOrEmpty(fileName))
                {
                    return;
                }

                FileName = fileName;
                Save();
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

        private void RenderPackageFormats()
        {
            foreach (var packageBuilder in PackageBuilders)
            {
                var comboBoxItem = new ComboBoxItem
                {
                    Content = packageBuilder.Name,
                    Tag = packageBuilder
                };

                PackageFormatComboBox.Items.Add(comboBoxItem);
            }

            PackageFormatComboBox.SelectedIndex = 0;
            Modified = false;

            PackageFormatComboBox.SelectionChanged += SetModified;
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

        private void SetModified([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));
            Debug.ArgumentNotNull(sender, nameof(sender));

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
