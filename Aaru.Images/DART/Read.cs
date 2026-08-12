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
//     Reads Apple Disk Archival/Retrieval Tool format.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Text.RegularExpressions;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Compression.Apple;
using Aaru.Helpers;
using Aaru.Logging;
using Claunia.Encoding;
using Claunia.RsrcFork;
using Sentry;
using Version = Resources.Version;

namespace Aaru.Images;

public sealed partial class Dart
{
#region IMediaImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();

        if(stream.Length < 84) return ErrorNumber.InvalidArgument;

        stream.Seek(0, SeekOrigin.Begin);
        var headerB = new byte[Marshal.SizeOf<Header>()];

        stream.EnsureRead(headerB, 0, Marshal.SizeOf<Header>());
        Header header = Marshal.ByteArrayToStructureBigEndian<Header>(headerB);

        if(header.srcCmp > COMPRESS_NONE) return ErrorNumber.NotSupported;

        int expectedMaxSize = 84 + header.srcSize * 2 * 524;

        switch(header.srcType)
        {
            case DISK_MAC:
                if(header.srcSize != SIZE_MAC_SS && header.srcSize != SIZE_MAC) return ErrorNumber.InvalidArgument;

                break;
            case DISK_LISA:
                if(header.srcSize != SIZE_LISA) return ErrorNumber.InvalidArgument;

                break;
            case DISK_APPLE2:
                if(header.srcSize != SIZE_APPLE2) return ErrorNumber.InvalidArgument;

                break;
            case DISK_MAC_HD:
                if(header.srcSize != SIZE_MAC_HD) return ErrorNumber.InvalidArgument;

                expectedMaxSize += 64;

                break;
            case DISK_DOS:
                if(header.srcSize != SIZE_DOS) return ErrorNumber.InvalidArgument;

                break;
            case DISK_DOS_HD:
                if(header.srcSize != SIZE_DOS_HD) return ErrorNumber.InvalidArgument;

                expectedMaxSize += 64;

                break;
            default:
                return ErrorNumber.InvalidArgument;
        }

        if(stream.Length > expectedMaxSize) return ErrorNumber.InvalidArgument;

        var bLength =
            new short[header.srcType is DISK_MAC_HD or DISK_DOS_HD ? BLOCK_ARRAY_LEN_HIGH : BLOCK_ARRAY_LEN_LOW];

        for(var i = 0; i < bLength.Length; i++)
        {
            var tmpShort = new byte[2];
            stream.EnsureRead(tmpShort, 0, 2);
            bLength[i] = BigEndianBitConverter.ToInt16(tmpShort, 0);
        }

        var dataMs = new MemoryStream();
        var tagMs  = new MemoryStream();

        foreach(short l in bLength)
        {
            if(l == 0) continue;

            var buffer = new byte[BUFFER_SIZE];

            if(l == -1)
                stream.EnsureRead(buffer, 0, BUFFER_SIZE);
            else
            {
                byte[] temp;

                if(header.srcCmp == COMPRESS_RLE)
                {
                    temp = new byte[l * 2];
                    stream.EnsureRead(temp, 0, temp.Length);
                    buffer = new byte[BUFFER_SIZE];

                    Rle.DecodeBuffer(temp, buffer);
                }
                else
                {
                    temp = new byte[l * 2];
                    stream.EnsureRead(temp, 0, temp.Length);
                    buffer = new byte[BUFFER_SIZE];

                    Lzh.DecodeBuffer(temp, buffer);
                }
            }

            dataMs.Write(buffer, 0, DATA_SIZE);
            tagMs.Write(buffer, DATA_SIZE, TAG_SIZE);
        }

        _dataCache = dataMs.ToArray();

        if(header.srcType is DISK_LISA or DISK_MAC or DISK_APPLE2)
        {
            _imageInfo.ReadableSectorTags.Add(SectorTagType.AppleSonyTag);
            _tagCache = tagMs.ToArray();
        }

        try
        {
            if(imageFilter.HasResourceFork)
            {
                var rsrcFork = new ResourceFork(imageFilter.GetResourceForkStream());

                // "vers"
                if(rsrcFork.ContainsKey(0x76657273))
                {
                    Resource versRsrc = rsrcFork.GetResource(0x76657273);

                    byte[] vers = versRsrc?.GetResource(versRsrc.GetIds()[0]);

                    if(vers != null)
                    {
                        var version = new Version(vers);

                        string release = null;
                        string pre     = null;

                        var major = $"{version.MajorVersion}";
                        var minor = $".{version.MinorVersion / 10}";

                        if(version.MinorVersion % 10 > 0) release = $".{version.MinorVersion % 10}";

                        string dev = version.DevStage switch
                                     {
                                         Version.DevelopmentStage.Alpha    => "a",
                                         Version.DevelopmentStage.Beta     => "b",
                                         Version.DevelopmentStage.PreAlpha => "d",
                                         _                                 => null
                                     };

                        if(dev == null && version.PreReleaseVersion > 0) dev = "f";

                        if(dev != null) pre = $"{version.PreReleaseVersion}";

                        _imageInfo.ApplicationVersion = $"{major}{minor}{release}{dev}{pre}";
                        _imageInfo.Application        = version.VersionString;
                        _imageInfo.Comments           = version.VersionMessage;
                    }
                }

                // "dart"
                if(rsrcFork.ContainsKey(0x44415254))
                {
                    Resource dartRsrc = rsrcFork.GetResource(0x44415254);

                    if(dartRsrc != null)
                    {
                        string dArt = StringHandlers.PascalToString(dartRsrc.GetResource(dartRsrc.GetIds()[0]),
                                                                    Encoding.GetEncoding("macintosh"));

                        Regex dArtEx    = DartRegex();
                        Match dArtMatch = dArtEx.Match(dArt);

                        if(dArtMatch.Success)
                        {
                            _imageInfo.Application        = "DART";
                            _imageInfo.ApplicationVersion = dArtMatch.Groups["version"].Value;
                            _dataChecksum                 = Convert.ToUInt32(dArtMatch.Groups["datachk"].Value, 16);
                            _tagChecksum                  = Convert.ToUInt32(dArtMatch.Groups["tagchk"].Value,  16);
                        }
                    }
                }

                // "cksm"
                if(rsrcFork.ContainsKey(0x434B534D))
                {
                    Resource cksmRsrc = rsrcFork.GetResource(0x434B534D);

                    if(cksmRsrc?.ContainsId(1) == true)
                    {
                        byte[] tagChk = cksmRsrc.GetResource(1);
                        _tagChecksum = BigEndianBitConverter.ToUInt32(tagChk, 0);
                    }

                    if(cksmRsrc?.ContainsId(2) == true)
                    {
                        byte[] dataChk = cksmRsrc.GetResource(2);
                        _dataChecksum = BigEndianBitConverter.ToUInt32(dataChk, 0);
                    }
                }
            }
        }
        catch(InvalidCastException ex)
        {
            SentrySdk.CaptureException(ex);
        }

        AaruLogging.Debug(MODULE_NAME,
                          Localization.Image_application_0_version_1,
                          _imageInfo.Application,
                          _imageInfo.ApplicationVersion);

        _imageInfo.Sectors              = (ulong)(header.srcSize * 2);
        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.Filename);
        _imageInfo.SectorSize           = SECTOR_SIZE;
        _imageInfo.MetadataMediaType    = MetadataMediaType.BlockMedia;
        _imageInfo.ImageSize            = _imageInfo.Sectors * SECTOR_SIZE;
        _imageInfo.Version              = header.srcCmp == COMPRESS_NONE ? "1.4" : "1.5";

        switch(header.srcSize)
        {
            case SIZE_MAC_SS:
                _imageInfo.Cylinders       = 80;
                _imageInfo.Heads           = 1;
                _imageInfo.SectorsPerTrack = 10;
                _imageInfo.MediaType       = MediaType.AppleSonySS;

                break;
            case SIZE_MAC:
                _imageInfo.Cylinders       = 80;
                _imageInfo.Heads           = 2;
                _imageInfo.SectorsPerTrack = 10;
                _imageInfo.MediaType       = MediaType.AppleSonyDS;

                break;
            case SIZE_DOS:
                _imageInfo.Cylinders       = 80;
                _imageInfo.Heads           = 2;
                _imageInfo.SectorsPerTrack = 9;
                _imageInfo.MediaType       = MediaType.DOS_35_DS_DD_9;

                break;
            case SIZE_MAC_HD:
                _imageInfo.Cylinders       = 80;
                _imageInfo.Heads           = 2;
                _imageInfo.SectorsPerTrack = 18;
                _imageInfo.MediaType       = MediaType.DOS_35_HD;

                break;
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, bool negative, out byte[] buffer, out SectorStatus sectorStatus)
    {
        sectorStatus = SectorStatus.Dumped;

        return ReadSectors(sectorAddress, negative, 1, out buffer, out _);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, bool negative, SectorTagType tag, out byte[] buffer) =>
        ReadSectorsTag(sectorAddress, negative, 1, tag, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong              sectorAddress, bool negative, uint length, out byte[] buffer,
                                   out SectorStatus[] sectorStatus)
    {
        buffer       = null;
        sectorStatus = null;

        if(negative) return ErrorNumber.NotSupported;

        if(sectorAddress > _imageInfo.Sectors - 1) return ErrorNumber.OutOfRange;

        if(sectorAddress + length > _imageInfo.Sectors) return ErrorNumber.OutOfRange;

        buffer       = new byte[length * _imageInfo.SectorSize];
        sectorStatus = new SectorStatus[length];
        for(uint i = 0; i < length; i++) sectorStatus[i] = SectorStatus.Dumped;

        Array.Copy(_dataCache, (int)sectorAddress * _imageInfo.SectorSize, buffer, 0, length * _imageInfo.SectorSize);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong      sectorAddress, bool negative, uint length, SectorTagType tag,
                                      out byte[] buffer)
    {
        buffer = null;

        if(negative) return ErrorNumber.NotSupported;

        if(tag != SectorTagType.AppleSonyTag) return ErrorNumber.NotSupported;

        if(_tagCache == null || _tagCache.Length == 0) return ErrorNumber.NoData;

        if(sectorAddress > _imageInfo.Sectors - 1) return ErrorNumber.OutOfRange;

        if(sectorAddress + length > _imageInfo.Sectors) return ErrorNumber.OutOfRange;

        buffer = new byte[length * TAG_SECTOR_SIZE];

        Array.Copy(_tagCache, (int)sectorAddress * TAG_SECTOR_SIZE, buffer, 0, length * TAG_SECTOR_SIZE);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong            sectorAddress, bool negative, out byte[] buffer,
                                      out SectorStatus sectorStatus)
    {
        sectorStatus = SectorStatus.Dumped;

        return ReadSectorsLong(sectorAddress, negative, 1, out buffer, out _);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong              sectorAddress, bool negative, uint length, out byte[] buffer,
                                       out SectorStatus[] sectorStatus)
    {
        buffer       = null;
        sectorStatus = null;

        if(negative) return ErrorNumber.NotSupported;

        if(sectorAddress > _imageInfo.Sectors - 1) return ErrorNumber.OutOfRange;

        if(sectorAddress + length > _imageInfo.Sectors) return ErrorNumber.OutOfRange;

        ErrorNumber errno = ReadSectors(sectorAddress, false, length, out byte[] data, out sectorStatus);

        if(errno != ErrorNumber.NoError) return errno;

        errno = ReadSectorsTag(sectorAddress, false, length, SectorTagType.AppleSonyTag, out byte[] tags);

        if(errno != ErrorNumber.NoError) return errno;

        buffer = new byte[data.Length + tags.Length];

        for(uint i = 0; i < length; i++)
        {
            Array.Copy(data,
                       i * _imageInfo.SectorSize,
                       buffer,
                       i * (_imageInfo.SectorSize + TAG_SECTOR_SIZE),
                       _imageInfo.SectorSize);

            Array.Copy(tags,
                       i * TAG_SECTOR_SIZE,
                       buffer,
                       i * (_imageInfo.SectorSize + TAG_SECTOR_SIZE) + _imageInfo.SectorSize,
                       TAG_SECTOR_SIZE);
        }

        return ErrorNumber.NoError;
    }

#endregion
}