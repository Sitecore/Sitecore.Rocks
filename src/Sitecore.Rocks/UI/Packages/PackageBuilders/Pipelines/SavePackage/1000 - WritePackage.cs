// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.UI.Packages.PackageBuilders.Pipelines.SavePackage
{
    [Pipeline(typeof(SavePackagePipeline), 1000)]
    public class WritePackage : PipelineProcessor<SavePackagePipeline>
    {
        protected override void Process(SavePackagePipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            WriteProject(pipeline.Output, pipeline.PackageBuilder);
        }

        private void WriteConverter([NotNull] XmlTextWriter output, [NotNull] PackageBuilder packageBuilder)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(packageBuilder, nameof(packageBuilder));

            output.WriteStartElement(@"Converter");
            output.WriteStartElement(@"TrivialConverter");
            output.WriteStartElement(@"Transforms");
            output.WriteEndElement();
            output.WriteEndElement();
            output.WriteEndElement();

            output.WriteStartElement(@"Include");
            output.WriteEndElement();

            output.WriteStartElement(@"Exclude");
            output.WriteEndElement();

            output.WriteElementString(@"Name", string.Empty);
        }

        private void WriteFiles([NotNull] XmlTextWriter output, [NotNull] PackageBuilder packageBuilder)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(packageBuilder, nameof(packageBuilder));

            output.WriteStartElement(@"xfiles");

            output.WriteStartElement(@"Entries");

            foreach (var file in packageBuilder.Files)
            {
                output.WriteElementString(@"x-item", file.FileUri.FileName.Replace(@"\", @"/"));
            }

            output.WriteEndElement();

            output.WriteStartElement(@"Converter");
            output.WriteStartElement(@"FileToEntryConverter");
            output.WriteStartElement(@"Root");
            output.WriteEndElement();
            output.WriteStartElement(@"Transforms");
            output.WriteStartElement(@"InstallerConfigurationTransform");
            output.WriteStartElement(@"Options");
            output.WriteStartElement(@"BehaviourOptions");
            output.WriteElementString(@"ItemMode", @"Undefined");
            output.WriteElementString(@"ItemMergeMode", @"Undefined");
            output.WriteEndElement();
            output.WriteEndElement();
            output.WriteEndElement();
            output.WriteEndElement();
            output.WriteEndElement();
            output.WriteEndElement();

            output.WriteStartElement(@"Include");
            output.WriteEndElement();

            output.WriteStartElement(@"Exclude");
            output.WriteEndElement();

            output.WriteElementString(@"Name", @"Files");

            output.WriteEndElement();
        }

        private void WriteItems([NotNull] XmlTextWriter output, [NotNull] PackageBuilder packageBuilder)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(packageBuilder, nameof(packageBuilder));

            output.WriteStartElement(@"xitems");

            output.WriteStartElement(@"Entries");

            foreach (var item in packageBuilder.Items)
            {
                var value = string.Format(@"/{0}{1}/{2}/invariant/0", item.ItemHeader.ItemUri.DatabaseName, item.ItemHeader.Path, item.ItemHeader.ItemUri.ItemId);

                output.WriteStartElement(@"x-item");
                output.WriteValue(value);
                output.WriteEndElement();
            }

            output.WriteEndElement();

            output.WriteElementString(@"SkipVersions", @"False");

            output.WriteStartElement(@"Converter");
            output.WriteStartElement(@"ItemToEntryConverter");
            output.WriteStartElement(@"Transforms");
            output.WriteStartElement(@"InstallerConfigurationTransform");
            output.WriteStartElement(@"Options");
            output.WriteStartElement(@"BehaviourOptions");
            output.WriteElementString(@"ItemMode", @"Undefined");
            output.WriteElementString(@"ItemMergeMode", @"Undefined");
            output.WriteEndElement();
            output.WriteEndElement();
            output.WriteEndElement();
            output.WriteEndElement();
            output.WriteEndElement();
            output.WriteEndElement();

            output.WriteStartElement(@"Include");
            output.WriteEndElement();

            output.WriteStartElement(@"Exclude");
            output.WriteEndElement();

            output.WriteElementString(@"Name", @"Items");

            output.WriteEndElement();
        }

        private void WriteMetaData([NotNull] XmlTextWriter output, [NotNull] PackageBuilder packageBuilder)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(packageBuilder, nameof(packageBuilder));

            output.WriteStartElement(@"Metadata");
            output.WriteStartElement(@"metadata");

            output.WriteElementString(@"PackageName", packageBuilder.PackageNameTextBox.Text);

            output.WriteElementString(@"Author", packageBuilder.AuthorTextBox.Text);

            output.WriteElementString(@"Version", packageBuilder.VersionTextBox.Text);

            output.WriteStartElement(@"Revision");
            output.WriteEndElement();

            output.WriteElementString(@"License", packageBuilder.LicenseEditor.Text);

            output.WriteElementString(@"Comment", packageBuilder.CommentTextBox.Text);

            output.WriteStartElement(@"Attributes");
            output.WriteEndElement();

            output.WriteElementString(@"Readme", packageBuilder.ReadmeTextBox.Text);

            output.WriteElementString(@"Publisher", packageBuilder.PublisherTextBox.Text);

            output.WriteStartElement(@"PostStep");
            output.WriteEndElement();

            output.WriteStartElement(@"PackageID");
            output.WriteEndElement();

            output.WriteEndElement();
            output.WriteEndElement();

            output.WriteElementString(@"SaveProject", @"True");
        }

        private void WriteProject([NotNull] XmlTextWriter output, [NotNull] PackageBuilder packageBuilder)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(packageBuilder, nameof(packageBuilder));

            output.WriteStartElement(@"project");

            output.WriteAttributeString("site", packageBuilder.Site.Name);
            output.WriteAttributeString("format", packageBuilder.PackageFormatComboBox.Text);
            output.WriteAttributeString("targetfilefolder", packageBuilder.TargetComboBox.SelectedIndex == 0 ? "wwwroot" : "content");

            WriteMetaData(output, packageBuilder);

            WriteSources(output, packageBuilder);

            WriteConverter(output, packageBuilder);

            output.WriteEndElement();
        }

        private void WriteSources([NotNull] XmlTextWriter output, [NotNull] PackageBuilder packageBuilder)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(packageBuilder, nameof(packageBuilder));

            output.WriteStartElement(@"Sources");

            WriteItems(output, packageBuilder);
            WriteFiles(output, packageBuilder);

            output.WriteEndElement();
        }
    }
}
