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
//     Reads Spectrum FDI disk images.
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
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.DiscImages
{
    public sealed partial class UkvFdi
    {
        /// <inheritdoc />
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < Marshal.SizeOf<Header>())
                return false;

            byte[] hdrB = new byte[Marshal.SizeOf<Header>()];
            stream.Read(hdrB, 0, hdrB.Length);

            Header hdr = Marshal.ByteArrayToStructureLittleEndian<Header>(hdrB);

            AaruConsole.DebugWriteLine("UkvFdi plugin", "hdr.addInfoLen = {0}", hdr.addInfoLen);
            AaruConsole.DebugWriteLine("UkvFdi plugin", "hdr.cylinders = {0}", hdr.cylinders);
            AaruConsole.DebugWriteLine("UkvFdi plugin", "hdr.dataOff = {0}", hdr.dataOff);
            AaruConsole.DebugWriteLine("UkvFdi plugin", "hdr.descOff = {0}", hdr.descOff);
            AaruConsole.DebugWriteLine("UkvFdi plugin", "hdr.flags = {0}", hdr.flags);
            AaruConsole.DebugWriteLine("UkvFdi plugin", "hdr.heads = {0}", hdr.heads);

            stream.Seek(hdr.descOff, SeekOrigin.Begin);
            byte[] description = new byte[hdr.dataOff - hdr.descOff];
            stream.Read(description, 0, description.Length);
            _imageInfo.Comments = StringHandlers.CToString(description);

            AaruConsole.DebugWriteLine("UkvFdi plugin", "hdr.description = \"{0}\"", _imageInfo.Comments);

            stream.Seek(0xE + hdr.addInfoLen, SeekOrigin.Begin);

            long       spt        = long.MaxValue;
            uint[][][] sectorsOff = new uint[hdr.cylinders][][];
            _sectorsData = new byte[hdr.cylinders][][][];

            _imageInfo.Cylinders = hdr.cylinders;
            _imageInfo.Heads     = hdr.heads;

            // Read track descriptors
            for(ushort cyl = 0; cyl < hdr.cylinders; cyl++)
            {
                sectorsOff[cyl]   = new uint[hdr.heads][];
                _sectorsData[cyl] = new byte[hdr.heads][][];

                for(ushort head = 0; head < hdr.heads; head++)
                {
                    byte[] sctB = new byte[4];
                    stream.Read(sctB, 0, 4);
                    stream.Seek(2, SeekOrigin.Current);
                    byte sectors = (byte)stream.ReadByte();
                    uint trkOff  = BitConverter.ToUInt32(sctB, 0);

                    AaruConsole.DebugWriteLine("UkvFdi plugin", "trkhdr.c = {0}", cyl);
                    AaruConsole.DebugWriteLine("UkvFdi plugin", "trkhdr.h = {0}", head);
                    AaruConsole.DebugWriteLine("UkvFdi plugin", "trkhdr.sectors = {0}", sectors);
                    AaruConsole.DebugWriteLine("UkvFdi plugin", "trkhdr.off = {0}", trkOff);

                    sectorsOff[cyl][head]   = new uint[sectors];
                    _sectorsData[cyl][head] = new byte[sectors][];

                    if(sectors < spt &&
                       sectors > 0)
                        spt = sectors;

                    for(ushort sec = 0; sec < sectors; sec++)
                    {
                        byte   c    = (byte)stream.ReadByte();
                        byte   h    = (byte)stream.ReadByte();
                        byte   r    = (byte)stream.ReadByte();
                        byte   n    = (byte)stream.ReadByte();
                        var    f    = (SectorFlags)stream.ReadByte();
                        byte[] offB = new byte[2];
                        stream.Read(offB, 0, 2);
                        ushort secOff = BitConverter.ToUInt16(offB, 0);

                        AaruConsole.DebugWriteLine("UkvFdi plugin", "sechdr.c = {0}", c);
                        AaruConsole.DebugWriteLine("UkvFdi plugin", "sechdr.h = {0}", h);
                        AaruConsole.DebugWriteLine("UkvFdi plugin", "sechdr.r = {0}", r);
                        AaruConsole.DebugWriteLine("UkvFdi plugin", "sechdr.n = {0} ({1})", n, 128 << n);
                        AaruConsole.DebugWriteLine("UkvFdi plugin", "sechdr.f = {0}", f);

                        AaruConsole.DebugWriteLine("UkvFdi plugin", "sechdr.off = {0} ({1})", secOff,
                                                   secOff + trkOff + hdr.dataOff);

                        // TODO: This assumes sequential sectors.
                        sectorsOff[cyl][head][sec]   = secOff + trkOff + hdr.dataOff;
                        _sectorsData[cyl][head][sec] = new byte[128 << n];

                        if(128 << n > _imageInfo.SectorSize)
                            _imageInfo.SectorSize = (uint)(128 << n);
                    }
                }
            }

            // Read sectors
            for(ushort cyl = 0; cyl < hdr.cylinders; cyl++)
            {
                bool emptyCyl = false;

                for(ushort head = 0; head < hdr.heads; head++)
                {
                    for(ushort sec = 0; sec < sectorsOff[cyl][head].Length; sec++)
                    {
                        stream.Seek(sectorsOff[cyl][head][sec], SeekOrigin.Begin);
                        stream.Read(_sectorsData[cyl][head][sec], 0, _sectorsData[cyl][head][sec].Length);
                    }

                    // For empty cylinders
                    if(sectorsOff[cyl][head].Length != 0)
                        continue;

                    if(cyl + 1 == hdr.cylinders ||

                       // Next cylinder is also empty
                       sectorsOff[cyl + 1][head].Length == 0)
                        emptyCyl = true;

                    // Create empty sectors
                    else
                    {
                        _sectorsData[cyl][head] = new byte[spt][];

                        for(int i = 0; i < spt; i++)
                            _sectorsData[cyl][head][i] = new byte[_imageInfo.SectorSize];
                    }
                }

                if(emptyCyl)
                    _imageInfo.Cylinders--;
            }

            // TODO: What about double sided, half track pitch compact floppies?
            _imageInfo.MediaType            = MediaType.CompactFloppy;
            _imageInfo.ImageSize            = (ulong)stream.Length - hdr.dataOff;
            _imageInfo.CreationTime         = imageFilter.CreationTime;
            _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
            _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.Filename);
            _imageInfo.SectorsPerTrack      = (uint)spt;
            _imageInfo.Sectors              = _imageInfo.Cylinders * _imageInfo.Heads * _imageInfo.SectorsPerTrack;
            _imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;

            return true;
        }

        /// <inheritdoc />
        public byte[] ReadSector(ulong sectorAddress)
        {
            (ushort cylinder, byte head, byte sector) = LbaToChs(sectorAddress);

            if(cylinder >= _sectorsData.Length)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(head >= _sectorsData[cylinder].Length)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sector > _sectorsData[cylinder][head].Length)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            return _sectorsData[cylinder][head][sector - 1];
        }

        /// <inheritdoc />
        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > _imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > _imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            var buffer = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSector(sectorAddress + i);
                buffer.Write(sector, 0, sector.Length);
            }

            return buffer.ToArray();
        }
    }
}