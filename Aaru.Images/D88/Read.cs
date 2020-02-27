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
// Copyright © 2011-2020 Natalia Portillo
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
    public partial class D88
    {
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);
            // Even if disk name is supposedly ASCII, I'm pretty sure most emulators allow Shift-JIS to be used :p
            Encoding shiftjis = Encoding.GetEncoding("shift_jis");

            if(stream.Length < Marshal.SizeOf<D88Header>()) return false;

            byte[] hdrB = new byte[Marshal.SizeOf<D88Header>()];
            stream.Read(hdrB, 0, hdrB.Length);
            D88Header d88Hdr = Marshal.ByteArrayToStructureLittleEndian<D88Header>(hdrB);

            DicConsole.DebugWriteLine("D88 plugin", "d88hdr.name = \"{0}\"",
                                      StringHandlers.CToString(d88Hdr.name, shiftjis));
            DicConsole.DebugWriteLine("D88 plugin", "d88hdr.reserved is empty? = {0}",
                                      d88Hdr.reserved.SequenceEqual(reservedEmpty));
            DicConsole.DebugWriteLine("D88 plugin", "d88hdr.write_protect = 0x{0:X2}", d88Hdr.write_protect);
            DicConsole.DebugWriteLine("D88 plugin", "d88hdr.disk_type = {0} ({1})", d88Hdr.disk_type,
                                      (byte)d88Hdr.disk_type);
            DicConsole.DebugWriteLine("D88 plugin", "d88hdr.disk_size = {0}", d88Hdr.disk_size);

            if(d88Hdr.disk_size != stream.Length) return false;

            if(d88Hdr.disk_type != DiskType.D2 && d88Hdr.disk_type != DiskType.Dd2 &&
               d88Hdr.disk_type != DiskType.Hd2) return false;

            if(!d88Hdr.reserved.SequenceEqual(reservedEmpty)) return false;

            int trkCounter = 0;
            foreach(int t in d88Hdr.track_table)
            {
                if(t > 0) trkCounter++;

                if(t < 0 || t > stream.Length) return false;
            }

            DicConsole.DebugWriteLine("D88 plugin", "{0} tracks", trkCounter);

            if(trkCounter == 0) return false;

            hdrB = new byte[Marshal.SizeOf<SectorHeader>()];
            stream.Seek(d88Hdr.track_table[0], SeekOrigin.Begin);
            stream.Read(hdrB, 0, hdrB.Length);

            SectorHeader sechdr = Marshal.ByteArrayToStructureLittleEndian<SectorHeader>(hdrB);

            DicConsole.DebugWriteLine("D88 plugin", "sechdr.c = {0}",            sechdr.c);
            DicConsole.DebugWriteLine("D88 plugin", "sechdr.h = {0}",            sechdr.h);
            DicConsole.DebugWriteLine("D88 plugin", "sechdr.r = {0}",            sechdr.r);
            DicConsole.DebugWriteLine("D88 plugin", "sechdr.n = {0}",            sechdr.n);
            DicConsole.DebugWriteLine("D88 plugin", "sechdr.spt = {0}",          sechdr.spt);
            DicConsole.DebugWriteLine("D88 plugin", "sechdr.density = {0}",      sechdr.density);
            DicConsole.DebugWriteLine("D88 plugin", "sechdr.deleted_mark = {0}", sechdr.deleted_mark);
            DicConsole.DebugWriteLine("D88 plugin", "sechdr.status = {0}",       sechdr.status);
            DicConsole.DebugWriteLine("D88 plugin", "sechdr.size_of_data = {0}", sechdr.size_of_data);

            short             spt      = sechdr.spt;
            IBMSectorSizeCode bps      = sechdr.n;
            bool              allEqual = true;
            sectorsData = new List<byte[]>();

            for(int i = 0; i < trkCounter; i++)
            {
                stream.Seek(d88Hdr.track_table[i], SeekOrigin.Begin);
                stream.Read(hdrB, 0, hdrB.Length);
                SortedDictionary<byte, byte[]> sectors = new SortedDictionary<byte, byte[]>();

                sechdr = Marshal.ByteArrayToStructureLittleEndian<SectorHeader>(hdrB);

                if(sechdr.spt != spt || sechdr.n != bps)
                {
                    DicConsole.DebugWriteLine("D88 plugin",
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

                    if(sechdr.spt == spt && sechdr.n == bps) continue;

                    DicConsole.DebugWriteLine("D88 plugin",
                                              "Disk tracks are not same size. spt = {0} (expected {1}), bps = {2} (expected {3}) at track {4} sector {5}",
                                              sechdr.spt, spt, sechdr.n, bps, i, j, sechdr.deleted_mark);
                    allEqual = false;
                }

                secB = new byte[sechdr.size_of_data];
                stream.Read(secB, 0, secB.Length);
                sectors.Add(sechdr.r, secB);

                foreach(KeyValuePair<byte, byte[]> kvp in sectors) sectorsData.Add(kvp.Value);
            }

            DicConsole.DebugWriteLine("D88 plugin", "{0} sectors", sectorsData.Count);

            imageInfo.MediaType = MediaType.Unknown;
            if(allEqual)
                if(trkCounter == 154 && spt == 26 && bps == IBMSectorSizeCode.EighthKilo)
                    imageInfo.MediaType = MediaType.NEC_8_SD;
                else if(bps == IBMSectorSizeCode.QuarterKilo)
                    switch(trkCounter)
                    {
                        case 80 when spt == 16:
                            imageInfo.MediaType = MediaType.NEC_525_SS;
                            break;
                        case 154 when spt == 26:
                            imageInfo.MediaType = MediaType.NEC_8_DD;
                            break;
                        case 160 when spt == 16:
                            imageInfo.MediaType = MediaType.NEC_525_DS;
                            break;
                    }
                else if(trkCounter == 154 && spt == 8 && bps == IBMSectorSizeCode.Kilo)
                    imageInfo.MediaType = MediaType.NEC_525_HD;
                else if(bps == IBMSectorSizeCode.HalfKilo)
                    switch(d88Hdr.track_table.Length)
                    {
                        case 40:
                        {
                            switch(spt)
                            {
                                case 8:
                                    imageInfo.MediaType = MediaType.DOS_525_SS_DD_8;
                                    break;
                                case 9:
                                    imageInfo.MediaType = MediaType.DOS_525_SS_DD_9;
                                    break;
                            }
                        }

                            break;
                        case 80:
                        {
                            switch(spt)
                            {
                                case 8:
                                    imageInfo.MediaType = MediaType.DOS_525_DS_DD_8;
                                    break;
                                case 9:
                                    imageInfo.MediaType = MediaType.DOS_525_DS_DD_9;
                                    break;
                            }
                        }

                            break;
                        case 160:
                        {
                            switch(spt)
                            {
                                case 15:
                                    imageInfo.MediaType = MediaType.NEC_35_HD_15;
                                    break;
                                case 9:
                                    imageInfo.MediaType = MediaType.DOS_35_DS_DD_9;
                                    break;
                                case 18:
                                    imageInfo.MediaType = MediaType.DOS_35_HD;
                                    break;
                                case 36:
                                    imageInfo.MediaType = MediaType.DOS_35_ED;
                                    break;
                            }
                        }

                            break;
                        case 480:
                            if(spt == 38) imageInfo.MediaType = MediaType.NEC_35_TD;
                            break;
                    }

            DicConsole.DebugWriteLine("D88 plugin", "MediaType: {0}", imageInfo.MediaType);

            imageInfo.ImageSize            = (ulong)d88Hdr.disk_size;
            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.Sectors              = (ulong)sectorsData.Count;
            imageInfo.Comments             = StringHandlers.CToString(d88Hdr.name, shiftjis);
            imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;
            imageInfo.SectorSize           = (uint)(128 << (int)bps);

            switch(imageInfo.MediaType)
            {
                case MediaType.NEC_525_SS:
                    imageInfo.Cylinders       = 80;
                    imageInfo.Heads           = 1;
                    imageInfo.SectorsPerTrack = 16;
                    break;
                case MediaType.NEC_8_SD:
                case MediaType.NEC_8_DD:
                    imageInfo.Cylinders       = 77;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 26;
                    break;
                case MediaType.NEC_525_DS:
                    imageInfo.Cylinders       = 80;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 16;
                    break;
                case MediaType.NEC_525_HD:
                    imageInfo.Cylinders       = 77;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 8;
                    break;
                case MediaType.DOS_525_SS_DD_8:
                    imageInfo.Cylinders       = 40;
                    imageInfo.Heads           = 1;
                    imageInfo.SectorsPerTrack = 8;
                    break;
                case MediaType.DOS_525_SS_DD_9:
                    imageInfo.Cylinders       = 40;
                    imageInfo.Heads           = 1;
                    imageInfo.SectorsPerTrack = 9;
                    break;
                case MediaType.DOS_525_DS_DD_8:
                    imageInfo.Cylinders       = 40;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 8;
                    break;
                case MediaType.DOS_525_DS_DD_9:
                    imageInfo.Cylinders       = 40;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 9;
                    break;
                case MediaType.NEC_35_HD_15:
                    imageInfo.Cylinders       = 80;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 15;
                    break;
                case MediaType.DOS_35_DS_DD_9:
                    imageInfo.Cylinders       = 80;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 9;
                    break;
                case MediaType.DOS_35_HD:
                    imageInfo.Cylinders       = 80;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 18;
                    break;
                case MediaType.DOS_35_ED:
                    imageInfo.Cylinders       = 80;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 36;
                    break;
                case MediaType.NEC_35_TD:
                    imageInfo.Cylinders       = 240;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 38;
                    break;
            }

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress) => ReadSectors(sectorAddress, 1);

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            MemoryStream buffer = new MemoryStream();
            for(int i = 0; i < length; i++)
                buffer.Write(sectorsData[(int)sectorAddress + i], 0, sectorsData[(int)sectorAddress + i].Length);

            return buffer.ToArray();
        }
    }
}