// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Commands.Helix.Dialogs;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Extensions.ProjectHostExtensions;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.ContentTrees.Commands.Helix
{
    [Command(Submenu = HelixSubmenu.Name)]
    public class CreateHelixSolition : CommandBase
    {
        public CreateHelixSolition()
        {
            Text = "Create Helix Solution...";
            Group = "Helix";
            SortingValue = 1000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return false;
            }

            if (context.SelectedItems.Count() != 1)
            {
                return false;
            }

            var item = context.SelectedItems.First() as SiteTreeViewItem;
            if (item == null)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = (ContentTreeContext)parameter;
            var item = (SiteTreeViewItem)context.SelectedItems.First();

            var d = new CreateHelixSolutionDialog();
            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            InstallHelixSolution(item.Site, d.SolutionName, d.Location, d.Description, d.IgnoreGitIgnoreFile);
        }

        private void ConnectSitecoreRocks(Site site, string location, string name)
        {
            var projectFileName = Path.Combine(location, name + "\\" + name + ".sln");

            var project = AppHost.Projects.CreateBlankProject(projectFileName);

            if (site != null)
            {
                project.HostName = site.HostName;
                project.UserName = site.UserName;
            }

            project.Save();
        }

        private void InstallHelixSolution(Site site, string name, string location, string description, bool ignoreGitIgnoreFile)
        {
            var zipFileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "Resources\\Helix Solution template.zip");
            var packageFileName = Path.Combine(location, "Helix Solution Folders-1.0.zip");

            Unzip(zipFileName, location);
            WriteReadmeFile(location, description);
            ReplacePublishingSettingsTargets(location, site);
            RemoveGitFiles(location, ignoreGitIgnoreFile);
            RenameSolutionFile(location, name);
            RenameSolutionVsFolder(location, name);
            RenameSolutionFolder(location, name);
            ConnectSitecoreRocks(site, location, name);
            InstallPackage(site, packageFileName, () => AppHost.MessageBox("Solution has been successfully created.", "Information", MessageBoxButton.OK, MessageBoxImage.Information));
        }

        private void InstallPackage(Site site, string packageFileName, Action done)
        {
            ExecuteCompleted uploaded = delegate(string uploadedResponse, ExecuteResult uploadedResult)
            {
                if (!DataService.HandleExecute(uploadedResponse, uploadedResult))
                {
                    return;
                }

                var serverFileName = uploadedResponse;

                ExecuteCompleted installed = delegate(string installedResponse, ExecuteResult installedResult)
                {
                    DataService.HandleExecute(installedResponse, installedResult);
                    done();
                };

                site.DataService.ExecuteAsync("Packages.InstallPackage", installed, serverFileName, string.Empty);
            };

            var contents = File.ReadAllBytes(packageFileName);

            var data = Convert.ToBase64String(contents);

            site.DataService.ExecuteAsync("Packages.Upload", uploaded, data, Path.GetFileName(packageFileName));
        }

        private void RemoveGitFiles(string location, bool ignoreGitIgnoreFile)
        {
            if (ignoreGitIgnoreFile)
            {
                return;
            }

            var gitignoreFileName = Path.Combine(location, "SOLUTIONNAME\\.gitignore");
            File.Delete(gitignoreFileName);
            var gitattributesFileName = Path.Combine(location, "SOLUTIONNAME\\.gitattributes");
            File.Delete(gitattributesFileName);

            var fileName = Path.Combine(location, "SOLUTIONNAME\\SOLUTIONNAME.sln");
            var content = File.ReadAllText(fileName);

            content = content.Replace(".gitattributes = .gitattributes", string.Empty);
            content = content.Replace(".gitignore = .gitignore", string.Empty);

            File.WriteAllText(fileName, content, Encoding.UTF8);
        }

        private void RenameSolutionFile(string location, string name)
        {
            var sourceFileName = Path.Combine(location, "SOLUTIONNAME\\SOLUTIONNAME.sln");
            var destinationFileName = Path.Combine(location, "SOLUTIONNAME\\" + name + ".sln");
            File.Move(sourceFileName, destinationFileName);
        }

        private void RenameSolutionFolder(string location, string name)
        {
            var sourceFolder = Path.Combine(location, "SOLUTIONNAME");
            var destinationFolder = Path.Combine(location, name);
            Directory.Move(sourceFolder, destinationFolder);
        }

        private void RenameSolutionVsFolder(string location, string name)
        {
            var sourceFolder = Path.Combine(location, "SOLUTIONNAME\\.vs\\SOLUTIONNAME");
            var destinationFolder = Path.Combine(location, "SOLUTIONNAME\\.vs\\" + name);
            Directory.Move(sourceFolder, destinationFolder);
        }

        private void ReplacePublishingSettingsTargets(string location, Site site)
        {
            var fileName = Path.Combine(location, "SOLUTIONNAME\\publishsettings.targets");
            var content = File.ReadAllText(fileName);

            content = content.Replace("SITECORE-INSTANCE-URL", site.HostName);

            File.WriteAllText(fileName, content, Encoding.UTF8);
        }

        private void Unzip(string zipFileName, string destinationDirectory)
        {
            using (var zip = ZipFile.OpenRead(zipFileName))
            {
                foreach (var entry in zip.Entries)
                {
                    try
                    {
                        if (entry.FullName.EndsWith("/"))
                        {
                            Directory.CreateDirectory(Path.Combine(destinationDirectory, entry.FullName));
                        }
                        else
                        {
                            var fileName = Path.Combine(destinationDirectory, entry.FullName);
                            Directory.CreateDirectory(Path.GetDirectoryName(fileName) ?? string.Empty);
                            entry.ExtractToFile(fileName, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        private void WriteReadmeFile(string location, string description)
        {
            var readmeFileName = Path.Combine(location, "SOLUTIONNAME\\README.md");
            File.WriteAllText(readmeFileName, description);
        }
    }
}
