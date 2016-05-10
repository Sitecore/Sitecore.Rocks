// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Data;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Indexes.Dialogs
{
    public class DocumentListContext : ICommandContext, IItemSelectionContext
    {
        private IEnumerable<IItem> items;

        public DocumentListContext([NotNull] DocumentList documentList)
        {
            Assert.ArgumentNotNull(documentList, nameof(documentList));

            DocumentList = documentList;
        }

        [NotNull]
        public DocumentList DocumentList { get; }

        [NotNull]
        public IEnumerable<IItem> Items
        {
            get
            {
                if (items == null)
                {
                    items = GetItems();
                }

                return items;
            }
        }

        [NotNull]
        public IEnumerable<IItem> GetItems()
        {
            var result = new List<IItem>();

            var urlColumn = -1;
            var nameColumn = -1;

            var site = DocumentList.Site;

            foreach (var selectedItem in DocumentList.DataGrid.SelectedItems)
            {
                var dataRowView = selectedItem as DataRowView;
                if (dataRowView == null)
                {
                    continue;
                }

                if (urlColumn < 0)
                {
                    for (var n = 0; n < dataRowView.Row.Table.Columns.Count; n++)
                    {
                        var column = dataRowView.Row.Table.Columns[n];

                        if (column.ColumnName == "_url")
                        {
                            urlColumn = n;
                        }
                        else if (column.ColumnName == "_name")
                        {
                            nameColumn = n;
                        }
                    }

                    if (urlColumn < 0)
                    {
                        return result;
                    }
                }

                var itemUri = GetItemUri(site, dataRowView.Row.ItemArray[urlColumn]);
                if (itemUri == ItemUri.Empty)
                {
                    continue;
                }

                var name = dataRowView.Row.ItemArray[nameColumn] as string;
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                result.Add(new DocumentItem(itemUri, name));
            }

            return result;
        }

        [NotNull]
        private ItemUri GetItemUri([NotNull] Site site, [CanBeNull] object o)
        {
            Debug.ArgumentNotNull(site, nameof(site));

            var text = o as string;
            if (string.IsNullOrEmpty(text))
            {
                return ItemUri.Empty;
            }

            if (!text.StartsWith("sitecore://"))
            {
                return ItemUri.Empty;
            }

            text = text.Mid(11);

            var n = text.IndexOf("/", StringComparison.Ordinal);

            var databaseName = text.Left(n);

            text = text.Mid(n + 1);

            n = text.IndexOf("?", StringComparison.Ordinal);
            if (n >= 0)
            {
                text = text.Left(n);
            }

            return new ItemUri(new DatabaseUri(site, new DatabaseName(databaseName)), new ItemId(new Guid(text)));
        }

        public class DocumentItem : IItem
        {
            public DocumentItem([NotNull] ItemUri itemUri, [NotNull] string name)
            {
                Assert.ArgumentNotNull(itemUri, nameof(itemUri));
                Assert.ArgumentNotNull(name, nameof(name));

                ItemUri = itemUri;
                Name = name;
            }

            [NotNull]
            public Icon Icon
            {
                get { return Icon.Empty; }
            }

            public ItemUri ItemUri { get; }

            public string Name { get; }
        }
    }
}
