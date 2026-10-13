using System;
using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Humanizer;
using Partition = Aaru.CommonTypes.Partition;
using Track = Aaru.CommonTypes.Structs.Track;

namespace Aaru.Images;

public sealed partial class AaruFormat
{
#region IWritableOpticalImage Members

    /// <inheritdoc />
    public OpticalImageCapabilities OpticalCapabilities => OpticalImageCapabilities.CanStoreAudioTracks     |
                                                           OpticalImageCapabilities.CanStoreDataTracks      |
                                                           OpticalImageCapabilities.CanStorePregaps         |
                                                           OpticalImageCapabilities.CanStoreSubchannelRw    |
                                                           OpticalImageCapabilities.CanStoreSessions        |
                                                           OpticalImageCapabilities.CanStoreIsrc            |
                                                           OpticalImageCapabilities.CanStoreCdText          |
                                                           OpticalImageCapabilities.CanStoreMcn             |
                                                           OpticalImageCapabilities.CanStoreRawData         |
                                                           OpticalImageCapabilities.CanStoreCookedData      |
                                                           OpticalImageCapabilities.CanStoreMultipleTracks  |
                                                           OpticalImageCapabilities.CanStoreNotCdSessions   |
                                                           OpticalImageCapabilities.CanStoreNotCdTracks     |
                                                           OpticalImageCapabilities.CanStoreIndexes         |
                                                           OpticalImageCapabilities.CanStoreHiddenTracks    |
                                                           OpticalImageCapabilities.CanStoreNegativeSectors |
                                                           OpticalImageCapabilities.CanStoreOverflowSectors |
                                                           OpticalImageCapabilities.CanStoreDpm;

    /// <inheritdoc />

    // ReSharper disable once ConvertToAutoProperty
    public ImageInfo Info => _imageInfo;

    /// <inheritdoc />
    public string Name => Localization.AaruFormat_Name;

    /// <inheritdoc />
    public Guid Id => new("D98C9259-482C-4F0A-B428-18736DC039A6");

    /// <inheritdoc />
    public string Format => "Aaru";

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    /// <inheritdoc />
    public List<Partition> Partitions
    {
        get
        {
            if(IsTape)
            {
                if(TapePartitions is null) return null;

                ulong i = 0;

                return TapePartitions.ConvertAll(part => new Partition
                {
                    Start    = part.FirstBlock,
                    Length   = part.LastBlock - part.FirstBlock + 1,
                    Scheme   = "Tape",
                    Sequence = i++,
                    Type     = "Tape Partition",
                    Name     = $"Partition {part.Number}"
                });
            }

            if(Tracks is null) return null;

            ulong           currentTrackOffset = 0;
            List<Partition> partitions         = [];

            foreach(Track track in Tracks.OrderBy(static t => t.StartSector))
            {
                partitions.Add(new Partition
                {
                    Sequence = track.Sequence,
                    Type     = track.Type.Humanize(),
                    Name     = string.Format(Localization.Track_0, track.Sequence),
                    Offset   = currentTrackOffset,
                    Start    = (ulong)track.Indexes[1],
                    Size     = (track.EndSector - (ulong)track.Indexes[1] + 1) * (ulong)track.BytesPerSector,
                    Length   = track.EndSector - (ulong)track.Indexes[1] + 1,
                    Scheme   = Localization.Optical_disc_track
                });

                currentTrackOffset += (track.EndSector - track.StartSector + 1) * (ulong)track.BytesPerSector;
            }

            return partitions;
        }
    }

    /// <inheritdoc />
    public IEnumerable<MediaTagType> SupportedMediaTags => Enum.GetValues(typeof(MediaTagType)).Cast<MediaTagType>();

    /// <inheritdoc />
    public IEnumerable<SectorTagType> SupportedSectorTags =>
        Enum.GetValues(typeof(SectorTagType)).Cast<SectorTagType>();

    /// <inheritdoc />
    public IEnumerable<MediaType> SupportedMediaTypes => Enum.GetValues(typeof(MediaType)).Cast<MediaType>();

    /// <inheritdoc />
    public IEnumerable<(string name, Type type, string description, object @default)> SupportedOptions =>
    [
        ("dictionary", typeof(uint), Localization.Size_in_bytes_of_the_LZMA_dictionary, (uint)(1 << 25)),
        ("md5", typeof(bool), Localization.Calculate_and_store_MD5_of_image_user_data, false),
        ("sha1", typeof(bool), Localization.Calculate_and_store_SHA1_of_image_user_data, false),
        ("sha256", typeof(bool), Localization.Calculate_and_store_SHA256_of_image_user_data, false),
        ("spamsum", typeof(bool), Localization.Calculate_and_store_SpamSum_of_image_user_data, false),
        ("blake3", typeof(bool), "Calculate and store BLAKE3 of image's user data", false),
        ("deduplicate", typeof(bool),
         Localization.Store_only_unique_sectors_This_consumes_more_memory_and_is_slower_but_its_enabled_by_default,
         true),
        ("compress", typeof(bool), Localization.Compress_user_data_blocks_Other_blocks_will_always_be_compressed,
         true),
        ("table_shift", typeof(int),
         "Shift value of how many sectors are stored per primary table entry. -1 for automatic, 0 to use a single table level.",
         -1),
        ("data_shift", typeof(int), "Shift value of how many sectors are stored per data block.", 12),
        ("block_alignment", typeof(uint), "Shift value of alignment of blocks in the image", 9)
    ];

    /// <inheritdoc />
    public IEnumerable<string> KnownExtensions => [".aif", ".aaru", ".aaruformat", ".aaruf", ".dicf"];

    /// <inheritdoc />
    public bool IsWriting { get; private set; }

    /// <inheritdoc />
    public string ErrorMessage { get; private set; }

#endregion
}