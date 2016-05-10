// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Extensibility.Pipelines
{
    public class ActionPipeline<TArgs> : Pipeline<ActionPipeline<TArgs>>
    {
        private readonly List<ActionPipelineProcessor> _processors = new List<ActionPipelineProcessor>();

        public ActionPipeline(TArgs arguments)
        {
            Arguments = arguments;
            Processors = _processors;
        }

        public TArgs Arguments { get; }

        public void Add([NotNull] Action<ActionPipeline<TArgs>, TArgs> action)
        {
            Assert.ArgumentNotNull(action, nameof(action));

            _processors.Add(new ActionPipelineProcessor(action));
        }

        public void AddRange([NotNull] IEnumerable<Action<ActionPipeline<TArgs>, TArgs>> actions)
        {
            Assert.ArgumentNotNull(actions, nameof(actions));

            _processors.AddRange(actions.Select(a => new ActionPipelineProcessor(a)));
        }

        private class ActionPipelineProcessor : PipelineProcessor<ActionPipeline<TArgs>>
        {
            public ActionPipelineProcessor([NotNull] Action<ActionPipeline<TArgs>, TArgs> action)
            {
                Assert.ArgumentNotNull(action, nameof(action));

                Action = action;
            }

            [NotNull]
            private Action<ActionPipeline<TArgs>, TArgs> Action { get; }

            protected override void Process(ActionPipeline<TArgs> pipeline)
            {
                Debug.ArgumentNotNull(pipeline, nameof(pipeline));

                Action(pipeline, pipeline.Arguments);
            }
        }
    }
}
