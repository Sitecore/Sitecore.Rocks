// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentEditors.Validators;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.ContentEditors.Skins
{
    public abstract class Skin : ISkin
    {
        public ContentEditor ContentEditor { get; set; }

        public ContentModel ContentModel { get; set; }

        public Site Site { get; set; }

        [NotNull]
        public abstract Control GetControl();

        public ValidatorBar GetValidatorBar()
        {
            return null;
        }

        [CanBeNull]
        public abstract Control RenderFields();

        public abstract void RenderHeader();
    }
}
