using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Structs;

namespace Aaru.Tests;

/// <summary>Class to define expected data when testing media info</summary>
public class MediaInfoTest
{
    /// <summary>Expected media type</summary>
    public MediaType MediaType;
    /// <summary>Expected media sector size</summary>
    public uint      SectorSize;
    /// <summary>Expected number of sectors in media</summary>
    public ulong     Sectors;
    /// <summary>File that contains the image to test</summary>
    public string    TestFile;

    public override string ToString() => TestFile;
}

/// <inheritdoc />
/// <summary>Class to define expected data when testing filesystem info</summary>
public class FileSystemTest : MediaInfoTest
{
    /// <summary>Application ID</summary>
    public string ApplicationId;
    /// <summary>Can the volume boot?</summary>
    public bool   Bootable;
    /// <summary>Bytes per cluster</summary>
    public uint   ClusterSize;
    /// <summary>Clusters in volume</summary>
    public long   Clusters;
    [JsonIgnore]
    public Dictionary<string, FileData> Contents;
    [JsonIgnore]
    public string ContentsJson;
    [JsonIgnore]
    public Encoding Encoding;
    /// <summary>Name of the encoding to use when mounting, resolvable by Claunia.Encoding or System.Text</summary>
    public string EncodingName;
    /// <summary>True when this test was loaded from a sidecar .info.json instead of declared in code</summary>
    [JsonIgnore]
    public bool FromInfoJson;
    public string Namespace;
    /// <summary>System or OEM ID</summary>
    public string SystemId;
    /// <summary>Filesystem type. null if always the same, as defined in test class</summary>
    public string Type;
    /// <summary>Volume name</summary>
    public string VolumeName;
    /// <summary>Volume serial number or set identifier</summary>
    public string VolumeSerial;
}

/// <summary>Aggregate digest of a filesystem's full contents, stored as {testFile}.contents.digest.json</summary>
public class ContentsDigestFile
{
    /// <summary>Total bytes of file contents hashed</summary>
    public long   Bytes;
    /// <summary>SHA-256 over the canonical traversal records (paths, metadata, per-file MD5s, xattrs); no timestamps</summary>
    public string Digest;
    /// <summary>Number of entries visited</summary>
    public long   Entries;
    /// <summary>SHA-256 over paths and UTC timestamps only</summary>
    public string TimestampDigest;
    /// <summary>Canonical record format version</summary>
    public int    Version;
}

public class BlockImageTestExpected : MediaInfoTest
{
    public string                  Md5;
    public BlockPartitionVolumes[] Partitions;
}

public class ArchiveTestExpected
{
    public ArchiveEntryData[] Contents;
    public string             ContentsJson;
    public int                EntryCount;
    public string             TestFile;

    public override string ToString() => TestFile;
}

public class ArchiveEntryData
{
    public string Path       { get; set; }
    public long   Size       { get; set; }
    public string Type       { get; set; }
    public string LinkTarget { get; set; }
}

public class TrackInfoTestExpected
{
    public ulong            End;
    public FileSystemTest[] FileSystems;
    public byte?            Flags;
    public byte             Number;
    public ulong            Pregap;
    public int              Session;
    public ulong            Start;
}

public class OpticalImageTestExpected : BlockImageTestExpected
{
    public string                  LongMd5;
    public string                  SubchannelMd5;
    public TrackInfoTestExpected[] Tracks;
}

public class TapeImageTestExpected : BlockImageTestExpected
{
    public     TapeFile[]      Files;
    public new TapePartition[] Partitions;
}

public class FluxCaptureTestExpected
{
    /// <summary>Capture index for this head/track/subTrack combination</summary>
    public uint   CaptureIndex;
    /// <summary>Expected data resolution in picoseconds</summary>
    public ulong  DataResolution;
    /// <summary>Physical head (0-based)</summary>
    public uint   Head;
    /// <summary>Expected index resolution in picoseconds</summary>
    public ulong  IndexResolution;
    /// <summary>Physical sub-track (0-based, e.g. half-track)</summary>
    public byte   SubTrack;
    /// <summary>Physical track (0-based)</summary>
    public ushort Track;
}

public class FluxImageTestExpected : BlockImageTestExpected
{
    /// <summary>Expected number of flux captures in the image</summary>
    public uint                      FluxCaptureCount;
    /// <summary>Expected flux captures to validate</summary>
    public FluxCaptureTestExpected[] FluxCaptures;
}

public class PartitionTest
{
    public Partition[] Partitions;
    /// <summary>File that contains the partition scheme to test</summary>
    public string      TestFile;
}

public class FsExtractHashData
{
    public PartitionVolumes[] Partitions;
}

public class PartitionVolumes
{
    public VolumeData[] Volumes;
}

public class FileData
{
    public Dictionary<string, FileData> Children      { get; set; }
    public FileEntryInfo                Info          { get; set; }
    public string                       LinkTarget    { get; set; }
    public string                       Md5           { get; set; }
    public Dictionary<string, string>   XattrsWithMd5 { get; set; }
}

public class VolumeData
{
    public List<string>                 Directories;
    public Dictionary<string, FileData> Files;
    public string                       VolumeName;
}

public class BlockPartitionVolumes
{
    public ulong Length;
    public ulong Start;
}