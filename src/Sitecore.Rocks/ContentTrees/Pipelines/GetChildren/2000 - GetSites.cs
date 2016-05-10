// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.ContentTrees.Pipelines.GetChildren
{
    [Pipeline(typeof(GetChildrenPipeline), 2000)]
    public class GetSites : PipelineProcessor<GetChildrenPipeline>
    {
        protected override void Process(GetChildrenPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (pipeline.BaseFolder == null)
            {
                return;
            }

            var sites = SiteManager.Sites;

            foreach (var site in sites)
            {
                var path = Path.GetDirectoryName(site.Connection.FileName);
                if (string.Compare(path, pipeline.BaseFolder, StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    continue;
                }

                var item = site.GetTreeViewItem();
                if (item == null)
                {
                    continue;
                }

                item.Items.Add(DummyTreeViewItem.Instance);

                pipeline.Items.Add(item);
            }
        }
    }
}
