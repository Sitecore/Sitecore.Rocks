// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ListViewExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.Packages.AnalyzePackage
{
    public partial class AnalyzePackageDialog : IContextProvider
    {
        private readonly ListViewSorter fileListSorter;

        private readonly List<PackageFile> files = new List<PackageFile>();

        private readonly ListViewSorter itemListSorter;

        private readonly List<PackageItem> items = new List<PackageItem>();

        public AnalyzePackageDialog()
        {
            InitializeComponent();
            this.InitializeDialog();

            itemListSorter = new ListViewSorter(ItemList);
            fileListSorter = new ListViewSorter(FileList);

            Loaded += ControlLoaded;
        }

        [NotNull]
        public string PostStep { get; set; }

        [NotNull]
        public Site Site { get; private set; }

        [NotNull]
        protected string ServerFileName { get; private set; }

        [NotNull]
        public object GetContext()
        {
            return new AnalyzePackageDialogContext(this);
        }

        public void Initialize([NotNull] Site site, [NotNull] string serverFileName)
        {
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(serverFileName, nameof(serverFileName));

            Site = site;
            ServerFileName = serverFileName;
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                Loading.HideLoading(Tabs);

                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                Parse(root);

                RenderAnalysis();
            };

            Loading.ShowLoading(Tabs);

            Site.DataService.ExecuteAsync("Packages.AnalyzePackage", completed, ServerFileName);
        }

        private void FileListHeaderClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            fileListSorter.HeaderClick(sender, e);
        }

        private void ItemListHeaderClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            itemListSorter.HeaderClick(sender, e);
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(true);
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = GetContext();

            ItemListPanel.ContextMenu = AppHost.ContextMenus.Build(context, e);
        }

        private void Parse([NotNull] XElement element)
        {
            Debug.ArgumentNotNull(element, nameof(element));

            items.Clear();
            files.Clear();

            PostStep = element.GetAttributeValue("poststep");

            foreach (var item in element.Elements(@"item"))
            {
                items.Add(new PackageItem(Site, item));
            }

            foreach (var file in element.Elements(@"file"))
            {
                files.Add(new PackageFile(file));
            }
        }

        private void RenderAnalysis()
        {
            if (string.IsNullOrEmpty(PostStep))
            {
                PostStepTextBlock.Text = "The package does not contain a post step.";
            }
            else
            {
                PostStepTextBlock.Text = string.Format("The package executes the following post step: {0}", PostStep);
            }

            ItemList.ItemsSource = null;
            ItemList.ItemsSource = items;

            ItemList.ResizeColumn(ItemNameColumn);
            ItemList.ResizeColumn(ItemPathColumn);
            ItemList.ResizeColumn(ItemDatabaseColumn);
            ItemList.ResizeColumn(ItemActionColumn);

            FileList.ItemsSource = null;
            FileList.ItemsSource = files;

            FileList.ResizeColumn(FileNameColumn);
            FileList.ResizeColumn(FolderPathColumn);
            ItemList.ResizeColumn(FileActionColumn);
        }

        public class PackageFile
        {
            public PackageFile([NotNull] XElement element)
            {
                Assert.ArgumentNotNull(element, nameof(element));

                Element = element;

                var fileName = element.GetAttributeValue("filename");

                FileName = Path.GetFileName(fileName);
                FolderPath = Path.GetDirectoryName(fileName);
                Action = element.GetAttributeValue("action");
            }

            [NotNull]
            public string Action { get; private set; }

            [NotNull]
            public XElement Element { get; private set; }

            [NotNull]
            public string FileName { get; private set; }

            [NotNull]
            public string FolderPath { get; private set; }
        }

        public class PackageItem : IItem
        {
            public PackageItem([NotNull] Site site, [NotNull] XElement element)
            {
                Assert.ArgumentNotNull(site, nameof(site));
                Assert.ArgumentNotNull(element, nameof(element));

                Site = site;
                Element = element;
                DatabaseName = element.GetAttributeValue("databasename");
                ItemId = new ItemId(new Guid(element.GetAttributeValue("id")));
                Language = element.GetAttributeValue("language");
                Version = element.GetAttributeValue("version");
                Revision = element.GetAttributeValue("revision");
                ItemName = element.GetAttributeValue("name");
                Path = element.GetAttributeValue("path");
                Action = element.GetAttributeValue("action");
            }

            [NotNull]
            public string Action { get; private set; }

            [NotNull]
            public string DatabaseName { get; }

            [NotNull]
            public XElement Element { get; private set; }

            public Icon Icon
            {
                get { return Icon.Empty; }
            }

            [NotNull]
            public ItemId ItemId { get; }

            [NotNull]
            public string ItemName { get; }

            public ItemUri ItemUri
            {
                get { return new ItemUri(new DatabaseUri(Site, new DatabaseName(DatabaseName)), ItemId); }
            }

            [NotNull]
            public string Language { get; private set; }

            public string Name
            {
                get { return ItemName; }
            }

            [NotNull]
            public string Path { get; private set; }

            [NotNull]
            public string Revision { get; private set; }

            [NotNull]
            public Site Site { get; }

            [NotNull]
            public string Version { get; private set; }
        }
    }
}
