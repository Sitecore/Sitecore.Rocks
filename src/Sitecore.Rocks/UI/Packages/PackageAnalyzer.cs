// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.IO.Zip;

namespace Sitecore.Rocks.UI.Packages
{
    public class PackageAnalyzer
    {
        private readonly List<PackageFile> files = new List<PackageFile>();

        private readonly List<PackageItem> items = new List<PackageItem>();

        public PackageAnalyzer([NotNull] string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            FileName = fileName;
            Name = string.Empty;
            Author = string.Empty;
            Version = string.Empty;
            Publisher = string.Empty;
            License = string.Empty;
            Comment = string.Empty;
            Readme = string.Empty;

            Read(fileName);
        }

        [NotNull]
        public string Author { get; private set; }

        [NotNull]
        public string Comment { get; private set; }

        [NotNull]
        public string FileName { get; private set; }

        [NotNull]
        public IEnumerable<PackageFile> Files
        {
            get { return files; }
        }

        [NotNull]
        public IEnumerable<PackageItem> Items
        {
            get { return items; }
        }

        [NotNull]
        public string License { get; private set; }

        [NotNull]
        public string Name { get; private set; }

        [NotNull]
        public string PackageId { get; private set; }

        [NotNull]
        public string PostStep { get; private set; }

        [NotNull]
        public string Publisher { get; private set; }

        [NotNull]
        public string Readme { get; private set; }

        [NotNull]
        public string Revision { get; private set; }

        [NotNull]
        public string Version { get; private set; }

        [NotNull]
        private string GetData([NotNull] ZipEntry entry)
        {
            Debug.ArgumentNotNull(entry, nameof(entry));

            using (var stream = new StreamReader(entry.GetStream()))
            {
                return stream.ReadToEnd();
            }
        }

        private void Read([NotNull] string fileName)
        {
            Debug.ArgumentNotNull(fileName, nameof(fileName));

            using (var reader = new ZipReader(fileName))
            {
                var zipEntry = reader.GetEntry(@"package.zip");
                if (zipEntry != null)
                {
                    ReadPackage(zipEntry.GetStream());
                    return;
                }

                foreach (var entry in reader.Entries)
                {
                    ReadEntry(entry);
                }
            }
        }

        private void ReadEntry([NotNull] ZipEntry entry)
        {
            Debug.ArgumentNotNull(entry, nameof(entry));

            if (entry.Name.StartsWith(@"items/"))
            {
                items.Add(new PackageItem(entry));
                return;
            }

            if (entry.Name.StartsWith(@"files/"))
            {
                files.Add(new PackageFile(entry));
                return;
            }

            switch (entry.Name)
            {
                case "metadata/sc_author.txt":
                    Author = GetData(entry);
                    return;
                case "metadata/sc_comment.txt":
                    Comment = GetData(entry);
                    return;
                case "metadata/sc_license.txt":
                    License = GetData(entry);
                    return;
                case "metadata/sc_name.txt":
                    Name = GetData(entry);
                    return;
                case "metadata/sc_poststep.txt":
                    PostStep = GetData(entry);
                    return;
                case "metadata/sc_packageid.txt":
                    PackageId = GetData(entry);
                    return;
                case "metadata/sc_publisher.txt":
                    Publisher = GetData(entry);
                    return;
                case "metadata/sc_readme.txt":
                    Readme = GetData(entry);
                    return;
                case "metadata/sc_revision.txt":
                    Revision = GetData(entry);
                    return;
                case "metadata/sc_version.txt":
                    Version = GetData(entry);
                    return;
            }
        }

        private void ReadPackage([NotNull] Stream stream)
        {
            Debug.ArgumentNotNull(stream, nameof(stream));

            using (var memory = new MemoryStream())
            {
                stream.CopyTo(memory);
                memory.Seek(0, SeekOrigin.Begin);

                using (var reader = new ZipReader(memory))
                {
                    foreach (var entry in reader.Entries)
                    {
                        ReadEntry(entry);
                    }
                }
            }
        }

        public class PackageFile
        {
            public PackageFile([NotNull] ZipEntry entry)
            {
                Assert.ArgumentNotNull(entry, nameof(entry));

                Entry = entry;
                FileName = entry.Name.Mid(5);
            }

            [NotNull]
            public ZipEntry Entry { get; private set; }

            [NotNull]
            public string FileName { get; private set; }
        }

        public class PackageItem
        {
            public PackageItem([NotNull] ZipEntry entry)
            {
                Assert.ArgumentNotNull(entry, nameof(entry));

                Entry = entry;

                var parts = entry.Name.Split('/');

                DatabaseName = parts[0];
                ItemName = parts[parts.Length - 5];
                ItemId = new ItemId(new Guid(parts[parts.Length - 4]));
                Language = new Language(parts[parts.Length - 3]);
                Version = new Data.Version(int.Parse(parts[parts.Length - 2]));

                var path = string.Empty;
                for (var n = 2; n < parts.Length - 5; n++)
                {
                    path += @"/" + parts[n];
                }

                Path = path;
            }

            [NotNull]
            public string DatabaseName { get; private set; }

            [NotNull]
            public ZipEntry Entry { get; private set; }

            [NotNull]
            public ItemId ItemId { get; private set; }

            [NotNull]
            public string ItemName { get; private set; }

            [NotNull]
            public Language Language { get; private set; }

            [NotNull]
            public string Path { get; private set; }

            [NotNull]
            public Data.Version Version { get; private set; }
        }
    }
}
