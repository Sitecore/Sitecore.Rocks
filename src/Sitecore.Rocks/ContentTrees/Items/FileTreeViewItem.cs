// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Commands.Sorting.Files;
using Sitecore.Rocks.ContentTrees.Favorites;
using Sitecore.Rocks.ContentTrees.Items.SelectedObjects;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DataServiceExtensions;

namespace Sitecore.Rocks.ContentTrees.Items
{
    public class FileTreeViewItem : BaseSiteTreeViewItem, ICanRefresh, ICanDrag, IHasFileUri, IDragSetData, ISelectable
    {
        public const string FileItemDragIdentifier = "SitecoreFile";

        public FileTreeViewItem([NotNull] FileUri fileUri) : base(fileUri.Site)
        {
            Assert.ArgumentNotNull(fileUri, nameof(fileUri));

            FileUri = fileUri;
            Icon = new Icon("Resources/16x16/document.png");
        }

        public FileUri FileUri { get; }

        [NotNull]
        public string ServerFileName { get; set; }

        public DateTime Updated { get; set; }

        public override bool GetChildren(GetChildrenDelegate callback, bool async)
        {
            Assert.ArgumentNotNull(callback, nameof(callback));

            if (!async)
            {
                return false;
            }

            if (!FileUri.IsFolder)
            {
                OpenFile();
                return false;
            }

            Site.DataService.GetFiles(new DatabaseUri(Site, DatabaseName.Empty), FileUri.ToServerPath(), items => GetFiles(items, callback));

            return true;
        }

        public string GetDragIdentifier()
        {
            return FileItemDragIdentifier;
        }

        public void Initialize([NotNull] Favorite favorite)
        {
            Assert.ArgumentNotNull(favorite, nameof(favorite));

            Icon = favorite.Icon;
            ToolTip = favorite.FullPath;
        }

        private void GetFiles([NotNull] IEnumerable<ItemHeader> files, [NotNull] GetChildrenDelegate callback)
        {
            Debug.ArgumentNotNull(files, nameof(files));
            Debug.ArgumentNotNull(callback, nameof(callback));

            var result = new List<FileTreeViewItem>();

            foreach (var child in files)
            {
                var fileUri = new FileUri(FileUri.Site, child.Path, child.HasChildren);
                var fileName = ((IItemData)child).GetData("ServerFileName") ?? string.Empty;

                var item = new FileTreeViewItem(fileUri)
                {
                    Text = child.Name,
                    Updated = child.Updated,
                    ServerFileName = fileName
                };

                if (child.HasChildren)
                {
                    item.Icon = new Icon("Resources/16x16/folder.png");
                    item.Add(DummyTreeViewItem.Instance);
                }

                result.Add(item);
            }

            FileSortManager.Sort(FileUri, result);

            callback(result);
        }

        object ISelectable.GetSelectedObject()
        {
            return new FileUriSelectedObject(FileUri, ServerFileName);
        }

        private void OpenFile()
        {
            var webRootPath = Site.WebRootPath;
            if (string.IsNullOrEmpty(webRootPath))
            {
                return;
            }

            string fileName;
            if (FileUri.BaseFolder == FileUriBaseFolder.Data)
            {
                fileName = ServerFileName;
            }
            else
            {
                fileName = Path.Combine(webRootPath, FileUri.RelativeFileName);
            }

            if (!File.Exists(fileName))
            {
                return;
            }

            AppHost.Files.OpenFile(fileName);
        }

        void IDragSetData.SetData(DataObject dataObject)
        {
            Debug.ArgumentNotNull(dataObject, nameof(dataObject));

            dataObject.SetData(DataFormats.Text, FileUri.FileName.Replace('\\', '/'));
        }
    }
}
