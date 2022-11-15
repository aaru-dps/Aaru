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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.DiscImages;

using System;
using System.IO;
using System.Text.RegularExpressions;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Compression;
using Aaru.Console;
using Aaru.Helpers;
using Claunia.Encoding;
using Claunia.RsrcFork;
using Version = Resources.Version;

public sealed partial class Dart
{
    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();

        if(stream.Length < 84)
            return ErrorNumber.InvalidArgument;

        stream.Seek(0, SeekOrigin.Begin);
        var headerB = new byte[Marshal.SizeOf<Header>()];

        stream.EnsureRead(headerB, 0, Marshal.SizeOf<Header>());
        Header header = Marshal.ByteArrayToStructureBigEndian<Header>(headerB);

        if(header.srcCmp > COMPRESS_NONE)
            return ErrorNumber.NotSupported;

        int expectedMaxSize = 84 + header.srcSize * 2 * 524;

        switch(header.srcType)
        {
            case DISK_MAC:
                if(header.srcSize != SIZE_MAC_SS &&
                   header.srcSize != SIZE_MAC)
                    return ErrorNumber.InvalidArgument;

                break;
            case DISK_LISA:
                if(header.srcSize != SIZE_LISA)
                    return ErrorNumber.InvalidArgument;

                break;
            case DISK_APPLE2:
                if(header.srcSize != DISK_APPLE2)
                    return ErrorNumber.InvalidArgument;

                break;
            case DISK_MAC_HD:
                if(header.srcSize != SIZE_MAC_HD)
                    return ErrorNumber.InvalidArgument;

                expectedMaxSize += 64;

                break;
            case DISK_DOS:
                if(header.srcSize != SIZE_DOS)
                    return ErrorNumber.InvalidArgument;

                break;
            case DISK_DOS_HD:
                if(header.srcSize != SIZE_DOS_HD)
                    return ErrorNumber.InvalidArgument;

                expectedMaxSize += 64;

                break;
            default: return ErrorNumber.InvalidArgument;
        }

        if(stream.Length > expectedMaxSize)
            return ErrorNumber.InvalidArgument;

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
            if(l != 0)
            {
                var buffer = new byte[BUFFER_SIZE];

                if(l == -1)
                {
                    stream.EnsureRead(buffer, 0, BUFFER_SIZE);
                    dataMs.Write(buffer, 0, DATA_SIZE);
                    tagMs.Write(buffer, DATA_SIZE, TAG_SIZE);
                }
                else
                {
                    byte[] temp;

                    if(header.srcCmp == COMPRESS_RLE)
                    {
                        temp = new byte[l * 2];
                        stream.EnsureRead(temp, 0, temp.Length);
                        buffer = new byte[BUFFER_SIZE];

                        AppleRle.DecodeBuffer(temp, buffer);

                        dataMs.Write(buffer, 0, DATA_SIZE);
                        tagMs.Write(buffer, DATA_SIZE, TAG_SIZE);
                    }
                    else
                    {
                        temp = new byte[l];
                        stream.EnsureRead(temp, 0, temp.Length);

                        AaruConsole.ErrorWriteLine("LZH Compressed images not yet supported");

                        return ErrorNumber.NotImplemented;
                    }
                }
            }

        _dataCache = dataMs.ToArray();

        if(header.srcType is DISK_LISA or DISK_MAC or DISK_APPLE2)
        {
            _imageInfo.ReadableSectorTags.Add(SectorTagType.AppleSectorTag);
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

                        string major = $"{version.MajorVersion}";
                        string minor = $".{version.MinorVersion / 10}";

                        if(version.MinorVersion % 10 > 0)
                            release = $".{version.MinorVersion % 10}";

                        string dev = version.DevStage switch
                                     {
                                         Version.DevelopmentStage.Alpha    => "a",
                                         Version.DevelopmentStage.Beta     => "b",
                                         Version.DevelopmentStage.PreAlpha => "d",
                                         _                                 => null
                                     };

                        if(dev                       == null &&
                           version.PreReleaseVersion > 0)
                            dev = "f";

                        if(dev != null)
                            pre = $"{version.PreReleaseVersion}";

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

                        var   dArtEx    = new Regex(DART_REGEX);
                        Match dArtMatch = dArtEx.Match(dArt);

                        if(dArtMatch.Success)
                        {
                            _imageInfo.Application        = "DART";
                            _imageInfo.ApplicationVersion = dArtMatch.Groups["version"].Value;
                            _dataChecksum                 = Convert.ToUInt32(dArtMatch.Groups["datachk"].Value, 16);
                            _tagChecksum                  = Convert.ToUInt32(dArtMatch.Groups["tagchk"].Value, 16);
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
                        byte[] dataChk = cksmRsrc.GetResource(1);
                        _dataChecksum = BigEndianBitConverter.ToUInt32(dataChk, 0);
                    }
                }
            }
        }
        catch(InvalidCastException) {}

        AaruConsole.DebugWriteLine("DART plugin", "Image application = {0} version {1}", _imageInfo.Application,
                                   _imageInfo.ApplicationVersion);

        _imageInfo.Sectors              = (ulong)(header.srcSize * 2);
        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.Filename);
        _imageInfo.SectorSize           = SECTOR_SIZE;
        _imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;
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
    public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer) => ReadSectors(sectorAddress, 1, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, SectorTagType tag, out byte[] buffer) =>
        ReadSectorsTag(sectorAddress, 1, tag, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > _imageInfo.Sectors - 1)
            return ErrorNumber.OutOfRange;

        if(sectorAddress + length > _imageInfo.Sectors)
            return ErrorNumber.OutOfRange;

        buffer = new byte[length * _imageInfo.SectorSize];

        Array.Copy(_dataCache, (int)sectorAddress * _imageInfo.SectorSize, buffer, 0, length * _imageInfo.SectorSize);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag, out byte[] buffer)
    {
        buffer = null;

        if(tag != SectorTagType.AppleSectorTag)
            return ErrorNumber.NotSupported;

        if(_tagCache        == null ||
           _tagCache.Length == 0)
            return ErrorNumber.NoData;

        if(sectorAddress > _imageInfo.Sectors - 1)
            return ErrorNumber.OutOfRange;

        if(sectorAddress + length > _imageInfo.Sectors)
            return ErrorNumber.OutOfRange;

        buffer = new byte[length * TAG_SECTOR_SIZE];

        Array.Copy(_tagCache, (int)sectorAddress * TAG_SECTOR_SIZE, buffer, 0, length * TAG_SECTOR_SIZE);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong sectorAddress, out byte[] buffer) =>
        ReadSectorsLong(sectorAddress, 1, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > _imageInfo.Sectors - 1)
            return ErrorNumber.OutOfRange;

        if(sectorAddress + length > _imageInfo.Sectors)
            return ErrorNumber.OutOfRange;

        ErrorNumber errno = ReadSectors(sectorAddress, length, out byte[] data);

        if(errno != ErrorNumber.NoError)
            return errno;

        errno = ReadSectorsTag(sectorAddress, length, SectorTagType.AppleSectorTag, out byte[] tags);

        if(errno != ErrorNumber.NoError)
            return errno;

        buffer = new byte[data.Length + tags.Length];

        for(uint i = 0; i < length; i++)
        {
            Array.Copy(data, i * _imageInfo.SectorSize, buffer, i * (_imageInfo.SectorSize + TAG_SECTOR_SIZE),
                       _imageInfo.SectorSize);

            Array.Copy(tags, i * TAG_SECTOR_SIZE, buffer,
                       i * (_imageInfo.SectorSize + TAG_SECTOR_SIZE) + _imageInfo.SectorSize, TAG_SECTOR_SIZE);
        }

        return ErrorNumber.NoError;
    }
}