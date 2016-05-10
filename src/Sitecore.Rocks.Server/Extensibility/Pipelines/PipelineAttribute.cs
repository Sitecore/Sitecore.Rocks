// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Extensibility.Pipelines
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

        public Type PipelineType { get; private set; }

        public double Priority { get; private set; }

        public override void Initialize([NotNull] Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            PipelineManager.LoadType(type, this);
        }
    }
}
