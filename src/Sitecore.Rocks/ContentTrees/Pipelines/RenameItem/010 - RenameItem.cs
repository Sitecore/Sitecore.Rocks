// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.ContentTrees.Pipelines.RenameItem
{
    [Pipeline(typeof(RenameItemPipeline), 1000)]
    public class RenameItem : PipelineProcessor<RenameItemPipeline>
    {
        protected override void Process(RenameItemPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (!pipeline.ItemUri.Site.CanExecute)
            {
                pipeline.IsRenamed = pipeline.ItemUri.Site.DataService.Rename(pipeline.ItemUri, pipeline.NewName);
                return;
            }

            pipeline.ItemUri.Site.Execute("Items.Rename", response => { }, pipeline.ItemUri.DatabaseName.ToString(), pipeline.ItemUri.ItemId.ToString(), pipeline.NewName);
            pipeline.IsRenamed = true;
        }
    }
}
