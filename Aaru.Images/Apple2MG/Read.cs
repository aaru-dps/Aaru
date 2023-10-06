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
// Copyright © 2011-2023 Natalia Portillo
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

namespace Aaru.Images;

public sealed partial class Apple2Mg
{
#region IWritableImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        _imageHeader = new Header();

        var header = new byte[64];
        stream.EnsureRead(header, 0, 64);
        var magic   = new byte[4];
        var creator = new byte[4];

        Array.Copy(header, 0, magic,   0, 4);
        Array.Copy(header, 4, creator, 0, 4);

        _imageHeader = Marshal.SpanToStructureLittleEndian<Header>(header);

        if(_imageHeader.DataSize == 0x00800C00)
        {
            _imageHeader.DataSize = 0x000C8000;

            AaruConsole.DebugWriteLine(MODULE_NAME,
                                       Localization.Detected_incorrect_endian_on_data_size_field_correcting);
        }

        AaruConsole.DebugWriteLine(MODULE_NAME, "ImageHeader.magic = \"{0}\"", Encoding.ASCII.GetString(magic));

        AaruConsole.DebugWriteLine(MODULE_NAME, "ImageHeader.creator = \"{0}\"", Encoding.ASCII.GetString(creator));

        AaruConsole.DebugWriteLine(MODULE_NAME, "ImageHeader.headerSize = {0}",      _imageHeader.HeaderSize);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ImageHeader.version = {0}",         _imageHeader.Version);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ImageHeader.imageFormat = {0}",     _imageHeader.ImageFormat);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ImageHeader.flags = 0x{0:X8}",      _imageHeader.Flags);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ImageHeader.blocks = {0}",          _imageHeader.Blocks);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ImageHeader.dataOffset = 0x{0:X8}", _imageHeader.DataOffset);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ImageHeader.dataSize = {0}",        _imageHeader.DataSize);

        AaruConsole.DebugWriteLine(MODULE_NAME, "ImageHeader.commentOffset = 0x{0:X8}", _imageHeader.CommentOffset);

        AaruConsole.DebugWriteLine(MODULE_NAME, "ImageHeader.commentSize = {0}", _imageHeader.CommentSize);

        AaruConsole.DebugWriteLine(MODULE_NAME, "ImageHeader.creatorSpecificOffset = 0x{0:X8}",
                                   _imageHeader.CreatorSpecificOffset);

        AaruConsole.DebugWriteLine(MODULE_NAME, "ImageHeader.creatorSpecificSize = {0}",
                                   _imageHeader.CreatorSpecificSize);

        AaruConsole.DebugWriteLine(MODULE_NAME, "ImageHeader.reserved1 = 0x{0:X8}", _imageHeader.Reserved1);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ImageHeader.reserved2 = 0x{0:X8}", _imageHeader.Reserved2);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ImageHeader.reserved3 = 0x{0:X8}", _imageHeader.Reserved3);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ImageHeader.reserved4 = 0x{0:X8}", _imageHeader.Reserved4);

        if(_imageHeader is { DataSize: 0, Blocks: 0 } && _imageHeader.ImageFormat != SectorOrder.ProDos)
            return ErrorNumber.InvalidArgument;

        byte[] tmp;
        int[]  offsets;

        switch(_imageHeader.ImageFormat)
        {
            case SectorOrder.Nibbles:
                tmp = new byte[_imageHeader.DataSize];
                stream.Seek(_imageHeader.DataOffset, SeekOrigin.Begin);
                stream.EnsureRead(tmp, 0, tmp.Length);
                var nibPlugin = new AppleNib();
                var noFilter  = new ZZZNoFilter();
                noFilter.Open(tmp);
                nibPlugin.Open(noFilter);
                ErrorNumber errno = nibPlugin.ReadSectors(0, (uint)nibPlugin.Info.Sectors, out _decodedImage);

                if(errno != ErrorNumber.NoError)
                    return errno;

                _imageInfo.Sectors    = nibPlugin.Info.Sectors;
                _imageInfo.SectorSize = nibPlugin.Info.SectorSize;

                break;
            case SectorOrder.Dos when _imageHeader.DataSize    == 143360:
            case SectorOrder.ProDos when _imageHeader.DataSize == 143360:
                stream.Seek(_imageHeader.DataOffset, SeekOrigin.Begin);
                tmp = new byte[_imageHeader.DataSize];
                stream.EnsureRead(tmp, 0, tmp.Length);

                bool isDos = tmp[0x11001] == 17  &&
                             tmp[0x11002] < 16   &&
                             tmp[0x11027] <= 122 &&
                             tmp[0x11034] == 35  &&
                             tmp[0x11035] == 16  &&
                             tmp[0x11036] == 0   &&
                             tmp[0x11037] == 1;

                _decodedImage = new byte[_imageHeader.DataSize];

                offsets = _imageHeader.ImageFormat == SectorOrder.Dos ? isDos ? _deinterleave : _interleave :
                          isDos                                       ? _interleave : _deinterleave;

                for(var t = 0; t < 35; t++)
                {
                    for(var s = 0; s < 16; s++)
                        Array.Copy(tmp, t * 16 * 256 + s * 256, _decodedImage, t * 16 * 256 + offsets[s] * 256, 256);
                }

                _imageInfo.Sectors    = 560;
                _imageInfo.SectorSize = 256;

                break;
            case SectorOrder.Dos when _imageHeader.DataSize == 819200:
                stream.Seek(_imageHeader.DataOffset, SeekOrigin.Begin);
                tmp = new byte[_imageHeader.DataSize];
                stream.EnsureRead(tmp, 0, tmp.Length);
                _decodedImage = new byte[_imageHeader.DataSize];
                offsets       = _interleave;

                for(var t = 0; t < 200; t++)
                {
                    for(var s = 0; s < 16; s++)
                        Array.Copy(tmp, t * 16 * 256 + s * 256, _decodedImage, t * 16 * 256 + offsets[s] * 256, 256);
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

        _imageInfo.Application = _imageHeader.Creator switch
                                 {
                                     CREATOR_ASIMOV  => "ASIMOV2",
                                     CREATOR_BERNIE  => "Bernie ][ the Rescue",
                                     CREATOR_CATAKIG => "Catakig",
                                     CREATOR_SHEPPY  => "Sheppy's ImageMaker",
                                     CREATOR_SWEET   => "Sweet16",
                                     CREATOR_XGS     => "XGS",
                                     CREATOR_CIDER   => "CiderPress",
                                     CREATOR_DIC     => "DiscImageChef",
                                     CREATOR_AARU    => "Aaru",
                                     _ => string.Format(Localization.Unknown_creator_code_0,
                                                        Encoding.ASCII.GetString(creator))
                                 };

        _imageInfo.Version = _imageHeader.Version.ToString();

        if(_imageHeader.CommentOffset != 0 && _imageHeader.CommentSize != 0)
        {
            stream.Seek(_imageHeader.CommentOffset, SeekOrigin.Begin);

            var comments = new byte[_imageHeader.CommentSize];
            stream.EnsureRead(comments, 0, (int)_imageHeader.CommentSize);
            _imageInfo.Comments = Encoding.ASCII.GetString(comments);
        }

        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.Filename);
        _imageInfo.MediaType            = GetMediaType();

        _a2MgImageFilter = imageFilter;

        _imageInfo.MetadataMediaType = MetadataMediaType.BlockMedia;

        AaruConsole.VerboseWriteLine(Localization._2MG_image_contains_a_disk_of_type_0, _imageInfo.MediaType);

        if(!string.IsNullOrEmpty(_imageInfo.Comments))
            AaruConsole.VerboseWriteLine(Localization._2MG_comments_0, _imageInfo.Comments);

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
    public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer) => ReadSectors(sectorAddress, 1, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > _imageInfo.Sectors - 1)
            return ErrorNumber.OutOfRange;

        if(sectorAddress + length > _imageInfo.Sectors)
            return ErrorNumber.OutOfRange;

        buffer = new byte[length * _imageInfo.SectorSize];

        if(_decodedImage != null)
        {
            Array.Copy(_decodedImage, (long)(sectorAddress * _imageInfo.SectorSize), buffer, 0,
                       length * _imageInfo.SectorSize);
        }
        else
        {
            Stream stream = _a2MgImageFilter.GetDataForkStream();

            stream.Seek((long)(_imageHeader.DataOffset + sectorAddress * _imageInfo.SectorSize), SeekOrigin.Begin);

            stream.EnsureRead(buffer, 0, (int)(length * _imageInfo.SectorSize));
        }

        return ErrorNumber.NoError;
    }

#endregion
}