// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.ContentTrees.Favorites;
using Sitecore.Rocks.ContentTrees.VirtualItems.Favorites;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;

namespace Sitecore.Rocks.UI.Dialogs.SelectItemDialogs.Panes.ContentTreePanes
{
    [Export(typeof(IShortcut), Priority = 2000)]
    public class FavoriteShortcut : IShortcut
    {
        public FavoriteShortcut()
        {
            Header = "Favorites";
        }

        public string Header { get; }

        public void Close(SelectItemDialog selectItemDialog)
        {
            Assert.ArgumentNotNull(selectItemDialog, nameof(selectItemDialog));
        }

        public IEnumerable<IItem> GetItems(DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            foreach (var favorite in FavoriteManager.GetFavorites())
            {
                if (favorite.ItemVersionUri.DatabaseUri != databaseUri)
                {
                    continue;
                }

                var item = new FavoriteTreeViewItem();
                item.Initialize(favorite);

                yield return item;
            }
        }
    }
}
