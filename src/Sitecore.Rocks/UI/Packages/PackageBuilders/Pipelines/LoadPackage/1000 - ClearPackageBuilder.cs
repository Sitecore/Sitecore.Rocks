// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.UI.Packages.PackageBuilders.Pipelines.LoadPackage
{
    [Pipeline(typeof(LoadPackagePipeline), 1000)]
    public class ClearPackageBuilder : PipelineProcessor<LoadPackagePipeline>
    {
        protected override void Process(LoadPackagePipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            var packageBuilder = pipeline.PackageBuilder;

            packageBuilder.Items.Clear();
            packageBuilder.Files.Clear();

            packageBuilder.ItemList.ItemsSource = null;
            packageBuilder.FileList.ItemsSource = null;

            packageBuilder.PackageFormatComboBox.SelectedIndex = 0;
            packageBuilder.PackageNameTextBox.Text = string.Empty;
            packageBuilder.AuthorTextBox.Text = string.Empty;
            packageBuilder.VersionTextBox.Text = string.Empty;
            packageBuilder.VersionTextBox.Text = string.Empty;
            packageBuilder.LicenseEditor.Text = string.Empty;
            packageBuilder.CommentTextBox.Text = string.Empty;
            packageBuilder.ReadmeTextBox.Text = string.Empty;
            packageBuilder.TargetComboBox.SelectedIndex = 0;
        }
    }
}
