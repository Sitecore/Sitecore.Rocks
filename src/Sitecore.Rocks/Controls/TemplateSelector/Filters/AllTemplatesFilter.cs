// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;

namespace Sitecore.Rocks.Controls.TemplateSelector.Filters
{
    [Export(typeof(ITemplateSelectorFilter))]
    public class AllTemplatesFilter : TemplateSelectorFilterBase
    {
        public AllTemplatesFilter()
        {
            Name = "All";
            Priority = 9000;
        }

        [CanBeNull]
        protected IEnumerable<TemplateHeader> Templates { get; set; }

        public override void GetTemplates(TemplateSelectorFiltersParameters parameters, GetItemsCompleted<TemplateHeader> completed)
        {
            Assert.ArgumentNotNull(parameters, nameof(parameters));
            Assert.ArgumentNotNull(completed, nameof(completed));

            if (Templates == null)
            {
                completed(Enumerable.Empty<TemplateHeader>());
                return;
            }

            completed(Templates);
        }

        public override void SetTemplates(IEnumerable<TemplateHeader> templates)
        {
            Assert.ArgumentNotNull(templates, nameof(templates));

            Templates = templates;
        }
    }
}
