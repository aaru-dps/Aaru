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
//     Reads IBM SaveDskF disk images.
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
using System.IO;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Exceptions;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.DiscImages
{
    public partial class SaveDskF
    {
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            byte[] hdr = new byte[40];

            stream.Read(hdr, 0, 40);
            header = Marshal.ByteArrayToStructureLittleEndian<SaveDskFHeader>(hdr);

            AaruConsole.DebugWriteLine("SaveDskF plugin", "header.magic = 0x{0:X4}",      header.magic);
            AaruConsole.DebugWriteLine("SaveDskF plugin", "header.mediaType = 0x{0:X2}",  header.mediaType);
            AaruConsole.DebugWriteLine("SaveDskF plugin", "header.sectorSize = {0}",      header.sectorSize);
            AaruConsole.DebugWriteLine("SaveDskF plugin", "header.clusterMask = {0}",     header.clusterMask);
            AaruConsole.DebugWriteLine("SaveDskF plugin", "header.clusterShift = {0}",    header.clusterShift);
            AaruConsole.DebugWriteLine("SaveDskF plugin", "header.reservedSectors = {0}", header.reservedSectors);
            AaruConsole.DebugWriteLine("SaveDskF plugin", "header.fatCopies = {0}",       header.fatCopies);
            AaruConsole.DebugWriteLine("SaveDskF plugin", "header.rootEntries = {0}",     header.rootEntries);
            AaruConsole.DebugWriteLine("SaveDskF plugin", "header.firstCluster = {0}",    header.firstCluster);
            AaruConsole.DebugWriteLine("SaveDskF plugin", "header.clustersCopied = {0}",  header.clustersCopied);
            AaruConsole.DebugWriteLine("SaveDskF plugin", "header.sectorsPerFat = {0}",   header.sectorsPerFat);
            AaruConsole.DebugWriteLine("SaveDskF plugin", "header.checksum = 0x{0:X8}",   header.checksum);
            AaruConsole.DebugWriteLine("SaveDskF plugin", "header.cylinders = {0}",       header.cylinders);
            AaruConsole.DebugWriteLine("SaveDskF plugin", "header.heads = {0}",           header.heads);
            AaruConsole.DebugWriteLine("SaveDskF plugin", "header.sectorsPerTrack = {0}", header.sectorsPerTrack);
            AaruConsole.DebugWriteLine("SaveDskF plugin", "header.padding = {0}",         header.padding);
            AaruConsole.DebugWriteLine("SaveDskF plugin", "header.sectorsCopied = {0}",   header.sectorsCopied);
            AaruConsole.DebugWriteLine("SaveDskF plugin", "header.commentOffset = {0}",   header.commentOffset);
            AaruConsole.DebugWriteLine("SaveDskF plugin", "header.dataOffset = {0}",      header.dataOffset);

            if(header.dataOffset == 0 && header.magic == SDF_MAGIC_OLD) header.dataOffset = 512;

            byte[] cmt = new byte[header.dataOffset - header.commentOffset];
            stream.Seek(header.commentOffset, SeekOrigin.Begin);
            stream.Read(cmt, 0, cmt.Length);
            if(cmt.Length > 1) imageInfo.Comments = StringHandlers.CToString(cmt, Encoding.GetEncoding("ibm437"));

            calculatedChk = 0;
            stream.Seek(0, SeekOrigin.Begin);

            int b;
            do
            {
                b = stream.ReadByte();
                if(b >= 0) calculatedChk += (uint)b;
            }
            while(b >= 0);

            AaruConsole.DebugWriteLine("SaveDskF plugin", "Calculated checksum = 0x{0:X8}, {1}", calculatedChk,
                                      calculatedChk == header.checksum);

            imageInfo.Application          = "SaveDskF";
            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = imageFilter.GetFilename();
            imageInfo.ImageSize            = (ulong)(stream.Length - header.dataOffset);
            imageInfo.Sectors              = (ulong)(header.sectorsPerTrack * header.heads * header.cylinders);
            imageInfo.SectorSize           = header.sectorSize;

            imageInfo.MediaType = Geometry.GetMediaType((header.cylinders, (byte)header.heads, header.sectorsPerTrack,
                                                         header.sectorSize, MediaEncoding.MFM, false));

            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;

            AaruConsole.VerboseWriteLine("SaveDskF image contains a disk of type {0}", imageInfo.MediaType);
            if(!string.IsNullOrEmpty(imageInfo.Comments))
                AaruConsole.VerboseWriteLine("SaveDskF comments: {0}", imageInfo.Comments);

            // TODO: Support compressed images
            if(header.magic == SDF_MAGIC_COMPRESSED)
                throw new
                    FeatureSupportedButNotImplementedImageException("Compressed SaveDskF images are not supported.");

            // SaveDskF only ommits ending clusters, leaving no gaps behind, so reading all data we have...
            stream.Seek(header.dataOffset, SeekOrigin.Begin);
            decodedDisk = new byte[imageInfo.Sectors * imageInfo.SectorSize];
            stream.Read(decodedDisk, 0, (int)(stream.Length - header.dataOffset));

            imageInfo.Cylinders       = header.cylinders;
            imageInfo.Heads           = header.heads;
            imageInfo.SectorsPerTrack = header.sectorsPerTrack;

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress) => ReadSectors(sectorAddress, 1);

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * imageInfo.SectorSize];

            Array.Copy(decodedDisk, (int)sectorAddress * imageInfo.SectorSize, buffer, 0,
                       length                          * imageInfo.SectorSize);

            return buffer;
        }
    }
}