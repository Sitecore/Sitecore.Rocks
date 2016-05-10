// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Fixes
{
    public interface IFix
    {
        bool CanFix([NotNull] ValidationDescriptor validationDescriptor);

        void Fix([NotNull] ValidationDescriptor validationDescriptor);
    }
}
