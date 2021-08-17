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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
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

namespace Aaru.DiscImages
{
    public sealed partial class D88
    {
        /// <inheritdoc />
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            // Even if disk name is supposedly ASCII, I'm pretty sure most emulators allow Shift-JIS to be used :p
            var shiftjis = Encoding.GetEncoding("shift_jis");

            if(stream.Length < Marshal.SizeOf<Header>())
                return false;

            byte[] hdrB = new byte[Marshal.SizeOf<Header>()];
            stream.Read(hdrB, 0, hdrB.Length);
            Header hdr = Marshal.ByteArrayToStructureLittleEndian<Header>(hdrB);

            AaruConsole.DebugWriteLine("D88 plugin", "d88hdr.name = \"{0}\"",
                                       StringHandlers.CToString(hdr.name, shiftjis));

            AaruConsole.DebugWriteLine("D88 plugin", "d88hdr.reserved is empty? = {0}",
                                       hdr.reserved.SequenceEqual(_reservedEmpty));

            AaruConsole.DebugWriteLine("D88 plugin", "d88hdr.write_protect = 0x{0:X2}", hdr.write_protect);

            AaruConsole.DebugWriteLine("D88 plugin", "d88hdr.disk_type = {0} ({1})", hdr.disk_type,
                                       (byte)hdr.disk_type);

            AaruConsole.DebugWriteLine("D88 plugin", "d88hdr.disk_size = {0}", hdr.disk_size);

            if(hdr.disk_size != stream.Length)
                return false;

            if(hdr.disk_type != DiskType.D2  &&
               hdr.disk_type != DiskType.Dd2 &&
               hdr.disk_type != DiskType.Hd2)
                return false;

            if(!hdr.reserved.SequenceEqual(_reservedEmpty))
                return false;

            int trkCounter = 0;

            foreach(int t in hdr.track_table)
            {
                if(t > 0)
                    trkCounter++;

                if(t < 0 ||
                   t > stream.Length)
                    return false;
            }

            AaruConsole.DebugWriteLine("D88 plugin", "{0} tracks", trkCounter);

            if(trkCounter == 0)
                return false;

            hdrB = new byte[Marshal.SizeOf<SectorHeader>()];
            stream.Seek(hdr.track_table[0], SeekOrigin.Begin);
            stream.Read(hdrB, 0, hdrB.Length);

            SectorHeader sechdr = Marshal.ByteArrayToStructureLittleEndian<SectorHeader>(hdrB);

            AaruConsole.DebugWriteLine("D88 plugin", "sechdr.c = {0}", sechdr.c);
            AaruConsole.DebugWriteLine("D88 plugin", "sechdr.h = {0}", sechdr.h);
            AaruConsole.DebugWriteLine("D88 plugin", "sechdr.r = {0}", sechdr.r);
            AaruConsole.DebugWriteLine("D88 plugin", "sechdr.n = {0}", sechdr.n);
            AaruConsole.DebugWriteLine("D88 plugin", "sechdr.spt = {0}", sechdr.spt);
            AaruConsole.DebugWriteLine("D88 plugin", "sechdr.density = {0}", sechdr.density);
            AaruConsole.DebugWriteLine("D88 plugin", "sechdr.deleted_mark = {0}", sechdr.deleted_mark);
            AaruConsole.DebugWriteLine("D88 plugin", "sechdr.status = {0}", sechdr.status);
            AaruConsole.DebugWriteLine("D88 plugin", "sechdr.size_of_data = {0}", sechdr.size_of_data);

            short             spt      = sechdr.spt;
            IBMSectorSizeCode bps      = sechdr.n;
            bool              allEqual = true;
            _sectorsData = new List<byte[]>();

            for(int i = 0; i < trkCounter; i++)
            {
                stream.Seek(hdr.track_table[i], SeekOrigin.Begin);
                stream.Read(hdrB, 0, hdrB.Length);
                SortedDictionary<byte, byte[]> sectors = new SortedDictionary<byte, byte[]>();

                sechdr = Marshal.ByteArrayToStructureLittleEndian<SectorHeader>(hdrB);

                if(sechdr.spt != spt ||
                   sechdr.n   != bps)
                {
                    AaruConsole.DebugWriteLine("D88 plugin",
                                               "Disk tracks are not same size. spt = {0} (expected {1}), bps = {2} (expected {3}) at track {4} sector {5}",
                                               sechdr.spt, spt, sechdr.n, bps, i, 0);

                    allEqual = false;
                }

                short  maxJ = sechdr.spt;
                byte[] secB;

                for(short j = 1; j < maxJ; j++)
                {
                    secB = new byte[sechdr.size_of_data];
                    stream.Read(secB, 0, secB.Length);
                    sectors.Add(sechdr.r, secB);
                    stream.Read(hdrB, 0, hdrB.Length);

                    sechdr = Marshal.ByteArrayToStructureLittleEndian<SectorHeader>(hdrB);

                    if(sechdr.spt == spt &&
                       sechdr.n   == bps)
                        continue;

                    AaruConsole.DebugWriteLine("D88 plugin",
                                               "Disk tracks are not same size. spt = {0} (expected {1}), bps = {2} (expected {3}) at track {4} sector {5}",
                                               sechdr.spt, spt, sechdr.n, bps, i, j, sechdr.deleted_mark);

                    allEqual = false;
                }

                secB = new byte[sechdr.size_of_data];
                stream.Read(secB, 0, secB.Length);
                sectors.Add(sechdr.r, secB);

                foreach(KeyValuePair<byte, byte[]> kvp in sectors)
                    _sectorsData.Add(kvp.Value);
            }

            AaruConsole.DebugWriteLine("D88 plugin", "{0} sectors", _sectorsData.Count);

            _imageInfo.MediaType = MediaType.Unknown;

            if(allEqual)
                if(trkCounter == 154 &&
                   spt        == 26  &&
                   bps        == IBMSectorSizeCode.EighthKilo)
                    _imageInfo.MediaType = MediaType.NEC_8_SD;
                else if(bps == IBMSectorSizeCode.QuarterKilo)
                    switch(trkCounter)
                    {
                        case 80 when spt == 16:
                            _imageInfo.MediaType = MediaType.NEC_525_SS;

                            break;
                        case 154 when spt == 26:
                            _imageInfo.MediaType = MediaType.NEC_8_DD;

                            break;
                        case 160 when spt == 16:
                            _imageInfo.MediaType = MediaType.NEC_525_DS;

                            break;
                    }
                else if(trkCounter == 154 &&
                        spt        == 8   &&
                        bps        == IBMSectorSizeCode.Kilo)
                    _imageInfo.MediaType = MediaType.NEC_525_HD;
                else if(bps == IBMSectorSizeCode.HalfKilo)
                    switch(hdr.track_table.Length)
                    {
                        case 40:
                        {
                            switch(spt)
                            {
                                case 8:
                                    _imageInfo.MediaType = MediaType.DOS_525_SS_DD_8;

                                    break;
                                case 9:
                                    _imageInfo.MediaType = MediaType.DOS_525_SS_DD_9;

                                    break;
                            }
                        }

                            break;
                        case 80:
                        {
                            switch(spt)
                            {
                                case 8:
                                    _imageInfo.MediaType = MediaType.DOS_525_DS_DD_8;

                                    break;
                                case 9:
                                    _imageInfo.MediaType = MediaType.DOS_525_DS_DD_9;

                                    break;
                            }
                        }

                            break;
                        case 160:
                        {
                            switch(spt)
                            {
                                case 15:
                                    _imageInfo.MediaType = MediaType.NEC_35_HD_15;

                                    break;
                                case 9:
                                    _imageInfo.MediaType = MediaType.DOS_35_DS_DD_9;

                                    break;
                                case 18:
                                    _imageInfo.MediaType = MediaType.DOS_35_HD;

                                    break;
                                case 36:
                                    _imageInfo.MediaType = MediaType.DOS_35_ED;

                                    break;
                            }
                        }

                            break;
                        case 480:
                            if(spt == 38)
                                _imageInfo.MediaType = MediaType.NEC_35_TD;

                            break;
                    }

            AaruConsole.DebugWriteLine("D88 plugin", "MediaType: {0}", _imageInfo.MediaType);

            _imageInfo.ImageSize            = (ulong)hdr.disk_size;
            _imageInfo.CreationTime         = imageFilter.GetCreationTime();
            _imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            _imageInfo.Sectors              = (ulong)_sectorsData.Count;
            _imageInfo.Comments             = StringHandlers.CToString(hdr.name, shiftjis);
            _imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;
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
            }

            return true;
        }

        /// <inheritdoc />
        public byte[] ReadSector(ulong sectorAddress) => ReadSectors(sectorAddress, 1);

        /// <inheritdoc />
        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > _imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > _imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            var buffer = new MemoryStream();

            for(int i = 0; i < length; i++)
                buffer.Write(_sectorsData[(int)sectorAddress + i], 0, _sectorsData[(int)sectorAddress + i].Length);

            return buffer.ToArray();
        }
    }
}