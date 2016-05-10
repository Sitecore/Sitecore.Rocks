// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ProjectExtensions;
using Sitecore.Rocks.Projects;

namespace Sitecore.Rocks.Extensions.ProjectItemExtensions
{
    public static class ProjectItemExtensions
    {
        public static bool AddToVisualStudioProject([NotNull] this ProjectFileItem projectFile)
        {
            Assert.ArgumentNotNull(projectFile, nameof(projectFile));

            var project = projectFile.Project.GetVisualStudioProject();
            if (project == null)
            {
                return false;
            }

            var folder = Path.GetDirectoryName(projectFile.Path);
            if (folder == null)
            {
                return false;
            }

            var parent = CreateVisualStudioFolder(project, folder);

            if (parent == null)
            {
                project.ProjectItems.AddFromFile(projectFile.AbsoluteFileName);
            }
            else
            {
                parent.ProjectItems.AddFromFile(projectFile.AbsoluteFileName);
            }

            return true;
        }

        [NotNull]
        public static string GetFileName([NotNull] this EnvDTE.ProjectItem projectItem)
        {
            Assert.ArgumentNotNull(projectItem, nameof(projectItem));

            // weird work around
            try
            {
                return projectItem.FileNames[0];
            }
            catch
            {
                return projectItem.FileNames[1];
            }
        }

        [CanBeNull]
        private static EnvDTE.ProjectItem CreateVisualStudioFolder([NotNull] EnvDTE.Project project, [NotNull] string folder)
        {
            Assert.ArgumentNotNull(project, nameof(project));
            Assert.ArgumentNotNull(folder, nameof(folder));

            if (!string.IsNullOrEmpty(folder))
            {
                return null;
            }

            if (folder == @"\")
            {
                return null;
            }

            var parts = folder.Split(new[]
            {
                '\\'
            }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return null;
            }

            var result = project.ProjectItems.Item(parts[0]);
            if (result == null)
            {
                result = project.ProjectItems.AddFolder(parts[0]);
            }

            for (var n = 1; n < parts.Length; n++)
            {
                var i = result.ProjectItems.Item(parts[n]);
                if (i == null)
                {
                    i = project.ProjectItems.AddFolder(parts[n]);
                }

                result = i;
            }

            return result;
        }
    }
}
