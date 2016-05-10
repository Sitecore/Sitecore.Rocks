// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Install.Framework;
using Sitecore.Install.Zip;
using Sitecore.IO;
using Sitecore.Rocks.Server.IO;
using Sitecore.Rocks.Server.Packages;
using Sitecore.Web;

namespace Sitecore.Rocks.Server.Requests.Packages
{
    public class CreateAntiPackage
    {
        [NotNull]
        public string Execute([NotNull] string fileName)
        {
            var sourceFileName = LocalFile.MapPath(fileName);

            var targetFileName = GetTargetFileName(sourceFileName);

            var packageAnalyzer = new PackageAnalyzer(new SimpleProcessingContext());

            var reader = new PackageReader(sourceFileName);
            reader.Populate(packageAnalyzer);

            var newItems = new List<PackageAnalyzer.PackageItem>();
            var newFiles = new List<PackageAnalyzer.PackageFile>();

            var package = new ZipPackageBuilder(targetFileName)
            {
                PackageName = "Anti Package for " + Path.GetFileNameWithoutExtension(sourceFileName),
                Readme = string.Format("Anti Package for {0}. Created {1} by {2}.", Path.GetFileNameWithoutExtension(sourceFileName), DateTime.Now.ToString(), Context.GetUserName())
            };

            AddItems(packageAnalyzer, package, newItems);
            AddFiles(packageAnalyzer, package, newFiles);
            AddPostStep(package, newItems, newFiles);

            package.Build();

            return WebUtil.GetServerUrl() + LocalFile.UnmapPath(targetFileName);
        }

        private void AddDescendants(ZipPackageBuilder package, Item item)
        {
            package.Items.Add(item);

            foreach (Item child in item.Children)
            {
                AddDescendants(package, child);
            }
        }

        private void AddFiles(PackageAnalyzer packageAnalyzer, ZipPackageBuilder package, List<PackageAnalyzer.PackageFile> newFiles)
        {
            foreach (var packageFile in packageAnalyzer.Files)
            {
                var file = FileUtil.MapPath(packageFile.FileName);

                if (File.Exists(file))
                {
                    package.Files.Add(packageFile.FileName);
                }
                else
                {
                    newFiles.Add(packageFile);
                }
            }
        }

        private void AddItems(PackageAnalyzer packageAnalyzer, ZipPackageBuilder package, List<PackageAnalyzer.PackageItem> newItems)
        {
            var items = new List<Item>();
            var paths = new Dictionary<string, PackageAnalyzer.PackageItem>();

            foreach (var packageItem in packageAnalyzer.Items)
            {
                var database = Factory.GetDatabase(packageItem.DatabaseName);
                if (database == null)
                {
                    continue;
                }

                var item = database.GetItem(packageItem.ID);
                if (item == null)
                {
                    AddNewItem(paths, packageItem);
                    continue;
                }

                var isAdded = items.Any(i => i.ID == item.ID || i.Axes.IsAncestorOf(item));
                if (!isAdded)
                {
                    items.Add(item);
                }
            }

            newItems.AddRange(paths.Values);

            foreach (var item in items)
            {
                AddDescendants(package, item);
            }
        }

        private void AddNewItem(Dictionary<string, PackageAnalyzer.PackageItem> paths, PackageAnalyzer.PackageItem packageItem)
        {
            var path = packageItem.Path;

            if (paths.Keys.Any(p => path.StartsWith(p, StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }

            var descendants = paths.Keys.Where(p => p.StartsWith(path, StringComparison.InvariantCultureIgnoreCase)).ToList();

            foreach (var key in descendants)
            {
                paths.Remove(key);
            }

            paths[packageItem.Path] = packageItem;
        }

        private void AddPostStep(ZipPackageBuilder package, List<PackageAnalyzer.PackageItem> newItems, List<PackageAnalyzer.PackageFile> newFiles)
        {
            if (newItems.Count == 0 && newFiles.Count == 0)
            {
                return;
            }

            package.PostStep = "Sitecore.Rocks.Server.Requests.Packages.PackagePostStep,Sitecore.Rocks.Server";
            package.Comment = GetPostStepData(newItems, newFiles);
        }

        [NotNull]
        private string GetPostStepData([NotNull] List<PackageAnalyzer.PackageItem> newItems, [NotNull] List<PackageAnalyzer.PackageFile> newFiles)
        {
            var writer = new StringWriter();
            var output = new XmlTextWriter(writer)
            {
                Formatting = Formatting.Indented
            };

            output.WriteStartElement("uninstall");

            WriteItems(output, newItems);
            WriteFiles(output, newFiles);

            output.WriteEndElement();

            return writer.ToString();
        }

        [NotNull]
        private string GetTargetFileName([NotNull] string sourceFileName)
        {
            var result = Path.Combine(Path.GetDirectoryName(sourceFileName) ?? string.Empty, Path.GetFileNameWithoutExtension(sourceFileName) + ".anti" + Path.GetExtension(sourceFileName));

            var index = 1;
            while (File.Exists(result))
            {
                result = Path.Combine(Path.GetDirectoryName(sourceFileName) ?? string.Empty, Path.GetFileNameWithoutExtension(sourceFileName) + ".anti (" + index + ")" + Path.GetExtension(sourceFileName));
                index++;
            }

            return result;
        }

        private void WriteFiles(XmlTextWriter output, List<PackageAnalyzer.PackageFile> newFiles)
        {
            if (!newFiles.Any())
            {
                return;
            }

            output.WriteStartElement("files");

            foreach (var packageFile in newFiles)
            {
                output.WriteStartElement("file");
                output.WriteAttributeString("filename", packageFile.FileName);
                output.WriteEndElement();
            }

            output.WriteEndElement();
        }

        private void WriteItems(XmlTextWriter output, List<PackageAnalyzer.PackageItem> newItems)
        {
            if (!newItems.Any())
            {
                return;
            }

            output.WriteStartElement("items");

            foreach (var packageItem in newItems)
            {
                output.WriteStartElement("item");
                output.WriteAttributeString("database", packageItem.DatabaseName);
                output.WriteAttributeString("id", packageItem.ID.ToString());
                output.WriteEndElement();
            }

            output.WriteEndElement();
        }
    }
}
