// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Write.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Writes CopyTape tape images.
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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Schemas;

namespace Aaru.DiscImages;

public sealed partial class CopyTape
{
    FileStream               _dataStream;
    ulong                    _lastWrittenBlock;
    Dictionary<ulong, ulong> _writtenBlockPositions;

    /// <inheritdoc />
    public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                       uint sectorSize)
    {
        if(!SupportedMediaTypes.Contains(mediaType))
        {
            ErrorMessage = string.Format(Localization.Unsupported_media_format_0, mediaType);

            return false;
        }

        _imageInfo = new ImageInfo
        {
            MediaType  = mediaType,
            SectorSize = sectorSize,
            Sectors    = sectors
        };

        try
        {
            _dataStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        }
        catch(IOException e)
        {
            ErrorMessage = string.Format(Localization.Could_not_create_new_image_file_exception_0, e.Message);

            return false;
        }

        _imageInfo.MediaType = mediaType;

        IsWriting              = true;
        IsTape                 = false;
        ErrorMessage           = null;
        _lastWrittenBlock      = 0;
        _writtenBlockPositions = new Dictionary<ulong, ulong>();

        return true;
    }

    /// <inheritdoc />
    public bool WriteMediaTag(byte[] data, MediaTagType tag)
    {
        ErrorMessage = Localization.Unsupported_feature;

        return false;
    }

    /// <inheritdoc />
    public bool WriteSector(byte[] data, ulong sectorAddress)
    {
        if(!_writtenBlockPositions.TryGetValue(sectorAddress, out ulong position))
        {
            if(_dataStream.Length != 0 &&
               _lastWrittenBlock  >= sectorAddress)
            {
                ErrorMessage = Localization.Cannot_write_unwritten_blocks;

                return false;
            }

            if(_lastWrittenBlock + 1 != sectorAddress &&
               sectorAddress         != 0             &&
               _lastWrittenBlock     != 0)
            {
                ErrorMessage = Localization.Cannot_skip_blocks;

                return false;
            }
        }
        else
            _dataStream.Position = (long)position;

        byte[] header = Encoding.ASCII.GetBytes($"CPTP:BLK {data.Length:D6}\n");

        _writtenBlockPositions[sectorAddress] = (ulong)_dataStream.Position;
        _dataStream.Write(header, 0, header.Length);
        _dataStream.Write(data, 0, data.Length);
        _dataStream.WriteByte(0x0A);

        if(sectorAddress > _lastWrittenBlock)
            _lastWrittenBlock = sectorAddress;

        _dataStream.Seek(0, SeekOrigin.End);

        return true;
    }

    /// <inheritdoc />
    public bool WriteSectors(byte[] data, ulong sectorAddress, uint length)
    {
        for(uint i = 0; i < length; i++)
        {
            bool ret = WriteSector(data, sectorAddress + i);

            if(!ret)
                return false;
        }

        return true;
    }

    /// <inheritdoc />
    public bool WriteSectorLong(byte[] data, ulong sectorAddress)
    {
        ErrorMessage = Localization.Unsupported_feature;

        return false;
    }

    /// <inheritdoc />
    public bool WriteSectorsLong(byte[] data, ulong sectorAddress, uint length)
    {
        ErrorMessage = Localization.Unsupported_feature;

        return false;
    }

    /// <inheritdoc />
    public bool Close()
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Image_is_not_opened_for_writing;

            return false;
        }

        byte[] footer = Encoding.ASCII.GetBytes("CPTP:EOT\n");

        _dataStream.Write(footer, 0, footer.Length);
        _dataStream.Flush();
        _dataStream.Close();

        IsWriting = false;

        return true;
    }

    /// <inheritdoc />
    public bool SetMetadata(ImageInfo metadata) => true;

    /// <inheritdoc />
    public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack)
    {
        ErrorMessage = Localization.Unsupported_feature;

        return false;
    }

    /// <inheritdoc />
    public bool WriteSectorTag(byte[] data, ulong sectorAddress, SectorTagType tag)
    {
        ErrorMessage = Localization.Unsupported_feature;

        return false;
    }

    /// <inheritdoc />
    public bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag)
    {
        ErrorMessage = Localization.Unsupported_feature;

        return false;
    }

    /// <inheritdoc />
    public bool SetDumpHardware(List<DumpHardwareType> dumpHardware) => false;

    /// <inheritdoc />
    public bool SetCicmMetadata(CICMMetadataType metadata) => false;

    /// <inheritdoc />
    public bool AddFile(TapeFile file)
    {
        if(file.Partition != 0)
        {
            ErrorMessage = Localization.Unsupported_feature;

            return false;
        }

        byte[] marker = Encoding.ASCII.GetBytes("CPTP:MRK\n");

        _dataStream.Write(marker, 0, marker.Length);

        return true;
    }

    /// <inheritdoc />
    public bool AddPartition(TapePartition partition)
    {
        if(partition.Number == 0)
            return true;

        ErrorMessage = Localization.Unsupported_feature;

        return false;
    }

    /// <inheritdoc />
    public bool SetTape()
    {
        IsTape = true;

        return true;
    }
}