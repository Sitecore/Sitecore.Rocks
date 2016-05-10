// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.TemplateHierarchies
{
    public class TemplateHierarchyFieldsContext : ICommandContext, IItemSelectionContext, IDatabaseSelectionContext, ISiteSelectionContext
    {
        public TemplateHierarchyFieldsContext([NotNull] TemplateHierarchyTab templateHierarchyTab)
        {
            Assert.ArgumentNotNull(templateHierarchyTab, nameof(templateHierarchyTab));

            TemplateHierarchyTab = templateHierarchyTab;
        }

        public IEnumerable<IItem> Items
        {
            get { return TemplateHierarchyTab.FieldsListView.SelectedItems.OfType<TemplateHierarchyTab.TemplateElement>(); }
        }

        [NotNull]
        public TemplateHierarchyTab TemplateHierarchyTab { get; }

        DatabaseUri IDatabaseSelectionContext.DatabaseUri
        {
            get
            {
                if (!Items.Any())
                {
                    return DatabaseUri.Empty;
                }

                return Items.First().ItemUri.DatabaseUri;
            }
        }

        Site ISiteSelectionContext.Site
        {
            get { return ((IDatabaseSelectionContext)this).DatabaseUri.Site; }
        }
    }
}
