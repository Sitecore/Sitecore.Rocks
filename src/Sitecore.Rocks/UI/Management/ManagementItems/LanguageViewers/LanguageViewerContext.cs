// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Management.ManagementItems.LanguageViewers
{
    public class LanguageViewerContext : ICommandContext
    {
        public LanguageViewerContext([NotNull] LanguageViewer languageViewer)
        {
            Assert.ArgumentNotNull(languageViewer, nameof(languageViewer));

            LanguageViewer = languageViewer;
        }

        public LanguageViewer LanguageViewer { get; private set; }
    }
}
