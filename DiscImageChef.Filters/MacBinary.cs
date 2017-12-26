// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace DiscImageChef.Filters
{
    // TODO: Interpret fdScript
    /// <summary>
    ///     Decodes MacBinary files
    /// </summary>
    public class MacBinary : IFilter
    {
        const uint MACBINARY_MAGIC = 0x6D42494E;
        string basePath;
        byte[] bytes;
        DateTime creationTime;

        long dataForkOff;
        string filename;
        MacBinaryHeader header;
        bool isBytes, isStream, isPath, opened;
        DateTime lastWriteTime;
        long rsrcForkOff;
        Stream stream;

        public string Name => "MacBinary";
        public Guid Id => new Guid("D7C321D3-E51F-45DF-A150-F6BFDF0D7704");

        public void Close()
        {
            bytes = null;
            stream?.Close();
            isBytes = false;
            isStream = false;
            isPath = false;
            opened = false;
        }

        public string GetBasePath()
        {
            return basePath;
        }

        public DateTime GetCreationTime()
        {
            return creationTime;
        }

        public long GetDataForkLength()
        {
            return header.dataLength;
        }

        public Stream GetDataForkStream()
        {
            if(header.dataLength == 0) return null;

            if(isBytes) return new OffsetStream(bytes, dataForkOff, dataForkOff + header.dataLength - 1);
            if(isStream) return new OffsetStream(stream, dataForkOff, dataForkOff + header.dataLength - 1);
            if(isPath)
                return new OffsetStream(basePath, FileMode.Open, FileAccess.Read, dataForkOff,
                                        dataForkOff + header.dataLength - 1);

            return null;
        }

        public string GetFilename()
        {
            return filename;
        }

        public DateTime GetLastWriteTime()
        {
            return lastWriteTime;
        }

        public long GetLength()
        {
            return header.dataLength + header.resourceLength;
        }

        public string GetParentFolder()
        {
            return Path.GetDirectoryName(basePath);
        }

        public string GetPath()
        {
            return basePath;
        }

        public long GetResourceForkLength()
        {
            return header.resourceLength;
        }

        public Stream GetResourceForkStream()
        {
            if(header.resourceLength == 0) return null;

            if(isBytes) return new OffsetStream(bytes, rsrcForkOff, rsrcForkOff + header.resourceLength - 1);
            if(isStream) return new OffsetStream(stream, rsrcForkOff, rsrcForkOff + header.resourceLength - 1);
            if(isPath)
                return new OffsetStream(basePath, FileMode.Open, FileAccess.Read, rsrcForkOff,
                                        rsrcForkOff + header.resourceLength - 1);

            return null;
        }

        public bool HasResourceFork()
        {
            return header.resourceLength > 0;
        }

        public bool Identify(byte[] buffer)
        {
            if(buffer == null || buffer.Length < 128) return false;

            byte[] hdr_b = new byte[128];
            Array.Copy(buffer, 0, hdr_b, 0, 128);
            header = BigEndianMarshal.ByteArrayToStructureBigEndian<MacBinaryHeader>(hdr_b);

            return header.magic == MACBINARY_MAGIC || header.version == 0 && header.filename[0] > 0 &&
                   header.filename[0] < 64 && header.zero1 == 0 && header.zero2 == 0 && header.reserved == 0 &&
                   (header.dataLength > 0 || header.resourceLength > 0);
        }

        public bool Identify(Stream stream)
        {
            if(stream == null || stream.Length < 128) return false;

            byte[] hdr_b = new byte[128];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(hdr_b, 0, 128);
            header = BigEndianMarshal.ByteArrayToStructureBigEndian<MacBinaryHeader>(hdr_b);

            return header.magic == MACBINARY_MAGIC || header.version == 0 && header.filename[0] > 0 &&
                   header.filename[0] < 64 && header.zero1 == 0 && header.zero2 == 0 && header.reserved == 0 &&
                   (header.dataLength > 0 || header.resourceLength > 0);
        }

        public bool Identify(string path)
        {
            FileStream fstream = new FileStream(path, FileMode.Open, FileAccess.Read);
            if(fstream.Length < 128) return false;

            byte[] hdr_b = new byte[128];
            fstream.Read(hdr_b, 0, 128);
            header = BigEndianMarshal.ByteArrayToStructureBigEndian<MacBinaryHeader>(hdr_b);

            fstream.Close();
            return header.magic == MACBINARY_MAGIC || header.version == 0 && header.filename[0] > 0 &&
                   header.filename[0] < 64 && header.zero1 == 0 && header.zero2 == 0 && header.reserved == 0 &&
                   (header.dataLength > 0 || header.resourceLength > 0);
        }

        public bool IsOpened()
        {
            return opened;
        }

        public void Open(byte[] buffer)
        {
            MemoryStream ms = new MemoryStream(buffer);
            ms.Seek(0, SeekOrigin.Begin);

            byte[] hdr_b = new byte[128];
            ms.Read(hdr_b, 0, 128);
            header = BigEndianMarshal.ByteArrayToStructureBigEndian<MacBinaryHeader>(hdr_b);

            uint blocks = 1;
            blocks += (uint)(header.secondaryHeaderLength / 128);
            if(header.secondaryHeaderLength % 128 > 0) blocks++;
            dataForkOff = blocks * 128;
            blocks += header.dataLength / 128;
            if(header.dataLength % 128 > 0) blocks++;
            rsrcForkOff = blocks * 128;

            filename = StringHandlers.PascalToString(header.filename, Encoding.GetEncoding("macintosh"));
            creationTime = DateHandlers.MacToDateTime(header.creationTime);
            lastWriteTime = DateHandlers.MacToDateTime(header.modificationTime);

            ms.Close();
            opened = true;
            isBytes = true;
            bytes = buffer;
        }

        public void Open(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            byte[] hdr_b = new byte[128];
            stream.Read(hdr_b, 0, 128);
            header = BigEndianMarshal.ByteArrayToStructureBigEndian<MacBinaryHeader>(hdr_b);

            uint blocks = 1;
            blocks += (uint)(header.secondaryHeaderLength / 128);
            if(header.secondaryHeaderLength % 128 > 0) blocks++;
            dataForkOff = blocks * 128;
            blocks += header.dataLength / 128;
            if(header.dataLength % 128 > 0) blocks++;
            rsrcForkOff = blocks * 128;

            filename = StringHandlers.PascalToString(header.filename, Encoding.GetEncoding("macintosh"));
            creationTime = DateHandlers.MacToDateTime(header.creationTime);
            lastWriteTime = DateHandlers.MacToDateTime(header.modificationTime);

            stream.Seek(0, SeekOrigin.Begin);
            opened = true;
            isStream = true;
            this.stream = stream;
        }

        public void Open(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            fs.Seek(0, SeekOrigin.Begin);

            byte[] hdr_b = new byte[128];
            fs.Read(hdr_b, 0, 128);
            header = BigEndianMarshal.ByteArrayToStructureBigEndian<MacBinaryHeader>(hdr_b);

            uint blocks = 1;
            blocks += (uint)(header.secondaryHeaderLength / 128);
            if(header.secondaryHeaderLength % 128 > 0) blocks++;
            dataForkOff = blocks * 128;
            blocks += header.dataLength / 128;
            if(header.dataLength % 128 > 0) blocks++;
            rsrcForkOff = blocks * 128;

            filename = StringHandlers.PascalToString(header.filename, Encoding.GetEncoding("macintosh"));
            creationTime = DateHandlers.MacToDateTime(header.creationTime);
            lastWriteTime = DateHandlers.MacToDateTime(header.modificationTime);

            fs.Close();
            opened = true;
            isPath = true;
            basePath = path;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct MacBinaryHeader
        {
            /// <summary>
            ///     0x00, MacBinary version, 0
            /// </summary>
            public byte version;
            /// <summary>
            ///     0x01, Str63 Pascal filename
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)] public byte[] filename;
            /// <summary>
            ///     0x41, File type
            /// </summary>
            public uint type;
            /// <summary>
            ///     0x45, File creator
            /// </summary>
            public uint creator;
            /// <summary>
            ///     0x49, High byte of Finder flags
            /// </summary>
            public byte finderFlags;
            /// <summary>
            ///     0x4A, Must be 0
            /// </summary>
            public byte zero1;
            /// <summary>
            ///     0x4B, File's icon vertical position within its window
            /// </summary>
            public ushort verticalPosition;
            /// <summary>
            ///     0x4D, File's icon horizontal position within its window
            /// </summary>
            public ushort horizontalPosition;
            /// <summary>
            ///     0x4F, File's window or folder ID
            /// </summary>
            public short windowID;
            /// <summary>
            ///     0x51, Protected flag
            /// </summary>
            public byte protect;
            /// <summary>
            ///     0x52, Must be 0
            /// </summary>
            public byte zero2;
            /// <summary>
            ///     0x53, Size of data fork
            /// </summary>
            public uint dataLength;
            /// <summary>
            ///     0x57, Size of resource fork
            /// </summary>
            public uint resourceLength;
            /// <summary>
            ///     0x5B, File's creation time
            /// </summary>
            public uint creationTime;
            /// <summary>
            ///     0x5F, File's last modified time
            /// </summary>
            public uint modificationTime;
            /// <summary>
            ///     0x63, Length of Get Info comment
            /// </summary>
            public ushort commentLength;
            /// <summary>
            ///     0x65, Low byte of Finder flags
            /// </summary>
            public byte finderFlags2;

            #region MacBinary III
            /// <summary>
            ///     0x66, magic identifier, "mBIN"
            /// </summary>
            public uint magic;
            /// <summary>
            ///     0x6A, fdScript from fxInfo, identifies codepage of filename
            /// </summary>
            public byte fdScript;
            /// <summary>
            ///     0x6B, fdXFlags from fxInfo, extended Mac OS 8 finder flags
            /// </summary>
            public byte fdXFlags;
            #endregion MacBinary III

            /// <summary>
            ///     0x6C, unused
            /// </summary>
            public ulong reserved;
            /// <summary>
            ///     0x74, Total unpacked files
            /// </summary>
            public uint totalPackedFiles;

            #region MacBinary II
            /// <summary>
            ///     0x78, Length of secondary header
            /// </summary>
            public ushort secondaryHeaderLength;
            /// <summary>
            ///     0x7A, version number of MacBinary that wrote this file, starts at 129
            /// </summary>
            public byte version2;
            /// <summary>
            ///     0x7B, version number of MacBinary required to open this file, starts at 129
            /// </summary>
            public byte minVersion;
            /// <summary>
            ///     0x7C, CRC of previous bytes
            /// </summary>
            public short crc;
            #endregion MacBinary II

            /// <summary>
            ///     0x7E, Reserved for computer type and OS ID
            /// </summary>
            public short computerID;
        }
    }
}