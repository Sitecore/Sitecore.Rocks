// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.TemplateDesigner
{
    public class TemplateDesignerContext : IItemSelectionContext, IDatabaseSelectionContext, ISiteSelectionContext, ICommandContext
    {
        [CanBeNull]
        public TemplateDesigner.TemplateField Field { get; set; }

        public IEnumerable<IItem> Items
        {
            get { yield return TemplateDesigner; }
        }

        [CanBeNull]
        public TemplateDesigner.TemplateSection Section { get; set; }

        [NotNull]
        public TemplateDesigner TemplateDesigner { get; set; }

        DatabaseUri IDatabaseSelectionContext.DatabaseUri => ((IItem)TemplateDesigner).ItemUri.DatabaseUri;

        Site ISiteSelectionContext.Site => ((IDatabaseSelectionContext)this).DatabaseUri.Site;
    }
}
