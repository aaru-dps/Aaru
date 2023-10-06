// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Read.cs
// Author(s)      : Rebecca Wallander <sakcheen@gmail.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads A2R flux images.
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
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.Images;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public sealed partial class A2R
{
#region IFluxImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        _a2RStream = imageFilter.GetDataForkStream();
        _a2RStream.Seek(0, SeekOrigin.Begin);

        _a2RFilter = imageFilter;

        var hdr = new byte[Marshal.SizeOf<A2RHeader>()];
        _a2RStream.EnsureRead(hdr, 0, Marshal.SizeOf<A2RHeader>());

        _header = Marshal.ByteArrayToStructureLittleEndian<A2RHeader>(hdr);

        AaruConsole.DebugWriteLine(MODULE_NAME, "header.signature = \"{0}\"",
                                   StringHandlers.CToString(_header.signature));

        AaruConsole.DebugWriteLine(MODULE_NAME, "header.version = {0}",        _header.version);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.highBitTest = {0:X2}", _header.highBitTest);

        AaruConsole.DebugWriteLine(MODULE_NAME, "header.lineTest = {0:X2} {1:X2} {2:X2}", _header.lineTest[0],
                                   _header.lineTest[1], _header.lineTest[2]);

        var infoMagic = new byte[4];
        _a2RStream.EnsureRead(infoMagic, 0, 4);

        // There must be an INFO chunk after the header (at byte 16)
        if(!_infoChunkSignature.SequenceEqual(infoMagic))
            return ErrorNumber.UnrecognizedFormat;

        _a2RStream.Seek(-4, SeekOrigin.Current);

        switch(_header.version)
        {
            case 0x32:
            {
                var infoChnk = new byte[Marshal.SizeOf<InfoChunkV2>()];
                _a2RStream.EnsureRead(infoChnk, 0, Marshal.SizeOf<InfoChunkV2>());
                _infoChunkV2 = Marshal.ByteArrayToStructureLittleEndian<InfoChunkV2>(infoChnk);

                AaruConsole.DebugWriteLine(MODULE_NAME, "infoChunk.header.chunkId = \"{0}\"",
                                           StringHandlers.CToString(_infoChunkV2.header.chunkId));

                AaruConsole.DebugWriteLine(MODULE_NAME, "infoChunk.header.chunkSize = {0}",
                                           _infoChunkV2.header.chunkSize);

                AaruConsole.DebugWriteLine(MODULE_NAME, "infoChunk.version = {0}", _infoChunkV2.version);

                AaruConsole.DebugWriteLine(MODULE_NAME, "infoChunk.creator = \"{0}\"",
                                           StringHandlers.CToString(_infoChunkV2.creator).TrimEnd());

                AaruConsole.DebugWriteLine(MODULE_NAME, "infoChunk.diskType = {0}", _infoChunkV2.diskType);

                AaruConsole.DebugWriteLine(MODULE_NAME, "infoChunk.writeProtected = {0}", _infoChunkV2.writeProtected);

                AaruConsole.DebugWriteLine(MODULE_NAME, "infoChunk.synchronized = {0}", _infoChunkV2.synchronized);

                _imageInfo.Creator = Encoding.ASCII.GetString(_infoChunkV2.creator).TrimEnd();

                switch(_infoChunkV2.diskType)
                {
                    case A2RDiskType._35:
                        _imageInfo.Heads           = 2;
                        _imageInfo.Cylinders       = 80;
                        _imageInfo.MediaType       = MediaType.AppleSonyDS;
                        _imageInfo.SectorsPerTrack = 10;

                        break;
                    case A2RDiskType._525:
                        _imageInfo.Heads     = 1;
                        _imageInfo.Cylinders = 40;
                        _imageInfo.MediaType = MediaType.Apple32SS;

                        break;
                    default:
                        return ErrorNumber.OutOfRange;
                }

                break;
            }
            case 0x33:
            {
                var infoChk = new byte[Marshal.SizeOf<InfoChunkV3>()];
                _a2RStream.EnsureRead(infoChk, 0, Marshal.SizeOf<InfoChunkV3>());
                _infoChunkV3 = Marshal.ByteArrayToStructureLittleEndian<InfoChunkV3>(infoChk);

                AaruConsole.DebugWriteLine(MODULE_NAME, "infoChunk.header.chunkId = \"{0}\"",
                                           StringHandlers.CToString(_infoChunkV3.header.chunkId));

                AaruConsole.DebugWriteLine(MODULE_NAME, "infoChunk.header.chunkSize = {0}",
                                           _infoChunkV3.header.chunkSize);

                AaruConsole.DebugWriteLine(MODULE_NAME, "infoChunk.version = {0}", _infoChunkV3.version);

                AaruConsole.DebugWriteLine(MODULE_NAME, "infoChunk.creator = \"{0}\"",
                                           StringHandlers.CToString(_infoChunkV3.creator).TrimEnd());

                AaruConsole.DebugWriteLine(MODULE_NAME, "infoChunk.driveType = {0}", _infoChunkV3.driveType);

                AaruConsole.DebugWriteLine(MODULE_NAME, "infoChunk.writeProtected = {0}", _infoChunkV3.writeProtected);

                AaruConsole.DebugWriteLine(MODULE_NAME, "infoChunk.synchronized = {0}", _infoChunkV3.synchronized);

                AaruConsole.DebugWriteLine(MODULE_NAME, "infoChunk.hardSectorCount = {0}",
                                           _infoChunkV3.hardSectorCount);

                _imageInfo.Creator = Encoding.ASCII.GetString(_infoChunkV3.creator).TrimEnd();

                switch(_infoChunkV3.driveType)
                {
                    case A2rDriveType.SS_525_40trk_quarterStep:
                        _imageInfo.Heads     = 1;
                        _imageInfo.Cylinders = 40;
                        _imageInfo.MediaType = MediaType.Apple32SS;

                        break;
                    case A2rDriveType.DS_35_80trk_appleCLV:
                        _imageInfo.Heads           = 2;
                        _imageInfo.Cylinders       = 80;
                        _imageInfo.MediaType       = MediaType.AppleSonyDS;
                        _imageInfo.SectorsPerTrack = 10;

                        break;
                    case A2rDriveType.DS_525_80trk:
                        _imageInfo.Heads     = 2;
                        _imageInfo.Cylinders = 80;
                        _imageInfo.MediaType = MediaType.DOS_525_HD;

                        break;
                    case A2rDriveType.DS_525_40trk:
                        _imageInfo.Heads           = 2;
                        _imageInfo.Cylinders       = 40;
                        _imageInfo.MediaType       = MediaType.DOS_525_DS_DD_9;
                        _imageInfo.SectorsPerTrack = 9;

                        break;
                    case A2rDriveType.DS_35_80trk:
                        _imageInfo.Heads           = 2;
                        _imageInfo.Cylinders       = 80;
                        _imageInfo.MediaType       = MediaType.DOS_35_HD;
                        _imageInfo.SectorsPerTrack = 18;

                        break;
                    case A2rDriveType.DS_8:
                        _imageInfo.Heads     = 2;
                        _imageInfo.Cylinders = 40;

                        break;
                    default:
                        return ErrorNumber.OutOfRange;
                }

                break;
            }
        }

        _a2RCaptures = new List<StreamCapture>();

        while(_a2RStream.Position < _a2RStream.Length)
        {
            var chunkHdr = new byte[Marshal.SizeOf<ChunkHeader>()];
            _a2RStream.EnsureRead(chunkHdr, 0, Marshal.SizeOf<ChunkHeader>());
            ChunkHeader chunkHeader = Marshal.ByteArrayToStructureLittleEndian<ChunkHeader>(chunkHdr);
            _a2RStream.Seek(-Marshal.SizeOf<ChunkHeader>(), SeekOrigin.Current);

            switch(chunkHeader.chunkId)
            {
                case var rwcp when rwcp.SequenceEqual(_rwcpChunkSignature):
                    var rwcpBuffer = new byte[Marshal.SizeOf<RwcpChunkHeader>()];
                    _a2RStream.EnsureRead(rwcpBuffer, 0, Marshal.SizeOf<RwcpChunkHeader>());
                    RwcpChunkHeader rwcpChunk = Marshal.ByteArrayToStructureLittleEndian<RwcpChunkHeader>(rwcpBuffer);

                    while(_a2RStream.ReadByte() == 0x43)
                    {
                        var capture = new StreamCapture
                        {
                            mark        = 0x43,
                            captureType = (byte)_a2RStream.ReadByte()
                        };

                        var location = new byte[2];
                        _a2RStream.EnsureRead(location, 0, 2);
                        capture.location = BitConverter.ToUInt16(location);

                        A2RLocationToHeadTrackSub(capture.location,  _imageInfo.MediaType, out capture.head,
                                                  out capture.track, out capture.subTrack);

                        if(capture.head + 1 > _imageInfo.Heads)
                            _imageInfo.Heads = capture.head + 1;

                        if(capture.track + 1 > _imageInfo.Cylinders)
                            _imageInfo.Cylinders = (uint)(capture.track + 1);

                        capture.numberOfIndexSignals = (byte)_a2RStream.ReadByte();
                        capture.indexSignals         = new uint[capture.numberOfIndexSignals];

                        for(var i = 0; capture.numberOfIndexSignals > i; i++)
                        {
                            var index = new byte[4];
                            _a2RStream.EnsureRead(index, 0, 4);
                            capture.indexSignals[i] = BitConverter.ToUInt32(index);
                        }

                        var dataSize = new byte[4];
                        _a2RStream.EnsureRead(dataSize, 0, 4);
                        capture.captureDataSize = BitConverter.ToUInt32(dataSize);

                        capture.dataOffset = _a2RStream.Position;

                        capture.resolution = rwcpChunk.resolution;

                        _a2RCaptures.Add(capture);

                        _a2RStream.Seek(capture.captureDataSize, SeekOrigin.Current);
                    }

                    break;
                case var meta when meta.SequenceEqual(_metaChunkSignature):
                    _meta = new Dictionary<string, string>();

                    _a2RStream.Seek(Marshal.SizeOf<ChunkHeader>(), SeekOrigin.Current);

                    var metadataBuffer = new byte[chunkHeader.chunkSize];
                    _a2RStream.EnsureRead(metadataBuffer, 0, (int)chunkHeader.chunkSize);

                    string metaData = Encoding.UTF8.GetString(metadataBuffer);

                    string[] metaFields = metaData.Split('\n');

                    foreach(string[] keyValue in metaFields.Select(field => field.Split('\t')).
                                                            Where(keyValue => keyValue.Length == 2))
                        _meta.Add(keyValue[0], keyValue[1]);

                    if(_meta.TryGetValue("image_date", out string imageDate))
                        _imageInfo.CreationTime = DateTime.Parse(imageDate);

                    if(_meta.TryGetValue("title", out string title))
                        _imageInfo.MediaTitle = title;

                    break;
                case var slvd when slvd.SequenceEqual(_slvdChunkSignature):
                    return ErrorNumber.NotImplemented;
                case var strm when strm.SequenceEqual(_strmChunkSignature):
                    var strmBuffer = new byte[Marshal.SizeOf<ChunkHeader>()];
                    _a2RStream.EnsureRead(strmBuffer, 0, Marshal.SizeOf<ChunkHeader>());
                    ChunkHeader strmChunk = Marshal.ByteArrayToStructureLittleEndian<ChunkHeader>(strmBuffer);

                    long end = strmChunk.chunkSize + _a2RStream.Position - 1;

                    while(_a2RStream.Position < end)
                    {
                        var capture = new StreamCapture
                        {
                            indexSignals         = new uint[1],
                            location             = (byte)_a2RStream.ReadByte(),
                            captureType          = (byte)_a2RStream.ReadByte(),
                            resolution           = 125000,
                            numberOfIndexSignals = 1
                        };

                        A2RLocationToHeadTrackSub(capture.location,  _imageInfo.MediaType, out capture.head,
                                                  out capture.track, out capture.subTrack);

                        if(capture.head + 1 > _imageInfo.Heads)
                            _imageInfo.Heads = capture.head + 1;

                        if(capture.track + 1 > _imageInfo.Cylinders)
                            _imageInfo.Cylinders = (uint)(capture.track + 1);

                        var dataSize = new byte[4];
                        _a2RStream.EnsureRead(dataSize, 0, 4);
                        capture.captureDataSize = BitConverter.ToUInt32(dataSize);

                        var index = new byte[4];
                        _a2RStream.EnsureRead(index, 0, 4);
                        capture.indexSignals[0] = BitConverter.ToUInt32(index);

                        capture.dataOffset = _a2RStream.Position;

                        _a2RCaptures.Add(capture);

                        _a2RStream.Seek(capture.captureDataSize, SeekOrigin.Current);
                    }

                    _a2RStream.ReadByte();

                    break;
            }
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CapturesLength(uint head, ushort track, byte subTrack, out uint length)
    {
        long index = HeadTrackSubToA2RLocation(head, track, subTrack, _imageInfo.MediaType);

        length = (uint)_a2RCaptures.FindAll(capture => index == capture.location).Count;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFluxIndexResolution(uint      head, ushort track, byte subTrack, uint captureIndex,
                                               out ulong resolution)
    {
        resolution = StreamCaptureAtIndex(head, track, subTrack, captureIndex).resolution;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFluxDataResolution(uint      head, ushort track, byte subTrack, uint captureIndex,
                                              out ulong resolution)
    {
        resolution = StreamCaptureAtIndex(head, track, subTrack, captureIndex).resolution;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFluxResolution(uint      head,            ushort    track, byte subTrack, uint captureIndex,
                                          out ulong indexResolution, out ulong dataResolution)
    {
        indexResolution = dataResolution = StreamCaptureAtIndex(head, track, subTrack, captureIndex).resolution;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFluxCapture(uint       head,            ushort    track, byte subTrack, uint captureIndex,
                                       out ulong  indexResolution, out ulong dataResolution, out byte[] indexBuffer,
                                       out byte[] dataBuffer)
    {
        dataBuffer = indexBuffer = null;

        ErrorNumber error =
            ReadFluxResolution(head, track, subTrack, captureIndex, out indexResolution, out dataResolution);

        if(error != ErrorNumber.NoError)
            return error;

        error = ReadFluxDataCapture(head, track, subTrack, captureIndex, out dataBuffer);

        if(error != ErrorNumber.NoError)
            return error;

        error = ReadFluxIndexCapture(head, track, subTrack, captureIndex, out indexBuffer);

        return error;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFluxIndexCapture(uint       head, ushort track, byte subTrack, uint captureIndex,
                                            out byte[] buffer)
    {
        buffer = null;

        List<byte> tmpBuffer = new()
        {
            // A2R always starts at index signal
            0
        };

        StreamCapture capture = StreamCaptureAtIndex(head, track, subTrack, captureIndex);

        uint previousTicks = 0;

        for(var i = 0; i < capture.numberOfIndexSignals; i++)
        {
            uint ticks = capture.indexSignals[i] - previousTicks;
            tmpBuffer.AddRange(UInt32ToFluxRepresentation(ticks));

            previousTicks = capture.indexSignals[i];
        }

        buffer = tmpBuffer.ToArray();

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFluxDataCapture(uint head, ushort track, byte subTrack, uint captureIndex, out byte[] buffer)
    {
        buffer = null;

        StreamCapture capture = StreamCaptureAtIndex(head, track, subTrack, captureIndex);

        if(capture.captureType == 2)
            return ErrorNumber.NotImplemented;

        Stream stream = _a2RFilter.GetDataForkStream();
        var    br     = new BinaryReader(stream);

        br.BaseStream.Seek(capture.dataOffset, SeekOrigin.Begin);
        buffer = br.ReadBytes((int)capture.captureDataSize);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber SubTrackLength(uint head, ushort track, out byte length)
    {
        length = 0;

        List<StreamCapture> captures = _a2RCaptures.FindAll(c => c.head == head && c.track == track);

        if(captures.Count <= 0)
            return ErrorNumber.OutOfRange;

        length = (byte)(captures.Max(static c => c.subTrack) + 1);

        return ErrorNumber.NoError;
    }

#endregion

#region IMediaImage Members

    /// <inheritdoc />
    public ErrorNumber ReadMediaTag(MediaTagType tag, out byte[] buffer) => throw new NotImplementedException();

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer) => throw new NotImplementedException();

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong sectorAddress, out byte[] buffer) => throw new NotImplementedException();

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong sectorAddress, uint length, out byte[] buffer) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong sectorAddress, uint length, out byte[] buffer) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag, out byte[] buffer) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, SectorTagType tag, out byte[] buffer) =>
        throw new NotImplementedException();

#endregion

    StreamCapture StreamCaptureAtIndex(uint head, ushort track, byte subTrack, uint captureIndex)
    {
        long index = HeadTrackSubToA2RLocation(head, track, subTrack, _imageInfo.MediaType);

        return _a2RCaptures.FindAll(capture => index == capture.location)[(int)captureIndex];
    }
}