// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Text;

namespace Sitecore.Rocks.IO.Zip.Utils
{
    internal class ZipConstants
    {
        public const uint EndOfCentralDirectorySignature = 0x06054b50;

        public const int ZipDirEntrySignature = 0x02014b50;

        public const int ZipEntryDataDescriptorSignature = 0x08074b50;

        public const int ZipEntrySignature = 0x04034b50;

        public static Encoding ZipEncoding
        {
            get { return Encoding.ASCII; }
        }
    }
}
