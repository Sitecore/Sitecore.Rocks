// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Indexes.Dialogs.TermExplorer
{
    public class TermExplorerContext : ICommandContext
    {
        public TermExplorerContext([NotNull] TermExplorerDialog termExplorer)
        {
            Assert.ArgumentNotNull(termExplorer, nameof(termExplorer));

            TermExplorer = termExplorer;
        }

        public TermExplorerDialog TermExplorer { get; private set; }
    }
}
