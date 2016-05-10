// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Packages.PackageBuilders
{
    public class PackageFile : IHasFileUri
    {
        public PackageFile([NotNull] FileUri fileUri)
        {
            Assert.ArgumentNotNull(fileUri, nameof(fileUri));

            FileUri = fileUri;
        }

        public FileUri FileUri { get; }

        [NotNull]
        public string FolderPath
        {
            get { return Path.GetDirectoryName(FileUri.FileName); }
        }

        [NotNull]
        public string Name
        {
            get { return Path.GetFileName(FileUri.FileName); }
        }

        public override string ToString()
        {
            return FileUri.FileName;
        }
    }
}
