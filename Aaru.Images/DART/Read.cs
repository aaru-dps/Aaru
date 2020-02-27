// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Text.RegularExpressions;
using Claunia.Encoding;
using Claunia.RsrcFork;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Exceptions;
using Aaru.CommonTypes.Interfaces;
using Aaru.Compression;
using Aaru.Console;
using Aaru.Helpers;
using Version = Resources.Version;

namespace Aaru.DiscImages
{
    public partial class Dart
    {
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            if(stream.Length < 84) return false;

            stream.Seek(0, SeekOrigin.Begin);
            byte[] headerB = new byte[Marshal.SizeOf<DartHeader>()];

            stream.Read(headerB, 0, Marshal.SizeOf<DartHeader>());
            DartHeader header = Marshal.ByteArrayToStructureBigEndian<DartHeader>(headerB);

            if(header.srcCmp > COMPRESS_NONE) return false;

            int expectedMaxSize = 84 + header.srcSize * 2 * 524;

            switch(header.srcType)
            {
                case DISK_MAC:
                    if(header.srcSize != SIZE_MAC_SS && header.srcSize != SIZE_MAC) return false;

                    break;
                case DISK_LISA:
                    if(header.srcSize != SIZE_LISA) return false;

                    break;
                case DISK_APPLE2:
                    if(header.srcSize != DISK_APPLE2) return false;

                    break;
                case DISK_MAC_HD:
                    if(header.srcSize != SIZE_MAC_HD) return false;

                    expectedMaxSize += 64;
                    break;
                case DISK_DOS:
                    if(header.srcSize != SIZE_DOS) return false;

                    break;
                case DISK_DOS_HD:
                    if(header.srcSize != SIZE_DOS_HD) return false;

                    expectedMaxSize += 64;
                    break;
                default: return false;
            }

            if(stream.Length > expectedMaxSize) return false;

            short[] bLength;

            if(header.srcType == DISK_MAC_HD || header.srcType == DISK_DOS_HD)
                bLength  = new short[BLOCK_ARRAY_LEN_HIGH];
            else bLength = new short[BLOCK_ARRAY_LEN_LOW];

            for(int i = 0; i < bLength.Length; i++)
            {
                byte[] tmpShort = new byte[2];
                stream.Read(tmpShort, 0, 2);
                bLength[i] = BigEndianBitConverter.ToInt16(tmpShort, 0);
            }

            MemoryStream dataMs = new MemoryStream();
            MemoryStream tagMs  = new MemoryStream();

            foreach(short l in bLength)
                if(l != 0)
                {
                    byte[] buffer = new byte[BUFFER_SIZE];
                    if(l == -1)
                    {
                        stream.Read(buffer, 0, BUFFER_SIZE);
                        dataMs.Write(buffer, 0, DATA_SIZE);
                        tagMs.Write(buffer, DATA_SIZE, TAG_SIZE);
                    }
                    else
                    {
                        byte[] temp;
                        if(header.srcCmp == COMPRESS_RLE)
                        {
                            temp = new byte[l * 2];
                            stream.Read(temp, 0, temp.Length);
                            AppleRle rle = new AppleRle(new MemoryStream(temp));
                            buffer = new byte[BUFFER_SIZE];
                            for(int i = 0; i < BUFFER_SIZE; i++) buffer[i] = (byte)rle.ProduceByte();
                            dataMs.Write(buffer, 0, DATA_SIZE);
                            tagMs.Write(buffer, DATA_SIZE, TAG_SIZE);
                        }
                        else
                        {
                            temp = new byte[l];
                            stream.Read(temp, 0, temp.Length);
                            throw new ImageNotSupportedException("LZH Compressed images not yet supported");
                        }
                    }
                }

            dataCache = dataMs.ToArray();
            if(header.srcType == DISK_LISA || header.srcType == DISK_MAC || header.srcType == DISK_APPLE2)
            {
                imageInfo.ReadableSectorTags.Add(SectorTagType.AppleSectorTag);
                tagCache = tagMs.ToArray();
            }

            try
            {
                if(imageFilter.HasResourceFork())
                {
                    ResourceFork rsrcFork = new ResourceFork(imageFilter.GetResourceForkStream());
                    // "vers"
                    if(rsrcFork.ContainsKey(0x76657273))
                    {
                        Resource versRsrc = rsrcFork.GetResource(0x76657273);

                        byte[] vers = versRsrc?.GetResource(versRsrc.GetIds()[0]);

                        if(vers != null)
                        {
                            Version version = new Version(vers);

                            string release = null;
                            string dev     = null;
                            string pre     = null;

                            string major                              = $"{version.MajorVersion}";
                            string minor                              = $".{version.MinorVersion / 10}";
                            if(version.MinorVersion % 10 > 0) release = $".{version.MinorVersion % 10}";
                            switch(version.DevStage)
                            {
                                case Version.DevelopmentStage.Alpha:
                                    dev = "a";
                                    break;
                                case Version.DevelopmentStage.Beta:
                                    dev = "b";
                                    break;
                                case Version.DevelopmentStage.PreAlpha:
                                    dev = "d";
                                    break;
                            }

                            if(dev == null && version.PreReleaseVersion > 0) dev = "f";

                            if(dev != null) pre = $"{version.PreReleaseVersion}";

                            imageInfo.ApplicationVersion = $"{major}{minor}{release}{dev}{pre}";
                            imageInfo.Application        = version.VersionString;
                            imageInfo.Comments           = version.VersionMessage;
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
                            const string DART_REGEX =
                                @"(?<version>\S+), tag checksum=\$(?<tagchk>[0123456789ABCDEF]{8}), data checksum=\$(?<datachk>[0123456789ABCDEF]{8})$";
                            Regex dArtEx    = new Regex(DART_REGEX);
                            Match dArtMatch = dArtEx.Match(dArt);

                            if(dArtMatch.Success)
                            {
                                imageInfo.Application        = "DART";
                                imageInfo.ApplicationVersion = dArtMatch.Groups["version"].Value;
                                dataChecksum                 = Convert.ToUInt32(dArtMatch.Groups["datachk"].Value, 16);
                                tagChecksum                  = Convert.ToUInt32(dArtMatch.Groups["tagchk"].Value,  16);
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
                            tagChecksum = BigEndianBitConverter.ToUInt32(tagChk, 0);
                        }

                        if(cksmRsrc?.ContainsId(2) == true)
                        {
                            byte[] dataChk = cksmRsrc.GetResource(1);
                            dataChecksum = BigEndianBitConverter.ToUInt32(dataChk, 0);
                        }
                    }
                }
            }
            catch(InvalidCastException) { }

            DicConsole.DebugWriteLine("DART plugin", "Image application = {0} version {1}", imageInfo.Application,
                                      imageInfo.ApplicationVersion);

            imageInfo.Sectors              = (ulong)(header.srcSize * 2);
            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.SectorSize           = SECTOR_SIZE;
            imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;
            imageInfo.ImageSize            = imageInfo.Sectors * SECTOR_SIZE;
            imageInfo.Version              = header.srcCmp == COMPRESS_NONE ? "1.4" : "1.5";

            switch(header.srcSize)
            {
                case SIZE_MAC_SS:
                    imageInfo.Cylinders       = 80;
                    imageInfo.Heads           = 1;
                    imageInfo.SectorsPerTrack = 10;
                    imageInfo.MediaType       = MediaType.AppleSonySS;
                    break;
                case SIZE_MAC:
                    imageInfo.Cylinders       = 80;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 10;
                    imageInfo.MediaType       = MediaType.AppleSonyDS;
                    break;
                case SIZE_DOS:
                    imageInfo.Cylinders       = 80;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 9;
                    imageInfo.MediaType       = MediaType.DOS_35_DS_DD_9;
                    break;
                case SIZE_MAC_HD:
                    imageInfo.Cylinders       = 80;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 18;
                    imageInfo.MediaType       = MediaType.DOS_35_HD;
                    break;
            }

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress) => ReadSectors(sectorAddress, 1);

        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag) => ReadSectorsTag(sectorAddress, 1, tag);

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * imageInfo.SectorSize];

            Array.Copy(dataCache, (int)sectorAddress * imageInfo.SectorSize, buffer, 0, length * imageInfo.SectorSize);

            return buffer;
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            if(tag != SectorTagType.AppleSectorTag)
                throw new FeatureUnsupportedImageException($"Tag {tag} not supported by image format");

            if(tagCache == null || tagCache.Length == 0)
                throw new FeatureNotPresentImageException("Disk image does not have tags");

            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * TAG_SECTOR_SIZE];

            Array.Copy(tagCache, (int)sectorAddress * TAG_SECTOR_SIZE, buffer, 0, length * TAG_SECTOR_SIZE);

            return buffer;
        }

        public byte[] ReadSectorLong(ulong sectorAddress) => ReadSectorsLong(sectorAddress, 1);

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] data   = ReadSectors(sectorAddress, length);
            byte[] tags   = ReadSectorsTag(sectorAddress, length, SectorTagType.AppleSectorTag);
            byte[] buffer = new byte[data.Length + tags.Length];

            for(uint i = 0; i < length; i++)
            {
                Array.Copy(data, i * imageInfo.SectorSize, buffer, i * (imageInfo.SectorSize + TAG_SECTOR_SIZE),
                           imageInfo.SectorSize);
                Array.Copy(tags, i * TAG_SECTOR_SIZE, buffer,
                           i * (imageInfo.SectorSize + TAG_SECTOR_SIZE) + imageInfo.SectorSize, TAG_SECTOR_SIZE);
            }

            return buffer;
        }
    }
}