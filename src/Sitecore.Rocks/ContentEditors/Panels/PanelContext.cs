// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors.Skins;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Panels
{
    public class PanelContext : ICommandContext
    {
        public PanelContext([NotNull] ISkin skin, [NotNull] ContentModel contentModel)
        {
            Debug.ArgumentNotNull(skin, nameof(skin));
            Debug.ArgumentNotNull(contentModel, nameof(contentModel));

            Skin = skin;
            ContentModel = contentModel;
        }

        public ContentModel ContentModel { get; set; }

        public ISkin Skin { get; set; }
    }
}
