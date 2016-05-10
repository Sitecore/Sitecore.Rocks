// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Favorites;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DataServiceExtensions;

namespace Sitecore.Rocks.ContentTrees.Items
{
    public class RootFileTreeViewItem : BaseSiteTreeViewItem, ICanRefresh
    {
        public RootFileTreeViewItem([NotNull] FileUri fileUri) : base(fileUri.Site)
        {
            Assert.ArgumentNotNull(fileUri, nameof(fileUri));

            FileUri = fileUri;

            Icon = new Icon("Resources/16x16/folderclosed.png");
            Margin = new Thickness(0, 12, 0, 0);
        }

        [NotNull]
        public FileUri FileUri { get; }

        public override bool GetChildren(GetChildrenDelegate callback, bool async)
        {
            Assert.ArgumentNotNull(callback, nameof(callback));

            if (!async)
            {
                return false;
            }

            var path = FileUri.ToServerPath();

            Site.DataService.GetFiles(new DatabaseUri(Site, DatabaseName.Empty), path, items => GetFiles(items, callback));

            return true;
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

            var result = new List<BaseTreeViewItem>();

            foreach (var child in files)
            {
                var fileUri = new FileUri(Site, child.Path, child.HasChildren);
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

            callback(result);
        }
    }
}
