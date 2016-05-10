// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentEditors.Appearances;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.ContentEditors.Pipelines.SetFieldVisibility
{
    public class SetFieldVisibilityPipeline : Pipeline<SetFieldVisibilityPipeline>
    {
        public AppearanceOptions Appearance { get; set; }

        public ContentModel ContentModel { get; set; }

        public Field Field { get; set; }

        public bool IsVisible { get; set; }

        [NotNull]
        public SetFieldVisibilityPipeline WithParameters([NotNull] ContentModel contentModel, [NotNull] AppearanceOptions appearance, [NotNull] Field field)
        {
            Assert.ArgumentNotNull(contentModel, nameof(contentModel));
            Assert.ArgumentNotNull(appearance, nameof(appearance));
            Assert.ArgumentNotNull(field, nameof(field));

            ContentModel = contentModel;
            Appearance = appearance;
            Field = field;
            IsVisible = true;

            Start();

            return this;
        }
    }
}
