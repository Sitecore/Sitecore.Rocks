// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensibility.Pipelines;

namespace Sitecore.Rocks.Server.Pipelines.DeleteItems
{
    public class DeleteItemPipeline : Pipeline<DeleteItemPipeline>
    {
        [NotNull]
        public IEnumerable<string> DeletingPaths { get; private set; }

        public bool DryRun { get; set; }

        [NotNull]
        public Item Item { get; private set; }

        [NotNull]
        public XmlTextWriter Output { get; set; }

        public bool WillBeDeleted(Item sourceItem)
        {
            return DeletingPaths.Any(p => sourceItem.Paths.Path.StartsWith(p, StringComparison.InvariantCultureIgnoreCase));
        }

        [NotNull]
        public DeleteItemPipeline WithParameters([NotNull] XmlTextWriter output, [NotNull] Item item, [NotNull] IEnumerable<string> deletingPaths, bool dryRun)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            Output = output;
            Item = item;
            DeletingPaths = deletingPaths;
            DryRun = dryRun;

            Start();

            return this;
        }
    }
}
