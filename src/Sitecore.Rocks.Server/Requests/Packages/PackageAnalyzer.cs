// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.IO;
using Sitecore.Data;
using Sitecore.Data.Managers;
using Sitecore.Extensions.StringExtensions;
using Sitecore.Globalization;
using Sitecore.Install.Framework;

namespace Sitecore.Rocks.Server.Requests.Packages
{
    public class PackageAnalyzer : BaseSink<PackageEntry>
    {
        private readonly List<PackageFile> files = new List<PackageFile>();

        private readonly List<PackageItem> items = new List<PackageItem>();

        public PackageAnalyzer(IProcessingContext context)
        {
            Initialize(context);

            Name = string.Empty;
            Author = string.Empty;
            Version = string.Empty;
            Publisher = string.Empty;
            License = string.Empty;
            Comment = string.Empty;
            Readme = string.Empty;
        }

        [NotNull]
        public string Author { get; private set; }

        [NotNull]
        public string Comment { get; private set; }

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

        public override void Put(PackageEntry entry)
        {
            if (entry.Key.StartsWith("items/"))
            {
                items.Add(new PackageItem(entry));
                return;
            }

            if (entry.Key.StartsWith("files/"))
            {
                files.Add(new PackageFile(entry));
                return;
            }

            switch (entry.Key)
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

        [NotNull]
        private string GetData([NotNull] PackageEntry entry)
        {
            var holder = entry.GetStream();
            if (holder == null)
            {
                return string.Empty;
            }

            if (holder.Stream == null)
            {
                return string.Empty;
            }

            var stream = new StreamReader(holder.Stream);

            return stream.ReadToEnd();
        }

        public class PackageFile
        {
            public PackageFile(PackageEntry entry)
            {
                Entry = entry;
                FileName = entry.Key.Mid(5);
            }

            [NotNull]
            public PackageEntry Entry { get; private set; }

            [NotNull]
            public string FileName { get; private set; }
        }

        public class PackageItem
        {
            public PackageItem(PackageEntry entry)
            {
                Entry = entry;
                DatabaseName = entry.Properties["database"];
                ID = new ID(entry.Properties["id"]);
                Language = LanguageManager.GetLanguage(entry.Properties["language"]);
                Version = Version.Parse(entry.Properties["version"]);
                Revision = entry.Properties["revision"];

                var parts = entry.Key.Split('/');

                ItemName = parts[parts.Length - 5];

                var path = string.Empty;
                for (var n = 2; n < parts.Length - 5; n++)
                {
                    path += "/" + parts[n];
                }

                Path = path;
            }

            [NotNull]
            public string DatabaseName { get; private set; }

            [NotNull]
            public PackageEntry Entry { get; private set; }

            [NotNull]
            public ID ID { get; private set; }

            [NotNull]
            public string ItemName { get; private set; }

            [NotNull]
            public Language Language { get; private set; }

            [NotNull]
            public string Path { get; private set; }

            [NotNull]
            public string Revision { get; private set; }

            [NotNull]
            public Version Version { get; private set; }
        }
    }
}
