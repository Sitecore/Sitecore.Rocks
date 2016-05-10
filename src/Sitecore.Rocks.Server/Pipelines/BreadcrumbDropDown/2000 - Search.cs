// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Pipelines;
using Sitecore.Pipelines.Search;
using Sitecore.Rocks.Server.Extensibility.Pipelines;
using Sitecore.Search;

namespace Sitecore.Rocks.Server.Pipelines.BreadcrumbDropDown
{
    [Pipeline(typeof(BreadcrumbDropDownPipeline), 2000)]
    public class Search : PipelineProcessor<BreadcrumbDropDownPipeline>
    {
        protected override void Process(BreadcrumbDropDownPipeline pipeline)
        {
            Assert.ArgumentNotNull(pipeline, nameof(pipeline));

            var args = new SearchArgs(pipeline.Path)
            {
                Type = SearchType.ContentEditor,
                Limit = 30,
            };

            using (new LongRunningOperationWatcher(250, "Search pipeline from instant search for '{0} query", pipeline.Path))
            {
                CorePipeline.Run("search", args);
            }

            var results = args.Result;

            if (results.Count == 0)
            {
                return;
            }

            foreach (var category in results.Categories)
            {
                foreach (var hit in category)
                {
                    var item = hit.GetObject<Item>();
                    if (item != null)
                    {
                        pipeline.Items.Add(item);
                    }
                }
            }
        }
    }
}
