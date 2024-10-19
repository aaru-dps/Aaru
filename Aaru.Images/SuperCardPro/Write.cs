// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Write.cs
// Author(s)      : Rebecca Wallander <sakcheen@gmail.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Writes SuperCardPro flux images.
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
// Copyright © 2011-2024 Rebecca Wallander
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.Console;

namespace Aaru.Images;

public sealed partial class SuperCardPro
{
#region IWritableFluxImage Members

    public ErrorNumber WriteFluxCapture(ulong  indexResolution, ulong dataResolution, byte[] indexBuffer,
                                        byte[] dataBuffer, uint head, ushort track, byte subTrack, uint captureIndex)
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return ErrorNumber.WriteError;
        }

        if(subTrack != 0) return ErrorNumber.NotSupported;

        Header.start = byte.Min((byte)HeadTrackSubToScpTrack(head, track, subTrack), Header.start);
        Header.end   = byte.Max((byte)HeadTrackSubToScpTrack(head, track, subTrack), Header.end);

        ulong scpResolution = dataResolution / DEFAULT_RESOLUTION - 1;

        if(!IsResolutionSet)
        {
            Header.resolution = (byte)scpResolution;
            IsResolutionSet   = true;
        }

        // SCP can only have one resolution for all tracks
        if(Header.resolution != scpResolution) return ErrorNumber.NotSupported;

        long scpTrack = HeadTrackSubToScpTrack(head, track, subTrack);

        _writingStream.Seek(0x10 + 4 * scpTrack, SeekOrigin.Begin);
        _writingStream.Write(BitConverter.GetBytes(_trackOffset), 0, 4);

        _writingStream.Seek(_trackOffset, SeekOrigin.Begin);
        _writingStream.Write(_trkSignature, 0, 3);
        _writingStream.WriteByte((byte)scpTrack);

        List<uint> scpIndices = FluxRepresentationsToUInt32List(indexBuffer);

        if(scpIndices[0] == 0)
        {
            // Stream starts at index
            Header.flags |= ScpFlags.StartsAtIndex;
            scpIndices.RemoveAt(0);
        }

        if(!IsRevolutionsSet)
        {
            Header.revolutions = (byte)scpIndices.Count;
            IsRevolutionsSet   = true;
        }

        // SCP can only have the same number of revolutions for all tracks
        if(Header.revolutions != scpIndices.Count) return ErrorNumber.NotSupported;

        List<byte> scpData = FluxRepresentationsToUInt16List(dataBuffer, scpIndices, out uint[] trackLengths);

        var offset = (uint)(4 + 12 * Header.revolutions);

        for(var i = 0; i < Header.revolutions; i++)
        {
            _writingStream.Write(BitConverter.GetBytes(scpIndices[i]),   0, 4);
            _writingStream.Write(BitConverter.GetBytes(trackLengths[i]), 0, 4);
            _writingStream.Write(BitConverter.GetBytes(offset),          0, 4);

            offset += trackLengths[i] * 2;
        }

        _writingStream.Write(scpData.ToArray(), 0, scpData.Count);
        _trackOffset = (uint)_writingStream.Position;

        return ErrorNumber.NoError;
    }

    public ErrorNumber WriteFluxIndexCapture(ulong resolution, byte[] index, uint head, ushort track, byte subTrack,
                                             uint  captureIndex) => ErrorNumber.NotImplemented;

    public ErrorNumber WriteFluxDataCapture(ulong resolution, byte[] data, uint head, ushort track, byte subTrack,
                                            uint  captureIndex) => ErrorNumber.NotImplemented;

#endregion

#region IWritableImage Members

    public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                       uint   sectorSize)
    {
        try
        {
            _writingStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        }
        catch(IOException ex)
        {
            ErrorMessage = string.Format(Localization.Could_not_create_new_image_file_exception_0, ex.Message);
            AaruConsole.WriteException(ex);

            return false;
        }

        _trackOffset = FULL_HEADER_OFFSET;

        IsWriting    = true;
        ErrorMessage = null;

        Header.signature = _scpSignature;

        Header.type = mediaType switch
                      {
                          MediaType.DOS_525_DS_DD_9 => ScpDiskType.PC360K,
                          MediaType.Unknown         => ScpDiskType.Generic720K,
                          _                         => Header.type
                      };

        return true;
    }

    public bool Close()
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Image_is_not_opened_for_writing;

            return false;
        }

        _writingStream.Seek(0, SeekOrigin.Begin);

        _writingStream.Write(Header.signature, 0, 3);
        _writingStream.WriteByte(Header.version);
        _writingStream.WriteByte((byte)Header.type);
        _writingStream.WriteByte(Header.revolutions);
        _writingStream.WriteByte(Header.start);
        _writingStream.WriteByte(Header.end);
        _writingStream.WriteByte((byte)Header.flags);
        _writingStream.WriteByte(Header.bitCellEncoding);
        _writingStream.WriteByte(Header.heads);
        _writingStream.WriteByte(Header.resolution);

        _writingStream.Seek(0, SeekOrigin.End);
        var date = DateTime.Now.ToString("G");
        _writingStream.Write(Encoding.ASCII.GetBytes(date), 0, date.Length);

        Header.checksum = CalculateChecksum(_writingStream);
        _writingStream.Seek(0x0C, SeekOrigin.Begin);
        _writingStream.Write(BitConverter.GetBytes(Header.checksum), 0, 4);

        _writingStream.Flush();
        _writingStream.Close();

        IsWriting    = false;
        ErrorMessage = "";

        return true;
    }

    public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack) => true;

    public bool WriteSectorTag(byte[] data, ulong sectorAddress, SectorTagType tag) => false;

    public bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag) => false;

    public bool SetDumpHardware(List<DumpHardware> dumpHardware) => false;

    public bool SetMetadata(Metadata metadata) => false;

    public bool WriteMediaTag(byte[] data, MediaTagType tag) => false;

    public bool WriteSector(byte[] data, ulong sectorAddress)
    {
        ErrorMessage = Localization.Flux_decoding_is_not_yet_implemented;

        return false;
    }

    public bool WriteSectors(byte[] data, ulong sectorAddress, uint length)
    {
        ErrorMessage = Localization.Flux_decoding_is_not_yet_implemented;

        return false;
    }

    /// <inheritdoc />
    public bool WriteSectorLong(byte[] data, ulong sectorAddress)
    {
        ErrorMessage = Localization.Flux_decoding_is_not_yet_implemented;

        return false;
    }

    /// <inheritdoc />
    public bool WriteSectorsLong(byte[] data, ulong sectorAddress, uint length)
    {
        ErrorMessage = Localization.Flux_decoding_is_not_yet_implemented;

        return false;
    }

    public bool SetImageInfo(ImageInfo imageInfo)
    {
        string[] version = imageInfo.ApplicationVersion.Split('.');

        Header.version         = (byte)((byte.Parse(version[0]) << 4) + byte.Parse(version[1]));
        Header.start           = byte.MaxValue;
        Header.end             = byte.MinValue;
        Header.bitCellEncoding = 0;
        Header.heads           = 0; // TODO: Support single sided disks

        return true;
    }

#endregion
}