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
//     Reads DIM disk images.
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
using System.Linq;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.DiscImages
{
    public partial class Dim
    {
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < DATA_OFFSET)
                return false;

            long diskSize = stream.Length - DATA_OFFSET;

            comment = new byte[60];
            hdrId   = new byte[13];
            stream.Seek(0, SeekOrigin.Begin);
            dskType = (DiskType)stream.ReadByte();
            stream.Seek(0xAB, SeekOrigin.Begin);
            stream.Read(hdrId, 0, 13);
            stream.Seek(0xC2, SeekOrigin.Begin);
            stream.Read(comment, 0, 60);

            if(!headerId.SequenceEqual(hdrId))
                return false;

            imageInfo.MediaType = MediaType.Unknown;

            switch(dskType)
            {
                // 8 spt, 1024 bps
                case DiskType.Hd2:
                    if(diskSize % (2 * 8 * 1024) != 0)
                    {
                        AaruConsole.ErrorWriteLine("DIM shows unknown image with {0} tracks",
                                                   diskSize / (2 * 8 * 1024));

                        return false;
                    }

                    if(diskSize / (2 * 8 * 1024) == 77)
                        imageInfo.MediaType = MediaType.SHARP_525;

                    imageInfo.SectorSize = 1024;

                    break;

                // 9 spt, 1024 bps
                case DiskType.Hs2:
                    if(diskSize % (2 * 9 * 512) != 0)
                    {
                        AaruConsole.ErrorWriteLine("DIM shows unknown image with {0} tracks", diskSize / (2 * 9 * 512));

                        return false;
                    }

                    if(diskSize / (2 * 9 * 512) == 80)
                        imageInfo.MediaType = MediaType.SHARP_525_9;

                    imageInfo.SectorSize = 512;

                    break;

                // 15 spt, 512 bps
                case DiskType.Hc2:
                    if(diskSize % (2 * 15 * 512) != 0)
                    {
                        AaruConsole.ErrorWriteLine("DIM shows unknown image with {0} tracks",
                                                   diskSize / (2 * 15 * 512));

                        return false;
                    }

                    if(diskSize / (2 * 15 * 512) == 80)
                        imageInfo.MediaType = MediaType.DOS_525_HD;

                    imageInfo.SectorSize = 512;

                    break;

                // 9 spt, 1024 bps
                case DiskType.Hde2:
                    if(diskSize % (2 * 9 * 512) != 0)
                    {
                        AaruConsole.ErrorWriteLine("DIM shows unknown image with {0} tracks", diskSize / (2 * 9 * 512));

                        return false;
                    }

                    if(diskSize / (2 * 9 * 512) == 80)
                        imageInfo.MediaType = MediaType.SHARP_35_9;

                    imageInfo.SectorSize = 512;

                    break;

                // 18 spt, 512 bps
                case DiskType.Hq2:
                    if(diskSize % (2 * 18 * 512) != 0)
                    {
                        AaruConsole.ErrorWriteLine("DIM shows unknown image with {0} tracks",
                                                   diskSize / (2 * 18 * 512));

                        return false;
                    }

                    if(diskSize / (2 * 18 * 512) == 80)
                        imageInfo.MediaType = MediaType.DOS_35_HD;

                    imageInfo.SectorSize = 512;

                    break;

                // 26 spt, 256 bps
                case DiskType.N88:
                    if(diskSize % (2 * 26 * 256) == 0)
                    {
                        if(diskSize % (2 * 26 * 256) == 77)
                            imageInfo.MediaType = MediaType.NEC_8_DD;

                        imageInfo.SectorSize = 256;
                    }
                    else if(diskSize % (2 * 26 * 128) == 0)
                    {
                        if(diskSize % (2 * 26 * 128) == 77)
                            imageInfo.MediaType = MediaType.NEC_8_SD;

                        imageInfo.SectorSize = 256;
                    }
                    else
                    {
                        AaruConsole.ErrorWriteLine("DIM shows unknown image with {0} tracks",
                                                   diskSize / (2 * 26 * 256));

                        return false;
                    }

                    break;
                default: return false;
            }

            AaruConsole.VerboseWriteLine("DIM image contains a disk of type {0}", imageInfo.MediaType);

            if(!string.IsNullOrEmpty(imageInfo.Comments))
                AaruConsole.VerboseWriteLine("DIM comments: {0}", imageInfo.Comments);

            dimImageFilter = imageFilter;

            imageInfo.ImageSize            = (ulong)diskSize;
            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.Sectors              = imageInfo.ImageSize / imageInfo.SectorSize;
            imageInfo.Comments             = StringHandlers.CToString(comment, Encoding.GetEncoding(932));
            imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;

            switch(imageInfo.MediaType)
            {
                case MediaType.SHARP_525:
                    imageInfo.Cylinders       = 77;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 8;

                    break;
                case MediaType.SHARP_525_9:
                    imageInfo.Cylinders       = 80;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 9;

                    break;
                case MediaType.DOS_525_HD:
                    imageInfo.Cylinders       = 80;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 15;

                    break;
                case MediaType.SHARP_35_9:
                    imageInfo.Cylinders       = 80;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 9;

                    break;
                case MediaType.DOS_35_HD:
                    imageInfo.Cylinders       = 80;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 18;

                    break;
                case MediaType.NEC_8_DD:
                case MediaType.NEC_8_SD:
                    imageInfo.Cylinders       = 77;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 26;

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

            byte[] buffer = new byte[length * imageInfo.SectorSize];

            Stream stream = dimImageFilter.GetDataForkStream();

            stream.Seek((long)(DATA_OFFSET + (sectorAddress * imageInfo.SectorSize)), SeekOrigin.Begin);

            stream.Read(buffer, 0, (int)(length * imageInfo.SectorSize));

            return buffer;
        }
    }
}