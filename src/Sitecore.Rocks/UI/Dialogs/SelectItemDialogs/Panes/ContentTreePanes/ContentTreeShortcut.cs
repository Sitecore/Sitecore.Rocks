// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;

namespace Sitecore.Rocks.UI.Dialogs.SelectItemDialogs.Panes.ContentTreePanes
{
    [Export(typeof(IShortcut), Priority = 1000)]
    public class ContentTreeShortcut : IShortcut
    {
        public ContentTreeShortcut()
        {
            Header = "Sitecore Explorer Selection";
        }

        public string Header { get; }

        public void Close(SelectItemDialog selectItemDialog)
        {
            Assert.ArgumentNotNull(selectItemDialog, nameof(selectItemDialog));
        }

        public IEnumerable<IItem> GetItems(DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            var activeTree = ActiveContext.ActiveContentTree;
            if (activeTree == null)
            {
                return Enumerable.Empty<IItem>();
            }

            var selectedItem = activeTree.ContentTreeView.SelectedItem as ItemTreeViewItem;
            if (selectedItem == null)
            {
                return Enumerable.Empty<IItem>();
            }

            if (selectedItem.Item.ItemUri.DatabaseUri != databaseUri)
            {
                return Enumerable.Empty<IItem>();
            }

            return new List<IItem>()
            {
                selectedItem
            };
        }
    }
}
