// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;

namespace Sitecore.Rocks.Controls.TemplateSelector.Filters
{
    [Export(typeof(ITemplateSelectorFilter))]
    public class SystemTemplates : TemplateSelectorFilterBase
    {
        public SystemTemplates()
        {
            Name = "System";
            Priority = 8000;
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

        public override void SetTemplates([NotNull] IEnumerable<TemplateHeader> templates)
        {
            Assert.ArgumentNotNull(templates, nameof(templates));

            Templates = templates.Where(t => t.Path.StartsWith(@"/sitecore/templates/system", StringComparison.InvariantCultureIgnoreCase)).ToList();
        }
    }
}
