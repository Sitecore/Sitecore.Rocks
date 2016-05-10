// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;

namespace Sitecore.Rocks.UI.LayoutDesigners.Dialogs.SelectRenderingsDialogs.Filters
{
    [Export(typeof(IRenderingSelectorFilter))]
    public class AllRenderingsFilter : RenderingSelectorFilterBase
    {
        public AllRenderingsFilter()
        {
            Name = "All";
            Priority = 9000;
        }

        [CanBeNull]
        protected IEnumerable<ItemHeader> Renderings { get; set; }

        public override void GetRenderings(RenderingSelectorFilterParameters parameters, GetItemsCompleted<ItemHeader> completed)
        {
            Assert.ArgumentNotNull(parameters, nameof(parameters));
            Assert.ArgumentNotNull(completed, nameof(completed));

            if (Renderings == null)
            {
                completed(Enumerable.Empty<ItemHeader>());
                return;
            }

            completed(Renderings);
        }

        public override void SetRenderings(IEnumerable<ItemHeader> templates)
        {
            Assert.ArgumentNotNull(templates, nameof(templates));

            Renderings = templates;
        }
    }
}
