// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Linq;
using System.Reflection;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.UI.Repositories;

namespace Sitecore.Rocks.Shell.Pipelines.Initialization
{
    [Pipeline(typeof(InitializationPipeline), 2000)]
    public class SetupDefaultRepositories : PipelineProcessor<InitializationPipeline>
    {
        protected override void Process(InitializationPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (!pipeline.IsStartUp)
            {
                return;
            }

            RepositoryManager.Register(RepositoryManager.LicenseFiles);
            RepositoryManager.Register(RepositoryManager.Packages);
            RepositoryManager.Register(RepositoryManager.Reports);
            RepositoryManager.Register(RepositoryManager.SitecoreZip);
            RepositoryManager.Register(RepositoryManager.StartPages);
            RepositoryManager.Register(RepositoryManager.Folders);

            var zipFiles = RepositoryManager.GetRepository(RepositoryManager.SitecoreZip);
            if (!zipFiles.Entries.Any())
            {
                var location = Path.Combine(AppHost.User.UserFolder, @"Repositories\SitecoreZipFiles");
                Directory.CreateDirectory(location);

                var repository = new RepositoryEntry("Sitecore Zip Files", location);
                zipFiles.Entries.Add(repository);
                zipFiles.Save();
            }

            var licenseFiles = RepositoryManager.GetRepository(RepositoryManager.LicenseFiles);
            if (!licenseFiles.Entries.Any())
            {
                var location = Path.Combine(AppHost.User.UserFolder, @"Repositories\LicenseFiles");
                Directory.CreateDirectory(location);

                var repository = new RepositoryEntry("License Files", location);
                licenseFiles.Entries.Add(repository);
                licenseFiles.Save();
            }

            var packages = RepositoryManager.GetRepository(RepositoryManager.Packages);
            if (!packages.Entries.Any())
            {
                var location = Path.Combine(AppHost.User.UserFolder, @"Repositories\Packages");
                Directory.CreateDirectory(location);

                var repository = new RepositoryEntry("Local Packages", location);
                packages.Entries.Add(repository);
                packages.Save();
            }

            var startPages = RepositoryManager.GetRepository(RepositoryManager.StartPages);
            if (!startPages.Entries.Any())
            {
                var location = Path.Combine(AppHost.User.UserFolder, @"Repositories\StartPages");
                Directory.CreateDirectory(location);

                var repository = new RepositoryEntry("Start Pages", location);
                startPages.Entries.Add(repository);
                startPages.Save();
            }

            var reports = RepositoryManager.GetRepository(RepositoryManager.Reports);
            if (!reports.Entries.Any())
            {
                var location = Path.Combine(AppHost.User.UserFolder, @"Repositories\Reports");
                Directory.CreateDirectory(location);

                var repository = new RepositoryEntry("Reports", location);
                reports.Entries.Add(repository);
                reports.Save();
            }

            var folders = RepositoryManager.GetRepository(RepositoryManager.Folders);
            if (!folders.Entries.Any())
            {
                var combine = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, @"Repositories\Folders");

                Directory.CreateDirectory(combine);

                var repository = new RepositoryEntry("Default", @".\Repositories\Folders");
                folders.Entries.Add(repository);
                folders.Save();
            }
        }
    }
}
