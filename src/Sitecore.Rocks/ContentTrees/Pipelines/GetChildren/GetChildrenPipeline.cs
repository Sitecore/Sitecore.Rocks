// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.ContentTrees.Pipelines.GetChildren
{
    public class GetChildrenPipeline : Pipeline<GetChildrenPipeline>
    {
        [CanBeNull]
        public string BaseFolder { get; private set; }

        [NotNull]
        public ICollection<BaseTreeViewItem> Items { get; private set; }

        [CanBeNull]
        public BaseTreeViewItem ParentItem { get; private set; }

        [NotNull]
        public GetChildrenPipeline WithParameters([NotNull] ICollection<BaseTreeViewItem> items, [CanBeNull] BaseTreeViewItem parentItem, [CanBeNull] string baseFolder)
        {
            Assert.ArgumentNotNull(items, nameof(items));

            ParentItem = parentItem;
            BaseFolder = baseFolder;

            Items = items;

            Start();

            return this;
        }
    }
}
