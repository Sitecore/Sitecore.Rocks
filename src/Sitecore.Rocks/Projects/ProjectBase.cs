// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.IO;
using Sitecore.Rocks.Projects.ProjectItems;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Projects
{
    public abstract class ProjectBase
    {
        private readonly List<ProjectItemBase> projectItems = new List<ProjectItemBase>();

        public readonly object SyncRoot = new object();

        private bool dontNag;

        private string fileName;

        protected ProjectBase([NotNull] string fileName)
        {
            Debug.ArgumentNotNull(fileName, nameof(fileName));

            FileName = fileName;

            ProjectSiteItem = new ProjectSiteItem(this);
            projectItems.Add(ProjectSiteItem);

            Notifications.SiteModified += SiteModified;
        }

        [NotNull]
        public string FileName
        {
            get { return fileName; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                fileName = value;

                var name = Path.GetFileNameWithoutExtension(FileName);
                if (name.EndsWith(@".csproj", StringComparison.InvariantCultureIgnoreCase))
                {
                    name = name.Left(name.Length - 7);
                }

                Name = name;
            }
        }

        [NotNull]
        public string FolderName
        {
            get { return Path.GetDirectoryName(FileName) ?? string.Empty; }
        }

        [NotNull]
        public string HostName
        {
            get { return ProjectSiteItem.HostName; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                ProjectSiteItem.HostName = value;
                Modified = true;
            }
        }

        public bool IsRemoteSitecore
        {
            get { return ProjectSiteItem.IsRemoteSitecore; }

            set { ProjectSiteItem.IsRemoteSitecore = value; }
        }

        public bool Modified { get; set; }

        [NotNull]
        public string Name { get; private set; }

        [NotNull]
        public virtual string OutputFileName
        {
            get { return string.Empty; }
        }

        [NotNull]
        public virtual string OutputFolder
        {
            get { return string.Empty; }
        }

        [NotNull]
        public IEnumerable<ProjectItemBase> ProjectItems
        {
            get { return projectItems; }
        }

        [NotNull]
        public ProjectSiteItem ProjectSiteItem { get; }

        [CanBeNull]
        public Site Site
        {
            get
            {
                if (string.IsNullOrEmpty(HostName))
                {
                    return null;
                }

                var result = SiteManager.FindSite(HostName, UserName);
                if (result != null)
                {
                    return result;
                }

                if (dontNag)
                {
                    return null;
                }

                dontNag = true;

                if (AppHost.MessageBox(string.Format("The project \"{0}\" uses a connection to \"{0} - {1}\".\n\nDo you want to create the connection now?", HostName, UserName), Resources.Information, MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                {
                    return null;
                }

                var site = SiteManager.NewConnection(HostName, UserName);
                if (site != null)
                {
                    HostName = site.HostName;
                    UserName = site.UserName;
                    Save();
                }

                return site;
            }
        }

        public bool SynchronizeOutputFolderOnBuild { get; set; }

        [NotNull]
        public string UserName
        {
            get { return ProjectSiteItem.UserName; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                ProjectSiteItem.UserName = value;
                Modified = true;
            }
        }

        public void Add([NotNull] ProjectItemBase projectItem)
        {
            Assert.ArgumentNotNull(projectItem, nameof(projectItem));

            lock (SyncRoot)
            {
                projectItems.Add(projectItem);
            }

            Modified = true;
        }

        public void Clear()
        {
            projectItems.Clear();
            projectItems.Add(ProjectSiteItem);
            Modified = true;
        }

        public bool Contains([NotNull] string relativeFileName)
        {
            Assert.ArgumentNotNull(relativeFileName, nameof(relativeFileName));

            if (IsFolder(relativeFileName))
            {
                return false;
            }

            return projectItems.OfType<ProjectItem>().FirstOrDefault(item => string.Compare(item.Path, relativeFileName, StringComparison.InvariantCultureIgnoreCase) == 0) != null;
        }

        [CanBeNull]
        public string GetLinkedFileName([NotNull] ItemUri itemUri)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            return AppHost.Projects.GetLinkedFileName(this, itemUri);
        }

        [CanBeNull]
        public ProjectItem GetProjectItem([NotNull] string relativeFileName)
        {
            Assert.ArgumentNotNull(relativeFileName, nameof(relativeFileName));

            if (IsFolder(relativeFileName))
            {
                return null;
            }

            return projectItems.OfType<ProjectItem>().FirstOrDefault(item => string.Compare(item.Path, relativeFileName, StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        [NotNull]
        public string GetRelativeFileName([NotNull] string sourceFileName)
        {
            Assert.ArgumentNotNull(sourceFileName, nameof(sourceFileName));

            var folder = Path.GetDirectoryName(FileName) ?? string.Empty;

            if (sourceFileName.StartsWith(folder, StringComparison.InvariantCultureIgnoreCase))
            {
                sourceFileName = sourceFileName.Mid(folder.Length);

                if (sourceFileName.StartsWith(@"\"))
                {
                    sourceFileName = sourceFileName.Mid(1);
                }
            }

            return sourceFileName;
        }

        public void LinkItemAndFile([NotNull] ItemUri itemUri, [NotNull] string relativeFileName, bool saveProject = true)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(relativeFileName, nameof(relativeFileName));

            AppHost.Projects.LinkItemAndFile(this, itemUri, relativeFileName, saveProject);
        }

        [NotNull]
        public string MakeAbsoluteFileName([NotNull] string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            fileName = fileName.Replace("/", "\\");

            if (fileName.StartsWith("\\"))
            {
                fileName = fileName.Mid(1);
            }

            return Path.Combine(FolderName, fileName);
        }

        public void Reload()
        {
            var projectLoader = AppHost.Container.Get<ProjectLoader>();
            projectLoader.Load(this, FileName);

            // make sure the project file is updated
            Save();

            Modified = false;
        }

        public void Remove([NotNull] ProjectItem projectItem)
        {
            Assert.ArgumentNotNull(projectItem, nameof(projectItem));

            lock (SyncRoot)
            {
                projectItems.Remove(projectItem);
            }

            Modified = true;
        }

        public virtual void Save()
        {
            var writer = new StringWriter();
            var output = new OutputWriter(writer);

            output.WriteStartElement("Project");

            foreach (var projectItem in projectItems)
            {
                projectItem.Save(output);
            }

            output.WriteEndElement();

            Saving();
            try
            {
                System.IO.File.WriteAllText(FileName, writer.ToString(), Encoding.UTF8);
            }
            catch (UnauthorizedAccessException)
            {
                AppHost.MessageBox(string.Format("You do not have write access to the file:\n\n{0}", FileName), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Saved();
            }

            Modified = false;
        }

        public virtual void Unload()
        {
            Notifications.SiteModified -= SiteModified;
        }

        protected virtual void Saved()
        {
        }

        protected virtual void Saving()
        {
        }

        private bool IsFolder([NotNull] string relativeFileName)
        {
            Debug.ArgumentNotNull(relativeFileName, nameof(relativeFileName));

            return relativeFileName.EndsWith(@"\");
        }

        private void SiteModified([NotNull] object sender, [NotNull] Site site, [NotNull] string oldHostName, [NotNull] string oldUserName)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(site, nameof(site));
            Debug.ArgumentNotNull(oldHostName, nameof(oldHostName));
            Debug.ArgumentNotNull(oldUserName, nameof(oldUserName));

            if (HostName != oldHostName || UserName != oldUserName)
            {
                return;
            }

            HostName = site.HostName;
            UserName = site.UserName;
            Save();
        }
    }
}
