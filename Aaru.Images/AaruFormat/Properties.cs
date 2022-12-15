// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Properties.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains properties for Aaru Format disk images.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Partition = Aaru.CommonTypes.Partition;
using Track = Aaru.CommonTypes.Structs.Track;

namespace Aaru.DiscImages;

public sealed partial class AaruFormat
{
    /// <inheritdoc />
    public OpticalImageCapabilities OpticalCapabilities => OpticalImageCapabilities.CanStoreAudioTracks    |
                                                           OpticalImageCapabilities.CanStoreDataTracks     |
                                                           OpticalImageCapabilities.CanStorePregaps        |
                                                           OpticalImageCapabilities.CanStoreSubchannelRw   |
                                                           OpticalImageCapabilities.CanStoreSessions       |
                                                           OpticalImageCapabilities.CanStoreIsrc           |
                                                           OpticalImageCapabilities.CanStoreCdText         |
                                                           OpticalImageCapabilities.CanStoreMcn            |
                                                           OpticalImageCapabilities.CanStoreRawData        |
                                                           OpticalImageCapabilities.CanStoreCookedData     |
                                                           OpticalImageCapabilities.CanStoreMultipleTracks |
                                                           OpticalImageCapabilities.CanStoreNotCdSessions  |
                                                           OpticalImageCapabilities.CanStoreNotCdTracks    |
                                                           OpticalImageCapabilities.CanStoreIndexes        |
                                                           OpticalImageCapabilities.CanStoreHiddenTracks;
    /// <inheritdoc />
    public ImageInfo Info => _imageInfo;
    /// <inheritdoc />
    public string Name => Localization.AaruFormat_Name;
    /// <inheritdoc />
    public Guid Id => new("49360069-1784-4A2F-B723-0C844D610B0A");
    /// <inheritdoc />
    public string Format => "Aaru";
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;
    /// <inheritdoc />
    public List<Partition> Partitions { get; private set; }
    /// <inheritdoc />
    public List<Track> Tracks { get; private set; }
    /// <inheritdoc />
    public List<Session> Sessions { get; private set; }
    /// <inheritdoc />
    public List<DumpHardware> DumpHardware { get; private set; }
    /// <inheritdoc />
    public Metadata AaruMetadata { get; private set; }
    /// <inheritdoc />
    public IEnumerable<MediaTagType> SupportedMediaTags => Enum.GetValues(typeof(MediaTagType)).Cast<MediaTagType>();
    /// <inheritdoc />
    public IEnumerable<SectorTagType> SupportedSectorTags =>
        Enum.GetValues(typeof(SectorTagType)).Cast<SectorTagType>();
    /// <inheritdoc />
    public IEnumerable<MediaType> SupportedMediaTypes => Enum.GetValues(typeof(MediaType)).Cast<MediaType>();
    /// <inheritdoc />
    public IEnumerable<(string name, Type type, string description, object @default)> SupportedOptions => new[]
    {
        ("sectors_per_block", typeof(uint),
         Localization.How_many_sectors_to_store_per_block_will_be_rounded_to_next_power_of_two, 4096U),
        ("dictionary", typeof(uint), Localization.Size_in_bytes_of_the_LZMA_dictionary, (uint)(1 << 25)),
        ("max_ddt_size", typeof(uint),
         Localization.Maximum_size_in_mebibytes_for_in_memory_DDT_If_image_needs_a_bigger_one_it_will_be_on_disk, 256U),
        ("md5", typeof(bool), Localization.Calculate_and_store_MD5_of_image_user_data, false),
        ("sha1", typeof(bool), Localization.Calculate_and_store_SHA1_of_image_user_data, false),
        ("sha256", typeof(bool), Localization.Calculate_and_store_SHA256_of_image_user_data, false),
        ("spamsum", typeof(bool), Localization.Calculate_and_store_SpamSum_of_image_user_data, false),
        ("deduplicate", typeof(bool),
         Localization.Store_only_unique_sectors_This_consumes_more_memory_and_is_slower_but_its_enabled_by_default,
         true),
        ("compress", typeof(bool), Localization.Compress_user_data_blocks_Other_blocks_will_always_be_compressed,
         (object)true)
    };
    /// <inheritdoc />
    public IEnumerable<string> KnownExtensions => new[]
    {
        ".dicf", ".aaru", ".aaruformat", ".aaruf", ".aif"
    };
    /// <inheritdoc />
    public bool IsWriting { get; private set; }
    /// <inheritdoc />
    public string ErrorMessage { get; private set; }
}