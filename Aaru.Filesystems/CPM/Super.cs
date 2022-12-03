// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Super.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : CP/M filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Handles mounting and umounting the CP/M filesystem.
//     Caches the whole volume on mounting (shouldn't be a problem, maximum
//     volume size for CP/M is 8 MiB).
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
using System.IO;
using System.Linq;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Helpers;
using Schemas;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;
using FileSystemInfo = Aaru.CommonTypes.Structs.FileSystemInfo;

namespace Aaru.Filesystems;

public sealed partial class CPM
{
    /// <inheritdoc />
    public ErrorNumber Mount(IMediaImage imagePlugin, Partition partition, Encoding encoding,
                             Dictionary<string, string> options, string @namespace)
    {
        _device  = imagePlugin;
        Encoding = encoding ?? Encoding.GetEncoding("IBM437");

        // As the identification is so complex, just call Identify() and relay on its findings
        if(!Identify(_device, partition) ||
           !_cpmFound                    ||
           _workingDefinition == null    ||
           _dpb               == null)
            return ErrorNumber.InvalidArgument;

        // Build the software interleaving sector mask
        if(_workingDefinition.sides == 1)
        {
            _sectorMask = new int[_workingDefinition.side1.sectorIds.Length];

            for(int m = 0; m < _sectorMask.Length; m++)
                _sectorMask[m] = _workingDefinition.side1.sectorIds[m] - _workingDefinition.side1.sectorIds[0];
        }
        else
        {
            // Head changes after every track
            if(string.Compare(_workingDefinition.order, "SIDES", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                _sectorMask = new int[_workingDefinition.side1.sectorIds.Length +
                                      _workingDefinition.side2.sectorIds.Length];

                for(int m = 0; m < _workingDefinition.side1.sectorIds.Length; m++)
                    _sectorMask[m] = _workingDefinition.side1.sectorIds[m] - _workingDefinition.side1.sectorIds[0];

                // Skip first track (first side)
                for(int m = 0; m < _workingDefinition.side2.sectorIds.Length; m++)
                    _sectorMask[m + _workingDefinition.side1.sectorIds.Length] =
                        _workingDefinition.side2.sectorIds[m] - _workingDefinition.side2.sectorIds[0] +
                        _workingDefinition.side1.sectorIds.Length;
            }

            // Head changes after whole side
            else if(string.Compare(_workingDefinition.order, "CYLINDERS",
                                   StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                for(int m = 0; m < _workingDefinition.side1.sectorIds.Length; m++)
                    _sectorMask[m] = _workingDefinition.side1.sectorIds[m] - _workingDefinition.side1.sectorIds[0];

                // Skip first track (first side) and first track (second side)
                for(int m = 0; m < _workingDefinition.side1.sectorIds.Length; m++)
                    _sectorMask[m + _workingDefinition.side1.sectorIds.Length] =
                        _workingDefinition.side1.sectorIds[m] - _workingDefinition.side1.sectorIds[0] +
                        _workingDefinition.side1.sectorIds.Length + _workingDefinition.side2.sectorIds.Length;

                // TODO: Implement CYLINDERS ordering
                AaruConsole.DebugWriteLine("CP/M Plugin", Localization.CYLINDERS_ordering_not_yet_implemented);

                return ErrorNumber.NotImplemented;
            }

            // TODO: Implement COLUMBIA ordering
            else if(string.Compare(_workingDefinition.order, "COLUMBIA", StringComparison.InvariantCultureIgnoreCase) ==
                    0)
            {
                AaruConsole.DebugWriteLine("CP/M Plugin",
                                           Localization.
                                               Dont_know_how_to_handle_COLUMBIA_ordering_not_proceeding_with_this_definition);

                return ErrorNumber.NotImplemented;
            }

            // TODO: Implement EAGLE ordering
            else if(string.Compare(_workingDefinition.order, "EAGLE", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                AaruConsole.DebugWriteLine("CP/M Plugin",
                                           Localization.
                                               Don_know_how_to_handle_EAGLE_ordering_not_proceeding_with_this_definition);

                return ErrorNumber.NotImplemented;
            }
            else
            {
                AaruConsole.DebugWriteLine("CP/M Plugin",
                                           Localization.Unknown_order_type_0_not_proceeding_with_this_definition,
                                           _workingDefinition.order);

                return ErrorNumber.NotSupported;
            }
        }

        // Deinterleave whole volume
        Dictionary<ulong, byte[]> deinterleavedSectors = new();

        if(_workingDefinition.sides                                                                       == 1 ||
           string.Compare(_workingDefinition.order, "SIDES", StringComparison.InvariantCultureIgnoreCase) == 0)
        {
            AaruConsole.DebugWriteLine("CP/M Plugin", Localization.Deinterleaving_whole_volume);

            for(int p = 0; p <= (int)(partition.End - partition.Start); p++)
            {
                ErrorNumber errno =
                    _device.ReadSector((ulong)((int)partition.Start + (p / _sectorMask.Length * _sectorMask.Length) + _sectorMask[p % _sectorMask.Length]),
                                       out byte[] readSector);

                if(errno != ErrorNumber.NoError)
                    return errno;

                if(_workingDefinition.complement)
                    for(int b = 0; b < readSector.Length; b++)
                        readSector[b] = (byte)(~readSector[b] & 0xFF);

                deinterleavedSectors.Add((ulong)p, readSector);
            }
        }

        int                       blockSize        = 128 << _dpb.bsh;
        var                       blockMs          = new MemoryStream();
        ulong                     blockNo          = 0;
        int                       sectorsPerBlock  = 0;
        Dictionary<ulong, byte[]> allocationBlocks = new();

        AaruConsole.DebugWriteLine("CP/M Plugin", Localization.Creating_allocation_blocks);

        // For each volume sector
        for(ulong a = 0; a < (ulong)deinterleavedSectors.Count; a++)
        {
            deinterleavedSectors.TryGetValue(a, out byte[] sector);

            // May it happen? Just in case, CP/M blocks are smaller than physical sectors
            if(sector.Length > blockSize)
                for(int i = 0; i < sector.Length / blockSize; i++)
                {
                    byte[] tmp = new byte[blockSize];
                    Array.Copy(sector, blockSize * i, tmp, 0, blockSize);
                    allocationBlocks.Add(blockNo++, tmp);
                }

            // CP/M blocks are larger than physical sectors
            else if(sector.Length < blockSize)
            {
                blockMs.Write(sector, 0, sector.Length);
                sectorsPerBlock++;

                if(sectorsPerBlock != blockSize / sector.Length)
                    continue;

                allocationBlocks.Add(blockNo++, blockMs.ToArray());
                sectorsPerBlock = 0;
                blockMs         = new MemoryStream();
            }

            // CP/M blocks are same size than physical sectors
            else
                allocationBlocks.Add(blockNo++, sector);
        }

        AaruConsole.DebugWriteLine("CP/M Plugin", Localization.Reading_directory);

        int dirOff;
        int dirSectors = (_dpb.drm + 1) * 32 / _workingDefinition.bytesPerSector;

        if(_workingDefinition.sofs > 0)
            dirOff = _workingDefinition.sofs;
        else
            dirOff = _workingDefinition.ofs * _workingDefinition.sectorsPerTrack;

        // Read the whole directory blocks
        var dirMs = new MemoryStream();

        for(int d = 0; d < dirSectors; d++)
        {
            deinterleavedSectors.TryGetValue((ulong)(d + dirOff), out byte[] sector);
            dirMs.Write(sector, 0, sector.Length);
        }

        byte[] directory = dirMs.ToArray();

        if(directory == null)
            return ErrorNumber.InvalidArgument;

        int    dirCnt = 0;
        string file1  = null;
        string file2  = null;
        string file3  = null;

        Dictionary<string, Dictionary<int, List<ushort>>> fileExtents = new();

        _statCache = new Dictionary<string, FileEntryInfo>();
        _cpmStat   = new FileSystemInfo();
        bool atime = false;
        _dirList           = new List<string>();
        _labelCreationDate = null;
        _labelUpdateDate   = null;
        _passwordCache     = new Dictionary<string, byte[]>();

        AaruConsole.DebugWriteLine("CP/M Plugin", Localization.Traversing_directory);

        // For each directory entry
        for(int dOff = 0; dOff < directory.Length; dOff += 32)

            switch(directory[dOff] & 0x7F)
            {
                // Describes a file (does not support PDOS entries with user >= 16, because they're identical to password entries
                case < 0x10 when allocationBlocks.Count > 256:
                {
                    DirectoryEntry16 entry =
                        Marshal.ByteArrayToStructureLittleEndian<DirectoryEntry16>(directory, dOff, 32);

                    bool hidden = (entry.statusUser & 0x80) == 0x80;
                    bool rdOnly = (entry.filename[0] & 0x80) == 0x80 || (entry.extension[0] & 0x80) == 0x80;
                    bool system = (entry.filename[1] & 0x80) == 0x80 || (entry.extension[2] & 0x80) == 0x80;

                    //bool backed = (entry.filename[3] & 0x80) == 0x80 || (entry.extension[3] & 0x80) == 0x80;
                    int user = entry.statusUser & 0x0F;

                    bool validEntry = true;

                    for(int i = 0; i < 8; i++)
                    {
                        entry.filename[i] &= 0x7F;
                        validEntry        &= entry.filename[i] >= 0x20;
                    }

                    for(int i = 0; i < 3; i++)
                    {
                        entry.extension[i] &= 0x7F;
                        validEntry         &= entry.extension[i] >= 0x20;
                    }

                    if(!validEntry)
                        continue;

                    string filename  = Encoding.ASCII.GetString(entry.filename).Trim();
                    string extension = Encoding.ASCII.GetString(entry.extension).Trim();

                    // If user is != 0, append user to name to have identical filenames
                    if(user > 0)
                        filename = $"{user:X1}:{filename}";

                    if(!string.IsNullOrEmpty(extension))
                        filename = filename + "." + extension;

                    filename = filename.Replace('/', '\u2215');

                    int entryNo = ((32 * entry.extentCounter) + entry.extentCounterHigh) / (_dpb.exm + 1);

                    // Do we have a stat for the file already?
                    if(_statCache.TryGetValue(filename, out FileEntryInfo fInfo))
                        _statCache.Remove(filename);
                    else
                        fInfo = new FileEntryInfo
                        {
                            Attributes = new FileAttributes()
                        };

                    // And any extent?
                    if(fileExtents.TryGetValue(filename, out Dictionary<int, List<ushort>> extentBlocks))
                        fileExtents.Remove(filename);
                    else
                        extentBlocks = new Dictionary<int, List<ushort>>();

                    // Do we already have this extent? Should never happen
                    if(extentBlocks.TryGetValue(entryNo, out List<ushort> blocks))
                        extentBlocks.Remove(entryNo);
                    else
                        blocks = new List<ushort>();

                    // Attributes
                    if(hidden)
                        fInfo.Attributes |= FileAttributes.Hidden;

                    if(rdOnly)
                        fInfo.Attributes |= FileAttributes.ReadOnly;

                    if(system)
                        fInfo.Attributes |= FileAttributes.System;

                    // Supposedly there is a value in the directory entry telling how many blocks are designated in
                    // this entry. However some implementations tend to do whatever they wish, but none will ever
                    // allocate block 0 for a file because that's where the directory resides.
                    // There is also a field telling how many bytes are used in the last block, but its meaning is
                    // non-standard so we must ignore it.
                    foreach(ushort blk in entry.allocations.Where(blk => !blocks.Contains(blk) && blk != 0))
                        blocks.Add(blk);

                    // Save the file
                    fInfo.UID = (ulong)user;
                    extentBlocks.Add(entryNo, blocks);
                    fileExtents.Add(filename, extentBlocks);
                    _statCache.Add(filename, fInfo);

                    // Add the file to the directory listing
                    if(!_dirList.Contains(filename))
                        _dirList.Add(filename);

                    // Count entries 3 by 3 for timestamps
                    switch(dirCnt % 3)
                    {
                        case 0:
                            file1 = filename;

                            break;
                        case 1:
                            file2 = filename;

                            break;
                        case 2:
                            file3 = filename;

                            break;
                    }

                    dirCnt++;

                    break;
                }
                case < 0x10:
                {
                    DirectoryEntry entry =
                        Marshal.ByteArrayToStructureLittleEndian<DirectoryEntry>(directory, dOff, 32);

                    bool hidden = (entry.statusUser & 0x80) == 0x80;
                    bool rdOnly = (entry.filename[0] & 0x80) == 0x80 || (entry.extension[0] & 0x80) == 0x80;
                    bool system = (entry.filename[1] & 0x80) == 0x80 || (entry.extension[2] & 0x80) == 0x80;

                    //bool backed = (entry.filename[3] & 0x80) == 0x80 || (entry.extension[3] & 0x80) == 0x80;
                    int user = entry.statusUser & 0x0F;

                    bool validEntry = true;

                    for(int i = 0; i < 8; i++)
                    {
                        entry.filename[i] &= 0x7F;
                        validEntry        &= entry.filename[i] >= 0x20;
                    }

                    for(int i = 0; i < 3; i++)
                    {
                        entry.extension[i] &= 0x7F;
                        validEntry         &= entry.extension[i] >= 0x20;
                    }

                    if(!validEntry)
                        continue;

                    string filename  = Encoding.ASCII.GetString(entry.filename).Trim();
                    string extension = Encoding.ASCII.GetString(entry.extension).Trim();

                    // If user is != 0, append user to name to have identical filenames
                    if(user > 0)
                        filename = $"{user:X1}:{filename}";

                    if(!string.IsNullOrEmpty(extension))
                        filename = filename + "." + extension;

                    filename = filename.Replace('/', '\u2215');

                    int entryNo = ((32 * entry.extentCounterHigh) + entry.extentCounter) / (_dpb.exm + 1);

                    // Do we have a stat for the file already?
                    if(_statCache.TryGetValue(filename, out FileEntryInfo fInfo))
                        _statCache.Remove(filename);
                    else
                        fInfo = new FileEntryInfo
                        {
                            Attributes = new FileAttributes()
                        };

                    // And any extent?
                    if(fileExtents.TryGetValue(filename, out Dictionary<int, List<ushort>> extentBlocks))
                        fileExtents.Remove(filename);
                    else
                        extentBlocks = new Dictionary<int, List<ushort>>();

                    // Do we already have this extent? Should never happen
                    if(extentBlocks.TryGetValue(entryNo, out List<ushort> blocks))
                        extentBlocks.Remove(entryNo);
                    else
                        blocks = new List<ushort>();

                    // Attributes
                    if(hidden)
                        fInfo.Attributes |= FileAttributes.Hidden;

                    if(rdOnly)
                        fInfo.Attributes |= FileAttributes.ReadOnly;

                    if(system)
                        fInfo.Attributes |= FileAttributes.System;

                    // Supposedly there is a value in the directory entry telling how many blocks are designated in
                    // this entry. However some implementations tend to do whatever they wish, but none will ever
                    // allocate block 0 for a file because that's where the directory resides.
                    // There is also a field telling how many bytes are used in the last block, but its meaning is
                    // non-standard so we must ignore it.
                    foreach(ushort blk in entry.allocations.Where(blk => !blocks.Contains(blk) && blk != 0))
                        blocks.Add(blk);

                    // Save the file
                    fInfo.UID = (ulong)user;
                    extentBlocks.Add(entryNo, blocks);
                    fileExtents.Add(filename, extentBlocks);
                    _statCache.Add(filename, fInfo);

                    // Add the file to the directory listing
                    if(!_dirList.Contains(filename))
                        _dirList.Add(filename);

                    // Count entries 3 by 3 for timestamps
                    switch(dirCnt % 3)
                    {
                        case 0:
                            file1 = filename;

                            break;
                        case 1:
                            file2 = filename;

                            break;
                        case 2:
                            file3 = filename;

                            break;
                    }

                    dirCnt++;

                    break;
                }

                // A password entry (or a file entry in PDOS, but this does not handle that case)
                case >= 0x10 and < 0x20:
                {
                    PasswordEntry entry = Marshal.ByteArrayToStructureLittleEndian<PasswordEntry>(directory, dOff, 32);

                    int user = entry.userNumber & 0x0F;

                    for(int i = 0; i < 8; i++)
                        entry.filename[i] &= 0x7F;

                    for(int i = 0; i < 3; i++)
                        entry.extension[i] &= 0x7F;

                    string filename  = Encoding.ASCII.GetString(entry.filename).Trim();
                    string extension = Encoding.ASCII.GetString(entry.extension).Trim();

                    // If user is != 0, append user to name to have identical filenames
                    if(user > 0)
                        filename = $"{user:X1}:{filename}";

                    if(!string.IsNullOrEmpty(extension))
                        filename = filename + "." + extension;

                    filename = filename.Replace('/', '\u2215');

                    // Do not repeat passwords
                    if(_passwordCache.ContainsKey(filename))
                        _passwordCache.Remove(filename);

                    // Copy whole password entry
                    byte[] tmp = new byte[32];
                    Array.Copy(directory, dOff, tmp, 0, 32);
                    _passwordCache.Add(filename, tmp);

                    // Count entries 3 by 3 for timestamps
                    switch(dirCnt % 3)
                    {
                        case 0:
                            file1 = filename;

                            break;
                        case 1:
                            file2 = filename;

                            break;
                        case 2:
                            file3 = filename;

                            break;
                    }

                    dirCnt++;

                    break;
                }

                // Volume label and password entry. Volume password is ignored.
                default:
                    switch(directory[dOff] & 0x7F)
                    {
                        case 0x20:
                            LabelEntry labelEntry =
                                Marshal.ByteArrayToStructureLittleEndian<LabelEntry>(directory, dOff, 32);

                            // The volume label defines if one of the fields in CP/M 3 timestamp is a creation or an
                            // access time
                            atime |= (labelEntry.flags & 0x40) == 0x40;

                            _label             = Encoding.ASCII.GetString(directory, dOff + 1, 11).Trim();
                            _labelCreationDate = new byte[4];
                            _labelUpdateDate   = new byte[4];
                            Array.Copy(directory, dOff + 24, _labelCreationDate, 0, 4);
                            Array.Copy(directory, dOff + 28, _labelUpdateDate, 0, 4);

                            // Count entries 3 by 3 for timestamps
                            switch(dirCnt % 3)
                            {
                                case 0:
                                    file1 = null;

                                    break;
                                case 1:
                                    file2 = null;

                                    break;
                                case 2:
                                    file3 = null;

                                    break;
                            }

                            dirCnt++;

                            break;
                        case 0x21:
                            if(directory[dOff + 10] == 0x00 &&
                               directory[dOff + 20] == 0x00 &&
                               directory[dOff + 30] == 0x00 &&
                               directory[dOff + 31] == 0x00)
                            {
                                DateEntry dateEntry =
                                    Marshal.ByteArrayToStructureLittleEndian<DateEntry>(directory, dOff, 32);

                                FileEntryInfo fInfo;

                                // Entry contains timestamps for last 3 entries, whatever the kind they are.
                                if(!string.IsNullOrEmpty(file1))
                                {
                                    if(_statCache.TryGetValue(file1, out fInfo))
                                        _statCache.Remove(file1);
                                    else
                                        fInfo = new FileEntryInfo();

                                    if(atime)
                                        fInfo.AccessTime = DateHandlers.CpmToDateTime(dateEntry.date1);
                                    else
                                        fInfo.CreationTime = DateHandlers.CpmToDateTime(dateEntry.date1);

                                    fInfo.LastWriteTime = DateHandlers.CpmToDateTime(dateEntry.date2);

                                    _statCache.Add(file1, fInfo);
                                }

                                if(!string.IsNullOrEmpty(file2))
                                {
                                    if(_statCache.TryGetValue(file2, out fInfo))
                                        _statCache.Remove(file2);
                                    else
                                        fInfo = new FileEntryInfo();

                                    if(atime)
                                        fInfo.AccessTime = DateHandlers.CpmToDateTime(dateEntry.date3);
                                    else
                                        fInfo.CreationTime = DateHandlers.CpmToDateTime(dateEntry.date3);

                                    fInfo.LastWriteTime = DateHandlers.CpmToDateTime(dateEntry.date4);

                                    _statCache.Add(file2, fInfo);
                                }

                                if(!string.IsNullOrEmpty(file3))
                                {
                                    if(_statCache.TryGetValue(file3, out fInfo))
                                        _statCache.Remove(file3);
                                    else
                                        fInfo = new FileEntryInfo();

                                    if(atime)
                                        fInfo.AccessTime = DateHandlers.CpmToDateTime(dateEntry.date5);
                                    else
                                        fInfo.CreationTime = DateHandlers.CpmToDateTime(dateEntry.date5);

                                    fInfo.LastWriteTime = DateHandlers.CpmToDateTime(dateEntry.date6);

                                    _statCache.Add(file3, fInfo);
                                }

                                file1  = null;
                                file2  = null;
                                file3  = null;
                                dirCnt = 0;
                            }

                            // However, if this byte is 0, timestamp is in Z80DOS or DOS+ format
                            else if(directory[dOff + 1] == 0x00)
                            {
                                TrdPartyDateEntry trdPartyDateEntry =
                                    Marshal.ByteArrayToStructureLittleEndian<TrdPartyDateEntry>(directory, dOff, 32);

                                FileEntryInfo fInfo;

                                // Entry contains timestamps for last 3 entries, whatever the kind they are.
                                if(!string.IsNullOrEmpty(file1))
                                {
                                    if(_statCache.TryGetValue(file1, out fInfo))
                                        _statCache.Remove(file1);
                                    else
                                        fInfo = new FileEntryInfo();

                                    byte[] ctime = new byte[4];
                                    ctime[0] = trdPartyDateEntry.create1[0];
                                    ctime[1] = trdPartyDateEntry.create1[1];

                                    fInfo.AccessTime    = DateHandlers.CpmToDateTime(trdPartyDateEntry.access1);
                                    fInfo.CreationTime  = DateHandlers.CpmToDateTime(ctime);
                                    fInfo.LastWriteTime = DateHandlers.CpmToDateTime(trdPartyDateEntry.modify1);

                                    _statCache.Add(file1, fInfo);
                                }

                                if(!string.IsNullOrEmpty(file2))
                                {
                                    if(_statCache.TryGetValue(file2, out fInfo))
                                        _statCache.Remove(file2);
                                    else
                                        fInfo = new FileEntryInfo();

                                    byte[] ctime = new byte[4];
                                    ctime[0] = trdPartyDateEntry.create2[0];
                                    ctime[1] = trdPartyDateEntry.create2[1];

                                    fInfo.AccessTime    = DateHandlers.CpmToDateTime(trdPartyDateEntry.access2);
                                    fInfo.CreationTime  = DateHandlers.CpmToDateTime(ctime);
                                    fInfo.LastWriteTime = DateHandlers.CpmToDateTime(trdPartyDateEntry.modify2);

                                    _statCache.Add(file2, fInfo);
                                }

                                if(!string.IsNullOrEmpty(file3))
                                {
                                    if(_statCache.TryGetValue(file1, out fInfo))
                                        _statCache.Remove(file3);
                                    else
                                        fInfo = new FileEntryInfo();

                                    byte[] ctime = new byte[4];
                                    ctime[0] = trdPartyDateEntry.create3[0];
                                    ctime[1] = trdPartyDateEntry.create3[1];

                                    fInfo.AccessTime    = DateHandlers.CpmToDateTime(trdPartyDateEntry.access3);
                                    fInfo.CreationTime  = DateHandlers.CpmToDateTime(ctime);
                                    fInfo.LastWriteTime = DateHandlers.CpmToDateTime(trdPartyDateEntry.modify3);

                                    _statCache.Add(file3, fInfo);
                                }

                                file1  = null;
                                file2  = null;
                                file3  = null;
                                dirCnt = 0;
                            }

                            break;
                    }

                    break;
            }

        // Cache all files. As CP/M maximum volume size is 8 Mib
        // this should not be a problem
        AaruConsole.DebugWriteLine("CP/M Plugin", "Reading files.");
        long usedBlocks = 0;
        _fileCache = new Dictionary<string, byte[]>();

        foreach(string filename in _dirList)
        {
            var fileMs = new MemoryStream();

            if(_statCache.TryGetValue(filename, out FileEntryInfo fInfo))
                _statCache.Remove(filename);

            fInfo.Blocks = 0;

            if(fileExtents.TryGetValue(filename, out Dictionary<int, List<ushort>> extents))
                for(int ex = 0; ex < extents.Count; ex++)
                {
                    if(!extents.TryGetValue(ex, out List<ushort> alBlks))
                        continue;

                    foreach(ushort alBlk in alBlks)
                    {
                        allocationBlocks.TryGetValue(alBlk, out byte[] blk);
                        fileMs.Write(blk, 0, blk.Length);
                        fInfo.Blocks++;
                    }
                }

            // If you insist to call CP/M "extent based"
            fInfo.Attributes |= FileAttributes.Extents;
            fInfo.BlockSize  =  blockSize;
            fInfo.Length     =  fileMs.Length;
            _cpmStat.Files++;
            usedBlocks += fInfo.Blocks;

            _statCache.Add(filename, fInfo);
            _fileCache.Add(filename, fileMs.ToArray());
        }

        _decodedPasswordCache = new Dictionary<string, byte[]>();

        // For each stored password, store a decoded version of it
        if(_passwordCache.Count > 0)
            foreach(KeyValuePair<string, byte[]> kvp in _passwordCache)
            {
                byte[] tmp = new byte[8];
                Array.Copy(kvp.Value, 16, tmp, 0, 8);

                for(int t = 0; t < 8; t++)
                    tmp[t] ^= kvp.Value[13];

                _decodedPasswordCache.Add(kvp.Key, tmp);
            }

        // Generate statfs.
        _cpmStat.Blocks         = (ulong)(_dpb.dsm + 1);
        _cpmStat.FilenameLength = 11;
        _cpmStat.Files          = (ulong)_fileCache.Count;
        _cpmStat.FreeBlocks     = _cpmStat.Blocks - (ulong)usedBlocks;
        _cpmStat.PluginId       = Id;
        _cpmStat.Type           = FS_TYPE;

        // Generate XML info
        XmlFsType = new FileSystemType
        {
            Clusters              = _cpmStat.Blocks,
            ClusterSize           = (uint)blockSize,
            Files                 = (ulong)_fileCache.Count,
            FilesSpecified        = true,
            FreeClusters          = _cpmStat.FreeBlocks,
            FreeClustersSpecified = true,
            Type                  = FS_TYPE
        };

        if(_labelCreationDate != null)
        {
            XmlFsType.CreationDate          = DateHandlers.CpmToDateTime(_labelCreationDate);
            XmlFsType.CreationDateSpecified = true;
        }

        if(_labelUpdateDate != null)
        {
            XmlFsType.ModificationDate          = DateHandlers.CpmToDateTime(_labelUpdateDate);
            XmlFsType.ModificationDateSpecified = true;
        }

        if(!string.IsNullOrEmpty(_label))
            XmlFsType.VolumeName = _label;

        _mounted = true;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber StatFs(out FileSystemInfo stat)
    {
        stat = null;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        stat = _cpmStat;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Unmount()
    {
        _mounted              = false;
        _definitions          = null;
        _cpmFound             = false;
        _workingDefinition    = null;
        _dpb                  = null;
        _sectorMask           = null;
        _label                = null;
        _thirdPartyTimestamps = false;
        _standardTimestamps   = false;
        _labelCreationDate    = null;
        _labelUpdateDate      = null;

        return ErrorNumber.NoError;
    }
}