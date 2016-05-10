// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.UI.Libraries.QueryLibraries
{
    public class QueryLibrary : LibraryBase, IDynamicLibrary, IDisposable
    {
        private readonly ObservableCollection<IItem> items;

        private int bulkUpdating;

        private bool disposed;

        public QueryLibrary([NotNull] string fileName, [NotNull] string name) : base(fileName, name)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));
            Assert.ArgumentNotNull(name, nameof(name));

            items = new ObservableItemCollection(this);

            Query = string.Empty;
            Icon = new Icon("Resources/16x16/Folder-Edit.png");

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
        public string Query { get; set; }

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

            if (string.IsNullOrEmpty(Query))
            {
                return;
            }

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                LoadItems(root);
            };

            DatabaseUri.Site.DataService.ExecuteAsync("UI.XpathBuilder.Evaluate", completed, "/", DatabaseUri.DatabaseName.ToString(), Query, "sitecore");
        }

        public override void Save()
        {
            var writer = new StringWriter();
            var output = new XmlTextWriter(writer)
            {
                Formatting = Formatting.Indented
            };

            output.WriteStartElement("query");
            output.WriteAttributeString("name", Name);
            output.WriteAttributeString("databaseuri", DatabaseUri.ToString());
            output.WriteValue(Query);
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
                Query = root.Value;
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

        private void LoadItems([NotNull] XElement root)
        {
            Debug.ArgumentNotNull(root, nameof(root));

            Items.Clear();

            foreach (var element in root.Elements())
            {
                var name = element.GetAttributeValue("name");
                var itemId = element.GetAttributeValue("id");
                var icon = element.GetAttributeValue("icon");

                var itemUri = new ItemUri(DatabaseUri, new ItemId(new Guid(itemId)));

                var item = new LibraryItemDescriptor(items, itemUri, name, new Icon(DatabaseUri.Site, icon));

                Items.Add(item);
            }
        }

        ~QueryLibrary()
        {
            DisposeObject(false);
        }
    }
}
