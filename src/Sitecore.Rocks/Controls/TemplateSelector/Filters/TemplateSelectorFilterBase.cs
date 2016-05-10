// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.ComponentModel;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Controls.TemplateSelector.Filters
{
    public abstract class TemplateSelectorFilterBase : ITemplateSelectorFilter
    {
        [Localizable(true)]
        public string Name { get; protected set; }

        public double Priority { get; protected set; }

        public virtual void AddToRecent(TemplateHeader templateHeader)
        {
            Assert.ArgumentNotNull(templateHeader, nameof(templateHeader));
        }

        public abstract void GetTemplates(TemplateSelectorFiltersParameters parameters, GetItemsCompleted<TemplateHeader> completed);

        public abstract void SetTemplates(IEnumerable<TemplateHeader> templates);
    }
}
