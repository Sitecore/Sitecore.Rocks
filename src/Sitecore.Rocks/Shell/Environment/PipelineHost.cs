// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.Shell.Environment
{
    public class PipelineHost
    {
        [NotNull]
        public T Execute<T>([NotNull] Action<T> initialize) where T : Pipeline<T>
        {
            Assert.ArgumentNotNull(initialize, nameof(initialize));

            var pipeline = PipelineManager.GetPipeline<T>();

            initialize(pipeline);

            pipeline.Start();

            return pipeline;
        }

        [NotNull]
        public T Get<T>() where T : Pipeline<T>
        {
            return PipelineManager.GetPipeline<T>();
        }

        public void Register([NotNull] Type type, [NotNull] Type pipelineType, double priority)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(pipelineType, nameof(pipelineType));

            var attribute = new PipelineAttribute(pipelineType, priority);

            PipelineManager.LoadType(type, attribute);
        }
    }
}
