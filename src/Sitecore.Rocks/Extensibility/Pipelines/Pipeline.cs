// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Extensibility.Pipelines
{
    public abstract class Pipeline<T> where T : Pipeline<T>
    {
        public delegate void PipelineEventHandler([NotNull] Pipeline<T> pipeline);

        private bool _aborted;

        private int _current;

        private Dictionary<string, object> _customData;

        private bool _suspended;

        [NotNull]
        public Dictionary<string, object> CustomData
        {
            get
            {
                if (_customData == null)
                {
                    _customData = new Dictionary<string, object>();
                }

                return _customData;
            }
        }

        public bool HasCustomData => _customData != null;

        [CanBeNull]
        internal IEnumerable<PipelineProcessor<T>> Processors { get; set; }

        public void Abort()
        {
            _aborted = true;
        }

        public event PipelineEventHandler Completed;

        public void Resume()
        {
            _suspended = false;
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
            _current = -1;
            _aborted = false;
            _suspended = false;

            Continue();

            return (T)this;
        }

        public void Suspend()
        {
            _suspended = true;
        }

        internal void Continue()
        {
            if (_aborted)
            {
                return;
            }

            if (_suspended)
            {
                return;
            }

            var processors = Processors;
            if (processors == null)
            {
                return;
            }

            _current++;

            if (_current >= processors.Count())
            {
                RaiseCompleted();
                return;
            }

            var processor = processors.ElementAt(_current);

            processor.ProcessInternal((T)this);
        }

        private void RaiseCompleted()
        {
            var completed = Completed;
            if (completed != null)
            {
                completed(this);
            }
        }
    }
}
