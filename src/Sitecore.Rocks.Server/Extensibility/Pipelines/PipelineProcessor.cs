// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Extensibility.Pipelines
{
    public abstract class PipelineProcessor<T> where T : Pipeline<T>
    {
        protected abstract void Process([NotNull] T pipeline);

        internal void ProcessInternal([NotNull] T pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            Process(pipeline);
            pipeline.Continue();
        }
    }
}
