// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Media
{
    public class MediaContext : ICommandContext, IItemSelectionContext, ISiteContext, IDatabaseSelectionContext, ISiteSelectionContext, IDeleteItemContext
    {
        public MediaContext([NotNull] MediaViewer mediaViewer)
        {
            Assert.ArgumentNotNull(mediaViewer, nameof(mediaViewer));

            MediaViewer = mediaViewer;
        }

        [NotNull]
        public IEnumerable<IItem> Items
        {
            get
            {
                if (SelectedItems != null)
                {
                    foreach (var itemHeader in SelectedItems)
                    {
                        yield return itemHeader;
                    }
                }
            }
        }

        [NotNull]
        public MediaViewer MediaViewer { get; }

        [CanBeNull]
        public IEnumerable<ItemHeader> SelectedItems { get; set; }

        DatabaseUri IDatabaseSelectionContext.DatabaseUri
        {
            get
            {
                if (SelectedItems == null)
                {
                    return DatabaseUri.Empty;
                }

                var item = SelectedItems.FirstOrDefault();
                if (item == null)
                {
                    return DatabaseUri.Empty;
                }

                return item.ItemUri.DatabaseUri;
            }
        }

        [NotNull]
        Site ISiteContext.Site
        {
            get { return MediaViewer.Site; }
        }

        Site ISiteSelectionContext.Site
        {
            get { return ((IDatabaseSelectionContext)this).DatabaseUri.Site; }
        }

        void ISiteContext.SetSite(Site site)
        {
            Debug.ArgumentNotNull(site, nameof(site));

            MediaViewer.SetSite(site);
        }
    }
}
