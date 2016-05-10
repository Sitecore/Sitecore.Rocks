// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.UI.Packages.PackageBuilders.Pipelines.LoadPackage
{
    [Pipeline(typeof(LoadPackagePipeline), 1500)]
    public class LoadPackageInformation : PipelineProcessor<LoadPackagePipeline>
    {
        protected override void Process(LoadPackagePipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            var element = pipeline.PackageElement.Element(@"Metadata");
            if (element == null)
            {
                return;
            }

            element = element.Element(@"metadata");
            if (element == null)
            {
                return;
            }

            var packageBuilder = pipeline.PackageBuilder;

            packageBuilder.PackageNameTextBox.Text = GetInformation(element, "PackageName");
            packageBuilder.AuthorTextBox.Text = GetInformation(element, "Author");
            packageBuilder.VersionTextBox.Text = GetInformation(element, "Version");
            packageBuilder.PublisherTextBox.Text = GetInformation(element, "Publisher");
            packageBuilder.LicenseEditor.Text = GetInformation(element, "License");
            packageBuilder.CommentTextBox.Text = GetInformation(element, "Comment");
            packageBuilder.ReadmeTextBox.Text = GetInformation(element, "Readme");

            var targetFileFolder = pipeline.PackageElement.GetAttributeValue("targetfilefolder");
            if (string.IsNullOrEmpty(targetFileFolder) || targetFileFolder == "wwwroot")
            {
                packageBuilder.TargetComboBox.SelectedIndex = 0;
            }
            else
            {
                packageBuilder.TargetComboBox.SelectedIndex = 1;
            }

            var format = pipeline.PackageElement.GetAttributeValue("format");
            if (string.IsNullOrEmpty(format))
            {
                return;
            }

            var selectedItem = packageBuilder.PackageFormatComboBox.Items.OfType<ComboBoxItem>().FirstOrDefault(item => string.Compare(format, item.Content as string, StringComparison.InvariantCultureIgnoreCase) == 0);
            if (selectedItem != null)
            {
                selectedItem.IsSelected = true;
            }
        }

        [NotNull]
        private string GetInformation([NotNull] XElement metadataElement, [Localizable(false), NotNull] string elementName)
        {
            Debug.ArgumentNotNull(metadataElement, nameof(metadataElement));
            Debug.ArgumentNotNull(elementName, nameof(elementName));

            var element = metadataElement.Element(elementName);
            if (element == null)
            {
                return string.Empty;
            }

            return element.Value;
        }
    }
}
