// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Fixes
{
    [Fix]
    public class AddPageCode : IFix
    {
        public bool CanFix(ValidationDescriptor validationDescriptor)
        {
            Assert.ArgumentNotNull(validationDescriptor, nameof(validationDescriptor));

            return validationDescriptor.Name == "Page is missing 'PageCode' rendering";
        }

        public void Fix(ValidationDescriptor validationDescriptor)
        {
            Assert.ArgumentNotNull(validationDescriptor, nameof(validationDescriptor));

            var item = validationDescriptor.ItemUri.Site.DataService.GetItemFields(validationDescriptor.ItemUri);

            var document = item.Uri.ToString();

            AppHost.Windows.OpenLayoutDesigner(document, item);
        }
    }
}
