// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Controls.TemplateSelector
{
    public class TemplateSelectorFiltersParameters
    {
        public TemplateSelectorFiltersParameters([NotNull] DatabaseUri databaseUri, bool includeBranches)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            DatabaseUri = databaseUri;
            IncludeBranches = includeBranches;
        }

        [NotNull]
        public DatabaseUri DatabaseUri { get; private set; }

        public bool IncludeBranches { get; private set; }
    }
}
