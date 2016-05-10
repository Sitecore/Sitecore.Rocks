// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.Controls.TemplateSelector.Filters
{
    public interface ITemplateSelectorFilter
    {
        [NotNull]
        string Name { get; }

        double Priority { get; }

        void AddToRecent([NotNull] TemplateHeader templateHeader);

        void GetTemplates([NotNull] TemplateSelectorFiltersParameters parameters, [NotNull] GetItemsCompleted<TemplateHeader> completed);

        void SetTemplates([NotNull] IEnumerable<TemplateHeader> templates);
    }
}
