// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.Links
{
    public class LinksContext : ICommandContext, IItemSelectionContext, IDatabaseSelectionContext, ISiteSelectionContext
    {
        public LinksContext([NotNull] LinkTab linkTab)
        {
            Assert.ArgumentNotNull(linkTab, nameof(linkTab));

            LinkTab = linkTab;
        }

        [NotNull]
        public IEnumerable<IItem> Items
        {
            get
            {
                var selectedItem = SelectedItem;

                if (selectedItem != null)
                {
                    yield return selectedItem;
                }
            }
        }

        [NotNull]
        public LinkTab LinkTab { get; private set; }

        [CanBeNull]
        public ItemHeader SelectedItem { get; set; }

        DatabaseUri IDatabaseSelectionContext.DatabaseUri
        {
            get
            {
                var selectedItem = SelectedItem;
                return selectedItem == null ? DatabaseUri.Empty : selectedItem.ItemUri.DatabaseUri;
            }
        }

        Site ISiteSelectionContext.Site
        {
            get { return ((IDatabaseSelectionContext)this).DatabaseUri.Site; }
        }
    }
}
