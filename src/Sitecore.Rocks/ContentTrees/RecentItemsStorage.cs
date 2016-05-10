// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Applications.Storages;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees
{
    public class RecentItemsStorage
    {
        [NotNull]
        public IEnumerable<RecentItem> Load([NotNull] string storageKey)
        {
            Assert.ArgumentNotNull(storageKey, nameof(storageKey));

            var n = 0;

            var recentItems = new List<RecentItem>();

            while (n < 99)
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

                var item = new RecentItem(itemUri, name, icon, templateId, templateName);

                recentItems.Insert(0, item);

                n++;
            }

            return recentItems;
        }

        public void Save([NotNull] string storageKey, [NotNull] IEnumerable<RecentItem> items)
        {
            Assert.ArgumentNotNull(storageKey, nameof(storageKey));
            Assert.ArgumentNotNull(items, nameof(items));

            Storage.Delete(storageKey);

            var n = 0;
            foreach (var item in items)
            {
                AppHost.Settings.Set(storageKey, "itemuri" + n, item.ItemUri);
                AppHost.Settings.Set(storageKey, "name" + n, item.Name);
                AppHost.Settings.Set(storageKey, "icon" + n, item.Icon.IconPath);
                AppHost.Settings.Set(storageKey, "templateid" + n, item.TemplateId.ToString());
                AppHost.Settings.Set(storageKey, "templatename" + n, item.TemplateName);

                n++;
            }
        }

        public class RecentItem
        {
            public RecentItem([NotNull] ItemUri itemUri, [NotNull] string name, [NotNull] Icon icon, [NotNull] ItemId templateId, [NotNull] string templateName)
            {
                Assert.ArgumentNotNull(itemUri, nameof(itemUri));
                Assert.ArgumentNotNull(name, nameof(name));
                Assert.ArgumentNotNull(icon, nameof(icon));
                Assert.ArgumentNotNull(templateId, nameof(templateId));
                Assert.ArgumentNotNull(templateName, nameof(templateName));

                ItemUri = itemUri;
                Name = name;
                Icon = icon;
                TemplateId = templateId;
                TemplateName = templateName;
            }

            [NotNull]
            public Icon Icon { get; }

            [NotNull]
            public ItemUri ItemUri { get; }

            [NotNull]
            public string Name { get; }

            [NotNull]
            public ItemId TemplateId { get; }

            [NotNull]
            public string TemplateName { get; }
        }
    }
}
