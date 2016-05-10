// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DateTimeExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.UI.Management.ManagementItems.TempFiles
{
    [Management(ItemName, 4000)]
    public partial class TempFileViewer : IManagementItem, IContextProvider
    {
        public const string ItemName = "Temp Files";

        private readonly List<FileFolder> fileFolders = new List<FileFolder>();

        public TempFileViewer()
        {
            InitializeComponent();

            Loaded += ControlLoaded;
        }

        [NotNull]
        public SiteManagementContext Context { get; set; }

        public bool CanExecute(IManagementContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            var site = context as SiteManagementContext;
            if (site == null)
            {
                return false;
            }

            return site.Site.DataService.CanExecuteAsync("Files.GetTempFolders");
        }

        [NotNull]
        public object GetContext()
        {
            return new TempFileViewerContext(this);
        }

        public UIElement GetControl(IManagementContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            Context = (SiteManagementContext)context;

            return this;
        }

        public void LoadFileFolders()
        {
            ExecuteCompleted callback = delegate(string response, ExecuteResult executeResult)
            {
                Loading.Swap(FolderList);

                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                ParseFileFolders(root);

                RenderFileFolders();
            };

            Loading.ShowLoading(FolderList);

            Context.Site.DataService.ExecuteAsync("Files.GetTempFolders", callback);
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            LoadFileFolders();
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = GetContext();

            ContextMenu = AppHost.ContextMenus.Build(context, e);
        }

        private void ParseFileFolders([NotNull] XElement root)
        {
            Debug.ArgumentNotNull(root, nameof(root));

            fileFolders.Clear();

            foreach (var element in root.Elements())
            {
                var lastWrite = DateTimeExtensions.FromIso(element.GetAttributeValue("lastwrite"));

                var fileFolder = new FileFolder
                {
                    Name = element.GetAttributeValue("name"),
                    Folder = element.GetAttributeValue("path"),
                    Size = element.GetAttributeLong("size", 0),
                    LastWrite = lastWrite
                };

                fileFolders.Add(fileFolder);
            }
        }

        private void RenderFileFolders()
        {
            FolderList.ItemsSource = null;
            FolderList.ItemsSource = fileFolders;

            ResizeGridViewColumn(FolderColumn);
            ResizeGridViewColumn(SizeColumn);

            if (fileFolders.Count > 0)
            {
                FolderList.SelectedIndex = 0;
            }
        }

        private void ResizeGridViewColumn([NotNull] GridViewColumn column)
        {
            Debug.ArgumentNotNull(column, nameof(column));

            if (double.IsNaN(column.Width))
            {
                column.Width = column.ActualWidth;
            }

            column.Width = double.NaN;
        }

        public class FileFolder
        {
            public string Folder { get; set; }

            [NotNull]
            public string FormattedLastWrite
            {
                get
                {
                    if (LastWrite == DateTime.MinValue)
                    {
                        return string.Empty;
                    }

                    return LastWrite.ToString();
                }
            }

            [NotNull]
            public string FormattedSize
            {
                get { return Size.ToString("#,##0 bytes"); }
            }

            public DateTime LastWrite { get; set; }

            public string Name { get; set; }

            public long Size { get; set; }
        }
    }
}
