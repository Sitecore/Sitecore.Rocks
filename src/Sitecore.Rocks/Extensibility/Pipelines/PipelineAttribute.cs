// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Extensibility.Pipelines
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true), MeansImplicitUse]
    public class PipelineAttribute : ExtensibilityAttribute
    {
        public PipelineAttribute([NotNull] Type pipelineType, double priority)
        {
            Assert.ArgumentNotNull(pipelineType, nameof(pipelineType));

            PipelineType = pipelineType;
            Priority = priority;
        }

        [NotNull]
        public Type PipelineType { get; private set; }

        public double Priority { get; private set; }

        public override void Initialize([NotNull] Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            PipelineManager.LoadType(type, this);
        }
    }
}
