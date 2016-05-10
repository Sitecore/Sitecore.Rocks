// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Fixes
{
    [Fix]
    public class CustomValidationFix : IFix
    {
        public bool CanFix(ValidationDescriptor validationDescriptor)
        {
            Assert.ArgumentNotNull(validationDescriptor, nameof(validationDescriptor));

            var customValidation = CustomValidationManager.CustomValidations.FirstOrDefault(c => c.Title == validationDescriptor.Title);
            if (customValidation == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(customValidation.Fix))
            {
                return false;
            }

            return true;
        }

        public void Fix(ValidationDescriptor validationDescriptor)
        {
            Assert.ArgumentNotNull(validationDescriptor, nameof(validationDescriptor));

            var customValidation = CustomValidationManager.CustomValidations.FirstOrDefault(c => c.Title == validationDescriptor.Title);
            if (customValidation == null)
            {
                return;
            }

            if (AppHost.MessageBox(string.Format("Are you sure you want to apply the fix?\n\nFix:\n{0}", customValidation.Fix), "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            ExecuteCompleted completed = delegate(string response, ExecuteResult result) { DataService.HandleExecute(response, result, true); };

            validationDescriptor.ItemUri.DatabaseUri.Site.DataService.ExecuteAsync("QueryAnalyzer.Run", completed, validationDescriptor.ItemUri.DatabaseUri.DatabaseName.ToString(), validationDescriptor.ItemUri.ItemId.ToString(), customValidation.Fix, "0");
        }
    }
}
