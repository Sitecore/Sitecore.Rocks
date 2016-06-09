// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Projects;

namespace Sitecore.Rocks.UI.FolderSynchronization
{
    public class FolderSynchronizer
    {
        public FolderSynchronizer([NotNull] Project project, [NotNull] string sourceFolder, [NotNull] string destinationFolder, FolderSynchronizationMode mode, string pattern)
        {
            Assert.ArgumentNotNull(project, nameof(project));
            Assert.ArgumentNotNull(sourceFolder, nameof(sourceFolder));
            Assert.ArgumentNotNull(destinationFolder, nameof(destinationFolder));
            Assert.ArgumentNotNull(pattern, nameof(pattern));

            Project = project;
            SourceFolder = sourceFolder;
            DestinationFolder = destinationFolder;
            Mode = mode;
            Pattern = pattern;

            if (!Path.IsPathRooted(sourceFolder))
            {
                sourceFolder = Path.Combine(project.FolderName, sourceFolder);
            }

            if (!sourceFolder.EndsWith("\\"))
            {
                sourceFolder += "\\";
            }

            AbsoluteSourceFolder = sourceFolder;
            AbsoluteDestinationFolder = string.Empty;

            if (!Path.IsPathRooted(destinationFolder))
            {
                var site = project.Site;
                if (site == null)
                {
                    AppHost.Output.Log("Folder Synchronization: Folder are not synchronized as Sitecore site does not have a Web Root Path");
                    return;
                }

                destinationFolder = Path.Combine(site.WebRootPath, destinationFolder);
            }

            if (!destinationFolder.EndsWith("\\"))
            {
                destinationFolder += "\\";
            }

            AbsoluteDestinationFolder = destinationFolder;
        }

        [NotNull]
        public string AbsoluteDestinationFolder { get; }

        [NotNull]
        public string AbsoluteSourceFolder { get; }

        [NotNull]
        public string DestinationFolder { get; private set; }

        public FolderSynchronizationMode Mode { get; set; }

        [NotNull]
        public string Pattern { get; }

        [NotNull]
        public Project Project { get; private set; }

        [NotNull]
        public string SourceFolder { get; private set; }

        public void Copy()
        {
            var folderSynchronizationManager = AppHost.Container.Get<FolderSynchronizationManager>();

            var pattern = GetPatterns();
            foreach (var p in pattern)
            {
                folderSynchronizationManager.LogOperation(string.Format("SyncNow/Copy ({0})", p), AbsoluteSourceFolder, AbsoluteDestinationFolder);

                var process = new Process
                {
                    StartInfo =
                    {
                        Arguments = string.Format("\"{0}{2}\" \"{1}\" /e /d /i /y /r /c", AbsoluteSourceFolder, AbsoluteDestinationFolder, p),
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        FileName = "xcopy"
                    }
                };

                try
                {
                    process.Start();
                    process.WaitForExit(1000 * 60);
                }
                catch (Exception ex)
                {
                    folderSynchronizationManager.LogException(string.Format("SyncNow/Copy ({0})", p), AbsoluteSourceFolder, AbsoluteDestinationFolder, ex);
                    AppHost.MessageBox(string.Format("Failed to copy:\n\n{0}", ex.Message), "Error,", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public string[] GetPatterns()
        {
            if (string.IsNullOrEmpty(Pattern))
            {
                return new[]
                {
                    "*.*"
                };
            }

            return Pattern.Split(',').Select(p => p.Trim()).ToArray();
        }

        public void Mirror()
        {
            var folderSynchronizationManager = AppHost.Container.Get<FolderSynchronizationManager>();

            var patterns = GetPatterns();
            foreach (var p in patterns)
            {
                folderSynchronizationManager.LogOperation(string.Format("SyncNow/Mirror ({0})", p), AbsoluteSourceFolder, AbsoluteDestinationFolder);

                var process = new Process
                {
                    StartInfo =
                    {
                        Arguments = string.Format("\"{0}\" \"{1}\" \"{2}\" /mir /njh /njs /ndl /nc /ns /np /A-:R", AbsoluteSourceFolder.TrimEnd('\\'), AbsoluteDestinationFolder.TrimEnd('\\'), p),
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        FileName = "robocopy"
                    }
                };

                try
                {
                    process.Start();
                    process.WaitForExit(1000 * 60);
                }
                catch (Exception ex)
                {
                    folderSynchronizationManager.LogException(string.Format("SyncNow/Mirror ({0})", p), AbsoluteSourceFolder, AbsoluteDestinationFolder, ex);
                    AppHost.MessageBox(string.Format("Failed to mirror:\n\n{0}", ex.Message), "Error,", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void Sync()
        {
            if (Mode == FolderSynchronizationMode.Mirror)
            {
                Mirror();
            }
            else
            {
                Copy();
            }
        }
    }
}
