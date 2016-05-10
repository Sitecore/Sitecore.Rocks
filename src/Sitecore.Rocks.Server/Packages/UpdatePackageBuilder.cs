// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.IO;
using Sitecore.Data.Serialization;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Update;
using Sitecore.Update.Commands;
using Sitecore.Update.Data.Items;
using Sitecore.Update.Engine;
using Sitecore.Update.Interfaces;

namespace Sitecore.Rocks.Server.Packages
{
    public class UpdatePackageBuilder : PackageBuilderBase
    {
        public UpdatePackageBuilder([NotNull] string fileName) : base(fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));
        }

        protected override string BuildPackage()
        {
            var commands = new List<ICommand>();

            foreach (var item in Items)
            {
                var syncItem = ItemSynchronization.BuildSyncItem(item);

                var contentDataItem = new ContentDataItem(string.Empty, item.Paths.ParentPath, item.Name, syncItem);

                var command = new AddItemCommand(contentDataItem);

                commands.Add(command);
            }

            var rootPath = FileUtil.MapPath("/");
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

                        var fileItem = new FileSystemDataItem(rootPath, Path.GetDirectoryName(file), Path.GetFileName(file));
                        var command = new AddFileCommand(fileItem);
                        commands.Add(command);
                    }
                }
                else
                {
                    var fileItem = new FileSystemDataItem(rootPath, Path.GetDirectoryName(fileName), Path.GetFileName(fileName));
                    var command = new AddFileCommand(fileItem);
                    commands.Add(command);
                }
            }

            var diff = new DiffInfo(commands)
            {
                Author = Author,
                PostStep = PostStep,
                Publisher = Publisher,
                Readme = Readme,
                Version = Version,
                Title = PackageName
            };

            var directoryName = Path.GetDirectoryName(FileName) ?? string.Empty;
            directoryName = directoryName.Replace("\\", "/");
            var folderPath = FileUtil.MapPath(directoryName);

            diff.Serialize(folderPath);

            var packageFileName = FileUtil.UnmapPath(folderPath, false) + "/" + DiffInfo.OutputFileName;

            PackageGenerator.GeneratePackage(diff, packageFileName);

            return packageFileName;
        }
    }
}
