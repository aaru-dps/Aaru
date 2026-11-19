// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Mount.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : High Performance Optical File System plugin.
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
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using Aaru.Logging;
using FileSystemInfo = Aaru.CommonTypes.Structs.FileSystemInfo;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

public sealed partial class HPOFS
{
    /// <inheritdoc />
    public ErrorNumber Mount(IMediaImage                imagePlugin, Partition partition, Encoding encoding,
                             Dictionary<string, string> options,     string    @namespace)
    {
        _image     = imagePlugin;
        _partition = partition;

        ErrorNumber errno = ReadSuperblocks(imagePlugin, partition, out _bpb, out _medInfo, out bool worm);

        if(errno != ErrorNumber.NoError) return errno;

        _bytesPerSector = _bpb.bps;

        if(_bytesPerSector == 0 || _bytesPerSector != imagePlugin.Info.SectorSize) return ErrorNumber.InvalidArgument;

        _encoding = GetVolumeEncoding(_medInfo, encoding);

        ulong  totalSectors = _medInfo.sectors > 0 ? _medInfo.sectors : partition.End - partition.Start + 1;
        ulong  freeSectors;
        string owner;

        if(worm)
        {
            errno = ReadWormVmi(out byte[] vmi);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "Could not find volume management information");

                return errno;
            }

            errno = ReadWormDirectory(vmi);

            // Nothing can be written past the next writable sector
            uint nextWrite = BigEndianBitConverter.ToUInt32(vmi, VMI_NEXT_WRITE_OFFSET);
            freeSectors = nextWrite < totalSectors ? totalSectors - nextWrite : 0;
            owner       = DecodePadded(vmi[VMI_OWNER_OFFSET..(VMI_OWNER_OFFSET + 32)], _encoding);
        }
        else
        {
            errno = ReadVolumeInformation(out _volInfo);

            if(errno != ErrorNumber.NoError) return errno;

            errno = ReadDirectory();

            if(CountFreeSectors(out freeSectors) != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "Could not read free space");
                freeSectors = 0;
            }

            owner = DecodePadded(_volInfo.owner, _encoding);
        }

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Could not read directory: {0}", errno);

            return errno;
        }

        Metadata = new FileSystem
        {
            Clusters               = totalSectors,
            ClusterSize            = _bytesPerSector,
            CreationDate           = DateHandlers.DosToDateTime(_medInfo.creationDate, _medInfo.creationTime),
            Type                   = FS_TYPE,
            VolumeName             = DecodePadded(_medInfo.volumeLabel, _encoding),
            VolumeSerial           = $"{_medInfo.serial:X8}",
            SystemIdentifier       = StringHandlers.SpacePaddedToString(_bpb.oem_name),
            DataPreparerIdentifier = owner,
            FreeClusters           = freeSectors
        };

        _statfs = new FileSystemInfo
        {
            Blocks         = totalSectors,
            FilenameLength = MAX_FILENAME_LENGTH,
            Files          = (ulong)_entries.Count,
            FreeBlocks     = freeSectors,
            Id = new FileSystemId
            {
                IsInt    = true,
                Serial32 = _medInfo.serial
            },
            PluginId = Id,
            Type     = FS_TYPE
        };

        _mounted = true;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Unmount()
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        _mounted  = false;
        _entries  = null;
        _children = null;
        _root     = null;

        return ErrorNumber.NoError;
    }

    /// <summary>Reads the boot sector and the media information block, from the copies if the primaries are bad</summary>
    ErrorNumber ReadSuperblocks(IMediaImage imagePlugin, Partition partition, out BiosParameterBlock bpb,
                                out MediaInformationBlock medInfo, out bool worm)
    {
        bpb     = default(BiosParameterBlock);
        medInfo = default(MediaInformationBlock);
        worm    = false;

        if(partition.Start + BACKUP_AREA_OFFSET + MEDINFO_SECTOR >= partition.End) return ErrorNumber.InvalidArgument;

        foreach(ulong area in _superblockAreas)
        {
            ErrorNumber errno = imagePlugin.ReadSector(partition.Start + area, false, out byte[] bootSector, out _);

            if(errno != ErrorNumber.NoError || bootSector.Length < 512) continue;

            bpb = Marshal.ByteArrayToStructureLittleEndian<BiosParameterBlock>(bootSector);

            if(!bpb.fs_type.SequenceEqual(_type)) continue;

            errno = imagePlugin.ReadSector(partition.Start + area + MEDINFO_SECTOR, false, out byte[] medSector, out _);

            if(errno != ErrorNumber.NoError || medSector.Length < 512) continue;

            medInfo = Marshal.ByteArrayToStructureBigEndian<MediaInformationBlock>(medSector, 0, 512);

            if(medInfo.blockId.SequenceEqual(_medinfoSignature)) return ErrorNumber.NoError;

            if(!medInfo.blockId.SequenceEqual(_smiSignature)) continue;

            worm = true;

            return ErrorNumber.NoError;
        }

        return ErrorNumber.InvalidArgument;
    }

    /// <summary>Reads the volume information block, from the copy if the primary is bad</summary>
    ErrorNumber ReadVolumeInformation(out VolumeInformationBlock volInfo)
    {
        volInfo = default(VolumeInformationBlock);

        foreach(ulong area in _superblockAreas)
        {
            ErrorNumber errno = ReadSectors(area + VOLINFO_SECTOR, 1, out byte[] sector);

            if(errno != ErrorNumber.NoError || sector.Length < 512) continue;

            volInfo = Marshal.ByteArrayToStructureBigEndian<VolumeInformationBlock>(sector, 0, 512);

            if(volInfo.blockId.SequenceEqual(_volinfoSignature)) return ErrorNumber.NoError;
        }

        return ErrorNumber.InvalidArgument;
    }

    /// <summary>Gets the encoding used for names and labels on the volume</summary>
    static Encoding GetVolumeEncoding(MediaInformationBlock medInfo, Encoding requested)
    {
        if(requested is not null) return requested;

        int codepage = medInfo.codepageType == CODEPAGE_TYPE_EBCDIC && medInfo.codepage == 0 ? 500 : medInfo.codepage;

        try
        {
            if(codepage > 0) return Encoding.GetEncoding(codepage);
        }
        catch(Exception ex) when(ex is ArgumentException or NotSupportedException)
        {
            AaruLogging.Debug(MODULE_NAME, "Codepage {0} is not supported", codepage);
        }

        return medInfo.codepageType == CODEPAGE_TYPE_EBCDIC
                   ? Encoding.GetEncoding("IBM500")
                   : Encoding.GetEncoding("ibm850");
    }

    /// <summary>Reads the whole directory B-tree and builds the directory hierarchy</summary>
    ErrorNumber ReadDirectory()
    {
        ErrorNumber errno = GetSystemFileExtents(DIRECTORY_RECORD_SECTOR, out Extent[] extents);

        if(errno != ErrorNumber.NoError) return errno;

        errno = ReadBTree(extents, out List<(byte[] key, byte[] record)> leaves);

        if(errno != ErrorNumber.NoError) return errno;

        // RPS 10 and 1 store names in uppercase and are case insensitive
        StringComparer comparer = _medInfo.rps is RPS_REWRITABLE_MIXED_CASE or RPS_WORM_MIXED_CASE
                                      ? StringComparer.Ordinal
                                      : StringComparer.OrdinalIgnoreCase;

        _entries  = new Dictionary<string, Entry>(comparer);
        _children = new Dictionary<string, List<string>>(comparer);
        _root     = null;

        _children[""] = [];

        foreach((byte[] key, byte[] record) in leaves)
        {
            if(key.Length < 4) continue;

            int nameLength = BigEndianBitConverter.ToUInt16(key, 0);

            if(nameLength == 0 || 4 + nameLength > key.Length) continue;

            byte[] rawName = new byte[nameLength];
            Array.Copy(key, 4, rawName, 0, nameLength);

            Entry entry = DecodeRecord(record);

            if(entry is null) continue;

            // System files
            if(rawName[0] is SYSTEM_NAME_PREFIX or 0)
            {
                // "\x0F\x0Froot" is the root directory
                if(rawName.Length == 6 && rawName[1] == SYSTEM_NAME_PREFIX && rawName[2] == (byte)'r') _root = entry;

                continue;
            }

            // Path separator is 0x2F regardless of the volume codepage, so split before decoding
            string path = string.Join("/",
                                      SplitPath(rawName, PATH_SEPARATOR)
                                         .Select(c => StringHandlers.CToString(c, _encoding)));

            if(!_entries.TryAdd(path, entry))
            {
                AaruLogging.Debug(MODULE_NAME, "Duplicate directory entry {0}", path);

                continue;
            }

            if(entry.IsDirectory) _children.TryAdd(path, []);
        }

        // Attach every entry to its parent. Keys are sorted so parents come first, but do not rely on it.
        foreach(string path in _entries.Keys)
        {
            int    separator = path.LastIndexOf('/');
            string parent    = separator < 0 ? "" : path[..separator];
            string name      = separator < 0 ? path : path[(separator + 1)..];

            if(!_children.TryGetValue(parent, out List<string> siblings))
            {
                AaruLogging.Debug(MODULE_NAME, "Entry {0} has no parent directory", path);

                continue;
            }

            siblings.Add(name);
        }

        AaruLogging.Debug(MODULE_NAME, "Directory contains {0} entries", _entries.Count);

        return ErrorNumber.NoError;
    }

    /// <summary>Decodes a fixed length string padded with spaces in the volume codepage</summary>
    static string DecodePadded(byte[] data, Encoding encoding)
    {
        if(data is null) return null;

        string decoded = encoding.GetString(data).TrimEnd(' ', '\0');

        return decoded.Length == 0 ? null : decoded;
    }

    /// <summary>Splits a raw directory key into path components</summary>
    static IEnumerable<byte[]> SplitPath(byte[] rawName, byte separator)
    {
        var start = 0;

        for(var i = 0; i <= rawName.Length; i++)
        {
            if(i < rawName.Length && rawName[i] != separator) continue;

            byte[] component = new byte[i - start];
            Array.Copy(rawName, start, component, 0, i - start);

            yield return component;

            start = i + 1;
        }
    }
}