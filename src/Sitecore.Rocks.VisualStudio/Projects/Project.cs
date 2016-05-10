// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Windows.Threading;
using EnvDTE;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ProjectExtensions;

namespace Sitecore.Rocks.Projects
{
    public class Project : ProjectBase
    {
        public delegate void ProjectBuiltEventHandler(Project project, vsBuildScope scope, vsBuildAction action);

        [CanBeNull]
        private FileSystemWatcher fileSystemWatcher;

        public Project([NotNull] string fileName) : base(fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            StartFileWatcher();
        }

        public override string OutputFileName
        {
            get
            {
                var project = this.GetVisualStudioProject();
                if (project == null)
                {
                    return string.Empty;
                }

                var property = project.Properties.Item(@"OutputFileName");
                if (property == null)
                {
                    return string.Empty;
                }

                var value = property.Value as string ?? string.Empty;
                if (string.IsNullOrEmpty(value))
                {
                    return string.Empty;
                }

                return value;
            }
        }

        public override string OutputFolder
        {
            get
            {
                var project = this.GetVisualStudioProject();
                if (project == null)
                {
                    return string.Empty;
                }

                var configurationManager = project.ConfigurationManager;
                if (configurationManager == null)
                {
                    return string.Empty;
                }

                var activeConfiguration = configurationManager.ActiveConfiguration;
                if (activeConfiguration == null)
                {
                    return string.Empty;
                }

                var property = activeConfiguration.Properties.Item("OutputPath");
                if (property == null)
                {
                    return string.Empty;
                }

                var value = property.Value as string ?? string.Empty;
                if (string.IsNullOrEmpty(value))
                {
                    return string.Empty;
                }

                if (Path.IsPathRooted(value))
                {
                    return value;
                }

                return Path.Combine(Path.GetDirectoryName(FileName) ?? string.Empty, value);
            }
        }

        public void Delete()
        {
            File.Delete(FileName);
        }

        public event FileSystemEventHandler FileChanged;

        public event FileSystemEventHandler FileCreated;

        public event FileSystemEventHandler FileDeleted;

        public event RenamedEventHandler FileRenamed;

        [NotNull]
        public static Project Load([NotNull] string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            var result = new Project(fileName);

            result.Reload();

            return result;
        }

        public event ProjectBuiltEventHandler ProjectBuilt;

        public void RaiseProjectBuilt(vsBuildScope scope, vsBuildAction action)
        {
            var handler = ProjectBuilt;
            if (handler != null)
            {
                handler(this, scope, action);
            }
        }

        public void Rename([NotNull] string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            File.Delete(FileName);
            FileName = fileName;
            Save();
        }

        public override void Unload()
        {
            base.Unload();

            if (fileSystemWatcher == null)
            {
                return;
            }

            fileSystemWatcher.EnableRaisingEvents = false;
            fileSystemWatcher.Dispose();
            fileSystemWatcher = null;
        }

        protected override void Saved()
        {
            StartFileWatcher();
        }

        protected override void Saving()
        {
            StopFileWatcher();
        }

        private void HandleFileChanged([NotNull] object sender, [NotNull] FileSystemEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.FullPath == FileName)
            {
                Dispatcher.CurrentDispatcher.Invoke(new Action(Reload));
            }

            var handler = FileChanged;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        private void HandleFileCreated([NotNull] object sender, [NotNull] FileSystemEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var handler = FileCreated;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        private void HandleFileDeleted([NotNull] object sender, [NotNull] FileSystemEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var handler = FileDeleted;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        private void HandleFileRenamed([NotNull] object sender, [NotNull] RenamedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var handler = FileRenamed;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        private void StartFileWatcher()
        {
            if (fileSystemWatcher == null)
            {
                if (!File.Exists(FileName))
                {
                    return;
                }

                fileSystemWatcher = new FileSystemWatcher(Path.GetDirectoryName(FileName) ?? string.Empty)
                {
                    IncludeSubdirectories = true
                };

                fileSystemWatcher.Changed += HandleFileChanged;
                fileSystemWatcher.Created += HandleFileCreated;
                fileSystemWatcher.Deleted += HandleFileDeleted;
                fileSystemWatcher.Renamed += HandleFileRenamed;

                fileSystemWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
            }

            fileSystemWatcher.EnableRaisingEvents = true;
        }

        private void StopFileWatcher()
        {
            if (fileSystemWatcher != null)
            {
                fileSystemWatcher.EnableRaisingEvents = false;
            }
        }
    }
}
