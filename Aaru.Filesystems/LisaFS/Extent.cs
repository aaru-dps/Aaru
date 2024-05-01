// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Extent.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Lisa filesystem plugin.
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
using Aaru.CommonTypes.Enums;
using Aaru.Console;
using Aaru.Decoders;
using Aaru.Helpers;

namespace Aaru.Filesystems;

public sealed partial class LisaFS
{
    /// <summary>Searches the disk for an extents file (or gets it from cache)</summary>
    /// <returns>Error.</returns>
    /// <param name="fileId">File identifier.</param>
    /// <param name="file">Extents file.</param>
    ErrorNumber ReadExtentsFile(short fileId, out ExtentFile file)
    {
        file = new ExtentFile();
        ErrorNumber errno;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(fileId < 4 || fileId == 4 && _mddf.fsversion != LISA_V2 && _mddf.fsversion != LISA_V1)
            return ErrorNumber.InvalidArgument;

        if(_extentCache.TryGetValue(fileId, out file)) return ErrorNumber.NoError;

        // A file ID that cannot be stored in the S-Records File
        if(fileId >= _srecords.Length) return ErrorNumber.InvalidArgument;

        ulong ptr = _srecords[fileId].extent_ptr;

        // An invalid pointer denotes file does not exist
        if(ptr is 0xFFFFFFFF or 0x00000000) return ErrorNumber.NoSuchFile;

        // Pointers are relative to MDDF
        ptr += _mddf.mddf_block + _volumePrefix;

        LisaTag.PriamTag extTag;
        byte[]           tag;

        // This happens on some disks.
        // This is a filesystem corruption that makes LisaOS crash on scavenge.
        // This code just allow to ignore that corruption by searching the Extents File using sector tags
        if(ptr >= _device.Info.Sectors)
        {
            var found = false;

            for(ulong i = 0; i < _device.Info.Sectors; i++)
            {
                errno = _device.ReadSectorTag(i, SectorTagType.AppleSectorTag, out tag);

                if(errno != ErrorNumber.NoError) continue;

                DecodeTag(tag, out extTag);

                if(extTag.FileId != fileId * -1) continue;

                ptr   = i;
                found = true;

                break;
            }

            if(!found) return ErrorNumber.InvalidArgument;
        }

        // Checks that the sector tag indicates its the Extents File we are searching for
        errno = _device.ReadSectorTag(ptr, SectorTagType.AppleSectorTag, out tag);

        if(errno != ErrorNumber.NoError) return errno;

        DecodeTag(tag, out extTag);

        if(extTag.FileId != (short)(-1 * fileId)) return ErrorNumber.NoSuchFile;

        errno = _mddf.fsversion == LISA_V1
                    ? _device.ReadSectors(ptr, 2, out byte[] sector)
                    : _device.ReadSector(ptr, out sector);

        if(errno != ErrorNumber.NoError) return errno;

        if(sector[0] >= 32 || sector[0] == 0) return ErrorNumber.InvalidArgument;

        file.filenameLen = sector[0];
        file.filename    = new byte[file.filenameLen];
        Array.Copy(sector, 0x01, file.filename, 0, file.filenameLen);
        file.unknown1  = BigEndianBitConverter.ToUInt16(sector, 0x20);
        file.file_uid  = BigEndianBitConverter.ToUInt64(sector, 0x22);
        file.unknown2  = sector[0x2A];
        file.etype     = sector[0x2B];
        file.ftype     = (FileType)sector[0x2C];
        file.unknown3  = sector[0x2D];
        file.dtc       = BigEndianBitConverter.ToUInt32(sector, 0x2E);
        file.dta       = BigEndianBitConverter.ToUInt32(sector, 0x32);
        file.dtm       = BigEndianBitConverter.ToUInt32(sector, 0x36);
        file.dtb       = BigEndianBitConverter.ToUInt32(sector, 0x3A);
        file.dts       = BigEndianBitConverter.ToUInt32(sector, 0x3E);
        file.serial    = BigEndianBitConverter.ToUInt32(sector, 0x42);
        file.unknown4  = sector[0x46];
        file.locked    = sector[0x47];
        file.protect   = sector[0x48];
        file.master    = sector[0x49];
        file.scavenged = sector[0x4A];
        file.closed    = sector[0x4B];
        file.open      = sector[0x4C];
        file.unknown5  = new byte[11];
        Array.Copy(sector, 0x4D, file.unknown5, 0, 11);
        file.release        = BigEndianBitConverter.ToUInt16(sector, 0x58);
        file.build          = BigEndianBitConverter.ToUInt16(sector, 0x5A);
        file.compatibility  = BigEndianBitConverter.ToUInt16(sector, 0x5C);
        file.revision       = BigEndianBitConverter.ToUInt16(sector, 0x5E);
        file.unknown6       = BigEndianBitConverter.ToUInt16(sector, 0x60);
        file.password_valid = sector[0x62];
        file.password       = new byte[8];
        Array.Copy(sector, 0x63, file.password, 0, 8);
        file.unknown7 = new byte[3];
        Array.Copy(sector, 0x6B, file.unknown7, 0, 3);
        file.overhead = BigEndianBitConverter.ToUInt16(sector, 0x6E);
        file.unknown8 = new byte[16];
        Array.Copy(sector, 0x70, file.unknown8, 0, 16);
        file.unknown10 = BigEndianBitConverter.ToInt16(sector, 0x17E);
        file.LisaInfo  = new byte[128];
        Array.Copy(sector, 0x180, file.LisaInfo, 0, 128);

        var extentsCount = 0;
        int extentsOffset;

        if(_mddf.fsversion == LISA_V1)
        {
            file.length   = BigEndianBitConverter.ToInt32(sector, 0x200);
            file.unknown9 = BigEndianBitConverter.ToInt32(sector, 0x204);
            extentsOffset = 0x208;
        }
        else
        {
            file.length   = BigEndianBitConverter.ToInt32(sector, 0x80);
            file.unknown9 = BigEndianBitConverter.ToInt32(sector, 0x84);
            extentsOffset = 0x88;
        }

        for(var j = 0; j < 41; j++)
        {
            if(BigEndianBitConverter.ToInt16(sector, extentsOffset + j * 6 + 4) == 0) break;

            extentsCount++;
        }

        file.extents = new Extent[extentsCount];

        for(var j = 0; j < extentsCount; j++)
        {
            file.extents[j] = new Extent
            {
                start  = BigEndianBitConverter.ToInt32(sector, extentsOffset + j * 6),
                length = BigEndianBitConverter.ToInt16(sector, extentsOffset + j * 6 + 4)
            };
        }

        _extentCache.Add(fileId, file);

        if(!_debug) return ErrorNumber.NoError;

        if(_printedExtents.Contains(fileId)) return ErrorNumber.NoError;

        AaruConsole.DebugWriteLine(MODULE_NAME, "ExtentFile[{0}].filenameLen = {1}", fileId, file.filenameLen);

        AaruConsole.DebugWriteLine(MODULE_NAME,
                                   "ExtentFile[{0}].filename = {1}",
                                   fileId,
                                   StringHandlers.CToString(file.filename, _encoding));

        AaruConsole.DebugWriteLine(MODULE_NAME, "ExtentFile[{0}].unknown1 = 0x{1:X4}",  fileId, file.unknown1);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ExtentFile[{0}].file_uid = 0x{1:X16}", fileId, file.file_uid);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ExtentFile[{0}].unknown2 = 0x{1:X2}",  fileId, file.unknown2);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ExtentFile[{0}].etype = 0x{1:X2}",     fileId, file.etype);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ExtentFile[{0}].ftype = {1}",          fileId, file.ftype);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ExtentFile[{0}].unknown3 = 0x{1:X2}",  fileId, file.unknown3);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ExtentFile[{0}].dtc = {1}",            fileId, file.dtc);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ExtentFile[{0}].dta = {1}",            fileId, file.dta);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ExtentFile[{0}].dtm = {1}",            fileId, file.dtm);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ExtentFile[{0}].dtb = {1}",            fileId, file.dtb);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ExtentFile[{0}].dts = {1}",            fileId, file.dts);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ExtentFile[{0}].serial = {1}",         fileId, file.serial);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ExtentFile[{0}].unknown4 = 0x{1:X2}",  fileId, file.unknown4);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ExtentFile[{0}].locked = {1}",         fileId, file.locked    > 0);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ExtentFile[{0}].protect = {1}",        fileId, file.protect   > 0);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ExtentFile[{0}].master = {1}",         fileId, file.master    > 0);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ExtentFile[{0}].scavenged = {1}",      fileId, file.scavenged > 0);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ExtentFile[{0}].closed = {1}",         fileId, file.closed    > 0);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ExtentFile[{0}].open = {1}",           fileId, file.open      > 0);

        AaruConsole.DebugWriteLine(MODULE_NAME,
                                   "ExtentFile[{0}].unknown5 = 0x{1:X2}{2:X2}{3:X2}{4:X2}{5:X2}{6:X2}{7:X2}{8:X2}{9:X2}" +
                                   "{10:X2}{11:X2}",
                                   fileId,
                                   file.unknown5[0],
                                   file.unknown5[1],
                                   file.unknown5[2],
                                   file.unknown5[3],
                                   file.unknown5[4],
                                   file.unknown5[5],
                                   file.unknown5[6],
                                   file.unknown5[7],
                                   file.unknown5[8],
                                   file.unknown5[9],
                                   file.unknown5[10]);

        AaruConsole.DebugWriteLine(MODULE_NAME, "ExtentFile[{0}].release = {1}", fileId, file.release);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ExtentFile[{0}].build = {1}",   fileId, file.build);

        AaruConsole.DebugWriteLine(MODULE_NAME, "ExtentFile[{0}].compatibility = {1}", fileId, file.compatibility);

        AaruConsole.DebugWriteLine(MODULE_NAME, "ExtentFile[{0}].revision = {1}",      fileId, file.revision);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ExtentFile[{0}].unknown6 = 0x{1:X4}", fileId, file.unknown6);

        AaruConsole.DebugWriteLine(MODULE_NAME,
                                   "ExtentFile[{0}].password_valid = {1}",
                                   fileId,
                                   file.password_valid > 0);

        AaruConsole.DebugWriteLine(MODULE_NAME,
                                   "ExtentFile[{0}].password = {1}",
                                   fileId,
                                   _encoding.GetString(file.password));

        AaruConsole.DebugWriteLine(MODULE_NAME,
                                   "ExtentFile[{0}].unknown7 = 0x{1:X2}{2:X2}{3:X2}",
                                   fileId,
                                   file.unknown7[0],
                                   file.unknown7[1],
                                   file.unknown7[2]);

        AaruConsole.DebugWriteLine(MODULE_NAME, "ExtentFile[{0}].overhead = {1}", fileId, file.overhead);

        AaruConsole.DebugWriteLine(MODULE_NAME,
                                   "ExtentFile[{0}].unknown8 = 0x{1:X2}{2:X2}{3:X2}{4:X2}{5:X2}{6:X2}{7:X2}{8:X2}{9:X2}" +
                                   "{10:X2}{11:X2}{12:X2}{13:X2}{14:X2}{15:X2}{16:X2}",
                                   fileId,
                                   file.unknown8[0],
                                   file.unknown8[1],
                                   file.unknown8[2],
                                   file.unknown8[3],
                                   file.unknown8[4],
                                   file.unknown8[5],
                                   file.unknown8[6],
                                   file.unknown8[7],
                                   file.unknown8[8],
                                   file.unknown8[9],
                                   file.unknown8[10],
                                   file.unknown8[11],
                                   file.unknown8[12],
                                   file.unknown8[13],
                                   file.unknown8[14],
                                   file.unknown8[15]);

        AaruConsole.DebugWriteLine(MODULE_NAME, "ExtentFile[{0}].length = {1}",        fileId, file.length);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ExtentFile[{0}].unknown9 = 0x{1:X8}", fileId, file.unknown9);

        for(var ext = 0; ext < file.extents.Length; ext++)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME,
                                       "ExtentFile[{0}].extents[{1}].start = {2}",
                                       fileId,
                                       ext,
                                       file.extents[ext].start);

            AaruConsole.DebugWriteLine(MODULE_NAME,
                                       "ExtentFile[{0}].extents[{1}].length = {2}",
                                       fileId,
                                       ext,
                                       file.extents[ext].length);
        }

        AaruConsole.DebugWriteLine(MODULE_NAME, "ExtentFile[{0}].unknown10 = 0x{1:X4}", fileId, file.unknown10);

        _printedExtents.Add(fileId);

        return ErrorNumber.NoError;
    }

    /// <summary>Reads all the S-Records and caches it</summary>
    ErrorNumber ReadSRecords()
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        // Searches the S-Records place using MDDF pointers
        ErrorNumber errno = _device.ReadSectors(_mddf.srec_ptr + _mddf.mddf_block + _volumePrefix,
                                                _mddf.srec_len,
                                                out byte[] sectors);

        if(errno != ErrorNumber.NoError) return errno;

        // Each entry takes 14 bytes
        _srecords = new SRecord[sectors.Length / 14];

        for(var s = 0; s < _srecords.Length; s++)
        {
            _srecords[s] = new SRecord
            {
                extent_ptr = BigEndianBitConverter.ToUInt32(sectors, 0x00 + 14 * s),
                unknown    = BigEndianBitConverter.ToUInt32(sectors, 0x04 + 14 * s),
                filesize   = BigEndianBitConverter.ToUInt32(sectors, 0x08 + 14 * s),
                flags      = BigEndianBitConverter.ToUInt16(sectors, 0x0C + 14 * s)
            };
        }

        return ErrorNumber.NoError;
    }
}