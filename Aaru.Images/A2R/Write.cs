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
//     Writes A2R flux images.
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
// Copyright © 2011-2023 Rebecca Wallander
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.Images;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public sealed partial class A2R
{
#region IWritableFluxImage Members

    /// <inheritdoc />
    public ErrorNumber WriteFluxCapture(ulong  indexResolution, ulong dataResolution, byte[] indexBuffer,
                                        byte[] dataBuffer, uint head, ushort track, byte subTrack, uint captureIndex)
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return ErrorNumber.WriteError;
        }

        // An RWCP chunk can only have one capture resolution. If the resolution changes we need to create a new chunk.
        if(_currentResolution != dataResolution)
        {
            if(IsWritingRwcps)
            {
                CloseRwcpChunk();

                _writingStream.Seek(_currentRwcpStart, SeekOrigin.Begin);
                WriteRwcpHeader();

                _currentRwcpStart     = _writingStream.Length;
                _currentCaptureOffset = 16;
            }

            IsWritingRwcps = true;

            _currentResolution = (uint)dataResolution;
        }

        _writingStream.Seek(_currentRwcpStart + _currentCaptureOffset + Marshal.SizeOf<ChunkHeader>(),
                            SeekOrigin.Begin);

        _writingStream.WriteByte(0x43);

        _writingStream.WriteByte(IsCaptureTypeTiming(dataResolution, dataBuffer) ? (byte)1 : (byte)3);

        _writingStream.Write(BitConverter.GetBytes((ushort)HeadTrackSubToA2RLocation(head,
                                                       track,
                                                       subTrack,
                                                       _infoChunkV3.driveType)),
                             0,
                             2);

        List<uint> a2RIndices = FluxRepresentationsToUInt32List(indexBuffer);

        if(a2RIndices[0] == 0) a2RIndices.RemoveAt(0);

        _writingStream.WriteByte((byte)a2RIndices.Count);

        long previousIndex = 0;

        foreach(uint index in a2RIndices)
        {
            _writingStream.Write(BitConverter.GetBytes(index + previousIndex), 0, 4);
            previousIndex += index;
        }

        _writingStream.Write(BitConverter.GetBytes(dataBuffer.Length), 0, 4);
        _writingStream.Write(dataBuffer,                               0, dataBuffer.Length);

        _currentCaptureOffset += (uint)(9 + a2RIndices.Count * 4 + dataBuffer.Length);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber WriteFluxIndexCapture(ulong resolution, byte[] index, uint head, ushort track, byte subTrack,
                                             uint  captureIndex) => ErrorNumber.NoError;

    /// <inheritdoc />
    public ErrorNumber WriteFluxDataCapture(ulong resolution, byte[] data, uint head, ushort track, byte subTrack,
                                            uint  captureIndex) => ErrorNumber.NoError;

#endregion

#region IWritableImage Members

    /// <inheritdoc />
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

        IsWriting    = true;
        ErrorMessage = null;

        _header.signature   = "A2R"u8.ToArray();
        _header.version     = 0x33;
        _header.highBitTest = 0xFF;
        _header.lineTest    = "\n\r\n"u8.ToArray();

        _infoChunkV3.driveType = mediaType switch
                                 {
                                     MediaType.DOS_525_DS_DD_9 => A2rDriveType.DS_525_40trk,
                                     MediaType.Apple32SS       => A2rDriveType.SS_525_40trk_quarterStep,
                                     MediaType.Unknown         => A2rDriveType.DS_35_80trk,
                                     _                         => _infoChunkV3.driveType
                                 };

        return true;
    }

    /// <inheritdoc />
    public bool Close()
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Image_is_not_opened_for_writing;

            return false;
        }

        _writingStream.Seek(0, SeekOrigin.Begin);

        _writingStream.Write(_header.signature, 0, 3);
        _writingStream.WriteByte(_header.version);
        _writingStream.WriteByte(_header.highBitTest);
        _writingStream.Write(_header.lineTest, 0, 3);

        // First chunk needs to be an INFO chunk
        WriteInfoChunk();

        _writingStream.Seek(_currentRwcpStart, SeekOrigin.Begin);

        WriteRwcpHeader();

        _writingStream.Seek(0, SeekOrigin.End);
        CloseRwcpChunk();

        WriteMetaChunk();

        _writingStream.Flush();
        _writingStream.Close();

        IsWriting    = false;
        ErrorMessage = "";

        return true;
    }

    /// <inheritdoc />
    public bool SetImageInfo(ImageInfo imageInfo)
    {
        _meta = new Dictionary<string, string>();

        _infoChunkV3.header.chunkId   = _infoChunkSignature;
        _infoChunkV3.header.chunkSize = 37;
        _infoChunkV3.version          = 1;

        _infoChunkV3.creator =
            Encoding.UTF8.GetBytes($"Aaru v{typeof(A2R).Assembly.GetName().Version?.ToString()}".PadRight(32, ' '));

        _infoChunkV3.writeProtected  = 1;
        _infoChunkV3.synchronized    = 1;
        _infoChunkV3.hardSectorCount = 0;

        _meta.Add("image_date", DateTime.Now.ToString("O"));
        _meta.Add("title",      imageInfo.MediaTitle);

        return true;
    }

    /// <inheritdoc />
    public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack) => true;

    /// <inheritdoc />
    public bool WriteSectorTag(byte[] data, ulong sectorAddress, SectorTagType tag) => false;

    /// <inheritdoc />
    public bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag) => false;

    /// <inheritdoc />
    public bool SetDumpHardware(List<DumpHardware> dumpHardware) => false;

    /// <inheritdoc />
    public bool SetMetadata(Metadata metadata) => false;

    /// <inheritdoc />
    public bool WriteMediaTag(byte[] data, MediaTagType tag) => false;

    /// <inheritdoc />
    public bool WriteSector(byte[] data, ulong sectorAddress) => throw new NotImplementedException();

    /// <inheritdoc />
    public bool WriteSectorLong(byte[] data, ulong sectorAddress) => throw new NotImplementedException();

    /// <inheritdoc />
    public bool WriteSectors(byte[] data, ulong sectorAddress, uint length) => throw new NotImplementedException();

    /// <inheritdoc />
    public bool WriteSectorsLong(byte[] data, ulong sectorAddress, uint length) => throw new NotImplementedException();

#endregion

    /// <summary>
    ///     writes the header to an RWCP chunk, up to and including the reserved bytes, to stream.
    /// </summary>
    /// <returns></returns>
    ErrorNumber WriteRwcpHeader()
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return ErrorNumber.WriteError;
        }

        _writingStream.Write(_rwcpChunkSignature,                              0, 4);
        _writingStream.Write(BitConverter.GetBytes(_currentCaptureOffset + 1), 0, 4);
        _writingStream.WriteByte(1);
        _writingStream.Write(BitConverter.GetBytes(_currentResolution), 0, 4);

        byte[] reserved =
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        };

        _writingStream.Write(reserved, 0, 11);

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Writes the entire INFO chunk to stream.
    /// </summary>
    /// <returns></returns>
    ErrorNumber WriteInfoChunk()
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return ErrorNumber.WriteError;
        }

        _writingStream.Write(_infoChunkV3.header.chunkId,                          0, 4);
        _writingStream.Write(BitConverter.GetBytes(_infoChunkV3.header.chunkSize), 0, 4);
        _writingStream.WriteByte(_infoChunkV3.version);
        _writingStream.Write(_infoChunkV3.creator, 0, 32);
        _writingStream.WriteByte((byte)_infoChunkV3.driveType);
        _writingStream.WriteByte(_infoChunkV3.writeProtected);
        _writingStream.WriteByte(_infoChunkV3.synchronized);
        _writingStream.WriteByte(_infoChunkV3.hardSectorCount);

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Writes the entire META chunk to stream.
    /// </summary>
    /// <returns></returns>
    ErrorNumber WriteMetaChunk()
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return ErrorNumber.WriteError;
        }

        _writingStream.Write(_metaChunkSignature, 0, 4);

        byte[] metaString = Encoding.UTF8.GetBytes(_meta.Select(static m => $"{m.Key}\t{m.Value}")
                                                        .Aggregate(static (concat, str) => $"{concat}\n{str}") +
                                                   '\n');

        _writingStream.Write(BitConverter.GetBytes((uint)metaString.Length), 0, 4);
        _writingStream.Write(metaString,                                     0, metaString.Length);

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Writes the closing byte to an RWCP chunk signaling its end, to stream.
    /// </summary>
    /// <returns></returns>
    ErrorNumber CloseRwcpChunk()
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return ErrorNumber.WriteError;
        }

        _writingStream.WriteByte(0x58);

        return ErrorNumber.NoError;
    }
}