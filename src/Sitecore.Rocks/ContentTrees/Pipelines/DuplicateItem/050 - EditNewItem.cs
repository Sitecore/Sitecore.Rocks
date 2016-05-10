// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.ContentTrees.Pipelines.DuplicateItem
{
    [Pipeline(typeof(DuplicateItemPipeline), 5000)]
    public class EditNewItem : PipelineProcessor<DuplicateItemPipeline>
    {
        protected override void Process([NotNull] DuplicateItemPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (pipeline.NewItemUri == ItemUri.Empty)
            {
                return;
            }

            var itemVersionUri = new ItemVersionUri(pipeline.NewItemUri, LanguageManager.CurrentLanguage, Version.Latest);

            AppHost.OpenContentEditor(itemVersionUri);
        }
    }
}
