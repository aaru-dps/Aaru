// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Open.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : CPIO plugin.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Archives;

public sealed partial class Cpio
{
    List<Entry> _entries;
    CpioFormat  _format;

    long ReadOctalNumber(int digits)
    {
        var buf = new byte[digits];
        _stream.ReadExactly(buf, 0, digits);

        long value = 0;

        for(var i = 0; i < digits; i++)
        {
            if(buf[i] < (byte)'0' || buf[i] > (byte)'7') break;

            value = value * 8 + (buf[i] - '0');
        }

        return value;
    }

    long ReadHexNumber(int digits)
    {
        var buf = new byte[digits];
        _stream.ReadExactly(buf, 0, digits);

        long value = 0;

        for(var i = 0; i < digits; i++)
        {
            int c = buf[i];

            if(c >= '0' && c <= '9')
                value = value * 16 + (c - '0');
            else if(c >= 'a' && c <= 'f')
                value = value * 16 + (c - 'a' + 10);
            else if(c >= 'A' && c <= 'F')
                value = value * 16 + (c - 'A' + 10);
            else
                break;
        }

        return value;
    }

    uint ReadUInt16BE()
    {
        var buf = new byte[2];
        _stream.ReadExactly(buf, 0, 2);

        return BinaryPrimitives.ReadUInt16BigEndian(buf);
    }

    uint ReadUInt16LE()
    {
        var buf = new byte[2];
        _stream.ReadExactly(buf, 0, 2);

        return BinaryPrimitives.ReadUInt16LittleEndian(buf);
    }

    long ReadUInt32BE()
    {
        var buf = new byte[4];
        _stream.ReadExactly(buf, 0, 4);

        return BinaryPrimitives.ReadUInt32BigEndian(buf);
    }

#region IArchive Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter filter, Encoding encoding)
    {
        if(Opened) Close();

        _stream   = filter.GetDataForkStream();
        _encoding = encoding ?? Encoding.UTF8;
        _entries  = [];

        _stream.Position = 0;

        var        formatDetected = false;
        CpioFormat format         = CpioFormat.NewAscii;
        var        hasDirectories = false;

        while(_stream.Position < _stream.Length)
        {
            long headerStart = _stream.Position;

            var magic = new byte[2];

            if(_stream.Read(magic, 0, 2) < 2) break;

            uint devMajor;
            uint devMinor;
            uint ino;
            uint mode;
            uint uid;
            uint gid;
            uint nlink;
            uint rdevMajor;
            uint rdevMinor;
            uint nameSize;
            uint checksum = 0;
            long mtime;
            long fileSize;
            var  hasCrc = false;
            int  dataPad;

            if(magic[0] == (byte)'0' && magic[1] == (byte)'7')
            {
                var more = new byte[4];

                if(_stream.Read(more, 0, 4) < 4) break;

                if(more[0] == (byte)'0' && more[1] == (byte)'7' && more[2] == (byte)'0' && more[3] == (byte)'7')
                {
                    // Old character (odc) format
                    if(_stream.Length - _stream.Position < 70) break;

                    if(!formatDetected)
                    {
                        format         = CpioFormat.OldCharacter;
                        formatDetected = true;
                    }

                    devMajor  = (uint)ReadOctalNumber(3);
                    devMinor  = (uint)ReadOctalNumber(3);
                    ino       = (uint)ReadOctalNumber(6);
                    mode      = (uint)ReadOctalNumber(6);
                    uid       = (uint)ReadOctalNumber(6);
                    gid       = (uint)ReadOctalNumber(6);
                    nlink     = (uint)ReadOctalNumber(6);
                    rdevMajor = (uint)ReadOctalNumber(3);
                    rdevMinor = (uint)ReadOctalNumber(3);
                    mtime     = ReadOctalNumber(11);
                    nameSize  = (uint)ReadOctalNumber(6);
                    fileSize  = ReadOctalNumber(11);
                    dataPad   = 0;
                }
                else if(more[0] == (byte)'0' &&
                        more[1] == (byte)'7' &&
                        more[2] == (byte)'0' &&
                        (more[3] == (byte)'1' || more[3] == (byte)'2'))
                {
                    // New ASCII (newc) or New CRC (newcrc) format
                    if(_stream.Length - _stream.Position < 104) break;

                    hasCrc = more[3] == (byte)'2';

                    if(!formatDetected)
                    {
                        format         = hasCrc ? CpioFormat.NewCrc : CpioFormat.NewAscii;
                        formatDetected = true;
                    }

                    ino       = (uint)ReadHexNumber(8);
                    mode      = (uint)ReadHexNumber(8);
                    uid       = (uint)ReadHexNumber(8);
                    gid       = (uint)ReadHexNumber(8);
                    nlink     = (uint)ReadHexNumber(8);
                    mtime     = ReadHexNumber(8);
                    fileSize  = ReadHexNumber(8);
                    devMajor  = (uint)ReadHexNumber(8);
                    devMinor  = (uint)ReadHexNumber(8);
                    rdevMajor = (uint)ReadHexNumber(8);
                    rdevMinor = (uint)ReadHexNumber(8);
                    nameSize  = (uint)ReadHexNumber(8);
                    checksum  = (uint)ReadHexNumber(8);
                    dataPad   = 4;
                }
                else
                    return ErrorNumber.InvalidArgument;
            }
            else if(magic[0] == 0x71 && magic[1] == 0xC7)
            {
                // Old binary big-endian format
                if(_stream.Length - _stream.Position < 24) break;

                if(!formatDetected)
                {
                    format         = CpioFormat.OldBinaryBE;
                    formatDetected = true;
                }

                uint dev = ReadUInt16BE();
                ino   = ReadUInt16BE();
                mode  = ReadUInt16BE();
                uid   = ReadUInt16BE();
                gid   = ReadUInt16BE();
                nlink = ReadUInt16BE();
                uint rdev = ReadUInt16BE();
                mtime    = ReadUInt32BE();
                nameSize = ReadUInt16BE();
                fileSize = ReadUInt32BE();

                devMajor  = dev >> 9;
                devMinor  = dev & 0x1FF;
                rdevMajor = rdev >> 9;
                rdevMinor = rdev & 0x1FF;
                dataPad   = 2;
            }
            else if(magic[0] == 0xC7 && magic[1] == 0x71)
            {
                // Old binary little-endian format
                if(_stream.Length - _stream.Position < 24) break;

                if(!formatDetected)
                {
                    format         = CpioFormat.OldBinaryLE;
                    formatDetected = true;
                }

                uint dev = ReadUInt16LE();
                ino   = ReadUInt16LE();
                mode  = ReadUInt16LE();
                uid   = ReadUInt16LE();
                gid   = ReadUInt16LE();
                nlink = ReadUInt16LE();
                uint rdev      = ReadUInt16LE();
                uint mtimeHigh = ReadUInt16LE();
                uint mtimeLow  = ReadUInt16LE();
                nameSize = ReadUInt16LE();
                uint sizeHigh = ReadUInt16LE();
                uint sizeLow  = ReadUInt16LE();

                mtime    = (mtimeHigh << 16) + mtimeLow;
                fileSize = (sizeHigh  << 16) + sizeLow;

                devMajor  = dev >> 9;
                devMinor  = dev & 0x1FF;
                rdevMajor = rdev >> 9;
                rdevMinor = rdev & 0x1FF;
                dataPad   = 2;
            }
            else
                return ErrorNumber.InvalidArgument;

            // Read filename (nameSize includes the null terminator)
            if(nameSize < 1 || nameSize - 1 > _stream.Length - _stream.Position) return ErrorNumber.InvalidArgument;

            var nameBytes = new byte[nameSize - 1];

            if(nameSize > 1) _stream.ReadExactly(nameBytes, 0, (int)(nameSize - 1));

            // Skip the null terminator
            _stream.Seek(1, SeekOrigin.Current);

            // Apply name padding
            if(dataPad == 4)
            {
                // newc/newcrc: name + header is padded to 4-byte boundary from header start
                // Header is 110 bytes (6 magic + 13*8 fields), name follows immediately
                // Total = 110 + nameSize, pad to multiple of 4
                long totalHeaderAndName = 110 + nameSize;
                long namePadding        = (4 - totalHeaderAndName % 4) % 4;
                _stream.Seek(namePadding, SeekOrigin.Current);
            }
            else if(dataPad == 2)
            {
                // Binary: name is padded to 2-byte boundary
                if(nameSize % 2 != 0) _stream.Seek(1, SeekOrigin.Current);
            }

            string filename = _encoding.GetString(nameBytes);

            // Check for trailer
            if(filename == TRAILER_NAME) break;

            long dataOffset = _stream.Position;

            // Calculate data padding
            long compressedSize;

            if(dataPad == 4)
            {
                long dataPadding = (4 - fileSize % 4) % 4;
                compressedSize = fileSize + dataPadding;
            }
            else if(dataPad == 2)
            {
                long dataPadding = fileSize & 1;
                compressedSize = fileSize + dataPadding;
            }
            else
                compressedSize = fileSize;

            var fileType = (CpioFileType)(mode & FILE_TYPE_MASK);

            if(fileType == CpioFileType.Directory) hasDirectories = true;

            var entry = new Entry
            {
                Filename         = filename,
                Size             = fileSize,
                DataOffset       = dataOffset,
                CompressedSize   = compressedSize,
                Mode             = mode & 0xFFF,
                Uid              = uid,
                Gid              = gid,
                Nlink            = nlink,
                LastWriteTimeUtc = DateTimeOffset.FromUnixTimeSeconds(mtime).UtcDateTime,
                DevMajor         = devMajor,
                DevMinor         = devMinor,
                RdevMajor        = rdevMajor,
                RdevMinor        = rdevMinor,
                Inode            = ino,
                Checksum         = checksum,
                HasChecksum      = hasCrc,
                FileType         = fileType
            };

            _entries.Add(entry);

            // Seek past the file data and padding
            _stream.Seek(dataOffset + compressedSize, SeekOrigin.Begin);
        }

        _format = format;

        _features = ArchiveSupportedFeature.SupportsFilenames | ArchiveSupportedFeature.HasEntryTimestamp;

        if(hasDirectories)
            _features |= ArchiveSupportedFeature.SupportsSubdirectories |
                         ArchiveSupportedFeature.HasExplicitDirectories;

        Opened = true;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public void Close()
    {
        _entries = null;
        Opened   = false;
    }

#endregion
}