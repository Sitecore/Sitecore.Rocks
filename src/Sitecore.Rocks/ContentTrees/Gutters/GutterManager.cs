// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI;

namespace Sitecore.Rocks.ContentTrees.Gutters
{
    public static class GutterManager
    {
        public static void UpdateGutter([NotNull] ItemUri itemUri)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            var databaseName = itemUri.DatabaseName.Name;
            var items = itemUri.ItemId.ToString();
            var site = itemUri.Site;

            RenderGutters(site, databaseName, items, false);
        }

        public static void UpdateGutter([NotNull] IEnumerable<ItemUri> list)
        {
            Assert.ArgumentNotNull(list, nameof(list));

            var databaseName = string.Empty;
            var items = string.Empty;
            Site site = null;

            foreach (var itemUri in list)
            {
                if (site == null)
                {
                    site = itemUri.Site;
                    databaseName = itemUri.DatabaseName.Name;
                }

                if (!string.IsNullOrEmpty(items))
                {
                    items += @"|";
                }

                items += itemUri.ItemId.ToString();
            }

            if (site == null)
            {
                return;
            }

            RenderGutters(site, databaseName, items, false);
        }

        private static void RenderGutters([NotNull] Site site, [NotNull] string databaseName, [NotNull] string items, bool subitems)
        {
            Debug.ArgumentNotNull(site, nameof(site));
            Debug.ArgumentNotNull(databaseName, nameof(databaseName));
            Debug.ArgumentNotNull(items, nameof(items));

            ExecuteCompleted executeCallback = (response, executeResult) => UpdateGutter(site, databaseName, response, executeResult);

            site.DataService.ExecuteAsync("GetGutterItems", executeCallback, databaseName, items, subitems);
        }

        private static void UpdateGutter([NotNull] Site site, [NotNull] string databaseName, [NotNull] string response, [NotNull] ExecuteResult executeResult)
        {
            Debug.ArgumentNotNull(site, nameof(site));
            Debug.ArgumentNotNull(databaseName, nameof(databaseName));
            Debug.ArgumentNotNull(response, nameof(response));
            Debug.ArgumentNotNull(executeResult, nameof(executeResult));

            if (!DataService.HandleExecute(response, executeResult))
            {
                return;
            }

            XDocument doc;
            try
            {
                doc = XDocument.Parse(response);
            }
            catch
            {
                return;
            }

            var root = doc.Root;
            if (root == null)
            {
                return;
            }

            foreach (var element in root.Elements())
            {
                UpdateItem(site, databaseName, element);
            }
        }

        private static void UpdateItem([NotNull] Site site, [NotNull] string databaseName, [NotNull] XElement element)
        {
            Debug.ArgumentNotNull(site, nameof(site));
            Debug.ArgumentNotNull(databaseName, nameof(databaseName));
            Debug.ArgumentNotNull(element, nameof(element));

            var id = Guid.Parse(element.GetAttributeValue("id"));

            var itemUri = new ItemUri(new DatabaseUri(site, new DatabaseName(databaseName)), new ItemId(id));

            var activeContentTree = ActiveContext.ActiveContentTree;
            if (activeContentTree == null)
            {
                return;
            }

            var itemTreeViewItem = activeContentTree.ItemTreeView.FindItem<ItemTreeViewItem>(itemUri);
            if (itemTreeViewItem == null)
            {
                return;
            }

            itemTreeViewItem.Item.Gutters.Clear();

            foreach (var g in element.Elements())
            {
                var icon = new Icon(itemUri.Site, g.GetAttributeValue("icon"));
                var toolTip = g.GetAttributeValue("tooltip");

                itemTreeViewItem.Item.Gutters.Add(new GutterDescriptor(icon, toolTip));
            }

            itemTreeViewItem.UpdateGutters();
        }
    }
}
