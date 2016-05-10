// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Applications.Storages;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.ContentTrees.VirtualItems.RecentItems
{
    public abstract class RecentItemsTreeViewItemBase : BaseTreeViewItem
    {
        private readonly List<TemplatedTreeViewItem> recentItems = new List<TemplatedTreeViewItem>();

        protected RecentItemsTreeViewItemBase()
        {
            MaxItems = 10;

            Notifications.RegisterItemEvents(this, renamed: ItemRenamed, deleted: ItemDeleted);
            Notifications.RegisterSiteEvents(this, deleted: SiteDeleted);

            Load();
        }

        protected int MaxItems { get; set; }

        public void Clear()
        {
            recentItems.Clear();
            Items.Clear();
            Save();
        }

        public override bool GetChildren(GetChildrenDelegate callback, bool async)
        {
            Assert.ArgumentNotNull(callback, nameof(callback));

            callback(recentItems);

            return true;
        }

        protected void AddToJournal([NotNull] ITemplatedItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var treeViewItem = GetTreeViewItem(item.ItemUri, item.Name, item.Icon, item.TemplateId, item.TemplateName);
            if (treeViewItem == null)
            {
                return;
            }

            for (var n = recentItems.Count - 1; n >= 0; n--)
            {
                var recentItem = recentItems[n];

                if (recentItem.ItemUri == item.ItemUri)
                {
                    recentItems.Remove(recentItem);
                    Items.Remove(recentItem);
                }
            }

            recentItems.Add(treeViewItem);

            Insert(0, treeViewItem);

            while (recentItems.Count > MaxItems)
            {
                Items.RemoveAt(0);
                recentItems.RemoveAt(0);
            }

            Save();
        }

        [NotNull, Localizable(false)]
        protected abstract string GetStorageKey();

        [CanBeNull]
        protected virtual TemplatedTreeViewItem GetTreeViewItem([NotNull] ItemUri itemUri, [NotNull] string name, [NotNull] Icon icon, [NotNull] ItemId templateId, [NotNull] string templateName)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(name, nameof(name));
            Debug.ArgumentNotNull(icon, nameof(icon));
            Debug.ArgumentNotNull(templateId, nameof(templateId));
            Debug.ArgumentNotNull(templateName, nameof(templateName));

            var result = new TemplatedTreeViewItem
            {
                ItemUri = itemUri,
                Icon = icon,
                Text = name,
                TemplateId = templateId,
                TemplateName = templateName
            };

            return result;
        }

        private void ItemDeleted([NotNull] object sender, [NotNull] ItemUri itemUri)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));

            var changed = false;
            for (var n = recentItems.Count - 1; n >= 0; n--)
            {
                var item = recentItems[n];

                if (item.ItemUri == itemUri)
                {
                    Items.Remove(item);
                    recentItems.Remove(item);
                    changed = true;
                }
            }

            if (changed)
            {
                Save();
            }
        }

        private void ItemRenamed([NotNull] object sender, [NotNull] ItemUri itemUri, [NotNull] string newName)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(newName, nameof(newName));

            var changed = false;
            foreach (var item in recentItems.Where(item => item.ItemUri == itemUri))
            {
                item.Text = newName;
                changed = true;
            }

            if (changed)
            {
                Save();
            }
        }

        private void Load()
        {
            var storageKey = GetStorageKey();
            var n = 0;

            while (n < MaxItems)
            {
                var uri = AppHost.Settings.Get(storageKey, "itemuri" + n, null) as string;
                if (string.IsNullOrEmpty(uri))
                {
                    break;
                }

                ItemUri itemUri;
                if (!ItemUri.TryParse(uri, out itemUri))
                {
                    n++;
                    continue;
                }

                var name = AppHost.Settings.Get(storageKey, "name" + n, string.Empty) as string ?? string.Empty;
                var icon = new Icon(itemUri.Site, AppHost.Settings.Get(storageKey, "icon" + n, string.Empty) as string ?? string.Empty);
                var templateId = new ItemId(new Guid(AppHost.Settings.Get(storageKey, "templateid" + n, string.Empty) as string ?? string.Empty));
                var templateName = AppHost.Settings.Get(storageKey, "templatename" + n, string.Empty) as string ?? string.Empty;

                var item = GetTreeViewItem(itemUri, name, icon, templateId, templateName);

                recentItems.Insert(0, item);

                n++;
            }
        }

        private void Save()
        {
            var storageKey = GetStorageKey();

            Storage.Delete(storageKey);

            for (var n = 0; n < recentItems.Count; n++)
            {
                var item = recentItems[n];

                AppHost.Settings.Set(storageKey, "itemuri" + n, item.ItemUri);
                AppHost.Settings.Set(storageKey, "name" + n, item.Text);
                AppHost.Settings.Set(storageKey, "icon" + n, item.Icon.IconPath);
                AppHost.Settings.Set(storageKey, "templateid" + n, item.TemplateId.ToString());
                AppHost.Settings.Set(storageKey, "templatename" + n, item.TemplateName);
            }
        }

        private void SiteDeleted([NotNull] object sender, [NotNull] Site site)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(site, nameof(site));

            var changed = false;
            for (var n = recentItems.Count - 1; n >= 0; n--)
            {
                var item = recentItems[n];

                if (item.ItemUri.Site == site)
                {
                    Items.Remove(item);
                    recentItems.Remove(item);
                    changed = true;
                }
            }

            if (changed)
            {
                Save();
            }
        }
    }
}
