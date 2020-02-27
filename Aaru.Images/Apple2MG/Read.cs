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
//     Reads XGS emulator disk images.
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
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Filters;
using Aaru.Helpers;

namespace Aaru.DiscImages
{
    public partial class Apple2Mg
    {
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            imageHeader = new A2ImgHeader();

            byte[] header = new byte[64];
            stream.Read(header, 0, 64);
            byte[] magic   = new byte[4];
            byte[] creator = new byte[4];

            Array.Copy(header, 0, magic,   0, 4);
            Array.Copy(header, 4, creator, 0, 4);

            imageHeader = Marshal.SpanToStructureLittleEndian<A2ImgHeader>(header);

            if(imageHeader.DataSize == 0x00800C00)
            {
                imageHeader.DataSize = 0x000C8000;
                DicConsole.DebugWriteLine("2MG plugin", "Detected incorrect endian on data size field, correcting.");
            }

            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.magic = \"{0}\"",
                                      Encoding.ASCII.GetString(magic));
            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.creator = \"{0}\"",
                                      Encoding.ASCII.GetString(creator));
            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.headerSize = {0}",         imageHeader.HeaderSize);
            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.version = {0}",            imageHeader.Version);
            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.imageFormat = {0}",        imageHeader.ImageFormat);
            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.flags = 0x{0:X8}",         imageHeader.Flags);
            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.blocks = {0}",             imageHeader.Blocks);
            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.dataOffset = 0x{0:X8}",    imageHeader.DataOffset);
            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.dataSize = {0}",           imageHeader.DataSize);
            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.commentOffset = 0x{0:X8}", imageHeader.CommentOffset);
            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.commentSize = {0}",        imageHeader.CommentSize);
            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.creatorSpecificOffset = 0x{0:X8}",
                                      imageHeader.CreatorSpecificOffset);
            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.creatorSpecificSize = {0}",
                                      imageHeader.CreatorSpecificSize);
            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.reserved1 = 0x{0:X8}", imageHeader.Reserved1);
            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.reserved2 = 0x{0:X8}", imageHeader.Reserved2);
            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.reserved3 = 0x{0:X8}", imageHeader.Reserved3);
            DicConsole.DebugWriteLine("2MG plugin", "ImageHeader.reserved4 = 0x{0:X8}", imageHeader.Reserved4);

            if(imageHeader.DataSize    == 0 && imageHeader.Blocks == 0 &&
               imageHeader.ImageFormat != SectorOrder.ProDos) return false;

            byte[] tmp;
            int[]  offsets;

            switch(imageHeader.ImageFormat)
            {
                case SectorOrder.Nibbles:
                    tmp = new byte[imageHeader.DataSize];
                    stream.Seek(imageHeader.DataOffset, SeekOrigin.Begin);
                    stream.Read(tmp, 0, tmp.Length);
                    AppleNib    nibPlugin = new AppleNib();
                    ZZZNoFilter noFilter  = new ZZZNoFilter();
                    noFilter.Open(tmp);
                    nibPlugin.Open(noFilter);
                    decodedImage         = nibPlugin.ReadSectors(0, (uint)nibPlugin.Info.Sectors);
                    imageInfo.Sectors    = nibPlugin.Info.Sectors;
                    imageInfo.SectorSize = nibPlugin.Info.SectorSize;
                    break;
                case SectorOrder.Dos when imageHeader.DataSize    == 143360:
                case SectorOrder.ProDos when imageHeader.DataSize == 143360:
                    stream.Seek(imageHeader.DataOffset, SeekOrigin.Begin);
                    tmp = new byte[imageHeader.DataSize];
                    stream.Read(tmp, 0, tmp.Length);
                    bool isDos = tmp[0x11001] == 17 && tmp[0x11002] < 16 && tmp[0x11027] <= 122 && tmp[0x11034] == 35 &&
                                 tmp[0x11035] == 16 && tmp[0x11036] == 0 && tmp[0x11037] == 1;
                    decodedImage = new byte[imageHeader.DataSize];
                    offsets = imageHeader.ImageFormat == SectorOrder.Dos
                                  ? isDos
                                        ? deinterleave
                                        : interleave
                                  : isDos
                                      ? interleave
                                      : deinterleave;
                    for(int t = 0; t < 35; t++)
                    {
                        for(int s = 0; s < 16; s++)
                            Array.Copy(tmp, t * 16 * 256 + s * 256, decodedImage, t * 16 * 256 + offsets[s] * 256, 256);
                    }

                    imageInfo.Sectors    = 560;
                    imageInfo.SectorSize = 256;
                    break;
                case SectorOrder.Dos when imageHeader.DataSize == 819200:
                    stream.Seek(imageHeader.DataOffset, SeekOrigin.Begin);
                    tmp = new byte[imageHeader.DataSize];
                    stream.Read(tmp, 0, tmp.Length);
                    decodedImage = new byte[imageHeader.DataSize];
                    offsets      = interleave;
                    for(int t = 0; t < 200; t++)
                    {
                        for(int s = 0; s < 16; s++)
                            Array.Copy(tmp, t * 16 * 256 + s * 256, decodedImage, t * 16 * 256 + offsets[s] * 256, 256);
                    }

                    imageInfo.Sectors    = 1600;
                    imageInfo.SectorSize = 512;
                    break;
                default:
                    decodedImage         = null;
                    imageInfo.SectorSize = 512;
                    imageInfo.Sectors    = imageHeader.DataSize / 512;
                    break;
            }

            imageInfo.ImageSize = imageHeader.DataSize;

            switch(imageHeader.Creator)
            {
                case CREATOR_ASIMOV:
                    imageInfo.Application = "ASIMOV2";
                    break;
                case CREATOR_BERNIE:
                    imageInfo.Application = "Bernie ][ the Rescue";
                    break;
                case CREATOR_CATAKIG:
                    imageInfo.Application = "Catakig";
                    break;
                case CREATOR_SHEPPY:
                    imageInfo.Application = "Sheppy's ImageMaker";
                    break;
                case CREATOR_SWEET:
                    imageInfo.Application = "Sweet16";
                    break;
                case CREATOR_XGS:
                    imageInfo.Application = "XGS";
                    break;
                case CREATOR_CIDER:
                    imageInfo.Application = "CiderPress";
                    break;
                case CREATOR_DIC:
                    imageInfo.Application = "DiscImageChef";
                    break;
                default:
                    imageInfo.Application = $"Unknown creator code \"{Encoding.ASCII.GetString(creator)}\"";
                    break;
            }

            imageInfo.Version = imageHeader.Version.ToString();

            if(imageHeader.CommentOffset != 0 && imageHeader.CommentSize != 0)
            {
                stream.Seek(imageHeader.CommentOffset, SeekOrigin.Begin);

                byte[] comments = new byte[imageHeader.CommentSize];
                stream.Read(comments, 0, (int)imageHeader.CommentSize);
                imageInfo.Comments = Encoding.ASCII.GetString(comments);
            }

            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.MediaType            = GetMediaType();

            a2MgImageFilter = imageFilter;

            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;

            DicConsole.VerboseWriteLine("2MG image contains a disk of type {0}", imageInfo.MediaType);
            if(!string.IsNullOrEmpty(imageInfo.Comments))
                DicConsole.VerboseWriteLine("2MG comments: {0}", imageInfo.Comments);

            switch(imageInfo.MediaType)
            {
                case MediaType.Apple32SS:
                    imageInfo.Cylinders       = 35;
                    imageInfo.Heads           = 1;
                    imageInfo.SectorsPerTrack = 13;
                    break;
                case MediaType.Apple32DS:
                    imageInfo.Cylinders       = 35;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 13;
                    break;
                case MediaType.Apple33SS:
                    imageInfo.Cylinders       = 35;
                    imageInfo.Heads           = 1;
                    imageInfo.SectorsPerTrack = 16;
                    break;
                case MediaType.Apple33DS:
                    imageInfo.Cylinders       = 35;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 16;
                    break;
                case MediaType.AppleSonySS:
                    imageInfo.Cylinders = 80;
                    imageInfo.Heads     = 1;
                    // Variable sectors per track, this suffices
                    imageInfo.SectorsPerTrack = 10;
                    break;
                case MediaType.AppleSonyDS:
                    imageInfo.Cylinders = 80;
                    imageInfo.Heads     = 2;
                    // Variable sectors per track, this suffices
                    imageInfo.SectorsPerTrack = 10;
                    break;
                case MediaType.DOS_35_HD:
                    imageInfo.Cylinders       = 80;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 18;
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

            if(decodedImage != null)
                Array.Copy(decodedImage, (long)(sectorAddress * imageInfo.SectorSize), buffer, 0,
                           length * imageInfo.SectorSize);
            else
            {
                Stream stream = a2MgImageFilter.GetDataForkStream();
                stream.Seek((long)(imageHeader.DataOffset + sectorAddress * imageInfo.SectorSize), SeekOrigin.Begin);
                stream.Read(buffer, 0, (int)(length * imageInfo.SectorSize));
            }

            return buffer;
        }
    }
}