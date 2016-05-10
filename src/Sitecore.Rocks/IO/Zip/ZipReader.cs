// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sitecore.Rocks.IO.Zip.Utils;

namespace Sitecore.Rocks.IO.Zip
{
    public class ZipReader : IDisposable
    {
        private readonly Encoding _encoding;

        private Stream _inputStream;

        public ZipReader(Stream input) : this(input, ZipConstants.ZipEncoding)
        {
        }

        public ZipReader(Stream input, Encoding encoding)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            if (!input.CanRead)
            {
                throw new ArgumentException("Stream should be readable", "input");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }

            _inputStream = input;
            _encoding = encoding;
        }

        public ZipReader(string filename) : this(filename, ZipConstants.ZipEncoding)
        {
        }

        public ZipReader(string filename, Encoding encoding)
        {
            if (filename == null)
            {
                throw new ArgumentNullException("filename");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            try
            {
                _inputStream = System.IO.File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Error while opening file '{0}'", filename), e);
            }
            _encoding = encoding;
        }

        public IEnumerable<ZipEntry> Entries
        {
            get
            {
                long pos = 0;
                while (true)
                {
                    _inputStream.Seek(pos, SeekOrigin.Begin);
                    var entry = ZipEntry.Read(_inputStream, _encoding);
                    if (entry == null)
                    {
                        break;
                    }

                    pos = entry.GetNextBlockPosition();

                    yield return entry;
                }
            }
        }

        public void Dispose()
        {
            if (_inputStream != null)
            {
                _inputStream.Dispose();
                _inputStream = null;
            }
        }

        public ZipEntry GetEntry(string entryName)
        {
            foreach (var entry in Entries)
            {
                if (entry.Name == entryName)
                {
                    return entry;
                }
            }
            return null;
        }
    }
}
