// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations
{
    public interface IValidationViewer
    {
        void Disable([NotNull] ValidationDescriptor item);

        void Hide([NotNull] ValidationDescriptor item);
    }
}
