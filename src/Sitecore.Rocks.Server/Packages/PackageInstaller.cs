// © 2015-2017 by Jakob Christensen. All rights reserved.

using Sitecore.Data.Engines;
using Sitecore.Install;
using Sitecore.Install.Files;
using Sitecore.Install.Framework;
using Sitecore.Install.Items;
using Sitecore.Install.Metadata;
using Sitecore.Install.Utils;
using Sitecore.Install.Zip;
using Sitecore.IO;
using Sitecore.SecurityModel;

namespace Sitecore.Rocks.Server.Packages
{
    public class PackageInstaller
    {
        public PackageInstaller(string fileName)
        {
            FileName = fileName;
        }

        [NotNull]
        public string FileName { get; }

        public void Install()
        {
            Context.SetActiveSite("shell");
            using (new SecurityDisabler())
            {
                using (new SyncOperationContext())
                {
                    IProcessingContext context = new SimpleProcessingContext();

                    IItemInstallerEvents events = new DefaultItemInstallerEvents(new BehaviourOptions(InstallMode.Overwrite, MergeMode.Clear));
                    context.AddAspect(events);

                    IFileInstallerEvents events1 = new DefaultFileInstallerEvents(true);
                    context.AddAspect(events1);

                    var inst = new Installer();

                    inst.InstallPackage(FileUtil.MapPath(FileName), context);

                    ISource<PackageEntry> source = new PackageReader(FileUtil.MapPath(FileName));
                    var previewContext = Installer.CreatePreviewContext();
                    var view = new MetadataView(previewContext);
                    var sink = new MetadataSink(view);
                    sink.Initialize(previewContext);
                    source.Populate(sink);
                    inst.ExecutePostStep(view.PostStep, previewContext);
                }
            }
        }
    }
}
