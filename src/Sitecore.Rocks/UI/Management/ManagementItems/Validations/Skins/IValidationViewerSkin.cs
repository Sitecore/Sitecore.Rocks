// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Skins
{
    public interface IValidationViewerSkin
    {
        [NotNull]
        IValidationViewer ValidationViewer { get; set; }

        [NotNull]
        Control GetControl();

        void Hide([NotNull] ValidationDescriptor item);

        void RenderValidations([NotNull] IEnumerable<ValidationDescriptor> items);
    }
}
