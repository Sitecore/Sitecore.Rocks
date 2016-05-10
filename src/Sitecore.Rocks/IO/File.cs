// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.IO
{
    public static class File
    {
        public static bool CopyDirectory([Localizable(false), NotNull] string sourcePath, [Localizable(false), NotNull] string targetPath, bool overwrite)
        {
            Assert.ArgumentNotNull(sourcePath, nameof(sourcePath));
            Assert.ArgumentNotNull(targetPath, nameof(targetPath));

            var result = true;
            try
            {
                sourcePath = sourcePath.EndsWith(@"\", StringComparison.OrdinalIgnoreCase) ? sourcePath : sourcePath + @"\";
                targetPath = targetPath.EndsWith(@"\", StringComparison.OrdinalIgnoreCase) ? targetPath : targetPath + @"\";

                if (Directory.Exists(sourcePath))
                {
                    if (!Directory.Exists(targetPath))
                    {
                        Directory.CreateDirectory(targetPath);
                    }

                    foreach (var fileName in AppHost.Files.GetFiles(sourcePath))
                    {
                        var fileInfo = new FileInfo(fileName);
                        var targetFileName = targetPath + fileInfo.Name;

                        fileInfo.CopyTo(targetFileName, overwrite);
                    }

                    foreach (var folder in AppHost.Files.GetDirectories(sourcePath))
                    {
                        var directoryInfo = new DirectoryInfo(folder);

                        if (!CopyDirectory(folder, targetPath + directoryInfo.Name, overwrite))
                        {
                            result = false;
                        }
                    }
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }

        public static void CopyFile([Localizable(false), NotNull] string targetFolder, [Localizable(false), NotNull] string sourceFile)
        {
            Assert.ArgumentNotNull(targetFolder, nameof(targetFolder));
            Assert.ArgumentNotNull(sourceFile, nameof(sourceFile));

            var targetFile = Path.Combine(targetFolder, Path.GetFileName(sourceFile));

            try
            {
                System.IO.File.Copy(sourceFile, targetFile, true);
            }
            catch (Exception ex)
            {
                AppHost.MessageBox(string.Format(Resources.CopyFiles_CopyFile_, sourceFile, ex.Message), Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void DeleteFolder([Localizable(false), NotNull] string folder)
        {
            Assert.ArgumentNotNull(folder, nameof(folder));

            DeleteFolder(folder, DateTime.MaxValue);
        }

        public static void DeleteFolder([Localizable(false), NotNull] string folder, DateTime timestamp)
        {
            Assert.ArgumentNotNull(folder, nameof(folder));

            DeleteInternal(folder, timestamp);
        }

        [NotNull]
        public static string GetRelativePath([Localizable(false), NotNull] string folder, [Localizable(false), NotNull] string baseFolder)
        {
            Assert.ArgumentNotNull(folder, nameof(folder));
            Assert.ArgumentNotNull(baseFolder, nameof(baseFolder));

            while (baseFolder.EndsWith(@"\"))
            {
                baseFolder = baseFolder.Left(baseFolder.Length - 1);
            }

            if (!folder.StartsWith(baseFolder, StringComparison.InvariantCultureIgnoreCase))
            {
                return folder;
            }

            return folder.Mid(baseFolder.Length + 1);
        }

        [NotNull]
        public static string GetSafeFileName([Localizable(false), NotNull] string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            var pattern = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

            var r = new Regex(string.Format(@"[{0}]", Regex.Escape(pattern)));

            return r.Replace(fileName, string.Empty);
        }

        [NotNull]
        public static string MakeUniqueFileName([Localizable(false), NotNull] string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            var folder = Path.GetDirectoryName(fileName) ?? string.Empty;
            fileName = Path.GetFileName(fileName) ?? string.Empty;
            var extension = string.Empty;

            var n = fileName.IndexOf('.');
            if (n >= 0)
            {
                extension = fileName.Mid(n + 1);
                fileName = fileName.Left(n);
            }

            return MakeUniqueFileName(folder, fileName, extension);
        }

        [NotNull]
        public static string MakeUniqueFileName([Localizable(false), NotNull] string folder, [Localizable(false), NotNull] string fileNameWithoutExtension, [Localizable(false), NotNull] string extension)
        {
            Assert.ArgumentNotNull(folder, nameof(folder));
            Assert.ArgumentNotNull(fileNameWithoutExtension, nameof(fileNameWithoutExtension));
            Assert.ArgumentNotNull(extension, nameof(extension));

            var fileName = Path.Combine(folder, fileNameWithoutExtension + extension);

            if (!System.IO.File.Exists(fileName))
            {
                return fileName;
            }

            var n = 1;
            do
            {
                fileName = Path.Combine(folder, fileNameWithoutExtension + @" " + n);
                fileName = Path.ChangeExtension(fileName, extension);
                n++;
            }
            while (System.IO.File.Exists(fileName));

            return fileName;
        }

        [NotNull]
        public static string Normalize([Localizable(false), NotNull] string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            return fileName.Replace(@"/", @"\");
        }

        public static void OpenInWindowsExplorer([Localizable(false), NotNull] string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            if (!System.IO.File.Exists(fileName))
            {
                Process.Start(@"explorer.exe", Path.GetDirectoryName(fileName));
                return;
            }

            Process.Start(@"explorer.exe", @"/select, """ + fileName + @"""");
        }

        public static void Save([Localizable(false), NotNull] string fileName, [Localizable(false), NotNull] string value)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));
            Assert.ArgumentNotNull(value, nameof(value));

            var file = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite), Encoding.UTF8);
            try
            {
                file.Write(value);
            }
            finally
            {
                file.Close();
            }
        }

        [NotNull]
        public static string ShortenPath([Localizable(false), NotNull] string path, int length)
        {
            Assert.ArgumentNotNull(path, nameof(path));

            if (path.Length <= length)
            {
                return path;
            }

            var sb = new StringBuilder();

            PathCompactPathEx(sb, path, length, 0);

            return sb.ToString();
        }

        private static int DeleteInternal([Localizable(false), NotNull] string folder, DateTime timestamp)
        {
            Diagnostics.Debug.ArgumentNotNull(folder, nameof(folder));

            var result = 0;

            foreach (var subfolder in AppHost.Files.GetDirectories(folder))
            {
                try
                {
                    result += DeleteInternal(subfolder, timestamp);
                }
                catch (Exception ex)
                {
                    AppHost.Output.LogException(ex);
                }
            }

            foreach (var file in AppHost.Files.GetFiles(folder))
            {
                if (System.IO.File.GetLastAccessTime(file) > timestamp)
                {
                    result++;
                    continue;
                }

                try
                {
                    System.IO.File.Delete(file);
                }
                catch (Exception ex)
                {
                    AppHost.Output.LogException(ex);
                }
            }

            if (result == 0)
            {
                try
                {
                    Directory.Delete(folder);
                }
                catch (Exception ex)
                {
                    AppHost.Output.LogException(ex);
                }
            }

            return result;
        }

        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        private static extern bool PathCompactPathEx([Out] StringBuilder pszOut, string szPath, int cchMax, int dwFlags);
    }
}
