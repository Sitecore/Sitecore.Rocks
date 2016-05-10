// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sitecore.Data;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Install;
using Sitecore.Install.Files;
using Sitecore.Install.Framework;
using Sitecore.Install.Items;
using Sitecore.Install.Zip;
using Sitecore.IO;

namespace Sitecore.Rocks.Server.Packages
{
    public class ZipPackageBuilder : PackageBuilderBase
    {
        public ZipPackageBuilder([NotNull] string fileName) : base(fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));
        }

        protected override string BuildPackage()
        {
            var project = new PackageProject();

            var sourceCollection = new SourceCollection<PackageEntry>();

            var itemSource = new ExplicitItemSource
            {
                SkipVersions = false
            };
            sourceCollection.Add(itemSource);

            var list = new List<ID>();
            foreach (var item in Items)
            {
                var i = item;
                if (list.Any(id => id == i.ID))
                {
                    continue;
                }

                list.Add(item.ID);

                var reference = new ItemReference(item.Database.Name, item.Paths.Path, item.ID, LanguageManager.DefaultLanguage, Data.Version.Latest).Reduce();

                itemSource.Entries.Add(reference.ToString());
            }

            var fileSource = new ExplicitFileSource();
            sourceCollection.Add(fileSource);

            foreach (var fileName in Files)
            {
                if (FileUtil.IsFolder(fileName))
                {
                    foreach (var file in Directory.GetFiles(FileUtil.MapPath(fileName), "*", SearchOption.AllDirectories))
                    {
                        var fileInfo = new FileInfo(file);
                        if ((fileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                        {
                            continue;
                        }

                        if ((fileInfo.Attributes & FileAttributes.System) == FileAttributes.System)
                        {
                            continue;
                        }

                        fileSource.Entries.Add(file);
                    }
                }
                else
                {
                    fileSource.Entries.Add(fileName);
                }
            }

            project.Sources.Add(sourceCollection);

            project.Name = "Sitecore Package";
            project.Metadata.PackageName = PackageName;
            project.Metadata.Author = Author;
            project.Metadata.Version = Version;
            project.Metadata.Publisher = Publisher;
            project.Metadata.License = License;
            project.Metadata.Comment = Comment;
            project.Metadata.Readme = Readme;
            project.Metadata.PostStep = PostStep;

            var context = new SimpleProcessingContext();
            var intermediateFile = GetIntermediateFileName(FileName);
            try
            {
                using (var writer = new PackageWriter(PathUtils.MapPath(intermediateFile)))
                {
                    writer.Initialize(context);
                    PackageGenerator.GeneratePackage(project, writer);
                }

                Commit(intermediateFile, FileName);
            }
            catch
            {
                Cleanup(intermediateFile);
                throw;
            }

            return FileName;
        }

        private static void Cleanup([NotNull] string tempFile)
        {
            Debug.ArgumentNotNull(tempFile, nameof(tempFile));

            try
            {
                File.Delete(tempFile);
            }
            catch
            {
                return;
            }
        }

        private static void Commit([NotNull] string tempFile, [NotNull] string filename)
        {
            Debug.ArgumentNotNull(filename, nameof(filename));
            Debug.ArgumentNotNull(tempFile, nameof(tempFile));

            if (filename != tempFile)
            {
                try
                {
                    File.Delete(filename);
                }
                catch (Exception e)
                {
                    Log.Error(e.Message, typeof(PackageGenerator));
                }

                try
                {
                    File.Move(tempFile, filename);
                }
                catch (Exception e)
                {
                    Log.Error(e.Message, typeof(PackageGenerator));
                }
            }
        }

        [NotNull]
        private static string GetIntermediateFileName([NotNull] string filename)
        {
            Debug.ArgumentNotNull(filename, nameof(filename));

            var result = filename;
            while (File.Exists(result))
            {
                result = Path.Combine(Path.GetDirectoryName(filename) ?? string.Empty, Path.GetRandomFileName() + Install.Constants.PackageExtension);
            }

            return result;
        }
    }
}
