// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DataServiceExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.UI.Libraries.SearchLibraries
{
    public class SearchLibrary : LibraryBase, IDynamicLibrary, IDisposable
    {
        private readonly ObservableCollection<IItem> items;

        private int bulkUpdating;

        private bool disposed;

        public SearchLibrary([NotNull] string fileName, [NotNull] string name) : base(fileName, name)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));
            Assert.ArgumentNotNull(name, nameof(name));

            items = new ObservableItemCollection(this);

            Icon = new Icon("Resources/16x16/Folder-Search.png");

            Notifications.ItemRenamed += ItemRenamed;
            Notifications.ItemDeleted += ItemDeleted;
        }

        public SearchLibrary([NotNull] string fileName, [NotNull] string name, [NotNull] DatabaseUri databaseUri, [NotNull] string searchText) : base(fileName, name)
        {
            items = new ObservableItemCollection(this);
            Assert.ArgumentNotNull(fileName, nameof(fileName));
            Assert.ArgumentNotNull(name, nameof(name));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(searchText, nameof(searchText));

            DatabaseUri = databaseUri;
            SearchText = searchText;

            Notifications.ItemRenamed += ItemRenamed;
            Notifications.ItemDeleted += ItemDeleted;
        }

        [NotNull]
        public DatabaseUri DatabaseUri { get; set; }

        [NotNull]
        public override ObservableCollection<IItem> Items
        {
            get { return items; }
        }

        [NotNull]
        public string SearchText { get; set; }

        public void Dispose()
        {
            DisposeObject(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Initialize()
        {
            Load();
        }

        public void Refresh()
        {
            Items.Clear();

            GetItemsCompleted<ItemHeader> completed = delegate(IEnumerable<ItemHeader> headers)
            {
                foreach (var itemHeader in headers)
                {
                    items.Add(itemHeader);
                }
            };

            DatabaseUri.Site.DataService.Search(SearchText, DatabaseUri, string.Empty, string.Empty, ItemUri.Empty, 0, completed);
        }

        public override void Save()
        {
            var writer = new StringWriter();
            var output = new XmlTextWriter(writer)
            {
                Formatting = Formatting.Indented
            };

            output.WriteStartElement("search");
            output.WriteAttributeString("name", Name);
            output.WriteAttributeString("databaseuri", DatabaseUri.ToString());
            output.WriteValue(SearchText);
            output.WriteEndElement();

            output.Flush();

            AppHost.Files.CreateDirectory(Path.GetDirectoryName(FileName));
            AppHost.Files.WriteAllText(FileName, writer.ToString());
        }

        protected virtual void Load()
        {
            bulkUpdating++;
            try
            {
                items.Clear();

                DatabaseUri = DatabaseUri.Empty;
                SearchText = string.Empty;
                Name = string.Empty;

                var contents = AppHost.Files.ReadAllText(FileName);

                var root = contents.ToXElement();
                if (root == null)
                {
                    return;
                }

                var folderName = root.GetAttributeValue("name");
                if (!string.IsNullOrEmpty(folderName))
                {
                    Name = folderName;
                }

                DatabaseUri databaseUri;
                if (!DatabaseUri.TryParse(root.GetAttributeValue("databaseuri"), out databaseUri))
                {
                    return;
                }

                if (databaseUri == DatabaseUri.Empty)
                {
                    return;
                }

                DatabaseUri = databaseUri;
                SearchText = root.Value;
            }
            finally
            {
                bulkUpdating--;
            }
        }

        private void DisposeObject(bool disposing)
        {
            if (!disposed)
            {
                Notifications.ItemRenamed -= ItemRenamed;
                Notifications.ItemDeleted -= ItemDeleted;
            }

            disposed = true;
        }

        private void ItemDeleted([NotNull] object sender, [NotNull] ItemUri itemuri)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(itemuri, nameof(itemuri));

            for (var index = items.Count - 1; index >= 0; index--)
            {
                var item = items[index];

                if (item.ItemUri == itemuri)
                {
                    items.RemoveAt(index);
                }
            }
        }

        private void ItemRenamed([NotNull] object sender, [NotNull] ItemUri itemuri, [NotNull] string newName)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(itemuri, nameof(itemuri));
            Debug.ArgumentNotNull(newName, nameof(newName));

            foreach (var source in items.Where(i => i.ItemUri == itemuri))
            {
                var item = source as LibraryItemDescriptor;
                if (item != null)
                {
                    item.Name = newName;
                }
            }
        }

        ~SearchLibrary()
        {
            DisposeObject(false);
        }
    }
}
