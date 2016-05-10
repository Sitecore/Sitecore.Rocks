// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.UI.Repositories
{
    public class RepositoryEntry
    {
        public RepositoryEntry([Localizable(false), NotNull] string name, [NotNull] string location)
        {
            Assert.ArgumentNotNull(name, nameof(name));
            Assert.ArgumentNotNull(location, nameof(location));

            Name = name;
            Location = location;
        }

        [NotNull]
        public string Location { get; }

        [NotNull]
        public string Name { get; private set; }

        [NotNull]
        public string Path
        {
            get
            {
                var folder = Location;

                // handle relative folders
                if (folder.StartsWith(".\\"))
                {
                    folder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, folder.Mid(2));
                }

                return folder;
            }
        }

        [NotNull]
        public virtual IEnumerable<string> GetFiles([NotNull] string pattern)
        {
            Assert.ArgumentNotNull(pattern, nameof(pattern));

            return GetFiles(Path, pattern);
        }

        [NotNull]
        protected virtual IEnumerable<string> GetFiles([NotNull] string folder, [NotNull] string pattern)
        {
            Debug.ArgumentNotNull(folder, nameof(folder));
            Debug.ArgumentNotNull(pattern, nameof(pattern));

            if (!Directory.Exists(folder))
            {
                yield break;
            }

            foreach (var fileName in AppHost.Files.GetFiles(folder, pattern))
            {
                yield return fileName;
            }

            foreach (var subfolder in AppHost.Files.GetDirectories(folder))
            {
                foreach (var fileName in GetFiles(subfolder, pattern))
                {
                    yield return fileName;
                }
            }
        }
    }
}
