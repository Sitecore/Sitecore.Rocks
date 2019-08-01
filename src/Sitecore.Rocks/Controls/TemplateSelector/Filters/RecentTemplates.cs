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
    public class RecentTemplates : TemplateSelectorFilterBase
    {
        public const string ControlsTemplateSelectorRecent = "Controls\\TemplateSelector\\RecentTemplates";

        public RecentTemplates()
        {
            Name = "Recent";
            Priority = 9000;
        }

        [CanBeNull]
        protected IEnumerable<TemplateHeader> Templates { get; set; }

        public override void AddToRecent(TemplateHeader templateHeader)
        {
            Assert.ArgumentNotNull(templateHeader, nameof(templateHeader));

            RecentTemplateManager.AddToRecent(templateHeader);
        }

        public override void GetTemplates(TemplateSelectorFiltersParameters parameters, GetItemsCompleted<TemplateHeader> completed)
        {
            Assert.ArgumentNotNull(parameters, nameof(parameters));
            Assert.ArgumentNotNull(completed, nameof(completed));

            var templates = Templates;
            if (templates == null)
            {
                completed(Enumerable.Empty<TemplateHeader>());
                return;
            }

            var templateList = templates.ToList();

            var recent = RecentTemplateManager.GetTemplates(parameters.DatabaseUri);

            var result = recent.Where(templateHeader => templateList.Any(t => t.TemplateUri == templateHeader.TemplateUri)).ToList();

            completed(result);
        }

        public override void SetTemplates([NotNull] IEnumerable<TemplateHeader> templates)
        {
            Assert.ArgumentNotNull(templates, nameof(templates));

            Templates = templates;
        }
    }
}
