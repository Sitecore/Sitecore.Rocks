// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Extensibility.Pipelines
{
    [ExtensibilityInitialization(PreInit = "Clear", PostInit = "SortPipelines")]
    public static class PipelineManager
    {
        static PipelineManager()
        {
            Pipelines = new Dictionary<Type, List<Processor>>();
        }

        public static Dictionary<Type, List<Processor>> Pipelines { get; set; }

        public static void Clear()
        {
            Pipelines.Clear();
        }

        [NotNull]
        public static T GetPipeline<T>() where T : Pipeline<T>
        {
            var type = typeof(T);

            var result = Activator.CreateInstance(type) as T;
            if (result == null)
            {
                throw new InvalidOperationException(string.Format("Pipeline {0} not found", type.FullName));
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
            public object Instance { get; set; }

            public double Priority { get; set; }
        }
    }
}
