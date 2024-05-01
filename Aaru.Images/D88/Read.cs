// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Read.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads Quasi88 disk images.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Decoders.Floppy;
using Aaru.Helpers;

namespace Aaru.Images;

public sealed partial class D88
{
#region IMediaImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        // Even if disk name is supposedly ASCII, I'm pretty sure most emulators allow Shift-JIS to be used :p
        var shiftjis = Encoding.GetEncoding("shift_jis");

        if(stream.Length < Marshal.SizeOf<Header>()) return ErrorNumber.InvalidArgument;

        var hdrB = new byte[Marshal.SizeOf<Header>()];
        stream.EnsureRead(hdrB, 0, hdrB.Length);
        Header hdr = Marshal.ByteArrayToStructureLittleEndian<Header>(hdrB);

        AaruConsole.DebugWriteLine(MODULE_NAME, "d88hdr.name = \"{0}\"", StringHandlers.CToString(hdr.name, shiftjis));

        AaruConsole.DebugWriteLine(MODULE_NAME,
                                   "d88hdr.reserved is empty? = {0}",
                                   hdr.reserved.SequenceEqual(_reservedEmpty));

        AaruConsole.DebugWriteLine(MODULE_NAME, "d88hdr.write_protect = 0x{0:X2}", hdr.write_protect);

        AaruConsole.DebugWriteLine(MODULE_NAME, "d88hdr.disk_type = {0} ({1})", hdr.disk_type, (byte)hdr.disk_type);

        AaruConsole.DebugWriteLine(MODULE_NAME, "d88hdr.disk_size = {0}", hdr.disk_size);

        if(hdr.disk_size != stream.Length) return ErrorNumber.InvalidArgument;

        if(hdr.disk_type != DiskType.D2 && hdr.disk_type != DiskType.Dd2 && hdr.disk_type != DiskType.Hd2)
            return ErrorNumber.InvalidArgument;

        if(!hdr.reserved.SequenceEqual(_reservedEmpty)) return ErrorNumber.InvalidArgument;

        var trkCounter = 0;

        foreach(int t in hdr.track_table)
        {
            if(t > 0) trkCounter++;

            if(t < 0 || t > stream.Length) return ErrorNumber.InvalidArgument;
        }

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization._0_tracks, trkCounter);

        if(trkCounter == 0) return ErrorNumber.InvalidArgument;

        hdrB = new byte[Marshal.SizeOf<SectorHeader>()];
        stream.Seek(hdr.track_table[0], SeekOrigin.Begin);
        stream.EnsureRead(hdrB, 0, hdrB.Length);

        SectorHeader sechdr = Marshal.ByteArrayToStructureLittleEndian<SectorHeader>(hdrB);

        AaruConsole.DebugWriteLine(MODULE_NAME, "sechdr.c = {0}",            sechdr.c);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sechdr.h = {0}",            sechdr.h);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sechdr.r = {0}",            sechdr.r);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sechdr.n = {0}",            sechdr.n);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sechdr.spt = {0}",          sechdr.spt);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sechdr.density = {0}",      sechdr.density);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sechdr.deleted_mark = {0}", sechdr.deleted_mark);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sechdr.status = {0}",       sechdr.status);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sechdr.size_of_data = {0}", sechdr.size_of_data);

        short             spt      = sechdr.spt;
        IBMSectorSizeCode bps      = sechdr.n;
        var               allEqual = true;
        _sectorsData = new List<byte[]>();

        for(var i = 0; i < trkCounter; i++)
        {
            stream.Seek(hdr.track_table[i], SeekOrigin.Begin);
            stream.EnsureRead(hdrB, 0, hdrB.Length);
            SortedDictionary<byte, byte[]> sectors = new();

            sechdr = Marshal.ByteArrayToStructureLittleEndian<SectorHeader>(hdrB);

            if(sechdr.spt != spt || sechdr.n != bps)
            {
                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization.Disk_tracks_are_not_same_size,
                                           sechdr.spt,
                                           spt,
                                           sechdr.n,
                                           bps,
                                           i,
                                           0);

                allEqual = false;
            }

            short  maxJ = sechdr.spt;
            byte[] secB;

            for(short j = 1; j < maxJ; j++)
            {
                secB = new byte[sechdr.size_of_data];
                stream.EnsureRead(secB, 0, secB.Length);
                sectors.Add(sechdr.r, secB);
                stream.EnsureRead(hdrB, 0, hdrB.Length);

                sechdr = Marshal.ByteArrayToStructureLittleEndian<SectorHeader>(hdrB);

                if(sechdr.spt == spt && sechdr.n == bps) continue;

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization.Disk_tracks_are_not_same_size,
                                           sechdr.spt,
                                           spt,
                                           sechdr.n,
                                           bps,
                                           i,
                                           j,
                                           sechdr.deleted_mark);

                allEqual = false;
            }

            secB = new byte[sechdr.size_of_data];
            stream.EnsureRead(secB, 0, secB.Length);
            sectors.Add(sechdr.r, secB);

            foreach(KeyValuePair<byte, byte[]> kvp in sectors) _sectorsData.Add(kvp.Value);
        }

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization._0_sectors, _sectorsData.Count);

        _imageInfo.MediaType = MediaType.Unknown;

        if(allEqual)
        {
            if(trkCounter == 154 && spt == 26 && bps == IBMSectorSizeCode.EighthKilo)
                _imageInfo.MediaType = MediaType.NEC_8_SD;
            else if(bps == IBMSectorSizeCode.QuarterKilo)
            {
                _imageInfo.MediaType = trkCounter switch
                                       {
                                           35 when spt  == 16 => MediaType.MetaFloppy_Mod_I,
                                           77 when spt  == 16 => MediaType.MetaFloppy_Mod_II,
                                           80 when spt  == 16 => MediaType.NEC_525_SS,
                                           154 when spt == 26 => MediaType.NEC_8_DD,
                                           160 when spt == 16 => MediaType.NEC_525_DS,
                                           _                  => _imageInfo.MediaType
                                       };
            }
            else if(trkCounter == 154 && spt == 8 && bps == IBMSectorSizeCode.Kilo)
                _imageInfo.MediaType = MediaType.NEC_525_HD;
            else if(bps == IBMSectorSizeCode.HalfKilo)
            {
                switch(hdr.track_table.Length)
                {
                    case 40:
                    {
                        _imageInfo.MediaType = spt switch
                                               {
                                                   8 => MediaType.DOS_525_SS_DD_8,
                                                   9 => MediaType.DOS_525_SS_DD_9,
                                                   _ => _imageInfo.MediaType
                                               };
                    }

                        break;
                    case 80:
                    {
                        _imageInfo.MediaType = spt switch
                                               {
                                                   8 => MediaType.DOS_525_DS_DD_8,
                                                   9 => MediaType.DOS_525_DS_DD_9,
                                                   _ => _imageInfo.MediaType
                                               };
                    }

                        break;
                    case 160:
                    {
                        _imageInfo.MediaType = spt switch
                                               {
                                                   15 => MediaType.NEC_35_HD_15,
                                                   9  => MediaType.DOS_35_DS_DD_9,
                                                   18 => MediaType.DOS_35_HD,
                                                   36 => MediaType.DOS_35_ED,
                                                   _  => _imageInfo.MediaType
                                               };
                    }

                        break;
                    case 480:
                        if(spt == 38) _imageInfo.MediaType = MediaType.NEC_35_TD;

                        break;
                }
            }
        }

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.MediaType_0, _imageInfo.MediaType);

        _imageInfo.ImageSize            = (ulong)hdr.disk_size;
        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.Filename);
        _imageInfo.Sectors              = (ulong)_sectorsData.Count;
        _imageInfo.Comments             = StringHandlers.CToString(hdr.name, shiftjis);
        _imageInfo.MetadataMediaType    = MetadataMediaType.BlockMedia;
        _imageInfo.SectorSize           = (uint)(128 << (int)bps);

        switch(_imageInfo.MediaType)
        {
            case MediaType.NEC_525_SS:
                _imageInfo.Cylinders       = 80;
                _imageInfo.Heads           = 1;
                _imageInfo.SectorsPerTrack = 16;

                break;
            case MediaType.NEC_8_SD:
            case MediaType.NEC_8_DD:
                _imageInfo.Cylinders       = 77;
                _imageInfo.Heads           = 2;
                _imageInfo.SectorsPerTrack = 26;

                break;
            case MediaType.NEC_525_DS:
                _imageInfo.Cylinders       = 80;
                _imageInfo.Heads           = 2;
                _imageInfo.SectorsPerTrack = 16;

                break;
            case MediaType.NEC_525_HD:
                _imageInfo.Cylinders       = 77;
                _imageInfo.Heads           = 2;
                _imageInfo.SectorsPerTrack = 8;

                break;
            case MediaType.DOS_525_SS_DD_8:
                _imageInfo.Cylinders       = 40;
                _imageInfo.Heads           = 1;
                _imageInfo.SectorsPerTrack = 8;

                break;
            case MediaType.DOS_525_SS_DD_9:
                _imageInfo.Cylinders       = 40;
                _imageInfo.Heads           = 1;
                _imageInfo.SectorsPerTrack = 9;

                break;
            case MediaType.DOS_525_DS_DD_8:
                _imageInfo.Cylinders       = 40;
                _imageInfo.Heads           = 2;
                _imageInfo.SectorsPerTrack = 8;

                break;
            case MediaType.DOS_525_DS_DD_9:
                _imageInfo.Cylinders       = 40;
                _imageInfo.Heads           = 2;
                _imageInfo.SectorsPerTrack = 9;

                break;
            case MediaType.NEC_35_HD_15:
                _imageInfo.Cylinders       = 80;
                _imageInfo.Heads           = 2;
                _imageInfo.SectorsPerTrack = 15;

                break;
            case MediaType.DOS_35_DS_DD_9:
                _imageInfo.Cylinders       = 80;
                _imageInfo.Heads           = 2;
                _imageInfo.SectorsPerTrack = 9;

                break;
            case MediaType.DOS_35_HD:
                _imageInfo.Cylinders       = 80;
                _imageInfo.Heads           = 2;
                _imageInfo.SectorsPerTrack = 18;

                break;
            case MediaType.DOS_35_ED:
                _imageInfo.Cylinders       = 80;
                _imageInfo.Heads           = 2;
                _imageInfo.SectorsPerTrack = 36;

                break;
            case MediaType.NEC_35_TD:
                _imageInfo.Cylinders       = 240;
                _imageInfo.Heads           = 2;
                _imageInfo.SectorsPerTrack = 38;

                break;
            case MediaType.MetaFloppy_Mod_I:
                _imageInfo.Cylinders       = 35;
                _imageInfo.Heads           = 1;
                _imageInfo.SectorsPerTrack = 16;

                break;
            case MediaType.MetaFloppy_Mod_II:
                _imageInfo.Cylinders       = 77;
                _imageInfo.Heads           = 1;
                _imageInfo.SectorsPerTrack = 16;

                break;
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer) => ReadSectors(sectorAddress, 1, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > _imageInfo.Sectors - 1) return ErrorNumber.OutOfRange;

        if(sectorAddress + length > _imageInfo.Sectors) return ErrorNumber.OutOfRange;

        var ms = new MemoryStream();

        for(var i = 0; i < length; i++)
            ms.Write(_sectorsData[(int)sectorAddress + i], 0, _sectorsData[(int)sectorAddress + i].Length);

        buffer = ms.ToArray();

        return ErrorNumber.NoError;
    }

#endregion
}