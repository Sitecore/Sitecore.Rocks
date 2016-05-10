// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.ContentTrees.Items
{
    public abstract class BaseSiteTreeViewItem : BaseTreeViewItem
    {
        protected BaseSiteTreeViewItem([NotNull] Site site)
        {
            Debug.ArgumentNotNull(site, nameof(site));

            Site = site;
        }

        [NotNull]
        public Site Site { get; private set; }
    }
}
