// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.Management.ManagementItems.PipelineViewers
{
    public class PipelineViewerContext : ICommandContext
    {
        public PipelineViewerContext(PipelineViewer pipelines)
        {
            PipelineViewer = pipelines;
        }

        public PipelineViewer PipelineViewer { get; private set; }
    }
}
