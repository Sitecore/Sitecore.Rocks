// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.ContentTrees.Pipelines.DefaultActions
{
    public class DefaultActionPipeline : Pipeline<DefaultActionPipeline>
    {
        [NotNull]
        public object Context { get; private set; }

        public bool Handled { get; set; }

        [NotNull]
        public DefaultActionPipeline WithParameters([NotNull] object context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            Context = context;

            Start();

            return this;
        }
    }
}
