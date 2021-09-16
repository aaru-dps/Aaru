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
// Copyright © 2011-2021 Natalia Portillo
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
    public sealed partial class Apple2Mg
    {
        /// <inheritdoc />
        public ErrorNumber Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            _imageHeader = new Header();

            byte[] header = new byte[64];
            stream.Read(header, 0, 64);
            byte[] magic   = new byte[4];
            byte[] creator = new byte[4];

            Array.Copy(header, 0, magic, 0, 4);
            Array.Copy(header, 4, creator, 0, 4);

            _imageHeader = Marshal.SpanToStructureLittleEndian<Header>(header);

            if(_imageHeader.DataSize == 0x00800C00)
            {
                _imageHeader.DataSize = 0x000C8000;
                AaruConsole.DebugWriteLine("2MG plugin", "Detected incorrect endian on data size field, correcting.");
            }

            AaruConsole.DebugWriteLine("2MG plugin", "ImageHeader.magic = \"{0}\"", Encoding.ASCII.GetString(magic));

            AaruConsole.DebugWriteLine("2MG plugin", "ImageHeader.creator = \"{0}\"",
                                       Encoding.ASCII.GetString(creator));

            AaruConsole.DebugWriteLine("2MG plugin", "ImageHeader.headerSize = {0}", _imageHeader.HeaderSize);
            AaruConsole.DebugWriteLine("2MG plugin", "ImageHeader.version = {0}", _imageHeader.Version);
            AaruConsole.DebugWriteLine("2MG plugin", "ImageHeader.imageFormat = {0}", _imageHeader.ImageFormat);
            AaruConsole.DebugWriteLine("2MG plugin", "ImageHeader.flags = 0x{0:X8}", _imageHeader.Flags);
            AaruConsole.DebugWriteLine("2MG plugin", "ImageHeader.blocks = {0}", _imageHeader.Blocks);
            AaruConsole.DebugWriteLine("2MG plugin", "ImageHeader.dataOffset = 0x{0:X8}", _imageHeader.DataOffset);
            AaruConsole.DebugWriteLine("2MG plugin", "ImageHeader.dataSize = {0}", _imageHeader.DataSize);

            AaruConsole.DebugWriteLine("2MG plugin", "ImageHeader.commentOffset = 0x{0:X8}",
                                       _imageHeader.CommentOffset);

            AaruConsole.DebugWriteLine("2MG plugin", "ImageHeader.commentSize = {0}", _imageHeader.CommentSize);

            AaruConsole.DebugWriteLine("2MG plugin", "ImageHeader.creatorSpecificOffset = 0x{0:X8}",
                                       _imageHeader.CreatorSpecificOffset);

            AaruConsole.DebugWriteLine("2MG plugin", "ImageHeader.creatorSpecificSize = {0}",
                                       _imageHeader.CreatorSpecificSize);

            AaruConsole.DebugWriteLine("2MG plugin", "ImageHeader.reserved1 = 0x{0:X8}", _imageHeader.Reserved1);
            AaruConsole.DebugWriteLine("2MG plugin", "ImageHeader.reserved2 = 0x{0:X8}", _imageHeader.Reserved2);
            AaruConsole.DebugWriteLine("2MG plugin", "ImageHeader.reserved3 = 0x{0:X8}", _imageHeader.Reserved3);
            AaruConsole.DebugWriteLine("2MG plugin", "ImageHeader.reserved4 = 0x{0:X8}", _imageHeader.Reserved4);

            if(_imageHeader.DataSize    == 0 &&
               _imageHeader.Blocks      == 0 &&
               _imageHeader.ImageFormat != SectorOrder.ProDos)
                return ErrorNumber.InvalidArgument;

            byte[] tmp;
            int[]  offsets;

            switch(_imageHeader.ImageFormat)
            {
                case SectorOrder.Nibbles:
                    tmp = new byte[_imageHeader.DataSize];
                    stream.Seek(_imageHeader.DataOffset, SeekOrigin.Begin);
                    stream.Read(tmp, 0, tmp.Length);
                    var nibPlugin = new AppleNib();
                    var noFilter  = new ZZZNoFilter();
                    noFilter.Open(tmp);
                    nibPlugin.Open(noFilter);
                    _decodedImage         = nibPlugin.ReadSectors(0, (uint)nibPlugin.Info.Sectors);
                    _imageInfo.Sectors    = nibPlugin.Info.Sectors;
                    _imageInfo.SectorSize = nibPlugin.Info.SectorSize;

                    break;
                case SectorOrder.Dos when _imageHeader.DataSize    == 143360:
                case SectorOrder.ProDos when _imageHeader.DataSize == 143360:
                    stream.Seek(_imageHeader.DataOffset, SeekOrigin.Begin);
                    tmp = new byte[_imageHeader.DataSize];
                    stream.Read(tmp, 0, tmp.Length);

                    bool isDos = tmp[0x11001] == 17 && tmp[0x11002] < 16 && tmp[0x11027] <= 122 && tmp[0x11034] == 35 &&
                                 tmp[0x11035] == 16 && tmp[0x11036] == 0 && tmp[0x11037] == 1;

                    _decodedImage = new byte[_imageHeader.DataSize];

                    offsets = _imageHeader.ImageFormat == SectorOrder.Dos
                                  ? isDos
                                        ? _deinterleave
                                        : _interleave
                                  : isDos
                                      ? _interleave
                                      : _deinterleave;

                    for(int t = 0; t < 35; t++)
                    {
                        for(int s = 0; s < 16; s++)
                            Array.Copy(tmp, (t * 16 * 256) + (s          * 256), _decodedImage,
                                       (t      * 16 * 256) + (offsets[s] * 256), 256);
                    }

                    _imageInfo.Sectors    = 560;
                    _imageInfo.SectorSize = 256;

                    break;
                case SectorOrder.Dos when _imageHeader.DataSize == 819200:
                    stream.Seek(_imageHeader.DataOffset, SeekOrigin.Begin);
                    tmp = new byte[_imageHeader.DataSize];
                    stream.Read(tmp, 0, tmp.Length);
                    _decodedImage = new byte[_imageHeader.DataSize];
                    offsets       = _interleave;

                    for(int t = 0; t < 200; t++)
                    {
                        for(int s = 0; s < 16; s++)
                            Array.Copy(tmp, (t * 16 * 256) + (s          * 256), _decodedImage,
                                       (t      * 16 * 256) + (offsets[s] * 256), 256);
                    }

                    _imageInfo.Sectors    = 1600;
                    _imageInfo.SectorSize = 512;

                    break;
                default:
                    _decodedImage         = null;
                    _imageInfo.SectorSize = 512;
                    _imageInfo.Sectors    = _imageHeader.DataSize / 512;

                    break;
            }

            _imageInfo.ImageSize = _imageHeader.DataSize;

            switch(_imageHeader.Creator)
            {
                case CREATOR_ASIMOV:
                    _imageInfo.Application = "ASIMOV2";

                    break;
                case CREATOR_BERNIE:
                    _imageInfo.Application = "Bernie ][ the Rescue";

                    break;
                case CREATOR_CATAKIG:
                    _imageInfo.Application = "Catakig";

                    break;
                case CREATOR_SHEPPY:
                    _imageInfo.Application = "Sheppy's ImageMaker";

                    break;
                case CREATOR_SWEET:
                    _imageInfo.Application = "Sweet16";

                    break;
                case CREATOR_XGS:
                    _imageInfo.Application = "XGS";

                    break;
                case CREATOR_CIDER:
                    _imageInfo.Application = "CiderPress";

                    break;
                case CREATOR_DIC:
                    _imageInfo.Application = "DiscImageChef";

                    break;
                case CREATOR_AARU:
                    _imageInfo.Application = "Aaru";

                    break;
                default:
                    _imageInfo.Application = $"Unknown creator code \"{Encoding.ASCII.GetString(creator)}\"";

                    break;
            }

            _imageInfo.Version = _imageHeader.Version.ToString();

            if(_imageHeader.CommentOffset != 0 &&
               _imageHeader.CommentSize   != 0)
            {
                stream.Seek(_imageHeader.CommentOffset, SeekOrigin.Begin);

                byte[] comments = new byte[_imageHeader.CommentSize];
                stream.Read(comments, 0, (int)_imageHeader.CommentSize);
                _imageInfo.Comments = Encoding.ASCII.GetString(comments);
            }

            _imageInfo.CreationTime         = imageFilter.CreationTime;
            _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
            _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.Filename);
            _imageInfo.MediaType            = GetMediaType();

            _a2MgImageFilter = imageFilter;

            _imageInfo.XmlMediaType = XmlMediaType.BlockMedia;

            AaruConsole.VerboseWriteLine("2MG image contains a disk of type {0}", _imageInfo.MediaType);

            if(!string.IsNullOrEmpty(_imageInfo.Comments))
                AaruConsole.VerboseWriteLine("2MG comments: {0}", _imageInfo.Comments);

            switch(_imageInfo.MediaType)
            {
                case MediaType.Apple32SS:
                    _imageInfo.Cylinders       = 35;
                    _imageInfo.Heads           = 1;
                    _imageInfo.SectorsPerTrack = 13;

                    break;
                case MediaType.Apple32DS:
                    _imageInfo.Cylinders       = 35;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 13;

                    break;
                case MediaType.Apple33SS:
                    _imageInfo.Cylinders       = 35;
                    _imageInfo.Heads           = 1;
                    _imageInfo.SectorsPerTrack = 16;

                    break;
                case MediaType.Apple33DS:
                    _imageInfo.Cylinders       = 35;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 16;

                    break;
                case MediaType.AppleSonySS:
                    _imageInfo.Cylinders = 80;
                    _imageInfo.Heads     = 1;

                    // Variable sectors per track, this suffices
                    _imageInfo.SectorsPerTrack = 10;

                    break;
                case MediaType.AppleSonyDS:
                    _imageInfo.Cylinders = 80;
                    _imageInfo.Heads     = 2;

                    // Variable sectors per track, this suffices
                    _imageInfo.SectorsPerTrack = 10;

                    break;
                case MediaType.DOS_35_HD:
                    _imageInfo.Cylinders       = 80;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 18;

                    break;
            }

            return ErrorNumber.NoError;
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

            byte[] buffer = new byte[length * _imageInfo.SectorSize];

            if(_decodedImage != null)
                Array.Copy(_decodedImage, (long)(sectorAddress * _imageInfo.SectorSize), buffer, 0,
                           length * _imageInfo.SectorSize);
            else
            {
                Stream stream = _a2MgImageFilter.GetDataForkStream();

                stream.Seek((long)(_imageHeader.DataOffset + (sectorAddress * _imageInfo.SectorSize)),
                            SeekOrigin.Begin);

                stream.Read(buffer, 0, (int)(length * _imageInfo.SectorSize));
            }

            return buffer;
        }
    }
}