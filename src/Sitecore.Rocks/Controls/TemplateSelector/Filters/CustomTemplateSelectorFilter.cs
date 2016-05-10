// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.Controls.TemplateSelector.Filters
{
    public class CustomTemplateSelectorFilter : TemplateSelectorFilterBase
    {
        public CustomTemplateSelectorFilter([NotNull] string name, [NotNull] string filterText, [NotNull] string rootPath)
        {
            Assert.ArgumentNotNull(name, nameof(name));
            Assert.ArgumentNotNull(filterText, nameof(filterText));
            Assert.ArgumentNotNull(rootPath, nameof(rootPath));

            Name = name;
            FilterText = filterText;
            RootPath = rootPath;

            Priority = 1000;
        }

        [NotNull]
        public string FilterText { get; }

        [NotNull]
        public string RootPath { get; }

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

            var filtered = Templates;

            if (!string.IsNullOrEmpty(FilterText))
            {
                filtered = filtered.Where(r => r.Name.IsFilterMatch(FilterText)).ToList();
            }

            if (!string.IsNullOrEmpty(RootPath))
            {
                filtered = filtered.Where(r => r.Path.StartsWith(RootPath, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }

            completed(filtered);
        }

        public override void SetTemplates(IEnumerable<TemplateHeader> templates)
        {
            Assert.ArgumentNotNull(templates, nameof(templates));

            Templates = templates;
        }
    }
}
