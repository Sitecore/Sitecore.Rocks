// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentEditors.Validators;

namespace Sitecore.Rocks.ContentEditors.Skins
{
    public interface ISkin
    {
        [NotNull]
        ContentEditor ContentEditor { get; set; }

        [NotNull]
        ContentModel ContentModel { get; set; }

        [NotNull]
        Control GetControl();

        [CanBeNull]
        ValidatorBar GetValidatorBar();

        [CanBeNull]
        Control RenderFields();
    }
}
