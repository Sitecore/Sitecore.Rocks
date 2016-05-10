// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.ContentEditors.Pipelines.SaveItem
{
    public class SaveItemPipeline : Pipeline<SaveItemPipeline>
    {
        [NotNull]
        public ContentModel ContentModel { get; private set; }

        [NotNull]
        public ContentEditor Editor { get; private set; }

        public bool PostMacro { get; private set; }

        [NotNull]
        public SaveItemPipeline WithParameters([NotNull] ContentModel contentModel, [NotNull] ContentEditor editor, bool postMacro)
        {
            Assert.ArgumentNotNull(contentModel, nameof(contentModel));
            Assert.ArgumentNotNull(editor, nameof(editor));

            ContentModel = contentModel;
            Editor = editor;
            PostMacro = postMacro;

            return Start();
        }
    }
}
