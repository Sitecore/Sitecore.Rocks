// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.GuidExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.Shell.Environment
{
    public enum FileStatus
    {
        NotLocked,

        AccessDenied,

        UsedByAnotherProcess,

        FileNotFound
    }

    public class FilesHost
    {
        private static readonly char[] Slashes =
        {
            '\\',
            '/'
        };

        public virtual bool ContainsInvalidFileNameChars([NotNull] string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            return fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0;
        }

        public virtual bool ContainsInvalidPathChars([NotNull] string path)
        {
            Assert.ArgumentNotNull(path, nameof(path));

            return path.IndexOfAny(Path.GetInvalidPathChars()) >= 0;
        }

        public virtual void Copy([NotNull] string sourceFileName, [NotNull] string destFileName, bool overwrite)
        {
            Assert.ArgumentNotNull(sourceFileName, nameof(sourceFileName));
            Assert.ArgumentNotNull(destFileName, nameof(destFileName));

            AccessFile(sourceFileName, FileAccess.Read, FileShare.Read);

            File.Copy(sourceFileName, destFileName, overwrite);
        }

        public virtual void CopyWithElevatedRights([NotNull] string source, [NotNull] string targetFolder)
        {
            Assert.ArgumentNotNull(source, nameof(source));
            Assert.ArgumentNotNull(targetFolder, nameof(targetFolder));

            if (!targetFolder.EndsWith("\\"))
            {
                targetFolder += "\\";
            }

            var arguments = "\"" + source + "\" \"" + targetFolder + "\" /i /y";

            var startInfo = new ProcessStartInfo("xcopy", arguments)
            {
                Verb = "runas",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = false
            };

            var process = Process.Start(startInfo);
            if (process == null)
            {
                return;
            }

            process.WaitForExit(5000);
        }

        public virtual void CreateDirectory([NotNull] string path)
        {
            Assert.ArgumentNotNull(path, nameof(path));

            try
            {
                Directory.CreateDirectory(path);
            }
            catch (IOException ex)
            {
                AppHost.Output.LogException(ex);
                AppHost.MessageBox($"An error occured while creating the directory:\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public virtual void Delete([NotNull] string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            File.Delete(fileName);
        }

        public void DeleteFolder([NotNull] string folder)
        {
            Assert.ArgumentNotNull(folder, nameof(folder));

            IO.File.DeleteFolder(folder);
        }

        [NotNull]
        public virtual Tuple<string, FileSystemWatcher> EditText([NotNull] string text, [NotNull] string fileName, [NotNull] string extension, [NotNull] Action<string> edited)
        {
            Assert.ArgumentNotNull(text, nameof(text));
            Assert.ArgumentNotNull(fileName, nameof(fileName));
            Assert.ArgumentNotNull(extension, nameof(extension));
            Assert.ArgumentNotNull(edited, nameof(edited));

            var tempFolder = Path.Combine(AppHost.User.UserFolder, "Editing");
            tempFolder = Path.Combine(tempFolder, Guid.NewGuid().ToShortId());
            AppHost.Files.CreateDirectory(tempFolder);

            fileName = Path.ChangeExtension(Path.Combine(tempFolder, fileName), extension);

            AppHost.Files.WriteAllText(fileName, text, Encoding.UTF8);

            AppHost.Files.OpenFile(fileName);

            var fileSystemWatcher = new FileSystemWatcher
            {
                Path = tempFolder,
                EnableRaisingEvents = true
            };

            var changedHandler = new Action(delegate
            {
                string newText;

                AppHost.Files.AccessFile(fileName, FileAccess.Read);

                try
                {
                    var file = new StreamReader(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.UTF8);
                    try
                    {
                        newText = file.ReadToEnd();
                    }
                    finally
                    {
                        file.Close();
                    }
                }
                catch (Exception ex)
                {
                    AppHost.Output.LogException(ex);
                    return;
                }

                edited(newText);
            });

            FileSystemEventHandler editedHandler = delegate(object sender, FileSystemEventArgs args)
            {
                if (args.FullPath == fileName)
                {
                    changedHandler();
                }
            };

            RenamedEventHandler renamedHandler = delegate(object sender, RenamedEventArgs args)
            {
                if (args.FullPath == fileName)
                {
                    changedHandler();
                }
            };

            fileSystemWatcher.Created += editedHandler;
            fileSystemWatcher.Changed += editedHandler;
            fileSystemWatcher.Renamed += renamedHandler;

            return new Tuple<string, FileSystemWatcher>(fileName, fileSystemWatcher);
        }

        public virtual bool FileExists([NotNull] string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            return File.Exists(fileName);
        }

        public virtual bool FolderExists([NotNull] string folder)
        {
            Assert.ArgumentNotNull(folder, nameof(folder));

            return Directory.Exists(folder);
        }

        [NotNull]
        public virtual string[] GetDirectories([NotNull] string path)
        {
            Assert.ArgumentNotNull(path, nameof(path));

            var result = new List<string>();

            var directories = Directory.GetDirectories(path);
            foreach (var directory in directories)
            {
                var info = new DirectoryInfo(directory);
                if ((info.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    continue;
                }

                if ((info.Attributes & FileAttributes.System) == FileAttributes.System)
                {
                    continue;
                }

                result.Add(directory);
            }

            return result.ToArray();
        }

        [NotNull]
        public virtual string GetDirectoryName([NotNull] string path)
        {
            Assert.ArgumentNotNull(path, nameof(path));

            var n = path.LastIndexOfAny(Slashes);
            return n >= 0 ? path.Left(n) : path;
        }

        [NotNull]
        public virtual string[] GetFiles([NotNull] string path)
        {
            Assert.ArgumentNotNull(path, nameof(path));

            var result = new List<string>();

            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                var info = new FileInfo(file);
                if ((info.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    continue;
                }

                if ((info.Attributes & FileAttributes.System) == FileAttributes.System)
                {
                    continue;
                }

                result.Add(file);
            }

            return result.ToArray();
        }

        [NotNull]
        public virtual string[] GetFiles([NotNull] string path, [NotNull] string searchPattern)
        {
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(searchPattern, nameof(searchPattern));

            var result = new List<string>();

            var files = Directory.GetFiles(path, searchPattern);
            foreach (var file in files)
            {
                var info = new FileInfo(file);
                if ((info.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    continue;
                }

                if ((info.Attributes & FileAttributes.System) == FileAttributes.System)
                {
                    continue;
                }

                result.Add(file);
            }

            return result.ToArray();
        }

        public FileStatus GetFileStatus([NotNull] string path, FileAccess fileAccess, FileShare fileShare)
        {
            Diagnostics.Debug.ArgumentNotNull(path, nameof(path));

            var fileInfo = new FileInfo(path);

            if (fileAccess == FileAccess.Write && !fileInfo.Exists)
            {
                return FileStatus.NotLocked;
            }

            var retries = 0;
            var sleep = 10;
            bool retry;

            do
            {
                retry = false;
                var isLocked = false;

                FileStream stream = null;
                try
                {
                    stream = fileInfo.Open(FileMode.Open, fileAccess, fileShare);
                }
                catch (UnauthorizedAccessException)
                {
                    return FileStatus.AccessDenied;
                }
                catch (SecurityException)
                {
                    return FileStatus.AccessDenied;
                }
                catch (FileNotFoundException)
                {
                    return FileStatus.FileNotFound;
                }
                catch (DirectoryNotFoundException)
                {
                    return FileStatus.FileNotFound;
                }
                catch (IOException)
                {
                    isLocked = true;
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Close();
                    }
                }

                if (isLocked)
                {
                    retries++;
                    if (retries >= 10)
                    {
                        return FileStatus.UsedByAnotherProcess;
                    }

                    retry = true;
                    Thread.Sleep(sleep);
                    sleep = sleep * 2;
                }
            }
            while (retry);

            return FileStatus.NotLocked;
        }

        public virtual void OpenFile([NotNull] string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));
        }

        public virtual void OpenInWindowsExplorer([NotNull] string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            IO.File.OpenInWindowsExplorer(fileName);
        }

        [NotNull]
        public virtual string ReadAllText([NotNull] string path)
        {
            Assert.ArgumentNotNull(path, nameof(path));

            AccessFile(path, FileAccess.Read, FileShare.Read);

            return File.ReadAllText(path);
        }

        public virtual void Start([NotNull] string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            try
            {
                Process.Start(fileName);
            }
            catch (Win32Exception ex)
            {
                if (ex.Message.Contains("Class not registered"))
                {
                    if (AppHost.MessageBox("The default browser could not be opened.\n\nDo you want to open Internet Explorer?", "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                    {
                        var startInfo = new ProcessStartInfo("explorer.exe", fileName);
                        Process.Start(startInfo);
                    }

                    return;
                }

                throw;
            }
        }

        public virtual void WriteAllText([NotNull] string path, [NotNull] string contents)
        {
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(contents, nameof(contents));

            WriteAllText(path, contents, Encoding.UTF8);
        }

        public virtual void WriteAllText([NotNull] string path, [NotNull] string contents, [NotNull] Encoding encoding)
        {
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(contents, nameof(contents));
            Assert.ArgumentNotNull(encoding, nameof(encoding));

            AccessFile(path, FileAccess.Write);

            try
            {
                File.WriteAllText(path, contents, encoding);
            }
            catch (UnauthorizedAccessException)
            {
                AppHost.MessageBox(string.Format("Access to the path \"{0}\" is denied.\n\nPlease make that you have write permission to this path.", path), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected void AccessFile([NotNull] string path, FileAccess fileAccess, FileShare fileShare = FileShare.None)
        {
            Diagnostics.Debug.ArgumentNotNull(path, nameof(path));

            switch (GetFileStatus(path, fileAccess, fileShare))
            {
                case FileStatus.AccessDenied:
                    AppHost.Output.Log(string.Format("Access to the path \"{0}\" is denied.", path));
                    AppHost.MessageBox(string.Format("Access to the path \"{0}\" is denied.\n\nPlease make that you have read permission to this path.", path), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    throw new SilentException();

                case FileStatus.UsedByAnotherProcess:
                    AppHost.Output.Log(string.Format("The file \"{0}\" is being used by another process.", path));
                    AppHost.MessageBox(string.Format("The file \"{0}\" is being used by another process.\n\nPlease close any applications that might access this file and try again.", path), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    throw new SilentException();

                case FileStatus.FileNotFound:
                    AppHost.Output.Log(string.Format("The file \"{0}\" was not found.", path));
                    AppHost.MessageBox(string.Format("The file \"{0}\" was not found.", path), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    throw new SilentException();
            }
        }
    }
}
