// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;

namespace Sitecore.Rocks.Server.Extensibility.Pipelines
{
    public abstract class Pipeline<T> where T : Pipeline<T>
    {
        private bool aborted;

        private int current;

        private bool suspended;

        internal IEnumerable<PipelineProcessor<T>> Processors { get; set; }

        public void Abort()
        {
            aborted = true;
        }

        public void Resume()
        {
            suspended = false;
            Continue();
        }

        [NotNull]
        public static T Run()
        {
            return PipelineManager.GetPipeline<T>();
        }

        [NotNull]
        public T Start()
        {
            current = -1;
            aborted = false;
            suspended = false;

            Continue();

            return (T)this;
        }

        public void Suspend()
        {
            suspended = true;
        }

        internal void Continue()
        {
            if (aborted)
            {
                return;
            }

            if (suspended)
            {
                return;
            }

            var processors = Processors;
            if (processors == null)
            {
                return;
            }

            current++;

            if (current >= processors.Count())
            {
                return;
            }

            var processor = processors.ElementAt(current);

            processor.ProcessInternal((T)this);
        }
    }
}
