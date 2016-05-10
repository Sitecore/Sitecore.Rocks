// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.ContentTrees
{
    public class Find
    {
        [NotNull]
        public string Execute([NotNull] string text, [NotNull] string id, [NotNull] string databaseName)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(id, nameof(id));
            Assert.ArgumentNotNull(text, nameof(text));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            if (ShortID.IsShortID(id))
            {
                id = ShortID.Decode(id);
            }

            var start = database.GetItem(id);

            var item = start;
            if (item == null)
            {
                item = database.GetRootItem();
            }

            var result = FindItem(item, ID.Null, text);

            if (result == null && start != null)
            {
                result = FindItem(database.GetRootItem(), start.ID, text);
            }

            if (result != null)
            {
                return result.ID.ToString();
            }

            return string.Empty;
        }

        [CanBeNull]
        public Item FindItem([NotNull] Item item, [NotNull] ID rootId, [NotNull] string text)
        {
            Assert.ArgumentNotNull(item, nameof(item));
            Assert.ArgumentNotNull(rootId, nameof(rootId));
            Assert.ArgumentNotNull(text, nameof(text));

            var result = item;

            while (result != null)
            {
                result = Next(result, rootId);
                if (result == null)
                {
                    continue;
                }

                if (result.ID.ToString() == text)
                {
                    return result;
                }

                if (IsFilterMatch(result.Name, text))
                {
                    return result;
                }
            }

            return result;
        }

        [NotNull]
        private bool IsFilterMatch([CanBeNull] string text, [NotNull] string filterText)
        {
            Assert.ArgumentNotNull(filterText, nameof(filterText));

            if (text == null)
            {
                return false;
            }

            return text.IndexOf(filterText, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        [CanBeNull]
        private Item Next([NotNull] Item item, [NotNull] ID rootId)
        {
            Debug.ArgumentNotNull(item, nameof(item));
            Debug.ArgumentNotNull(rootId, nameof(rootId));

            if (item.HasChildren)
            {
                return item.Children[0];
            }

            if (item.ID == rootId)
            {
                return null;
            }

            var nextSibling = item.Axes.GetNextSibling();
            if (nextSibling != null)
            {
                return nextSibling;
            }

            var parent = item;
            while (true)
            {
                parent = parent.Parent;
                if (parent == null)
                {
                    break;
                }

                if (parent.ID == rootId)
                {
                    return null;
                }

                var next = parent.Axes.GetNextSibling();
                if (next != null)
                {
                    return next;
                }
            }

            return null;
        }
    }
}
