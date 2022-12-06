// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MacBinary.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Filters.
//
// --[ Description ] ----------------------------------------------------------
//
//     Provides a filter to open MacBinary files.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filters
{
    // TODO: Interpret fdScript
    /// <inheritdoc />
    /// <summary>Decodes MacBinary files</summary>
    public sealed class MacBinary : IFilter
    {
        const uint MAGIC = 0x6D42494E;
        string     _basePath;
        byte[]     _bytes;
        DateTime   _creationTime;
        long       _dataForkOff;
        string     _filename;
        Header     _header;
        bool       _isBytes, _isStream, _isPath, _opened;
        DateTime   _lastWriteTime;
        long       _rsrcForkOff;
        Stream     _stream;

        /// <inheritdoc />
        public string Name => "MacBinary";
        /// <inheritdoc />
        public Guid Id => new Guid("D7C321D3-E51F-45DF-A150-F6BFDF0D7704");
        /// <inheritdoc />
        public string Author => "Natalia Portillo";

        /// <inheritdoc />
        public void Close()
        {
            _bytes = null;
            _stream?.Close();
            _isBytes  = false;
            _isStream = false;
            _isPath   = false;
            _opened   = false;
        }

        /// <inheritdoc />
        public string GetBasePath() => _basePath;

        /// <inheritdoc />
        public DateTime GetCreationTime() => _creationTime;

        /// <inheritdoc />
        public long GetDataForkLength() => _header.dataLength;

        /// <inheritdoc />
        public Stream GetDataForkStream()
        {
            if(_header.dataLength == 0)
                return null;

            if(_isBytes)
                return new OffsetStream(_bytes, _dataForkOff, _dataForkOff + _header.dataLength - 1);

            if(_isStream)
                return new OffsetStream(_stream, _dataForkOff, _dataForkOff + _header.dataLength - 1);

            if(_isPath)
                return new OffsetStream(_basePath, FileMode.Open, FileAccess.Read, _dataForkOff,
                                        _dataForkOff + _header.dataLength - 1);

            return null;
        }

        /// <inheritdoc />
        public string GetFilename() => _filename;

        /// <inheritdoc />
        public DateTime GetLastWriteTime() => _lastWriteTime;

        /// <inheritdoc />
        public long GetLength() => _header.dataLength + _header.resourceLength;

        /// <inheritdoc />
        public string GetParentFolder() => Path.GetDirectoryName(_basePath);

        /// <inheritdoc />
        public string GetPath() => _basePath;

        /// <inheritdoc />
        public long GetResourceForkLength() => _header.resourceLength;

        /// <inheritdoc />
        public Stream GetResourceForkStream()
        {
            if(_header.resourceLength == 0)
                return null;

            if(_isBytes)
                return new OffsetStream(_bytes, _rsrcForkOff, _rsrcForkOff + _header.resourceLength - 1);

            if(_isStream)
                return new OffsetStream(_stream, _rsrcForkOff, _rsrcForkOff + _header.resourceLength - 1);

            if(_isPath)
                return new OffsetStream(_basePath, FileMode.Open, FileAccess.Read, _rsrcForkOff,
                                        _rsrcForkOff + _header.resourceLength - 1);

            return null;
        }

        /// <inheritdoc />
        public bool HasResourceFork() => _header.resourceLength > 0;

        /// <inheritdoc />
        public bool Identify(byte[] buffer)
        {
            if(buffer        == null ||
               buffer.Length < 128)
                return false;

            byte[] hdrB = new byte[128];
            Array.Copy(buffer, 0, hdrB, 0, 128);
            _header = Marshal.ByteArrayToStructureBigEndian<Header>(hdrB);

            return _header.magic == MAGIC || (_header.version     == 0 && _header.filename[0] > 0 &&
                                              _header.filename[0] < 64 && _header.zero1 == 0 && _header.zero2 == 0 &&
                                              _header.reserved    == 0 &&
                                              (_header.dataLength > 0 || _header.resourceLength > 0));
        }

        /// <inheritdoc />
        public bool Identify(Stream stream)
        {
            if(stream        == null ||
               stream.Length < 128)
                return false;

            byte[] hdrB = new byte[128];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(hdrB, 0, 128);
            _header = Marshal.ByteArrayToStructureBigEndian<Header>(hdrB);

            return _header.magic == MAGIC || (_header.version     == 0 && _header.filename[0] > 0 &&
                                              _header.filename[0] < 64 && _header.zero1 == 0 && _header.zero2 == 0 &&
                                              _header.reserved    == 0 &&
                                              (_header.dataLength > 0 || _header.resourceLength > 0));
        }

        /// <inheritdoc />
        public bool Identify(string path)
        {
            if(!File.Exists(path))
                return false;

            var fstream = new FileStream(path, FileMode.Open, FileAccess.Read);

            if(fstream.Length < 128)
                return false;

            byte[] hdrB = new byte[128];
            fstream.Read(hdrB, 0, 128);
            _header = Marshal.ByteArrayToStructureBigEndian<Header>(hdrB);

            fstream.Close();

            return _header.magic == MAGIC || (_header.version     == 0 && _header.filename[0] > 0 &&
                                              _header.filename[0] < 64 && _header.zero1 == 0 && _header.zero2 == 0 &&
                                              _header.reserved    == 0 &&
                                              (_header.dataLength > 0 || _header.resourceLength > 0));
        }

        /// <inheritdoc />
        public bool IsOpened() => _opened;

        /// <inheritdoc />
        public void Open(byte[] buffer)
        {
            var ms = new MemoryStream(buffer);
            ms.Seek(0, SeekOrigin.Begin);

            byte[] hdrB = new byte[128];
            ms.Read(hdrB, 0, 128);
            _header = Marshal.ByteArrayToStructureBigEndian<Header>(hdrB);

            uint blocks = 1;
            blocks += (uint)(_header.secondaryHeaderLength / 128);

            if(_header.secondaryHeaderLength % 128 > 0)
                blocks++;

            _dataForkOff =  blocks             * 128;
            blocks       += _header.dataLength / 128;

            if(_header.dataLength % 128 > 0)
                blocks++;

            _rsrcForkOff = blocks * 128;

            _filename      = StringHandlers.PascalToString(_header.filename, Encoding.GetEncoding("macintosh"));
            _creationTime  = DateHandlers.MacToDateTime(_header.creationTime);
            _lastWriteTime = DateHandlers.MacToDateTime(_header.modificationTime);

            ms.Close();
            _opened  = true;
            _isBytes = true;
            _bytes   = buffer;
        }

        /// <inheritdoc />
        public void Open(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            byte[] hdrB = new byte[128];
            stream.Read(hdrB, 0, 128);
            _header = Marshal.ByteArrayToStructureBigEndian<Header>(hdrB);

            uint blocks = 1;
            blocks += (uint)(_header.secondaryHeaderLength / 128);

            if(_header.secondaryHeaderLength % 128 > 0)
                blocks++;

            _dataForkOff =  blocks             * 128;
            blocks       += _header.dataLength / 128;

            if(_header.dataLength % 128 > 0)
                blocks++;

            _rsrcForkOff = blocks * 128;

            _filename      = StringHandlers.PascalToString(_header.filename, Encoding.GetEncoding("macintosh"));
            _creationTime  = DateHandlers.MacToDateTime(_header.creationTime);
            _lastWriteTime = DateHandlers.MacToDateTime(_header.modificationTime);

            stream.Seek(0, SeekOrigin.Begin);
            _opened   = true;
            _isStream = true;
            _stream   = stream;
        }

        /// <inheritdoc />
        public void Open(string path)
        {
            var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            fs.Seek(0, SeekOrigin.Begin);

            byte[] hdrB = new byte[128];
            fs.Read(hdrB, 0, 128);
            _header = Marshal.ByteArrayToStructureBigEndian<Header>(hdrB);

            uint blocks = 1;
            blocks += (uint)(_header.secondaryHeaderLength / 128);

            if(_header.secondaryHeaderLength % 128 > 0)
                blocks++;

            _dataForkOff =  blocks             * 128;
            blocks       += _header.dataLength / 128;

            if(_header.dataLength % 128 > 0)
                blocks++;

            _rsrcForkOff = blocks * 128;

            _filename      = StringHandlers.PascalToString(_header.filename, Encoding.GetEncoding("macintosh"));
            _creationTime  = DateHandlers.MacToDateTime(_header.creationTime);
            _lastWriteTime = DateHandlers.MacToDateTime(_header.modificationTime);

            fs.Close();
            _opened   = true;
            _isPath   = true;
            _basePath = path;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Header
        {
            /// <summary>0x00, MacBinary version, 0</summary>
            public readonly byte version;
            /// <summary>0x01, Str63 Pascal filename</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public readonly byte[] filename;
            /// <summary>0x41, File type</summary>
            public readonly uint type;
            /// <summary>0x45, File creator</summary>
            public readonly uint creator;
            /// <summary>0x49, High byte of Finder flags</summary>
            public readonly byte finderFlags;
            /// <summary>0x4A, Must be 0</summary>
            public readonly byte zero1;
            /// <summary>0x4B, File's icon vertical position within its window</summary>
            public readonly ushort verticalPosition;
            /// <summary>0x4D, File's icon horizontal position within its window</summary>
            public readonly ushort horizontalPosition;
            /// <summary>0x4F, File's window or folder ID</summary>
            public readonly short windowID;
            /// <summary>0x51, Protected flag</summary>
            public readonly byte protect;
            /// <summary>0x52, Must be 0</summary>
            public readonly byte zero2;
            /// <summary>0x53, Size of data fork</summary>
            public readonly uint dataLength;
            /// <summary>0x57, Size of resource fork</summary>
            public readonly uint resourceLength;
            /// <summary>0x5B, File's creation time</summary>
            public readonly uint creationTime;
            /// <summary>0x5F, File's last modified time</summary>
            public readonly uint modificationTime;
            /// <summary>0x63, Length of Get Info comment</summary>
            public readonly ushort commentLength;
            /// <summary>0x65, Low byte of Finder flags</summary>
            public readonly byte finderFlags2;

            #region MacBinary III
            /// <summary>0x66, magic identifier, "mBIN"</summary>
            public readonly uint magic;
            /// <summary>0x6A, fdScript from fxInfo, identifies codepage of filename</summary>
            public readonly byte fdScript;
            /// <summary>0x6B, fdXFlags from fxInfo, extended Mac OS 8 finder flags</summary>
            public readonly byte fdXFlags;
            #endregion MacBinary III

            /// <summary>0x6C, unused</summary>
            public readonly ulong reserved;
            /// <summary>0x74, Total unpacked files</summary>
            public readonly uint totalPackedFiles;

            #region MacBinary II
            /// <summary>0x78, Length of secondary header</summary>
            public readonly ushort secondaryHeaderLength;
            /// <summary>0x7A, version number of MacBinary that wrote this file, starts at 129</summary>
            public readonly byte version2;
            /// <summary>0x7B, version number of MacBinary required to open this file, starts at 129</summary>
            public readonly byte minVersion;
            /// <summary>0x7C, CRC of previous bytes</summary>
            public readonly short crc;
            #endregion MacBinary II

            /// <summary>0x7E, Reserved for computer type and OS ID</summary>
            public readonly short computerID;
        }
    }
}