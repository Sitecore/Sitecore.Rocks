// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.ContentEditors.Pipelines.LoadItems
{
    public class LoadItemsPipeline : Pipeline<LoadItemsPipeline>
    {
        public ContentModel ContentModel { get; set; }

        public List<ItemVersionUri> Items { get; set; }

        public LoadItemsOperation LoadItemsOperation { get; set; }

        public Site Site { get; set; }

        [NotNull]
        public LoadItemsPipeline WithParameters([NotNull] List<ItemVersionUri> itemUriList, [NotNull] LoadItemsOperation loadItemsOperation)
        {
            Assert.ArgumentNotNull(itemUriList, nameof(itemUriList));
            Assert.ArgumentNotNull(loadItemsOperation, nameof(loadItemsOperation));

            Items = itemUriList;
            LoadItemsOperation = loadItemsOperation;

            Start();

            return this;
        }
    }
}
