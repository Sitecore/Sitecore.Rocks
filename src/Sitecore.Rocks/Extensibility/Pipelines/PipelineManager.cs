// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Extensibility.Pipelines
{
    [ExtensibilityInitialization(PreInit = "Clear", PostInit = "SortPipelines")]
    public static class PipelineManager
    {
        static PipelineManager()
        {
            Pipelines = new Dictionary<Type, List<Processor>>();
        }

        [NotNull]
        public static Dictionary<Type, List<Processor>> Pipelines { get; }

        [UsedImplicitly]
        public static void Clear()
        {
            Pipelines.Clear();
        }

        [NotNull]
        public static T GetPipeline<T>() where T : Pipeline<T>
        {
            var type = typeof(T);

            // UsageCounter.ReportUsage(type.FullName + ", Pipeline");
            var result = Activator.CreateInstance(type) as T;
            if (result == null)
            {
                Trace.TraceError("Pipeline not instantiated: {0}", typeof(T).FullName);
                throw Exceptions.InvalidOperation(string.Format(Resources.PipelineManager_GetPipeline_Pipeline__0__not_found, type.FullName));
            }

            List<Processor> processors;
            if (!Pipelines.TryGetValue(type, out processors))
            {
                return result;
            }

            var list = new List<PipelineProcessor<T>>();

            foreach (var processor in processors)
            {
                list.Add((PipelineProcessor<T>)processor.Instance);
            }

            result.Processors = list;

            return result;
        }

        public static void LoadType([NotNull] Type type, [NotNull] PipelineAttribute attribute)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(attribute, nameof(attribute));

            List<Processor> processors;
            if (!Pipelines.TryGetValue(attribute.PipelineType, out processors))
            {
                processors = new List<Processor>();

                Pipelines[attribute.PipelineType] = processors;
            }

            var processor = new Processor
            {
                Instance = Activator.CreateInstance(type),
                Priority = attribute.Priority
            };

            processors.Add(processor);
        }

        [UsedImplicitly]
        public static void SortPipelines()
        {
            var sorter = new PipelineSorter();

            foreach (var processors in Pipelines.Values)
            {
                processors.Sort(sorter);
            }
        }

        public class PipelineSorter : IComparer<Processor>
        {
            int IComparer<Processor>.Compare([CanBeNull] Processor x, [CanBeNull] Processor y)
            {
                if (x == null || y == null)
                {
                    return 0;
                }

                if (x.Priority > y.Priority)
                {
                    return 1;
                }

                if (x.Priority < y.Priority)
                {
                    return -1;
                }

                return 0;
            }
        }

        public class Processor
        {
            [NotNull]
            public object Instance { get; set; }

            public double Priority { get; set; }
        }
    }
}
