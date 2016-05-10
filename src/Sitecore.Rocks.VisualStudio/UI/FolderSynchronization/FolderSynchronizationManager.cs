// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using EnvDTE;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Managers;
using Sitecore.Rocks.Extensions.ProjectItemExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.IO;
using Sitecore.Rocks.Projects;

namespace Sitecore.Rocks.UI.FolderSynchronization
{
    public enum FolderSynchronizationMode
    {
        Mirror,

        Copy
    }

    public class FolderSynchronizationManager : ManagerBase
    {
        public readonly List<FolderSynchronizer> Folders = new List<FolderSynchronizer>();

        public readonly List<string> Log = new List<string>();

        public readonly object LogSyncObject = new object();

        private readonly object syncObject = new object();

        public void Add([NotNull] Projects.Project project, [NotNull] string sourceFolder, [NotNull] string destinationFolder, FolderSynchronizationMode mode, string pattern)
        {
            Assert.ArgumentNotNull(project, nameof(project));
            Assert.ArgumentNotNull(sourceFolder, nameof(sourceFolder));
            Assert.ArgumentNotNull(destinationFolder, nameof(destinationFolder));
            Assert.ArgumentNotNull(pattern, nameof(pattern));

            var folder = new FolderSynchronizer(project, sourceFolder, destinationFolder, mode, pattern);
            lock (syncObject)
            {
                Folders.Add(folder);
            }

            SaveProject(project);
        }

        [NotNull]
        public string GetFolderFileName([NotNull] ProjectBase project, [NotNull] EnvDTE.ProjectItem item)
        {
            Assert.ArgumentNotNull(project, nameof(project));
            Assert.ArgumentNotNull(item, nameof(item));

            var result = project.GetRelativeFileName(item.GetFileName());

            result = result.TrimStart('\\');
            if (!result.EndsWith("\\"))
            {
                result += "\\";
            }

            return result;
        }

        public bool GetSynchronizeOutputFolderOnBuild([NotNull] ProjectBase project)
        {
            Assert.ArgumentNotNull(project, nameof(project));

            return project.SynchronizeOutputFolderOnBuild;
        }

        [CanBeNull]
        public FolderSynchronizer GetSynchronizer([NotNull] ProjectBase project, [NotNull] EnvDTE.ProjectItem projectItem)
        {
            Assert.ArgumentNotNull(project, nameof(project));
            Assert.ArgumentNotNull(projectItem, nameof(projectItem));

            var sourceFolder = project.GetRelativeFileName(projectItem.GetFileName());
            lock (syncObject)
            {
                return Folders.FirstOrDefault(f => f.Project == project && string.Compare(f.SourceFolder, sourceFolder, StringComparison.InvariantCultureIgnoreCase) == 0);
            }
        }

        public bool IsSynced([NotNull] ProjectBase project, [NotNull] EnvDTE.ProjectItem item)
        {
            Assert.ArgumentNotNull(project, nameof(project));
            Assert.ArgumentNotNull(item, nameof(item));

            var sourceFolder = GetFolderFileName(project, item);

            lock (syncObject)
            {
                return Folders.Any(f => f.Project == project && string.Compare(f.SourceFolder, sourceFolder, StringComparison.InvariantCultureIgnoreCase) == 0);
            }
        }

        public void LogException([NotNull] string operation, [NotNull] string source, [NotNull] string destination, [NotNull] Exception ex)
        {
            Assert.ArgumentNotNull(operation, nameof(operation));
            Assert.ArgumentNotNull(source, nameof(source));
            Assert.ArgumentNotNull(destination, nameof(destination));
            Assert.ArgumentNotNull(ex, nameof(ex));

            var message = string.Format("Error: {0}: {1}, {4} [{2} -> {3}]", operation, Path.GetFileName(source), source, destination, ex.Message);

            LogMessage(message);
        }

        public void LogMessage([NotNull] string message)
        {
            Debug.ArgumentNotNull(message, nameof(message));

            lock (LogSyncObject)
            {
                while (Log.Count > 200)
                {
                    Log.RemoveAt(0);
                }

                Log.Add(message);
            }
        }

        public void LogOperation([NotNull] string operation, [NotNull] string source, [NotNull] string destination)
        {
            Debug.ArgumentNotNull(operation, nameof(operation));
            Debug.ArgumentNotNull(source, nameof(source));
            Debug.ArgumentNotNull(destination, nameof(destination));

            var message = string.Format("{0}: {1} [{2} -> {3}]", operation, Path.GetFileName(source), source, destination);

            LogMessage(message);
        }

        public void LogOperation([NotNull] string operation, [NotNull] string source)
        {
            Debug.ArgumentNotNull(operation, nameof(operation));
            Debug.ArgumentNotNull(source, nameof(source));

            var message = string.Format("{0}: {1} [{2}]", operation, Path.GetFileName(source), source);

            LogMessage(message);
        }

        public void Remove([NotNull] ProjectBase project, [NotNull] string sourceFolder)
        {
            Assert.ArgumentNotNull(project, nameof(project));
            Assert.ArgumentNotNull(sourceFolder, nameof(sourceFolder));

            lock (syncObject)
            {
                var folder = Folders.FirstOrDefault(f => f.Project == project && string.Compare(f.SourceFolder, sourceFolder, StringComparison.InvariantCultureIgnoreCase) == 0);
                if (folder == null)
                {
                    return;
                }

                Folders.Remove(folder);
            }

            SaveProject(project);
        }

        public void Remove([NotNull] ProjectBase project, [NotNull] FolderSynchronizer folder)
        {
            Assert.ArgumentNotNull(project, nameof(project));
            Assert.ArgumentNotNull(folder, nameof(folder));

            lock (syncObject)
            {
                Folders.Remove(folder);
            }

            SaveProject(project);
        }

        public void SetSynchronizeOutputFolderOnBuild([NotNull] ProjectBase project, bool isSynchronized)
        {
            Assert.ArgumentNotNull(project, nameof(project));

            project.SynchronizeOutputFolderOnBuild = isSynchronized;
            SaveProject(project);
        }

        protected override void Initialize()
        {
            ProjectManager.ProjectAdded += AddProject;
            ProjectManager.ProjectRemoved += RemoveProject;
        }

        private void AddProject([NotNull] Projects.Project project)
        {
            Debug.ArgumentNotNull(project, nameof(project));

            if (Folders.Any(f => f.Project == project))
            {
                return;
            }

            project.FileChanged += FileChanged;
            project.FileCreated += FileChanged;
            project.FileDeleted += FileDeleted;
            project.FileRenamed += FileRenamed;
            project.ProjectBuilt += ProjectBuilt;

            LoadProject(project, project.FileName);
        }

        private void FileChanged([NotNull] object sender, [NotNull] FileSystemEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (!System.IO.File.Exists(e.FullPath))
            {
                return;
            }

            var result = GetDestinationFile(e.FullPath);
            if (result == null)
            {
                return;
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(result.Item2) ?? string.Empty);

                if (!System.IO.File.Exists(e.FullPath))
                {
                    LogOperation("Created/Changed: Source file not found", e.FullPath);
                    return;
                }

                System.IO.File.Copy(e.FullPath, result.Item2, true);

                LogOperation("Created/Changed", e.FullPath, result.Item2);
            }
            catch (FileNotFoundException)
            {
                LogOperation("Created/Changed: Source file not found", e.FullPath);
            }
            catch (Exception ex)
            {
                AppHost.Output.LogException(ex);

                LogException("Created/Changed", e.FullPath, result.Item2, ex);
            }
        }

        private void FileDeleted([NotNull] object sender, [NotNull] FileSystemEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var result = GetDestinationFile(e.FullPath);
            if (result == null)
            {
                return;
            }

            if (result.Item1 == FolderSynchronizationMode.Copy || !System.IO.File.Exists(result.Item2))
            {
                return;
            }

            try
            {
                System.IO.File.Delete(result.Item2);
                LogOperation("Deleted", e.FullPath, result.Item2);
            }
            catch (FileNotFoundException)
            {
                LogOperation("Deleted: Source file not found", e.FullPath);
            }
            catch (Exception ex)
            {
                AppHost.Output.LogException(ex);
                LogException("Deleted", e.FullPath, result.Item2, ex);
            }
        }

        private void FileRenamed([NotNull] object sender, [NotNull] RenamedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var newFileName = GetDestinationFile(e.FullPath);
            if (newFileName == null)
            {
                return;
            }

            if (newFileName.Item1 == FolderSynchronizationMode.Mirror)
            {
                var oldFileName = GetDestinationFile(e.OldFullPath);
                if (oldFileName != null)
                {
                    try
                    {
                        System.IO.File.Delete(oldFileName.Item2);
                        LogOperation("Renamed/Deleted", oldFileName.Item2);
                    }
                    catch (Exception ex)
                    {
                        LogException("Renamed/Deleted", oldFileName.Item2, ex);
                        AppHost.Output.LogException(ex);
                    }
                }
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(newFileName.Item2) ?? string.Empty);

                System.IO.File.Copy(e.FullPath, newFileName.Item2, true);

                LogOperation("Renamed/Copied", e.FullPath, newFileName.Item2);
            }
            catch (Exception ex)
            {
                LogException("Renamed/Copied", e.FullPath, newFileName.Item2, ex);
            }
        }

        [NotNull]
        private string GetConfigFileName([NotNull] ProjectBase project)
        {
            Debug.ArgumentNotNull(project, nameof(project));

            return Path.ChangeExtension(project.FileName, ".sitecore.foldersync");
        }

        [CanBeNull]
        private Tuple<FolderSynchronizationMode, string> GetDestinationFile([NotNull] string sourceFile)
        {
            Debug.ArgumentNotNull(sourceFile, nameof(sourceFile));

            FolderSynchronizer folderDescriptor;

            lock (syncObject)
            {
                folderDescriptor = Folders.FirstOrDefault(f => sourceFile.StartsWith(f.AbsoluteSourceFolder, StringComparison.InvariantCultureIgnoreCase));
            }

            if (folderDescriptor == null)
            {
                return null;
            }

            if (!MatchesPattern(sourceFile, folderDescriptor))
            {
                return null;
            }

            var fileName = Path.Combine(folderDescriptor.AbsoluteDestinationFolder, sourceFile.Mid(folderDescriptor.AbsoluteSourceFolder.Length));

            return new Tuple<FolderSynchronizationMode, string>(folderDescriptor.Mode, fileName);
        }

        private void LoadProject([NotNull] Projects.Project project, [NotNull] string projectFileName)
        {
            Debug.ArgumentNotNull(project, nameof(project));
            Debug.ArgumentNotNull(projectFileName, nameof(projectFileName));

            var fileName = GetConfigFileName(project);

            if (!System.IO.File.Exists(fileName))
            {
                return;
            }

            var root = System.IO.File.ReadAllText(fileName).ToXElement();
            if (root == null)
            {
                AppHost.Output.Log("Folder Sync: Project is not well-formed: " + fileName);
                LogMessage(string.Format("Error: Load Project: {0} is not wellformed", fileName));
                return;
            }

            var folderSyncElement = root.Element("FolderSync");
            if (folderSyncElement == null)
            {
                AppHost.Output.Log("Folder Sync: Project root element is missing: " + fileName);
                LogMessage("Error: Load Project: FolderSync element is missing");
                return;
            }

            project.SynchronizeOutputFolderOnBuild = folderSyncElement.GetAttributeValue("SynchronizeProjectOutputFolderOnBuild") == "True";
            foreach (var element in folderSyncElement.Elements("Folder"))
            {
                var sourceFolder = element.GetAttributeValue("SourceFolder");
                var destinationFolder = element.GetAttributeValue("DestinationFolder");
                var pattern = element.GetAttributeValue("Pattern");
                var folder = new FolderSynchronizer(project, sourceFolder, destinationFolder, element.GetAttributeValue("Mode") == "Mirror" ? FolderSynchronizationMode.Mirror : FolderSynchronizationMode.Copy, pattern);

                lock (syncObject)
                {
                    Folders.Add(folder);
                }

                AppHost.Output.Log("Folder Sync: Monitoring : " + folder.AbsoluteSourceFolder + " => " + folder.AbsoluteDestinationFolder);
                LogMessage(string.Format("Load Project: Monitoring {0}", folder.AbsoluteSourceFolder));
            }
        }

        private void LogException([NotNull] string operation, [NotNull] string source, [NotNull] Exception ex)
        {
            Debug.ArgumentNotNull(operation, nameof(operation));
            Debug.ArgumentNotNull(source, nameof(source));
            Debug.ArgumentNotNull(ex, nameof(ex));

            var message = string.Format("Error: {0}: {1}, {3} [{2}]", operation, Path.GetFileName(source), source, ex.Message);

            LogMessage(message);
        }

        private bool MatchesPattern(string sourceFile, FolderSynchronizer folderDescriptor)
        {
            if (string.IsNullOrEmpty(folderDescriptor.Pattern))
            {
                return true;
            }

            var fileName = Path.GetFileName(sourceFile);

            var patterns = folderDescriptor.GetPatterns();
            foreach (var p in patterns)
            {
                var regex = "^" + Regex.Escape(p).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
                if (Regex.IsMatch(fileName, regex))
                {
                    return true;
                }
            }

            return false;
        }

        private void ProjectBuilt([NotNull] Projects.Project project, vsBuildScope scope, vsBuildAction action)
        {
            Debug.ArgumentNotNull(project, nameof(project));

            if (action == vsBuildAction.vsBuildActionClean || action == vsBuildAction.vsBuildActionDeploy)
            {
                return;
            }

            if (!project.SynchronizeOutputFolderOnBuild)
            {
                return;
            }

            var outputFolder = project.OutputFolder;
            if (outputFolder.EndsWith("\\") || outputFolder.EndsWith("/"))
            {
                outputFolder = outputFolder.Left(outputFolder.Length - 1);
            }

            if (!AppHost.Files.FolderExists(outputFolder))
            {
                LogMessage("Error: Build: Output folder not found: " + outputFolder);
                return;
            }

            var outputFileName = project.OutputFileName;
            if (string.IsNullOrEmpty(outputFileName))
            {
                return;
            }

            outputFileName = Path.Combine(outputFolder, outputFileName);

            if (!AppHost.Files.FileExists(outputFileName))
            {
                LogMessage("Error: Build: Output file not found: " + outputFileName);
                return;
            }

            var site = project.Site;
            if (site == null)
            {
                LogMessage("Error: Build: Project is not connected to a website");
                return;
            }

            if (string.IsNullOrEmpty(site.WebRootPath))
            {
                LogMessage("Error: Build: Site does not have a Web Root Path");
                return;
            }

            var targetFolder = Path.Combine(site.WebRootPath, "bin");
            if (!AppHost.Files.FolderExists(targetFolder))
            {
                LogMessage("Error: Build: Target folder does not exist: " + targetFolder);
                return;
            }

            var targetFileName = Path.Combine(targetFolder, Path.GetFileName(outputFileName) ?? string.Empty);

            var targetFileInfo = new FileInfo(targetFileName);
            if (targetFileInfo.Exists)
            {
                var sourceFileInfo = new FileInfo(outputFileName);
                if (targetFileInfo.LastWriteTimeUtc >= sourceFileInfo.LastWriteTimeUtc)
                {
                    return;
                }
            }

            try
            {
                AppHost.Files.Copy(outputFileName, targetFileName, true);
                LogOperation("Build", outputFileName, targetFileName);
            }
            catch (Exception ex)
            {
                LogException("Build", outputFileName, targetFileName, ex);
            }

            /*
      foreach (var sourceFileName in Directory.GetFiles(outputFolder))
      {
        var sourceFileInfo = new FileInfo(sourceFileName);
        if ((sourceFileInfo.Attributes & FileAttributes.System) == FileAttributes.System)
        {
          continue;
        }

        if ((sourceFileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
        {
          continue;
        }

        var targetFileName = Path.Combine(targetFolder, Path.GetFileName(sourceFileName) ?? string.Empty);

        var targetFileInfo = new FileInfo(targetFileName);
        if (targetFileInfo.Exists)
        {
          if (targetFileInfo.LastWriteTimeUtc >= sourceFileInfo.LastWriteTimeUtc)
          {
            continue;
          }
        }

        try
        {
          AppHost.Files.Copy(sourceFileName, targetFileName, true);
          this.LogOperation("Build", sourceFileName, targetFileName);
        }
        catch (Exception ex)
        {
          this.LogException("Build", sourceFileName, targetFileName, ex);
        }
      }
      */
        }

        private void RemoveProject([NotNull] Projects.Project project)
        {
            Debug.ArgumentNotNull(project, nameof(project));

            project.FileChanged -= FileChanged;
            project.FileCreated -= FileChanged;
            project.FileDeleted -= FileDeleted;
            project.FileRenamed -= FileRenamed;

            lock (syncObject)
            {
                for (var i = Folders.Count - 1; i >= 0; i--)
                {
                    var folder = Folders[i];
                    if (folder.Project == project)
                    {
                        Folders.Remove(folder);
                    }
                }
            }
        }

        private void SaveProject([NotNull] ProjectBase project)
        {
            Debug.ArgumentNotNull(project, nameof(project));

            var writer = new StringWriter();
            var output = new OutputWriter(writer);

            output.WriteStartElement("Project");
            output.WriteStartElement("FolderSync");

            output.WriteAttributeString("SynchronizeProjectOutputFolderOnBuild", project.SynchronizeOutputFolderOnBuild ? "True" : "False");

            lock (syncObject)
            {
                foreach (var folder in Folders.Where(f => f.Project == project))
                {
                    output.WriteStartElement("Folder");

                    output.WriteAttributeString("SourceFolder", folder.SourceFolder);
                    output.WriteAttributeString("DestinationFolder", folder.DestinationFolder);
                    output.WriteAttributeString("Mode", folder.Mode == FolderSynchronizationMode.Copy ? "Copy" : "Mirror");
                    output.WriteAttributeString("Pattern", folder.Pattern);

                    output.WriteEndElement();
                }
            }

            output.WriteEndElement();
            output.WriteEndElement();

            var configFileName = GetConfigFileName(project);
            System.IO.File.WriteAllText(configFileName, writer.ToString(), Encoding.UTF8);
        }
    }
}
