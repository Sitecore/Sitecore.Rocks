// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Projects;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Shell.Environment
{
    public class ProjectHost : IEnumerable<ProjectBase>
    {
        [CanBeNull]
        public virtual ProjectBase AddFileToProject([NotNull] string parentFileName, [NotNull] string fileName, bool dependOnParent)
        {
            Assert.ArgumentNotNull(parentFileName, nameof(parentFileName));
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            return null;
        }

        [CanBeNull]
        public virtual string CreateVisualStudioProject([NotNull] string projectType, [NotNull] string fileName = "", [NotNull] string defaultProjectName = "")
        {
            Assert.ArgumentNotNull(projectType, nameof(projectType));
            Assert.ArgumentNotNull(fileName, nameof(fileName));
            Assert.ArgumentNotNull(defaultProjectName, nameof(defaultProjectName));

            AppHost.MessageBox("This application does not support creating Visual Studio projects.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

            return null;
        }

        public virtual IEnumerator<ProjectBase> GetEnumerator()
        {
            return Enumerable.Empty<ProjectBase>().GetEnumerator();
        }

        [NotNull]
        public virtual string GetLinkedFileName([NotNull] ProjectBase project, [NotNull] ItemUri itemUri)
        {
            Assert.ArgumentNotNull(project, nameof(project));
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            return string.Empty;
        }

        [CanBeNull]
        public virtual ProjectBase GetProjectContainingFileName([NotNull] Site site, [NotNull] string relativeFileName)
        {
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(relativeFileName, nameof(relativeFileName));

            return null;
        }

        [CanBeNull]
        public virtual ProjectBase GetProjectContainingLinkedItem([NotNull] ItemUri itemUri)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            return null;
        }

        public virtual bool IsLinked([NotNull] ProjectBase project, [NotNull] ItemUri itemUri)
        {
            Assert.ArgumentNotNull(project, nameof(project));
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            return false;
        }

        public virtual bool IsSolutionOpen()
        {
            return false;
        }

        public virtual void LinkItemAndFile([NotNull] ProjectBase project, [NotNull] ItemUri itemUri, [NotNull] string relativeFileName, bool saveProject = true)
        {
            Assert.ArgumentNotNull(project, nameof(project));
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(relativeFileName, nameof(relativeFileName));
        }

        [CanBeNull]
        public virtual ProjectBase LinkSiteAndProject([NotNull] Site site, [NotNull] string projectFileName)
        {
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(projectFileName, nameof(projectFileName));

            return null;
        }

        [CanBeNull]
        public virtual string MakeAbsoluteFileName([NotNull] Site site, [NotNull] string relativeFileName)
        {
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(relativeFileName, nameof(relativeFileName));

            if (string.IsNullOrEmpty(site.WebRootPath))
            {
                return null;
            }

            relativeFileName = relativeFileName.Replace("/", "\\");
            if (relativeFileName.StartsWith("\\"))
            {
                relativeFileName = relativeFileName.Mid(1);
            }

            return Path.Combine(site.WebRootPath, relativeFileName);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
