// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using EnvDTE;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ProjectExtensions;
using Sitecore.Rocks.Projects.FileItems;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.Projects
{
    public static class ProjectManager
    {
        public delegate void ProjectEventHandler(Project project);

        private static readonly List<Project> ProjectList = new List<Project>();

        private static readonly System.Version VisualStudioVersion11 = new System.Version(11, 0);

        static ProjectManager()
        {
            ShellNotifications.ProjectItemRemoved += ProjectItemRemoved;
            ShellNotifications.ProjectItemRenamed += ProjectItemRenamed;
            ShellNotifications.ProjectItemMoved += ProjectItemMoved;

            ShellNotifications.ProjectAdded += AddProject;
            ShellNotifications.ProjectRenamed += ProjectRenamed;
            ShellNotifications.ProjectUnloaded += ProjectUnloaded;

            ShellNotifications.SolutionLoaded += SolutionLoaded;
            ShellNotifications.SolutionUnloaded += SolutionUnloaded;
            ShellNotifications.BuildDone += BuildDone;

            ProjectLoader.RegisterProjectElementHandler("File", LoadProjectFileElement);
        }

        [NotNull]
        public static IEnumerable<Project> Projects
        {
            get { return ProjectList; }
        }

        [NotNull]
        public static Project CreateBlankProject([NotNull] string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            fileName = GetProjectFileName(fileName);

            var project = new Project(fileName);

            var handler = ProjectAdded;
            if (handler != null)
            {
                handler(project);
            }

            return project;
        }

        [CanBeNull]
        public static Project FindProjectFromProjectItemFileName([NotNull] string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            foreach (var project in Projects)
            {
                var relativeFileName = project.GetRelativeFileName(fileName);
                if (project.Contains(relativeFileName))
                {
                    return project;
                }
            }

            return null;
        }

        [CanBeNull]
        public static Project GetProject([NotNull] EnvDTE.ProjectItem projectItem)
        {
            Assert.ArgumentNotNull(projectItem, nameof(projectItem));

            return GetProject(projectItem.ContainingProject.FileName);
        }

        [CanBeNull]
        public static Project GetProject([NotNull] string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            fileName = GetProjectFileName(fileName);

            var result = Projects.FirstOrDefault(project => string.Compare(project.FileName, fileName, StringComparison.InvariantCultureIgnoreCase) == 0);
            if (result != null)
            {
                return result;
            }

            result = LoadProject(fileName);
            if (result == null)
            {
                return null;
            }

            ProjectList.Add(result);

            return result;
        }

        [CanBeNull]
        public static ProjectFileItem GetProjectFileItem([NotNull] ItemUri itemUri)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            foreach (var project in Projects)
            {
                var site = project.Site;
                if (site == null)
                {
                    continue;
                }

                if (string.Compare(site.HostName, itemUri.Site.HostName, StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    continue;
                }

                foreach (var projectItem in project.ProjectItems)
                {
                    var projectFile = projectItem as ProjectFileItem;
                    if (projectFile == null)
                    {
                        continue;
                    }

                    foreach (var i in projectFile.Items)
                    {
                        if (itemUri.ItemId == i.ItemId && itemUri.DatabaseUri == i.DatabaseUri)
                        {
                            return projectFile;
                        }
                    }
                }
            }

            return null;
        }

        public static void LoadProject([NotNull] Project project)
        {
            Assert.ArgumentNotNull(project, nameof(project));

            ProjectList.Add(project);
        }

        public static void LoadProjects()
        {
            var solution = SitecorePackage.Instance.Dte.Solution;
            if (solution == null)
            {
                return;
            }

            foreach (var item in solution.Projects)
            {
                var project = item as EnvDTE.Project;
                if (project == null)
                {
                    continue;
                }

                GetProject(project.FileName);
            }
        }

        public static event ProjectEventHandler ProjectAdded;

        public static event ProjectEventHandler ProjectRemoved;

        public static void UnloadProject([NotNull] ProjectBase project)
        {
            Assert.ArgumentNotNull(project, nameof(project));

            project.Unload();
            ProjectList.Remove(project as Project);
        }

        private static void AddProject([NotNull] object sender, [NotNull] EnvDTE.Project project)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(project, nameof(project));

            var result = GetProject(project.FileName);
            if (result == null)
            {
                return;
            }

            var handler = ProjectAdded;
            if (handler != null)
            {
                handler(result);
            }
        }

        private static void BuildDone([NotNull] object sender, vsBuildScope scope, vsBuildAction action)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));

            foreach (var project in ProjectList)
            {
                project.RaiseProjectBuilt(scope, action);
            }
        }

        [NotNull]
        private static string GetProjectFileName([NotNull] string fileName)
        {
            Debug.ArgumentNotNull(fileName, nameof(fileName));

            if (!fileName.EndsWith(@".sitecore", StringComparison.InvariantCultureIgnoreCase))
            {
                fileName += @".sitecore";
            }

            return fileName;
        }

        [CanBeNull]
        private static Project LoadProject([NotNull] string fileName)
        {
            Debug.ArgumentNotNull(fileName, nameof(fileName));

            if (!File.Exists(fileName))
            {
                return null;
            }

            var result = Project.Load(fileName);

            return result;
        }

        [NotNull]
        private static ProjectItemBase LoadProjectFileElement([NotNull] ProjectBase project, [NotNull] XElement element)
        {
            Debug.ArgumentNotNull(project, nameof(project));
            Debug.ArgumentNotNull(element, nameof(element));

            var projectFileItem = new ProjectFileItem(project);
            projectFileItem.Load(element);

            return projectFileItem;
        }

        private static void ProjectItemMoved([NotNull] object sender, [NotNull] string newName, [NotNull] string oldName)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(newName, nameof(newName));
            Debug.ArgumentNotNull(oldName, nameof(oldName));

            ProjectFileItem projectFile = null;

            foreach (var project1 in Projects)
            {
                foreach (var projectItem in project1.ProjectItems)
                {
                    var file = projectItem as ProjectFileItem;
                    if (file == null)
                    {
                        continue;
                    }

                    if (string.Compare(file.AbsoluteFileName, oldName, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        projectFile = file;
                        break;
                    }
                }
            }

            if (projectFile == null)
            {
                return;
            }

            var project = projectFile.Project;
            var fileName = project.GetRelativeFileName(newName);

            projectFile.File = fileName;

            var fileItemHandler = FileItemManager.GetFileItemHandler(projectFile.Path);
            if (fileItemHandler != null)
            {
                foreach (var itemUri in projectFile.Items)
                {
                    fileItemHandler.UpdateItemPath(itemUri, projectFile.Path);
                }
            }

            project.Save();
        }

        private static void ProjectItemRemoved([NotNull] object sender, [NotNull] EnvDTE.ProjectItem item)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(item, nameof(item));

            var project = GetProject(item);
            if (project == null)
            {
                return;
            }

            var projectItem = project.GetProjectItem(item);
            if (projectItem == null)
            {
                return;
            }

            project.Remove(projectItem);
            project.Save();

            /*
      var site = project.Site;
      if (site == null)
      {
        return;
      }

      var projectFileItem = projectItem as ProjectFileItem;
      if (projectFileItem == null)
      {
        return;
      }

      foreach (var itemUri in projectFileItem.Items)
      {
        site.DataService.Delete(itemUri);
      }
      */
        }

        private static void ProjectItemRenamed([NotNull] object sender, [NotNull] EnvDTE.ProjectItem projectitem, [NotNull] string oldname)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(projectitem, nameof(projectitem));
            Debug.ArgumentNotNull(oldname, nameof(oldname));

            var project = GetProject(projectitem);
            if (project == null)
            {
                return;
            }

            var newFile = project.GetProjectItemFileName(projectitem);

            var oldFile = Path.Combine(Path.GetDirectoryName(newFile) ?? string.Empty, oldname);

            var projectFile = project.GetProjectItem(oldFile) as ProjectFileItem;
            if (projectFile == null)
            {
                return;
            }

            projectFile.File = newFile;

            var fileItemHandler = FileItemManager.GetFileItemHandler(projectFile.Path);
            if (fileItemHandler != null)
            {
                foreach (var itemUri in projectFile.Items)
                {
                    fileItemHandler.UpdateItemPath(itemUri, projectFile.Path);
                }
            }

            project.Save();
        }

        private static void ProjectRenamed([NotNull] object sender, [NotNull] EnvDTE.Project project, [NotNull] string oldname)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(project, nameof(project));
            Debug.ArgumentNotNull(oldname, nameof(oldname));

            var proj = GetProject(project.FileName);
            if (proj != null)
            {
                proj.Rename(project.FileName);
            }
        }

        private static void ProjectUnloaded([NotNull] object sender, [NotNull] EnvDTE.Project project)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(project, nameof(project));

            var p = GetProject(project.FileName);
            if (p == null)
            {
                return;
            }

            UnloadProject(p);

            var handler = ProjectRemoved;
            if (handler != null)
            {
                handler(p);
            }
        }

        private static void SolutionLoaded([NotNull] object sender)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));

            LoadProjects();

            // raise AddProject event for Visual Studio 2012 and lower
            if (AppHost.Shell.VisualStudioVersion > VisualStudioVersion11)
            {
                return;
            }

            var handler = ProjectAdded;
            if (handler == null)
            {
                return;
            }

            var solution = SitecorePackage.Instance.Dte.Solution;
            if (solution == null)
            {
                return;
            }

            foreach (var item in solution.Projects)
            {
                var project = item as EnvDTE.Project;
                if (project == null)
                {
                    continue;
                }

                var p = GetProject(project.FileName);
                if (p == null)
                {
                    continue;
                }

                handler(p);
            }
        }

        private static void SolutionUnloaded([NotNull] object sender)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));

            var solution = SitecorePackage.Instance.Dte.Solution;
            if (solution == null)
            {
                return;
            }

            foreach (var item in solution.Projects)
            {
                var project = item as EnvDTE.Project;
                if (project == null)
                {
                    continue;
                }

                var p = GetProject(project.FileName);
                if (p == null)
                {
                    continue;
                }

                UnloadProject(p);
            }
        }
    }
}
