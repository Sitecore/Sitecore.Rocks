// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Xml;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.UI.Libraries.ItemLibraries
{
    public class ItemLibrary : LibraryBase, IDisposable
    {
        private readonly ObservableCollection<IItem> items = new ObservableCollection<IItem>();

        private int bulkUpdating;

        private bool disposed;

        public ItemLibrary([NotNull] string fileName, [NotNull] string name) : base(fileName, name)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));
            Assert.ArgumentNotNull(name, nameof(name));

            items.CollectionChanged += Changed;

            Icon = new Icon("Resources/16x16/Folder-Closed.png");

            Notifications.ItemRenamed += ItemRenamed;
            Notifications.ItemDeleted += ItemDeleted;
        }

        [NotNull]
        public override ObservableCollection<IItem> Items
        {
            get { return items; }
        }

        public void Add([NotNull] IEnumerable<IItem> items)
        {
            Assert.ArgumentNotNull(items, nameof(items));

            var list = items.ToList();

            bulkUpdating++;
            try
            {
                for (var index = list.Count - 1; index >= 0; index--)
                {
                    var item = list[index];

                    foreach (var source in Items.Where(itm => itm.ItemUri == item.ItemUri).ToList())
                    {
                        Items.Remove(source);
                    }

                    Items.Add(new LibraryItemDescriptor(this.items, item.ItemUri, item.Name, item.Icon));
                }
            }
            finally
            {
                bulkUpdating--;
            }

            Save();
        }

        public void Dispose()
        {
            DisposeObject(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Initialize()
        {
            Load();
        }

        public override void Save()
        {
            var writer = new StringWriter();
            var output = new XmlTextWriter(writer)
            {
                Formatting = Formatting.Indented
            };

            output.WriteStartElement("items");
            output.WriteAttributeString("name", Name);

            foreach (var item in items)
            {
                output.WriteStartElement("item");

                output.WriteAttributeString("name", item.Name);
                output.WriteAttributeString("itemuri", item.ItemUri.ToString());
                output.WriteAttributeString("icon", item.Icon.IconPath);

                output.WriteEndElement();
            }

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

                foreach (var element in root.Elements())
                {
                    ItemUri itemUri;
                    if (!ItemUri.TryParse(element.GetAttributeValue("itemuri"), out itemUri))
                    {
                        continue;
                    }

                    var name = element.GetAttributeValue("name");
                    var icon = new Icon(element.GetAttributeValue("icon"));

                    var i = new LibraryItemDescriptor(items, itemUri, name, icon);

                    items.Add(i);
                }
            }
            finally
            {
                bulkUpdating--;
            }
        }

        private void Changed([NotNull] object sender, [NotNull] NotifyCollectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (bulkUpdating > 0)
            {
                return;
            }

            Save();
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

        ~ItemLibrary()
        {
            DisposeObject(false);
        }
    }
}
