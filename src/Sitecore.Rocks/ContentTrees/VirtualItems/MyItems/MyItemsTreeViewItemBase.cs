// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Dialogs;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DataServiceExtensions;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.ContentTrees.VirtualItems.MyItems
{
    public abstract class MyItemsTreeViewItemBase : BaseTreeViewItem
    {
        protected MyItemsTreeViewItemBase()
        {
            Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));

            Notifications.RegisterItemEvents(this, renamed: ItemRenamed, deleted: ItemDeleted);
            Notifications.RegisterSiteEvents(this, deleted: SiteDeleted);
        }

        [NotNull]
        public DatabaseUri DatabaseUri { get; set; }

        public override bool GetChildren(GetChildrenDelegate callback, bool async)
        {
            Assert.ArgumentNotNull(callback, nameof(callback));

            var busy = true;

            GetItemsCompleted<ItemHeader> c = delegate(IEnumerable<ItemHeader> items)
            {
                var result = new List<TemplatedTreeViewItem>();

                var count = items.Count();
                if (count > 100)
                {
                    count = LoadManyItemsDialog.Execute(count);
                    if (count < 0)
                    {
                        callback(result);
                        return;
                    }
                }

                for (var n = 0; n < count; n++)
                {
                    var item = items.ElementAt(n);

                    var i = GetTreeViewItem(item.ItemUri, item.Name, item.Icon, item.TemplateId, item.TemplateName);

                    result.Add(i);
                }

                callback(result);

                busy = false;
            };

            DatabaseUri.Site.DataService.SelectItems(GetQueryText(), DatabaseUri, c);

            if (!async)
            {
                while (busy)
                {
                    AppHost.DoEvents();
                }
            }

            return true;
        }

        [NotNull]
        protected abstract string GetQueryText();

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
                TemplateName = templateName,
                Foreground = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66))
            };

            return result;
        }

        private void ItemDeleted([NotNull] object sender, [NotNull] ItemUri itemUri)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));

            for (var n = Items.Count - 1; n >= 0; n--)
            {
                var item = Items[n] as TemplatedTreeViewItem;
                if (item == null)
                {
                    continue;
                }

                if (item.ItemUri == itemUri)
                {
                    Items.Remove(item);
                }
            }
        }

        private void ItemRenamed([NotNull] object sender, [NotNull] ItemUri itemUri, [NotNull] string newName)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(newName, nameof(newName));

            foreach (var t in Items)
            {
                var item = t as TemplatedTreeViewItem;
                if (item == null)
                {
                    continue;
                }

                if (item.ItemUri == itemUri)
                {
                    item.Text = newName;
                }
            }
        }

        private void SiteDeleted([NotNull] object sender, [NotNull] Site site)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(site, nameof(site));

            for (var n = Items.Count - 1; n >= 0; n--)
            {
                var item = Items[n] as TemplatedTreeViewItem;
                if (item == null)
                {
                    continue;
                }

                if (item.ItemUri.Site == site)
                {
                    Items.Remove(item);
                }
            }
        }
    }
}
