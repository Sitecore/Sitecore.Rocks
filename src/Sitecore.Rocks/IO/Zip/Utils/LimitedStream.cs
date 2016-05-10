// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;

namespace Sitecore.Rocks.IO.Zip.Utils
{
    internal class LimitedReadOnlyStream : Stream
    {
        private readonly Stream _innerStream;

        private readonly long _limit = 0;

        private long _totalBytesRead = 0;

        public LimitedReadOnlyStream(Stream innerStream, int limit)
        {
            _innerStream = innerStream;
            _limit = limit;
        }

        public override bool CanRead
        {
            get { return _innerStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length
        {
            get { return _limit; }
        }

        public override long Position
        {
            get { throw new Exception("The method or operation are not accessible."); }
            set { throw new Exception("The method or operation are not accessible."); }
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!CanRead)
            {
                throw new Exception("You cannot read this stream");
            }

            if (_totalBytesRead == _limit)
            {
                return 0;
            }

            count = (int)Math.Min(_limit - _totalBytesRead, count);
            var bytesRead = _innerStream.Read(buffer, offset, count);
            _totalBytesRead += bytesRead;
            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new Exception("Operation is not supported.");
        }

        public override void SetLength(long value)
        {
            throw new Exception("Operation is not supported.");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new Exception("Operation is not supported.");
        }
    }
}
