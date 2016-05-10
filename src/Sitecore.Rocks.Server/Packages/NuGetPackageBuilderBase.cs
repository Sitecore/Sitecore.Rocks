// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Sitecore.Data.Serialization;
using Sitecore.Diagnostics;
using Sitecore.IO;

namespace Sitecore.Rocks.Server.Packages
{
    public abstract class NuGetPackageBuilderBase : PackageBuilderBase
    {
        protected NuGetPackageBuilderBase([NotNull] string fileName) : base(fileName)
        {
            Debug.ArgumentNotNull(fileName, nameof(fileName));
        }

        protected override string BuildPackage()
        {
            var packageName = Path.GetFileNameWithoutExtension(FileName);
            var folder = Path.Combine(TempFolder.Folder, "nuget." + packageName);

            var index = 0;
            while (FileUtil.Exists(folder))
            {
                folder = Path.Combine(TempFolder.Folder, "nuget." + packageName + "[" + index + "]");
                index++;
            }

            folder = FileUtil.MapPath(folder);
            FileUtil.CreateFolder(folder);

            var nuspecFileName = Path.Combine(folder, packageName + ".nuspec.txt");

            var serializationFolder = Path.Combine(folder, "serialization");
            var packageId = Regex.Replace(PackageName, "\\W", string.Empty);

            using (var output = new XmlTextWriter(nuspecFileName, Encoding.UTF8))
            {
                output.Formatting = Formatting.Indented;

                output.WriteStartElement("package");

                output.WriteStartElement("metadata");
                output.WriteElementString("id", packageId);
                output.WriteElementString("version", Version);
                output.WriteElementString("title", PackageName);
                output.WriteElementString("authors", Author);
                output.WriteElementString("owners", Publisher);
                output.WriteElementString("description", Readme);
                output.WriteElementString("summary", Readme);
                output.WriteElementString("releaseNotes", Comment);
                output.WriteElementString("copyright", string.Empty);
                output.WriteElementString("tags", "Sitecore Package");

                WriteMetaData(output);

                output.WriteEndElement();

                output.WriteStartElement("files");

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

                            output.WriteStartElement("file");

                            output.WriteAttributeString("src", file);
                            output.WriteAttributeString("target", TargetFileFolder + FileUtil.UnmapPath(file, false));

                            output.WriteEndElement();
                        }
                    }
                    else
                    {
                        output.WriteStartElement("file");

                        output.WriteAttributeString("src", FileUtil.MapPath(fileName));
                        output.WriteAttributeString("target", TargetFileFolder + FileUtil.UnmapPath(fileName, false));

                        output.WriteEndElement();
                    }
                }

                // make sure install.ps1 and init.ps1 are run
                if (!Files.Any() || TargetFileFolder != "content")
                {
                    if (Items.Any())
                    {
                        var fileName = Path.Combine(serializationFolder, "nuget\\about.txt");
                        FileUtil.CreateFolder(Path.GetDirectoryName(fileName) ?? string.Empty);
                        File.WriteAllText(fileName, "This file ensures that the install.ps1 and init.ps1 PowerShell scripts are run when installing the NuGet package.");

                        output.WriteStartElement("file");

                        output.WriteAttributeString("src", fileName);
                        output.WriteAttributeString("target", "content\\nuget\\about.txt");

                        output.WriteEndElement();
                    }
                }

                foreach (var item in Items)
                {
                    var baseFileName = item.Database.Name + FileUtil.NormalizeWebPath(item.Paths.Path).Replace('/', '\\') + ".item";

                    var fileName = Path.Combine(serializationFolder, baseFileName);
                    FileUtil.CreateFolder(Path.GetDirectoryName(fileName) ?? string.Empty);

                    Manager.DumpItem(fileName, item);

                    output.WriteStartElement("file");

                    output.WriteAttributeString("src", fileName);
                    output.WriteAttributeString("target", "serialization\\" + baseFileName);

                    output.WriteEndElement();
                }

                WriteFiles(output);

                output.WriteEndElement();

                output.WriteEndElement();
            }

            return nuspecFileName;
        }

        protected virtual void WriteFiles([NotNull] XmlTextWriter output)
        {
            Debug.ArgumentNotNull(output, nameof(output));
        }

        protected virtual void WriteMetaData([NotNull] XmlTextWriter output)
        {
            Debug.ArgumentNotNull(output, nameof(output));
        }
    }
}
