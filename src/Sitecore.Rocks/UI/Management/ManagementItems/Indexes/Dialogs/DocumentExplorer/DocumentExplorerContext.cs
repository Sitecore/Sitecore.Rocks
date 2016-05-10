// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Indexes.Dialogs.DocumentExplorer
{
    public class DocumentExplorerContext : ICommandContext
    {
        public DocumentExplorerContext([NotNull] DocumentExplorerDialog documentExplorer)
        {
            Assert.ArgumentNotNull(documentExplorer, nameof(documentExplorer));

            DocumentExplorer = documentExplorer;
        }

        public DocumentExplorerDialog DocumentExplorer { get; private set; }
    }
}
