// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Management.ManagementItems.CommandViewers
{
    public class CommandViewerContext : ICommandContext
    {
        public CommandViewerContext([NotNull] CommandViewer commandViewer)
        {
            Assert.ArgumentNotNull(commandViewer, nameof(commandViewer));

            CommandViewer = commandViewer;
        }

        [NotNull]
        public CommandViewer CommandViewer { get; private set; }
    }
}
