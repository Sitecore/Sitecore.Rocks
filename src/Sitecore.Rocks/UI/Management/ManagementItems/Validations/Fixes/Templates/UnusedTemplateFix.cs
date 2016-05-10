// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Pipelines.DeleteItem;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Fixes.Templates
{
    [Fix]
    public class UnusedTemplateFix : IFix
    {
        public bool CanFix(ValidationDescriptor validationDescriptor)
        {
            Assert.ArgumentNotNull(validationDescriptor, nameof(validationDescriptor));

            return validationDescriptor.Name == "Unused templates";
        }

        public void Fix(ValidationDescriptor validationDescriptor)
        {
            Assert.ArgumentNotNull(validationDescriptor, nameof(validationDescriptor));

            var name = Parse(validationDescriptor.Problem);

            if (AppHost.MessageBox(string.Format(Resources.Delete_Execute_Are_you_sure_you_want_to_delete___0___, name), Resources.Confirmation, MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) != MessageBoxResult.OK)
            {
                return;
            }

            var pipeline = PipelineManager.GetPipeline<DeleteItemPipeline>();

            pipeline.ItemUri = validationDescriptor.ItemUri.ItemUri;
            pipeline.DeleteFiles = false;

            pipeline.Start();
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
