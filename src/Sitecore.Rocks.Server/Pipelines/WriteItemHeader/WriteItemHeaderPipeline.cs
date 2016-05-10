// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensibility.Pipelines;

namespace Sitecore.Rocks.Server.Pipelines.WriteItemHeader
{
    public class WriteItemHeaderPipeline : Pipeline<WriteItemHeaderPipeline>
    {
        public Item Item { get; set; }

        public XmlTextWriter Output { get; set; }

        [NotNull]
        public WriteItemHeaderPipeline WithParameters([NotNull] XmlTextWriter output, [NotNull] Item item)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(item, nameof(item));

            Output = output;
            Item = item;

            Start();

            return this;
        }
    }
}
