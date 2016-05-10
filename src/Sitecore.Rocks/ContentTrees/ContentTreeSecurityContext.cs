// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.ContentTrees
{
    public class ContentTreeSecurityContext : ICommandContext, IItemSelectionContext, IDatabaseSelectionContext, ISiteSelectionContext
    {
        public ContentTreeSecurityContext([NotNull] ItemTreeView contentTree, [NotNull] IEnumerable<BaseTreeViewItem> selectedItems)
        {
            Assert.ArgumentNotNull(contentTree, nameof(contentTree));
            Assert.ArgumentNotNull(selectedItems, nameof(selectedItems));

            ContentTree = contentTree;
            SelectedItems = selectedItems;
        }

        [NotNull]
        public ItemTreeView ContentTree { get; private set; }

        [NotNull]
        public IEnumerable<BaseTreeViewItem> SelectedItems { get; set; }

        DatabaseUri IDatabaseSelectionContext.DatabaseUri
        {
            get
            {
                DatabaseUri result = null;

                foreach (var baseTreeViewItem in SelectedItems)
                {
                    DatabaseUri d = null;

                    var item = baseTreeViewItem as IItem;
                    if (item != null)
                    {
                        d = item.ItemUri.DatabaseUri;
                    }

                    var databaseTreeViewItem = baseTreeViewItem as DatabaseTreeViewItem;
                    if (databaseTreeViewItem != null)
                    {
                        d = databaseTreeViewItem.DatabaseUri;
                    }

                    if (d == null)
                    {
                        return DatabaseUri.Empty;
                    }

                    if (result == null)
                    {
                        result = d;
                    }
                    else if (result != d)
                    {
                        return DatabaseUri.Empty;
                    }
                }

                return result ?? DatabaseUri.Empty;
            }
        }

        IEnumerable<IItem> IItemSelectionContext.Items
        {
            get
            {
                foreach (var item in SelectedItems)
                {
                    if (!(item is IItem))
                    {
                        yield break;
                    }
                }

                foreach (var item in SelectedItems.OfType<IItem>())
                {
                    yield return item;
                }
            }
        }

        Site ISiteSelectionContext.Site
        {
            get
            {
                Site result = null;

                foreach (var baseTreeViewItem in SelectedItems)
                {
                    Site s = null;

                    var item = baseTreeViewItem as IItem;
                    if (item != null)
                    {
                        s = item.ItemUri.Site;
                    }

                    var databaseTreeViewItem = baseTreeViewItem as DatabaseTreeViewItem;
                    if (databaseTreeViewItem != null)
                    {
                        s = databaseTreeViewItem.DatabaseUri.Site;
                    }

                    var siteTreeViewItem = baseTreeViewItem as SiteTreeViewItem;
                    if (siteTreeViewItem != null)
                    {
                        s = siteTreeViewItem.Site;
                    }

                    if (s == null)
                    {
                        return Site.Empty;
                    }

                    if (result == null)
                    {
                        result = s;
                    }
                    else if (result != s)
                    {
                        return Site.Empty;
                    }
                }

                return result ?? Site.Empty;
            }
        }
    }
}
