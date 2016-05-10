// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Sitecore.Rocks.IO.Zip.Utils;

namespace Sitecore.Rocks.IO.Zip
{
    public class ZipEntry
    {
        private short _bitField;

        private int _compressedSize;

        private short _compressionMethod;

        private int _crc32;

        private byte[] _extra;

        private Stream _fileStream;

        private int _fileStreamFilePosition = 0;

        private int _lastModDateTime;

        private DateTime _lastModified;

        private string _name;

        private int _uncompressedSize;

        private short _versionNeeded;

        private ZipEntry()
        {
        }

        public bool IsDirectory
        {
            get { return _name.EndsWith("/"); }
        }

        public string Name
        {
            get { return _name; }
        }

        public long Size
        {
            get { return _uncompressedSize; }
        }

        public Stream GetStream()
        {
            // extract a directory to streamwriter?  nothing to do!
            if (_name.EndsWith("/") || _uncompressedSize == 0)
            {
                return Stream.Null;
            }

            _fileStream.Seek(_fileStreamFilePosition, SeekOrigin.Begin);

            Stream source;

            if (_compressionMethod == 0)
            {
                // the System.IO.Compression.DeflateStream class does not handle uncompressed data.
                // so if an entry is not compressed, then we just translate the bytes directly.
                source = _fileStream;
            }
            else
            {
                source = new DeflateStream(_fileStream, CompressionMode.Decompress, true);
            }
            return new LimitedReadOnlyStream(source, _uncompressedSize);
        }

        public static ZipEntry Read(Stream s, Encoding nameEncoding)
        {
            var entry = new ZipEntry();

            if (!ReadHeader(s, entry, nameEncoding))
            {
                return null;
            }
            entry._fileStreamFilePosition = (int)s.Position;
            entry._fileStream = s;

            // finally, seek past the (already read) Data descriptor if necessary
            if ((entry._bitField & 0x0008) == 0x0008)
            {
                s.Seek(16, SeekOrigin.Current);
            }
            return entry;
        }

        protected internal static long FindSignature(Stream s, int SignatureToFind)
        {
            var startingPosition = s.Position;

            var BATCH_SIZE = 1024;
            var targetBytes = new byte[4];
            targetBytes[0] = (byte)(SignatureToFind >> 24);
            targetBytes[1] = (byte)((SignatureToFind & 0x00FF0000) >> 16);
            targetBytes[2] = (byte)((SignatureToFind & 0x0000FF00) >> 8);
            targetBytes[3] = (byte)(SignatureToFind & 0x000000FF);
            var batch = new byte[BATCH_SIZE];
            var success = false;
            do
            {
                var n = s.Read(batch, 0, batch.Length);
                if (n != 0)
                {
                    for (var i = 0; i < n; i++)
                    {
                        if (batch[i] == targetBytes[3])
                        {
                            s.Seek(i - n, SeekOrigin.Current);
                            var sig = ReadSignature(s);
                            success = sig == SignatureToFind;
                            if (!success)
                            {
                                s.Seek(-3, SeekOrigin.Current);
                            }
                            break; // out of for loop
                        }
                    }
                }
                else
                {
                    break;
                }
                if (success)
                {
                    break;
                }
            }
            while (true);
            if (!success)
            {
                s.Seek(startingPosition, SeekOrigin.Begin);
                return -1; // or throw?
            }

            // subtract 4 for the signature.
            var bytesRead = s.Position - startingPosition - 4;

            // number of bytes read, should be the same as compressed size of file            
            return bytesRead;
        }

        protected internal static int ReadSignature(Stream s)
        {
            var sig = new byte[4];
            var n = s.Read(sig, 0, sig.Length);
            if (n != sig.Length)
            {
                throw new Exception("Could not read signature - no data!");
            }
            return ((sig[3] * 256 + sig[2]) * 256 + sig[1]) * 256 + sig[0];
        }

        internal long GetNextBlockPosition()
        {
            return _fileStreamFilePosition + _compressedSize + ((_bitField & 8) > 0 ? 12 : 0);
        }

        private static bool ReadHeader(Stream s, ZipEntry ze, Encoding nameEncoding)
        {
            var signature = ReadSignature(s);

            // Return false if this is not a local file header signature.
            if (signature != ZipConstants.ZipEntrySignature)
            {
                s.Seek(-4, SeekOrigin.Current); // unread the signature

                // Getting "not a ZipEntry signature" is not always wrong or an error. 
                // This can happen when walking through a zipfile.  After the last compressed entry, 
                // we expect to read a ZipDirEntry signature.  When we get this is how we 
                // know we've reached the end of the compressed entries. 
                if (signature != ZipConstants.ZipDirEntrySignature)
                {
                    throw new Exception(string.Format("  ZipEntry::Read(): Bad signature ({0:X8}) at position  0x{1:X8}", signature, s.Position));
                }
                return false;
            }

            var block = new byte[26];
            var n = s.Read(block, 0, block.Length);
            if (n != block.Length)
            {
                return false;
            }

            var i = 0;
            ze._versionNeeded = (short)(block[i++] + block[i++] * 256);
            ze._bitField = (short)(block[i++] + block[i++] * 256);
            ze._compressionMethod = (short)(block[i++] + block[i++] * 256);
            ze._lastModDateTime = block[i++] + block[i++] * 256 + block[i++] * 256 * 256 + block[i++] * 256 * 256 * 256;

            // the PKZIP spec says that if bit 3 is set (0x0008), then the CRC, Compressed size, and uncompressed size
            // come directly after the file data.  The only way to find it is to scan the zip archive for the signature of 
            // the Data Descriptor, and presume that that signature does not appear in the (compressed) data of the compressed file.  

            if ((ze._bitField & 0x0008) != 0x0008)
            {
                ze._crc32 = block[i++] + block[i++] * 256 + block[i++] * 256 * 256 + block[i++] * 256 * 256 * 256;
                ze._compressedSize = block[i++] + block[i++] * 256 + block[i++] * 256 * 256 + block[i++] * 256 * 256 * 256;
                ze._uncompressedSize = block[i++] + block[i++] * 256 + block[i++] * 256 * 256 + block[i++] * 256 * 256 * 256;
            }
            else
            {
                // the CRC, compressed size, and uncompressed size are stored later in the stream.
                // here, we advance the pointer.
                i += 12;
            }

            var filenameLength = (short)(block[i++] + block[i++] * 256);
            var extraFieldLength = (short)(block[i++] + block[i++] * 256);

            block = new byte[filenameLength];
            s.Read(block, 0, block.Length);
            ze._name = StringFromBuffer(block, block.Length, nameEncoding);

            ze._extra = new byte[extraFieldLength];
            s.Read(ze._extra, 0, ze._extra.Length);

            // transform the time data into something usable
            ze._lastModified = DateUtil.Unpack(ze._lastModDateTime);

            // actually get the compressed size and CRC if necessary
            if ((ze._bitField & 0x0008) == 0x0008)
            {
                var posn = s.Position;
                var SizeOfDataRead = FindSignature(s, ZipConstants.ZipEntryDataDescriptorSignature);
                if (SizeOfDataRead == -1)
                {
                    return false;
                }

                // read 3x 4-byte fields (CRC, Compressed Size, Uncompressed Size)
                block = new byte[12];
                n = s.Read(block, 0, block.Length);
                if (n != 12)
                {
                    return false;
                }
                i = 0;
                ze._crc32 = block[i++] + block[i++] * 256 + block[i++] * 256 * 256 + block[i++] * 256 * 256 * 256;
                ze._compressedSize = block[i++] + block[i++] * 256 + block[i++] * 256 * 256 + block[i++] * 256 * 256 * 256;
                ze._uncompressedSize = block[i++] + block[i++] * 256 + block[i++] * 256 * 256 + block[i++] * 256 * 256 * 256;

                if (SizeOfDataRead != ze._compressedSize)
                {
                    throw new Exception("Data format error (bit 3 is set)");
                }

                // seek back to previous position, to read file data
                s.Seek(posn, SeekOrigin.Begin);
            }

            return true;
        }

        private static string StringFromBuffer(byte[] buf, int maxlength, Encoding encoding)
        {
            if (maxlength > buf.Length)
            {
                maxlength = buf.Length;
            }
            var length = Array.IndexOf(buf, 0, 0, maxlength);
            if (length < 0)
            {
                length = maxlength;
            }
            return encoding.GetString(buf, 0, length);
        }
    }
}
