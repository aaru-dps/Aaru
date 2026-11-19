// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Mount.cs
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
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using Aaru.Logging;
using FileSystemInfo = Aaru.CommonTypes.Structs.FileSystemInfo;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

public sealed partial class MDFS
{
    /// <inheritdoc />
    public ErrorNumber Mount(IMediaImage                imagePlugin, Partition partition, Encoding encoding,
                             Dictionary<string, string> options,     string    @namespace)
    {
        _image           = imagePlugin;
        _partition       = partition;
        _sectorsPerBlock = GetSectorsPerBlock(imagePlugin);

        if(_sectorsPerBlock == 0) return ErrorNumber.InvalidArgument;

        if(!FindVolumeDescriptor(imagePlugin, partition, _sectorsPerBlock, out _vd, out _))
            return ErrorNumber.InvalidArgument;

        switch(@namespace?.ToLowerInvariant())
        {
            case null:
            case "long":
                _dosNames = false;

                break;
            case "dos":
                _dosNames = true;

                break;
            default:
                return ErrorNumber.InvalidArgument;
        }

        _encoding  = encoding;
        _allocSize = _vd.AllocSize;

        ErrorNumber errno = ReadManagementTable();

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Could not read management table: {0}", errno);

            return errno;
        }

        _entries  = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
        _children = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        _root = new Entry
        {
            Attributes        = ATTR_DIRECTORY,
            DirectoryLocation = _vd.DrbLoc,
            Length            = _vd.DirLen,
            Id                = 2
        };

        errno = ReadDirectoryTree();

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Could not read root directory: {0}", errno);

            return errno;
        }

        Metadata = BuildMetadata(_vd, _encoding);

        _statfs = new FileSystemInfo
        {
            Blocks         = _vd.NumAlloc,
            FilenameLength = MAX_FILENAME_LENGTH,
            Files          = (ulong)_entries.Count,
            FreeBlocks     = _vd.NumAvailable,
            PluginId       = Id,
            Type           = FS_TYPE
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
        _mtb      = null;

        return ErrorNumber.NoError;
    }

    /// <summary>Reads the management table, which describes each sector of the volume management area</summary>
    ErrorNumber ReadManagementTable()
    {
        _mtb = new uint[_vd.MtbNum * (BLOCK_SIZE / 4)];

        for(uint i = 0; i < _vd.MtbNum; i++)
        {
            ErrorNumber errno = ReadVmaSector(_vd.MtbLoc + i, out byte[] sector);

            if(errno != ErrorNumber.NoError) return errno;

            for(int j = 0; j < BLOCK_SIZE / 4; j++)
                _mtb[i * (BLOCK_SIZE / 4) + j] = BigEndianBitConverter.ToUInt32(sector, j * 4);
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Gets the sectors of a directory, following the management table chain</summary>
    List<uint> GetDirectorySectors(Entry directory)
    {
        List<uint>    sectors  = [];
        HashSet<uint> visited  = [];
        uint          current  = directory.DirectoryLocation;
        uint          expected = Math.Max(1, (directory.Length + BLOCK_SIZE - 1) / BLOCK_SIZE);

        while(current < _mtb.Length && visited.Add(current) && sectors.Count < expected)
        {
            sectors.Add(current);

            uint tag = _mtb[current] >> 24;

            // First and middle sectors link to the next one; the last one and single sector directories carry the id
            if(tag != MTB_DIRECTORY_FIRST && tag != MTB_DIRECTORY_MIDDLE) break;

            current = _mtb[current] & 0xFFFF;
        }

        if(sectors.Count * BLOCK_SIZE != directory.Length)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Directory at {0} has {1} sectors but a length of {2} bytes",
                              directory.DirectoryLocation,
                              sectors.Count,
                              directory.Length);
        }

        return sectors;
    }

    /// <summary>Reads the directory tree, breadth first, as it can be hundreds of levels deep</summary>
    ErrorNumber ReadDirectoryTree()
    {
        HashSet<uint>                       visited = [];
        Queue<(string path, Entry directory)> pending = new();
        pending.Enqueue(("", _root));

        while(pending.Count > 0)
        {
            (string path, Entry directory) = pending.Dequeue();

            ErrorNumber errno = ReadDirectory(path, directory, visited, pending);

            // The root directory must be readable, a damaged subdirectory just loses its contents
            if(errno != ErrorNumber.NoError && path == "") return errno;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Reads a directory and queues its subdirectories</summary>
    /// <param name="path">Directory path without leading slash, empty for the root</param>
    /// <param name="directory">Directory entry</param>
    /// <param name="visited">Directories already read, to break loops</param>
    /// <param name="pending">Directories still to read</param>
    ErrorNumber ReadDirectory(string path, Entry directory, HashSet<uint> visited,
                              Queue<(string path, Entry directory)> pending)
    {
        List<string> children = [];
        _children[path] = children;

        if(!visited.Add(directory.DirectoryLocation)) return ErrorNumber.NoError;

        foreach(uint sectorNumber in GetDirectorySectors(directory))
        {
            ErrorNumber errno = ReadVmaSector(sectorNumber, out byte[] sector);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "Could not read directory sector {0}: {1}", sectorNumber, errno);

                if(path == "") return errno;

                continue;
            }

            int offset = 0;

            while(offset + 2 <= BLOCK_SIZE)
            {
                ushort recordLength = BigEndianBitConverter.ToUInt16(sector, offset + REC_LENGTH);

                if(recordLength == 0) break;

                if(recordLength < REC_DIR_NAME_LENGTH + 1 || offset + recordLength > BLOCK_SIZE)
                {
                    AaruLogging.Debug(MODULE_NAME,
                                      "Invalid record length {0} at offset {1} of sector {2}",
                                      recordLength,
                                      offset,
                                      sectorNumber);

                    break;
                }

                byte[] record = sector[offset..(offset + recordLength)];
                offset += recordLength;

                if(!DecodeRecord(record, out Entry entry, out string name)) continue;

                // The first record of each directory describes the directory itself
                if(record[REC_FCB] == 0x01)
                {
                    if(path == "") CopySelfRecord(entry, directory);

                    continue;
                }

                name = UniqueName(children, name);
                children.Add(name);

                string key = path == "" ? name : path + "/" + name;
                _entries[key] = entry;

                if(entry.IsDirectory) pending.Enqueue((key, entry));
            }
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Copies the times and attributes of the root directory from its self record</summary>
    static void CopySelfRecord(Entry self, Entry root)
    {
        root.Attributes       = (ushort)(self.Attributes | ATTR_DIRECTORY);
        root.CreationTime     = self.CreationTime;
        root.ModificationTime = self.ModificationTime;
        root.StatusTime       = self.StatusTime;
        root.Id               = self.Id;
        root.FinderInfo       = self.FinderInfo;
    }

    /// <summary>Decodes a directory record</summary>
    /// <returns><c>false</c> if the record is deleted, damaged or of an unknown kind</returns>
    bool DecodeRecord(byte[] record, out Entry entry, out string name)
    {
        entry = null;
        name  = null;

        ushort attributes = BigEndianBitConverter.ToUInt16(record, REC_ATTRIBUTES);

        if((attributes & (ATTR_DELETED | ATTR_UNKNOWN_EXTENT)) != 0) return false;

        bool isDirectory = (attributes & ATTR_DIRECTORY) != 0;
        int  nameLength  = isDirectory ? REC_DIR_NAME_LENGTH : REC_FILE_NAME_LENGTH;

        if(record.Length < nameLength + 1) return false;

        int nameStart = nameLength + 1;
        int nameBytes = record[nameLength];

        if(nameStart + nameBytes > record.Length) return false;

        entry = new Entry
        {
            Attributes       = attributes,
            CreationTime     = BigEndianBitConverter.ToUInt32(record, REC_CREATION),
            ModificationTime = BigEndianBitConverter.ToUInt32(record, REC_MODIFICATION),
            StatusTime       = BigEndianBitConverter.ToUInt32(record, REC_STATUS),
            Id               = BigEndianBitConverter.ToUInt32(record, REC_ID),
            Length           = BigEndianBitConverter.ToUInt32(record, REC_LENGTH_BYTES)
        };

        if(isDirectory)
            entry.DirectoryLocation = BigEndianBitConverter.ToUInt16(record, REC_DIR_LOCATION);
        else
        {
            entry.DataLocation     = BigEndianBitConverter.ToUInt32(record, REC_DATA_LOCATION);
            entry.DataCount        = BigEndianBitConverter.ToUInt32(record, REC_DATA_COUNT);
            entry.ResourceLength   = BigEndianBitConverter.ToUInt32(record, REC_RESOURCE_LENGTH);
            entry.ResourceLocation = BigEndianBitConverter.ToUInt32(record, REC_RESOURCE_LOCATION);
            entry.ResourceCount    = BigEndianBitConverter.ToUInt32(record, REC_RESOURCE_COUNT);
        }

        // Mac OS stores the Finder information after the name, at an even offset
        int extension = nameStart + nameBytes + 1 & ~1;

        if(extension + 4 + FINDER_INFO_LENGTH <= record.Length                                     &&
           BigEndianBitConverter.ToUInt16(record, extension) == MAC_EXTENSION_LENGTH &&
           record[extension + 2]                               == 1)
            entry.FinderInfo = record[(extension + 4)..(extension + 4 + FINDER_INFO_LENGTH)];

        byte charset = record[REC_CHARSET];

        ReadOnlySpan<byte> fcb = record.AsSpan(REC_FCB, FCB_LENGTH);

        name = _dosNames || nameBytes == 0
                   ? DecodeFcb(fcb, charset)
                   : DecodeString(record.AsSpan(nameStart, nameBytes), charset, _encoding);

        // Path separator, Mac OS allows it in names
        name = name.Replace('/', ':');

        if(name == "") name = $"{entry.Id:X8}";

        return true;
    }

    /// <summary>Decodes an 8.3 name, space padded and without the dot</summary>
    string DecodeFcb(ReadOnlySpan<byte> fcb, byte charset)
    {
        Encoding encoding = GetCharsetEncoding(charset, _encoding);

        string baseName  = encoding.GetString(fcb[..8]).TrimEnd(' ', '\0');
        string extension = encoding.GetString(fcb[8..]).TrimEnd(' ', '\0');

        return extension == "" ? baseName : baseName + "." + extension;
    }

    /// <summary>Makes a name unique in a directory, as 8.3 names of different files can collide</summary>
    static string UniqueName(List<string> children, string name)
    {
        if(!children.Contains(name, StringComparer.OrdinalIgnoreCase)) return name;

        for(int i = 1;; i++)
        {
            string candidate = $"{name}~{i}";

            if(!children.Contains(candidate, StringComparer.OrdinalIgnoreCase)) return candidate;
        }
    }
}