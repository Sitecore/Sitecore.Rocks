// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Searching
{
    public class SearchContext : ICommandContext, IItemSelectionContext, ISiteContext, IDatabaseSelectionContext, ISiteSelectionContext, IDeleteItemContext
    {
        public SearchContext([NotNull] SearchViewer searchViewer)
        {
            Assert.ArgumentNotNull(searchViewer, nameof(searchViewer));

            SearchViewer = searchViewer;
        }

        [NotNull]
        public IEnumerable<IItem> Items
        {
            get { return SelectedItems; }
        }

        [NotNull]
        public SearchViewer SearchViewer { get; }

        [NotNull]
        public IEnumerable<ItemHeader> SelectedItems { get; set; }

        public Site Site
        {
            get { return SearchViewer.Site; }
        }

        DatabaseUri IDatabaseSelectionContext.DatabaseUri
        {
            get
            {
                var itemHeader = SelectedItems.FirstOrDefault();
                if (itemHeader == null)
                {
                    return DatabaseUri.Empty;
                }

                return itemHeader.ItemUri.DatabaseUri;
            }
        }

        Site ISiteSelectionContext.Site
        {
            get { return ((IDatabaseSelectionContext)this).DatabaseUri.Site; }
        }

        public void SetSite(Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            SearchViewer.SetSite(site);
        }
    }
}
