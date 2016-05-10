// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;

namespace Sitecore.Rocks.IO.Zip.Utils
{
    public class StreamUtils
    {
        public static void CopyBlock(Stream from, Stream to, int blockSize)
        {
            const int bufferSize = 8192;
            var totalRead = 0;
            var buffer = new byte[bufferSize];
            var l = from.Read(buffer, 0, Math.Min(bufferSize, blockSize));
            while (l > 0)
            {
                to.Write(buffer, 0, l);
                l = from.Read(buffer, 0, Math.Min(bufferSize, blockSize - totalRead));
                totalRead += l;
            }
        }

        public static void CopyStream(Stream from, Stream to, int bufferSize)
        {
            var buffer = new byte[bufferSize];
            var l = from.Read(buffer, 0, bufferSize);
            while (l > 0)
            {
                to.Write(buffer, 0, l);
                l = from.Read(buffer, 0, bufferSize);
            }
        }
    }
}
