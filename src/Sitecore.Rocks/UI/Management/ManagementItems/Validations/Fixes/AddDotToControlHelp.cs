// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Fixes
{
    [Fix]
    public class AddDotToControlHelp : HelpFix
    {
        public override bool CanFix(ValidationDescriptor validationDescriptor)
        {
            Assert.ArgumentNotNull(validationDescriptor, nameof(validationDescriptor));

            return validationDescriptor.Name == "Control help text must end with a dot";
        }

        public override void Fix(ValidationDescriptor validationDescriptor)
        {
            Assert.ArgumentNotNull(validationDescriptor, nameof(validationDescriptor));

            var itemUri = validationDescriptor.ItemUri;

            SetHelp(itemUri);
        }
    }
}
