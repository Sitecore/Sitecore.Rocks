// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using EnvDTE80;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ProjectHostExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Projects;
using Sitecore.Rocks.Projects.Pipelines.AddToProject;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Shell.Environment
{
    public class VisualStudioProjectHost : ProjectHost
    {
        public override ProjectBase AddFileToProject(string parentFileName, string fileName, bool dependOnParent)
        {
            Assert.ArgumentNotNull(parentFileName, nameof(parentFileName));
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            var pipeline = AddToProjectPipeline.Run().WithParameters(parentFileName, fileName, dependOnParent);

            return pipeline.Project;
        }

        public override string CreateVisualStudioProject(string projectType, string fileName = "", string defaultProjectName = "")
        {
            Assert.ArgumentNotNull(projectType, nameof(projectType));
            Assert.ArgumentNotNull(fileName, nameof(fileName));
            Assert.ArgumentNotNull(defaultProjectName, nameof(defaultProjectName));

            if (string.IsNullOrEmpty(fileName))
            {
                var dialog = new SaveFileDialog
                {
                    Title = "Create Visual Studio Project",
                    CheckPathExists = true,
                    OverwritePrompt = true,
                    FileName = defaultProjectName,
                    DefaultExt = @".csproj",
                    Filter = @"Visual Studio Projects (.csproj)|*.csproj|All files|*.*"
                };

                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return null;
                }

                fileName = dialog.FileName;
            }

            var projectName = Path.GetFileNameWithoutExtension(fileName);
            var projectFolder = Path.GetDirectoryName(fileName);

            var solution = (Solution2)SitecorePackage.Instance.Dte.Solution;

            var templatePath = solution.GetProjectTemplate(projectType, "CSharp");

            try
            {
                solution.AddFromTemplate(templatePath, projectFolder, projectName);
            }
            catch (COMException ex)
            {
                AppHost.Output.LogException(ex);
                AppHost.MessageBox(string.Format("An error occured while creating the project:\n\n{0}", ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            return fileName;
        }

        public override IEnumerator<ProjectBase> GetEnumerator()
        {
            return ProjectManager.Projects.GetEnumerator();
        }

        public override string GetLinkedFileName(ProjectBase project, ItemUri itemUri)
        {
            Assert.ArgumentNotNull(project, nameof(project));
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            var projectFileItems = project.ProjectItems.OfType<ProjectFileItem>().ToList();

            var projectFileItem = projectFileItems.FirstOrDefault(i => i.Items.Any(u => u == itemUri));

            return projectFileItem == null ? string.Empty : projectFileItem.File;
        }

        public override ProjectBase GetProjectContainingFileName(Site site, string relativeFileName)
        {
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(relativeFileName, nameof(relativeFileName));

            relativeFileName = relativeFileName.Replace("/", "\\");
            if (relativeFileName.StartsWith("\\"))
            {
                relativeFileName = relativeFileName.Mid(1);
            }

            foreach (var project in this.Where(p => p.Site == site))
            {
                var f = project.MakeAbsoluteFileName(relativeFileName);
                if (AppHost.Files.FileExists(f))
                {
                    return project;
                }
            }

            return null;
        }

        public override ProjectBase GetProjectContainingLinkedItem(ItemUri itemUri)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            foreach (var project in this)
            {
                var f = project.GetLinkedFileName(itemUri);
                if (!string.IsNullOrEmpty(f))
                {
                    return project;
                }
            }

            return null;
        }

        public override bool IsLinked(ProjectBase project, ItemUri itemUri)
        {
            Assert.ArgumentNotNull(project, nameof(project));
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            return project.ProjectItems.OfType<ProjectFileItem>().Any(i => i.Items.Any(u => u == itemUri));
        }

        public override bool IsSolutionOpen()
        {
            return SitecorePackage.Instance.Dte.Solution != null;
        }

        public override void LinkItemAndFile(ProjectBase project, ItemUri itemUri, string relativeFileName, bool saveProject = true)
        {
            Assert.ArgumentNotNull(project, nameof(project));
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(relativeFileName, nameof(relativeFileName));

            var projectItem = project.ProjectItems.OfType<ProjectFileItem>().FirstOrDefault(i => string.Compare(i.File, relativeFileName, StringComparison.InvariantCultureIgnoreCase) == 0);
            if (projectItem == null)
            {
                projectItem = new ProjectFileItem(project)
                {
                    File = relativeFileName
                };

                project.Add(projectItem);
            }

            if (projectItem.Items.Contains(itemUri))
            {
                return;
            }

            projectItem.Items.Add(itemUri);

            if (saveProject)
            {
                project.Save();
            }
        }

        public override ProjectBase LinkSiteAndProject(Site site, string projectFileName)
        {
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(projectFileName, nameof(projectFileName));

            var newProject = ProjectManager.CreateBlankProject(projectFileName);

            newProject.HostName = site.HostName;
            newProject.UserName = site.UserName;
            newProject.IsRemoteSitecore = this.GetIsRemoteSitecore(projectFileName);

            newProject.Save();
            ProjectManager.LoadProject(newProject);

            return newProject;
        }

        public override string MakeAbsoluteFileName(Site site, string relativeFileName)
        {
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(relativeFileName, nameof(relativeFileName));

            var project = GetProjectContainingFileName(site, relativeFileName);
            if (project != null)
            {
                return project.MakeAbsoluteFileName(relativeFileName);
            }

            return base.MakeAbsoluteFileName(site, relativeFileName);
        }
    }
}
