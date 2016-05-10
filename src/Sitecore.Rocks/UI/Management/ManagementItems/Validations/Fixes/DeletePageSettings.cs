// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Pipelines.DeleteItem;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Fixes
{
    [Fix]
    public class DeletePageSettings : IFix
    {
        public bool CanFix(ValidationDescriptor validationDescriptor)
        {
            Assert.ArgumentNotNull(validationDescriptor, nameof(validationDescriptor));

            return validationDescriptor.Name == "Empty PageSettings items can safely be deleted";
        }

        public void Fix(ValidationDescriptor validationDescriptor)
        {
            Assert.ArgumentNotNull(validationDescriptor, nameof(validationDescriptor));

            var itemName = ParseItemName(validationDescriptor);

            if (AppHost.MessageBox(string.Format("Are you sure you want to delete '{0}'?", itemName), "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) != MessageBoxResult.OK)
            {
                return;
            }

            var pipeline = PipelineManager.GetPipeline<DeleteItemPipeline>();

            pipeline.ItemUri = validationDescriptor.ItemUri.ItemUri;
            pipeline.DeleteFiles = false;

            pipeline.Start();
        }

        [NotNull]
        private string ParseItemName([NotNull] ValidationDescriptor validationDescriptor)
        {
            var start = validationDescriptor.ItemPath.LastIndexOf("/", StringComparison.Ordinal);
            if (start < 0)
            {
                return string.Empty;
            }

            return validationDescriptor.ItemPath.Mid(start + 1);
        }
    }
}
