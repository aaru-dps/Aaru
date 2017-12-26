// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : IMediaImage.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines structures to be used by disc image plugins.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using DiscImageChef.CommonTypes;
using DiscImageChef.Filters;

namespace DiscImageChef.DiscImages
{
    /// <summary>
    ///     Contains information about a dump image and its contents
    /// </summary>
    public struct ImageInfo
    {
        /// <summary>Image contains partitions (or tracks for optical media)</summary>
        public bool HasPartitions;
        /// <summary>Image contains sessions (optical media only)</summary>
        public bool HasSessions;
        /// <summary>Size of the image without headers</summary>
        public ulong ImageSize;
        /// <summary>Sectors contained in the image</summary>
        public ulong Sectors;
        /// <summary>Size of sectors contained in the image</summary>
        public uint SectorSize;
        /// <summary>Media tags contained by the image</summary>
        public List<MediaTagType> ReadableMediaTags;
        /// <summary>Sector tags contained by the image</summary>
        public List<SectorTagType> ReadableSectorTags;
        /// <summary>Image version</summary>
        public string Version;
        /// <summary>Application that created the image</summary>
        public string Application;
        /// <summary>Version of the application that created the image</summary>
        public string ApplicationVersion;
        /// <summary>Who (person) created the image?</summary>
        public string Creator;
        /// <summary>Image creation time</summary>
        public DateTime CreationTime;
        /// <summary>Image last modification time</summary>
        public DateTime LastModificationTime;
        /// <summary>Title of the media represented by the image</summary>
        public string MediaTitle;
        /// <summary>Image comments</summary>
        public string Comments;
        /// <summary>Manufacturer of the media represented by the image</summary>
        public string MediaManufacturer;
        /// <summary>Model of the media represented by the image</summary>
        public string MediaModel;
        /// <summary>Serial number of the media represented by the image</summary>
        public string MediaSerialNumber;
        /// <summary>Barcode of the media represented by the image</summary>
        public string MediaBarcode;
        /// <summary>Part number of the media represented by the image</summary>
        public string MediaPartNumber;
        /// <summary>Media type represented by the image</summary>
        public MediaType MediaType;
        /// <summary>Number in sequence for the media represented by the image</summary>
        public int MediaSequence;
        /// <summary>Last media of the sequence the media represented by the image corresponds to</summary>
        public int LastMediaSequence;
        /// <summary>Manufacturer of the drive used to read the media represented by the image</summary>
        public string DriveManufacturer;
        /// <summary>Model of the drive used to read the media represented by the image</summary>
        public string DriveModel;
        /// <summary>Serial number of the drive used to read the media represented by the image</summary>
        public string DriveSerialNumber;
        /// <summary>Firmware revision of the drive used to read the media represented by the image</summary>
        public string DriveFirmwareRevision;
        /// <summary>Type of the media represented by the image to use in XML sidecars</summary>
        public XmlMediaType XmlMediaType;
        // CHS geometry...
        /// <summary>Cylinders of the media represented by the image</summary>
        public uint Cylinders;
        /// <summary>Heads of the media represented by the image</summary>
        public uint Heads;
        /// <summary>Sectors per track of the media represented by the image (for variable image, the smallest)</summary>
        public uint SectorsPerTrack;
    }

    /// <summary>
    ///     Session defining structure.
    /// </summary>
    public struct Session
    {
        /// <summary>Session number, 1-started</summary>
        public ushort SessionSequence;
        /// <summary>First track present on this session</summary>
        public uint StartTrack;
        /// <summary>Last track present on this session</summary>
        public uint EndTrack;
        /// <summary>First sector present on this session</summary>
        public ulong StartSector;
        /// <summary>Last sector present on this session</summary>
        public ulong EndSector;
    }

    /// <summary>
    ///     Track defining structure.
    /// </summary>
    public struct Track
    {
        /// <summary>Track number, 1-started</summary>
        public uint TrackSequence;
        /// <summary>Partition type</summary>
        public TrackType TrackType;
        /// <summary>Track starting sector</summary>
        public ulong TrackStartSector;
        /// <summary>Track ending sector</summary>
        public ulong TrackEndSector;
        /// <summary>Track pre-gap</summary>
        public ulong TrackPregap;
        /// <summary>Session this track belongs to</summary>
        public ushort TrackSession;
        /// <summary>Information that does not find space in this struct</summary>
        public string TrackDescription;
        /// <summary>Indexes, 00 to 99 and sector offset</summary>
        public Dictionary<int, ulong> Indexes;
        /// <summary>Which filter stores this track</summary>
        public IFilter TrackFilter;
        /// <summary>Which file stores this track</summary>
        public string TrackFile;
        /// <summary>Starting at which byte is this track stored</summary>
        public ulong TrackFileOffset;
        /// <summary>What kind of file is storing this track</summary>
        public string TrackFileType;
        /// <summary>How many main channel / user data bytes are per sector in this track</summary>
        public int TrackBytesPerSector;
        /// <summary>How many main channel bytes per sector are in the file with this track</summary>
        public int TrackRawBytesPerSector;
        /// <summary>Which filter stores this track's subchannel</summary>
        public IFilter TrackSubchannelFilter;
        /// <summary>Which file stores this track's subchannel</summary>
        public string TrackSubchannelFile;
        /// <summary>Starting at which byte are this track's subchannel stored</summary>
        public ulong TrackSubchannelOffset;
        /// <summary>Type of subchannel stored for this track</summary>
        public TrackSubchannelType TrackSubchannelType;
    }
}