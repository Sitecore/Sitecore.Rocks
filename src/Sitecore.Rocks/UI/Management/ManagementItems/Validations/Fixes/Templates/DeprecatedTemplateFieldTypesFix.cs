// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Fixes.Templates
{
    [Fix]
    public class DeprecatedTemplateFieldTypesFix : IFix
    {
        public bool CanFix(ValidationDescriptor validationDescriptor)
        {
            Assert.ArgumentNotNull(validationDescriptor, nameof(validationDescriptor));

            return validationDescriptor.Name == "Deprecated template field type";
        }

        public void Fix(ValidationDescriptor validationDescriptor)
        {
            Assert.ArgumentNotNull(validationDescriptor, nameof(validationDescriptor));

            var oldFieldType = Parse(validationDescriptor.Problem);
            var newFieldType = Parse(validationDescriptor.Solution);

            if (AppHost.MessageBox(string.Format("Are you sure you want to change the field type from \"{0}\" to \"{1}\"?", oldFieldType, newFieldType), "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            ItemModifier.Edit(validationDescriptor.ItemUri, "Type", newFieldType);
        }

        [NotNull]
        private string Parse([NotNull] string text)
        {
            Debug.ArgumentNotNull(text, nameof(text));

            var start = text.IndexOf("\"", StringComparison.Ordinal);
            if (start < 0)
            {
                return string.Empty;
            }

            var end = text.IndexOf("\"", start + 1, StringComparison.Ordinal);
            if (end < 0)
            {
                return string.Empty;
            }

            return text.Mid(start + 1, end - start - 1);
        }
    }
}
