// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.ContentTrees.Pipelines.DuplicateItem
{
    [Pipeline(typeof(DuplicateItemPipeline), 1000)]
    public class SetName : PipelineProcessor<DuplicateItemPipeline>
    {
        protected override void Process(DuplicateItemPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            var newName = string.Format(Resources.SetName_Process_Copy_of__0_, pipeline.NewName);

            newName = AppHost.Prompt(Resources.SetName_Process_Enter_the_Name_of_the_New_Item_, Resources.SetName_Process_Duplicate, newName);
            if (newName == null)
            {
                pipeline.Abort();
                return;
            }

            pipeline.NewName = newName;
        }
    }
}
