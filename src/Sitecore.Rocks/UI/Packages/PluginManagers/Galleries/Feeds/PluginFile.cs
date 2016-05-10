// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.IO;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Packages.PluginManagers.Galleries.Feeds
{
    public class PluginFile
    {
        private static readonly IEqualityComparer<PluginFile> FileNameComparerInstance = new FileNameEqualityComparer();

        public PluginFile([NotNull] string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            FileName = fileName;
        }

        public string FileName { get; }

        public static IEqualityComparer<PluginFile> FileNameComparer
        {
            get { return FileNameComparerInstance; }
        }

        [NotNull]
        public string FolderPath
        {
            get { return Path.GetDirectoryName(FileName); }
        }

        [NotNull]
        public string Name
        {
            get { return Path.GetFileName(FileName); }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((PluginFile)obj);
        }

        public override int GetHashCode()
        {
            return FileName != null ? FileName.GetHashCode() : 0;
        }

        public static bool operator ==(PluginFile left, PluginFile right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PluginFile left, PluginFile right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return FileName;
        }

        protected bool Equals(PluginFile other)
        {
            return string.Equals(FileName, other.FileName);
        }

        private sealed class FileNameEqualityComparer : IEqualityComparer<PluginFile>
        {
            public bool Equals(PluginFile x, PluginFile y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }
                if (ReferenceEquals(x, null))
                {
                    return false;
                }
                if (ReferenceEquals(y, null))
                {
                    return false;
                }
                if (x.GetType() != y.GetType())
                {
                    return false;
                }
                return string.Equals(x.FileName, y.FileName);
            }

            public int GetHashCode(PluginFile obj)
            {
                return obj.FileName != null ? obj.FileName.GetHashCode() : 0;
            }
        }
    }
}
