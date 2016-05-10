// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.TemplateFieldSorter
{
    public class TemplateFieldSorterContext : ICommandContext
    {
        public TemplateFieldSorterContext([NotNull] TemplateFieldSorter templateFieldSorter)
        {
            Assert.ArgumentNotNull(templateFieldSorter, nameof(templateFieldSorter));

            TemplateFieldSorter = templateFieldSorter;
        }

        public TemplateFieldSorter TemplateFieldSorter { get; set; }
    }
}
