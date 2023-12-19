// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : IMediaImage.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Common structures.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines structures to be used by media image plugins.
//
// --[ License ] --------------------------------------------------------------
//
//     Permission is hereby granted, free of charge, to any person obtaining a
//     copy of this software and associated documentation files (the
//     "Software"), to deal in the Software without restriction, including
//     without limitation the rights to use, copy, modify, merge, publish,
//     distribute, sublicense, and/or sell copies of the Software, and to
//     permit persons to whom the Software is furnished to do so, subject to
//     the following conditions:
//
//     The above copyright notice and this permission notice shall be included
//     in all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//     OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//     MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//     IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//     CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//     TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//     SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.CommonTypes.Structs;

/// <summary>Contains information about a dump image and its contents</summary>
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
    public MetadataMediaType MetadataMediaType;

    // CHS geometry...
    /// <summary>Cylinders of the media represented by the image</summary>
    public uint Cylinders;
    /// <summary>Heads of the media represented by the image</summary>
    public uint Heads;
    /// <summary>Sectors per track of the media represented by the image (for variable image, the smallest)</summary>
    public uint SectorsPerTrack;
}

/// <summary>Session defining structure.</summary>
public struct Session
{
    /// <summary>Session number, 1-started</summary>
    public ushort Sequence;
    /// <summary>First track present on this session</summary>
    public uint StartTrack;
    /// <summary>Last track present on this session</summary>
    public uint EndTrack;
    /// <summary>First sector present on this session</summary>
    public ulong StartSector;
    /// <summary>Last sector present on this session</summary>
    public ulong EndSector;
}

/// <summary>Track defining structure.</summary>
public class Track
{
    /// <summary>How many main channel / user data bytes are per sector in this track</summary>
    public int BytesPerSector;
    /// <summary>Information that does not find space in this struct</summary>
    public string Description;
    /// <summary>Track ending sector</summary>
    public ulong EndSector;
    /// <summary>Which file stores this track</summary>
    public string File;
    /// <summary>Starting at which byte is this track stored</summary>
    public ulong FileOffset;
    /// <summary>What kind of file is storing this track</summary>
    public string FileType;
    /// <summary>Which filter stores this track</summary>
    public IFilter Filter;
    /// <summary>Indexes, 00 to 99 and sector offset</summary>
    public Dictionary<ushort, int> Indexes;
    /// <summary>Track pre-gap</summary>
    public ulong Pregap;
    /// <summary>How many main channel bytes per sector are in the file with this track</summary>
    public int RawBytesPerSector;
    /// <summary>Track number, 1-started</summary>
    public uint Sequence;
    /// <summary>Session this track belongs to</summary>
    public ushort Session;
    /// <summary>Track starting sector</summary>
    public ulong StartSector;
    /// <summary>Which file stores this track's subchannel</summary>
    public string SubchannelFile;
    /// <summary>Which filter stores this track's subchannel</summary>
    public IFilter SubchannelFilter;
    /// <summary>Starting at which byte are this track's subchannel stored</summary>
    public ulong SubchannelOffset;
    /// <summary>Type of subchannel stored for this track</summary>
    public TrackSubchannelType SubchannelType;
    /// <summary>Partition type</summary>
    public TrackType Type;

    /// <summary>Initializes an empty instance of this structure</summary>
    public Track() => Indexes = new Dictionary<ushort, int>();
}

/// <summary>Floppy physical characteristics structure.</summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public struct FloppyInfo
{
    /// <summary>Physical floppy type.</summary>
    public FloppyTypes Type;
    /// <summary>Bitrate in bits per second used to write the floppy, 0 if unknown or track-variable.</summary>
    public uint Bitrate;
    /// <summary>Physical magnetic density (coercivity) of floppy medium.</summary>
    public FloppyDensities Coercivity;
    /// <summary>How many physical tracks are actually written in the floppy image.</summary>
    public ushort Tracks;
    /// <summary>How many physical heads are actually written in the floppy image.</summary>
    public byte Heads;
    /// <summary>How many tracks per inch are actually written in the floppy image.</summary>
    public ushort TrackDensity;
}

/// <summary>Provides a map for linear memory structure</summary>
public struct LinearMemoryMap
{
    /// <summary>List of devices that are mapped to linear memory</summary>
    [NotNull]
    public LinearMemoryDevice[] Devices { get; set; }
}

/// <summary>Provides information about a linear memory device</summary>
public struct LinearMemoryDevice
{
    /// <summary>Device manufacturer</summary>
    public string Manufacturer { get; set; }

    /// <summary>Device model</summary>
    public string Model { get; set; }

    /// <summary>Device package, e.g. DIP28</summary>
    public string Package { get; set; }

    /// <summary>Device location marking in PCB, e.g. U28</summary>
    public string Location { get; set; }

    /// <summary>Device functional type</summary>
    public LinearMemoryType Type { get; set; }

    /// <summary>Arbitrary device information</summary>
    public string Description { get; set; }

    /// <summary>
    ///     Physical addressing is the address considering all devices in the linear memory media, starting at the lowest
    ///     marking in PCB and going up incrementally. This is the view of the memory inside Aaru.
    /// </summary>
    public LinearMemoryAddressing PhysicalAddress { get; set; }

    /// <summary>
    ///     Virtual addressing is the address as seen by the hardware directly accessing the linear memory media. This
    ///     allows devices to be overlapped or banked.
    /// </summary>
    public LinearMemoryAddressing VirtualAddress { get; set; }
}

/// <summary>Maps the addressing of a linear memory device</summary>
public class LinearMemoryAddressing
{
    /// <summary>Start in memory where the device is mapped</summary>
    public ulong Start { get; set; }

    /// <summary>Length in bytes of the device, not including interleaving</summary>
    public ulong Length { get; set; }

    /// <summary>Interleaving information</summary>
    public LinearMemoryInterleave Interleave { get; set; }
}

/// <summary>Provides information about a device interleaving</summary>
public class LinearMemoryInterleave
{
    /// <summary>How many bytes to skip from start of map before device first byte starts</summary>
    public uint Offset { get; set; }

    /// <summary>How many bytes in memory to skip every device byte</summary>
    public uint Value { get; set; }
}