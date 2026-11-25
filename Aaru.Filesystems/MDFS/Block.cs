// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Block.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : MiniDisc File System plugin.
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Aaru.Logging;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

public sealed partial class MDFS
{
    /// <summary>Gets how many device sectors make a 2048-byte logical block, 0 if the image cannot hold MDFS</summary>
    static uint GetSectorsPerBlock(IMediaImage imagePlugin)
    {
        uint sectorSize = imagePlugin.Info.SectorSize;

        if(sectorSize == 0 || sectorSize > BLOCK_SIZE || BLOCK_SIZE % sectorSize != 0) return 0;

        return BLOCK_SIZE / sectorSize;
    }

    /// <summary>Reads logical blocks, relative to the partition</summary>
    static ErrorNumber ReadBlocks(IMediaImage imagePlugin, Partition partition, uint sectorsPerBlock, ulong block,
                                  uint        count,       out byte[] buffer)
    {
        buffer = null;

        if(sectorsPerBlock == 0 || count == 0) return ErrorNumber.InvalidArgument;

        ulong firstSector  = block * sectorsPerBlock;
        ulong totalSectors = (ulong)count * sectorsPerBlock;

        if(firstSector + totalSectors > partition.End - partition.Start + 1) return ErrorNumber.InvalidArgument;

        ErrorNumber errno = imagePlugin.ReadSectors(partition.Start + firstSector,
                                                    false,
                                                    (uint)totalSectors,
                                                    out buffer,
                                                    out _);

        if(errno != ErrorNumber.NoError) return errno;

        return buffer is null || buffer.Length < count * BLOCK_SIZE ? ErrorNumber.InOutError : ErrorNumber.NoError;
    }

    /// <summary>Reads logical blocks of the mounted volume</summary>
    ErrorNumber ReadBlocks(ulong block, uint count, out byte[] buffer) =>
        ReadBlocks(_image, _partition, _sectorsPerBlock, block, count, out buffer);

    /// <summary>Reads a sector of the volume management area, from its mirror copy if the primary is unreadable</summary>
    /// <param name="sector">Sector, relative to the volume management area</param>
    /// <param name="buffer">Sector contents</param>
    ErrorNumber ReadVmaSector(uint sector, out byte[] buffer)
    {
        if(!_vd.Mirrored) return ReadBlocks(_vd.VmaLoc + sector, 1, out buffer);

        // Runs of 16 sectors are followed by their 16 copies
        ulong physical = (sector & ~(uint)(MIRROR_RUN - 1)) * 2 + (sector & (MIRROR_RUN - 1));

        ErrorNumber errno = ReadBlocks(_vd.VmaLoc + physical, 1, out buffer);

        if(errno == ErrorNumber.NoError) return errno;

        AaruLogging.Debug(MODULE_NAME, "Reading mirror copy of management area sector {0}", sector);

        return ReadBlocks(_vd.VmaLoc + physical + MIRROR_RUN, 1, out buffer);
    }

    /// <summary>Reads and validates a volume descriptor at a logical block</summary>
    static bool ReadVolumeDescriptor(IMediaImage imagePlugin, Partition partition, uint sectorsPerBlock, ulong lba,
                                     out VolumeDescriptor vd)
    {
        vd = default(VolumeDescriptor);

        if(ReadBlocks(imagePlugin, partition, sectorsPerBlock, lba, 1, out byte[] sector) != ErrorNumber.NoError)
            return false;

        if(!sector.AsSpan(0, _signature.Length).SequenceEqual(_signature)) return false;

        vd = new VolumeDescriptor(sector);

        return vd.IsSane(lba);
    }

    /// <summary>Finds the volume descriptor</summary>
    /// <remarks>
    ///     The drive tells the host where the data track starts, and the volume descriptor is 32 sectors into it. If the
    ///     image keeps the drive's MiniDisc type or U-TOC responses they are used, otherwise the location used by every
    ///     Sony formatter is tried.
    /// </remarks>
    static bool FindVolumeDescriptor(IMediaImage imagePlugin, Partition partition, uint sectorsPerBlock,
                                     out VolumeDescriptor vd, out ulong lba)
    {
        foreach(ulong candidate in VolumeDescriptorCandidates(imagePlugin, partition))
        {
            if(!ReadVolumeDescriptor(imagePlugin, partition, sectorsPerBlock, candidate, out vd)) continue;

            lba = candidate;

            return true;
        }

        vd  = default(VolumeDescriptor);
        lba = 0;

        return false;
    }

    static IEnumerable<ulong> VolumeDescriptorCandidates(IMediaImage imagePlugin, Partition partition)
    {
        List<ulong> candidates = [];

        // Media tags describe the whole disc
        if(partition.Start == 0 && imagePlugin.Info.ReadableMediaTags is not null)
        {
            // Response to vendor command D8h: 01h and the first sector of the data track for MD DATA discs
            if(TryReadMediaTag(imagePlugin, MediaTagType.MiniDiscType, out byte[] type) &&
               type.Length >= 8                                                       &&
               type[0]     == 1)
                candidates.Add(BigEndianBitConverter.ToUInt32(type, 4) + VD_OFFSET_FROM_DATA_TRACK);

            // Dumps made by older versions of Aaru stored the U-TOC and the P-TOC under each other's tag
            foreach(MediaTagType tag in new[]
                    {
                        MediaTagType.MiniDiscUTOC, MediaTagType.MiniDiscPTOC
                    })
            {
                if(!TryReadMediaTag(imagePlugin, tag, out byte[] toc)) continue;

                long dataTrack = GetDataTrackStart(toc);

                if(dataTrack >= 0) candidates.Add((ulong)dataTrack + VD_OFFSET_FROM_DATA_TRACK);
            }
        }

        candidates.Add(DEFAULT_VD_LBA);

        return candidates.Distinct();
    }

    static bool TryReadMediaTag(IMediaImage imagePlugin, MediaTagType tag, out byte[] buffer)
    {
        buffer = null;

        if(!imagePlugin.Info.ReadableMediaTags.Contains(tag)) return false;

        try
        {
            return imagePlugin.ReadMediaTag(tag, out buffer) == ErrorNumber.NoError && buffer is not null;
        }
        catch(Exception ex)
        {
            AaruLogging.Exception(ex, "Could not read media tag {0}", tag);

            return false;
        }
    }

    /// <summary>Gets the first sector of the MD DATA data track from sector 0 of a U-TOC, or -1</summary>
    static long GetDataTrackStart(byte[] toc)
    {
        if(toc.Length < 0x128) return -1;

        // That is a P-TOC
        ReadOnlySpan<byte> signature = toc.AsSpan(8, 4);

        if(signature.SequenceEqual(_ptocAudio) || signature.SequenceEqual(_ptocData)) return -1;

        int firstTrack = toc[0x0E];
        int lastTrack  = toc[0x0F];

        if(firstTrack == 0 || lastTrack < firstTrack) return -1;

        for(int track = firstTrack; track <= lastTrack && 0x20 + track < toc.Length; track++)
        {
            int part = toc[0x20 + track];

            if(part == 0) continue;

            int entry = 0x120 + 8 * part;

            if(entry + 8 > toc.Length) continue;

            // Track mode of the data track, the system track uses DCh
            if(toc[entry + 3] != 0xD0) continue;

            uint address = (uint)(toc[entry] << 16 | toc[entry + 1] << 8 | toc[entry + 2]);
            uint cluster = address >> 10;
            uint sector  = address >> 4 & 0x3F;

            // Logical blocks start at cluster 50, 32 data sectors per cluster
            if(cluster < 50 || sector >= 32) continue;

            return (cluster - 50) * 32 + sector;
        }

        return -1;
    }

    /// <summary>Gets an encoding by name, or a fallback if this platform does not provide it</summary>
    static Encoding GetEncoding(string name, Encoding fallback)
    {
        try
        {
            return Encoding.GetEncoding(name);
        }
        catch(ArgumentException)
        {
            return fallback;
        }
        catch(NotSupportedException)
        {
            return fallback;
        }
    }

    /// <summary>Gets the encoding for a character set code</summary>
    /// <remarks>
    ///     Windows records its ANSI code page and DOS its OEM code page, which depend on the system that wrote the
    ///     volume, e.g. Shift-JIS on Japanese systems. An encoding given by the user overrides both.
    /// </remarks>
    /// <param name="charset">Character set code</param>
    /// <param name="userEncoding">Encoding requested by the user, or <c>null</c></param>
    static Encoding GetCharsetEncoding(byte charset, Encoding userEncoding)
    {
        if(charset == CSC_MAC) return GetEncoding("macintosh", Encoding.ASCII);

        if(userEncoding is not null) return userEncoding;

        return charset == CSC_WINDOWS
                   ? GetEncoding("windows-1252", Encoding.ASCII)
                   : GetEncoding("IBM437",       Encoding.ASCII);
    }

    /// <summary>Decodes a NUL padded string</summary>
    static string DecodeString(ReadOnlySpan<byte> bytes, byte charset, Encoding userEncoding)
    {
        int length = bytes.IndexOf((byte)0);

        if(length < 0) length = bytes.Length;

        return GetCharsetEncoding(charset, userEncoding).GetString(bytes[..length]).TrimEnd();
    }

    /// <summary>Converts an MDFS timestamp, a DOS date in the high word and a DOS time in the low word</summary>
    static DateTime? ToDateTime(uint timestamp)
    {
        if(timestamp == 0) return null;

        return DateHandlers.DosToDateTime((ushort)(timestamp >> 16), (ushort)(timestamp & 0xFFFF));
    }
}