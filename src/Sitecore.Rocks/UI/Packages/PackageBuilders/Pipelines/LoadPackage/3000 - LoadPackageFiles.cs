// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Xml.XPath;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.UI.Packages.PackageBuilders.Pipelines.LoadPackage
{
    [Pipeline(typeof(LoadPackagePipeline), 3000)]
    public class LoadPackageFiles : PipelineProcessor<LoadPackagePipeline>
    {
        protected override void Process(LoadPackagePipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            var elements = pipeline.PackageElement.XPathSelectElements(@"/project/Sources/xfiles/Entries/x-item");

            pipeline.PackageBuilder.InternalAddFiles(elements.Select(i => new FileUri(pipeline.Site, i.Value, false)));

            pipeline.PackageBuilder.ShowFileList();
        }
    }
}
