// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.IO;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Data
{
    public enum FileUriBaseFolder
    {
        Web,

        Data
    }

    public class FileUri
    {
        public FileUri([NotNull] Site site, [Localizable(false), NotNull] string fileName, FileUriBaseFolder baseFolder, bool isFolder)
        {
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            BaseFolder = baseFolder;
            IsFolder = isFolder;
            Site = site;
            FileName = File.Normalize(fileName);
        }

        public FileUri([NotNull] Site site, [Localizable(false), NotNull] string fileName, bool isFolder)
        {
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            var path = fileName;

            if (path.StartsWith(@"web:"))
            {
                BaseFolder = FileUriBaseFolder.Web;
                path = path.Mid(4);
            }
            else if (path.StartsWith(@"data:"))
            {
                BaseFolder = FileUriBaseFolder.Data;
                path = path.Mid(5);
            }
            else
            {
                BaseFolder = FileUriBaseFolder.Web;
            }

            IsFolder = isFolder;
            Site = site;
            FileName = File.Normalize(path);
        }

        public FileUriBaseFolder BaseFolder { get; }

        [NotNull]
        public string FileName { get; }

        public bool IsFolder { get; private set; }

        [NotNull]
        public string RelativeFileName
        {
            get
            {
                var fileName = FileName;
                if (fileName.StartsWith(@"/") || fileName.StartsWith(@"\"))
                {
                    return fileName.Mid(1);
                }

                return FileName;
            }
        }

        [NotNull]
        public Site Site { get; }

        public bool Equals([CanBeNull] FileUri other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(other.BaseFolder, BaseFolder) && Equals(other.FileName, FileName) && Equals(other.Site, Site);
        }

        public override bool Equals([CanBeNull] object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != typeof(FileUri))
            {
                return false;
            }

            return Equals((FileUri)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = BaseFolder.GetHashCode();
                result = (result * 397) ^ FileName.GetHashCode();
                result = (result * 397) ^ Site.GetHashCode();
                return result;
            }
        }

        public static bool operator ==([CanBeNull] FileUri left, [CanBeNull] FileUri right)
        {
            return Equals(left, right);
        }

        public static bool operator !=([CanBeNull] FileUri left, [CanBeNull] FileUri right)
        {
            return !Equals(left, right);
        }

        [NotNull]
        public string ToServerPath()
        {
            if (BaseFolder == FileUriBaseFolder.Data)
            {
                return @"data:" + FileName.Replace(@"\", @"/");
            }

            return @"web:" + FileName.Replace(@"\", @"/");
        }
    }
}
