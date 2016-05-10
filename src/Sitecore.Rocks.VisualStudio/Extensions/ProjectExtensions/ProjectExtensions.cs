// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ProjectItemExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Projects;

namespace Sitecore.Rocks.Extensions.ProjectExtensions
{
    public static class ProjectExtensions
    {
        public static bool Contains([NotNull] this ProjectBase project, [NotNull] EnvDTE.ProjectItem item)
        {
            Assert.ArgumentNotNull(project, nameof(project));
            Assert.ArgumentNotNull(item, nameof(item));

            return project.Contains(project.GetProjectItemFileName(item));
        }

        [CanBeNull]
        public static ProjectItem GetProjectItem([NotNull] this ProjectBase project, [NotNull] EnvDTE.ProjectItem item)
        {
            Assert.ArgumentNotNull(project, nameof(project));
            Assert.ArgumentNotNull(item, nameof(item));

            return project.GetProjectItem(project.GetProjectItemFileName(item));
        }

        [NotNull]
        public static string GetProjectItemFileName([NotNull] this ProjectBase project, [NotNull] EnvDTE.ProjectItem projectItem)
        {
            Assert.ArgumentNotNull(project, nameof(project));
            Assert.ArgumentNotNull(projectItem, nameof(projectItem));

            var fileName = projectItem.GetFileName();

            return project.GetRelativeFileName(fileName);
        }

        [CanBeNull]
        public static EnvDTE.Project GetVisualStudioProject([NotNull] this ProjectBase project)
        {
            Assert.ArgumentNotNull(project, nameof(project));

            var fileName = project.FileName;

            if (fileName.EndsWith(@".sitecore"))
            {
                fileName = fileName.Left(fileName.Length - 9);
            }

            try
            {
                var dte = SitecorePackage.Instance.Dte;
                var projects = dte.Solution.Projects.Cast<EnvDTE.Project>();

                return projects.FirstOrDefault(item => string.Compare(item.FileName, fileName, StringComparison.InvariantCultureIgnoreCase) == 0);
            }
            catch (Exception ex)
            {
                AppHost.Output.Log(ex.Message);
                return null;
            }
        }

        public static bool IsValidProjectKind([NotNull] this EnvDTE.Project project)
        {
            Assert.ArgumentNotNull(project, nameof(project));

            var kind = project.Kind;

            if (kind == @"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}")
            {
                return true;
            }

            if (kind == @"{F184B08F-C81C-45F6-A57F-5ABD9991F28F}")
            {
                return true;
            }

            if (kind == @"{349C5851-65DF-11DA-9384-00065B846F21}")
            {
                return true;
            }

            return false;
        }
    }
}
