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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Helpers;
using Schemas;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;
using FileSystemInfo = Aaru.CommonTypes.Structs.FileSystemInfo;

namespace Aaru.Filesystems
{
    internal partial class CPM
    {
        public Errno Mount(IMediaImage imagePlugin, Partition partition, Encoding encoding,
                           Dictionary<string, string> options, string @namespace)
        {
            device   = imagePlugin;
            Encoding = encoding ?? Encoding.GetEncoding("IBM437");

            // As the identification is so complex, just call Identify() and relay on its findings
            if(!Identify(device, partition) ||
               !cpmFound                    ||
               workingDefinition == null    ||
               dpb               == null)
                return Errno.InvalidArgument;

            // Build the software interleaving sector mask
            if(workingDefinition.sides == 1)
            {
                sectorMask = new int[workingDefinition.side1.sectorIds.Length];

                for(int m = 0; m < sectorMask.Length; m++)
                    sectorMask[m] = workingDefinition.side1.sectorIds[m] - workingDefinition.side1.sectorIds[0];
            }
            else
            {
                // Head changes after every track
                if(string.Compare(workingDefinition.order, "SIDES", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    sectorMask = new int[workingDefinition.side1.sectorIds.Length +
                                         workingDefinition.side2.sectorIds.Length];

                    for(int m = 0; m < workingDefinition.side1.sectorIds.Length; m++)
                        sectorMask[m] = workingDefinition.side1.sectorIds[m] - workingDefinition.side1.sectorIds[0];

                    // Skip first track (first side)
                    for(int m = 0; m < workingDefinition.side2.sectorIds.Length; m++)
                        sectorMask[m + workingDefinition.side1.sectorIds.Length] =
                            (workingDefinition.side2.sectorIds[m] - workingDefinition.side2.sectorIds[0]) +
                            workingDefinition.side1.sectorIds.Length;
                }

                // Head changes after whole side
                else if(string.Compare(workingDefinition.order, "CYLINDERS",
                                       StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    for(int m = 0; m < workingDefinition.side1.sectorIds.Length; m++)
                        sectorMask[m] = workingDefinition.side1.sectorIds[m] - workingDefinition.side1.sectorIds[0];

                    // Skip first track (first side) and first track (second side)
                    for(int m = 0; m < workingDefinition.side1.sectorIds.Length; m++)
                        sectorMask[m + workingDefinition.side1.sectorIds.Length] =
                            (workingDefinition.side1.sectorIds[m] - workingDefinition.side1.sectorIds[0]) +
                            workingDefinition.side1.sectorIds.Length + workingDefinition.side2.sectorIds.Length;

                    // TODO: Implement CYLINDERS ordering
                    AaruConsole.DebugWriteLine("CP/M Plugin", "CYLINDERS ordering not yet implemented.");

                    return Errno.NotImplemented;
                }

                // TODO: Implement COLUMBIA ordering
                else if(string.Compare(workingDefinition.order, "COLUMBIA",
                                       StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    AaruConsole.DebugWriteLine("CP/M Plugin",
                                               "Don't know how to handle COLUMBIA ordering, not proceeding with this definition.");

                    return Errno.NotImplemented;
                }

                // TODO: Implement EAGLE ordering
                else if(string.Compare(workingDefinition.order, "EAGLE", StringComparison.InvariantCultureIgnoreCase) ==
                        0)
                {
                    AaruConsole.DebugWriteLine("CP/M Plugin",
                                               "Don't know how to handle EAGLE ordering, not proceeding with this definition.");

                    return Errno.NotImplemented;
                }
                else
                {
                    AaruConsole.DebugWriteLine("CP/M Plugin",
                                               "Unknown order type \"{0}\", not proceeding with this definition.",
                                               workingDefinition.order);

                    return Errno.NotSupported;
                }
            }

            // Deinterleave whole volume
            Dictionary<ulong, byte[]> deinterleavedSectors = new Dictionary<ulong, byte[]>();

            if(workingDefinition.sides                                                                       == 1 ||
               string.Compare(workingDefinition.order, "SIDES", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                AaruConsole.DebugWriteLine("CP/M Plugin", "Deinterleaving whole volume.");

                for(int p = 0; p <= (int)(partition.End - partition.Start); p++)
                {
                    byte[] readSector =
                        device.ReadSector((ulong)((int)partition.Start + ((p / sectorMask.Length) * sectorMask.Length) +
                                                  sectorMask[p               % sectorMask.Length]));

                    if(workingDefinition.complement)
                        for(int b = 0; b < readSector.Length; b++)
                            readSector[b] = (byte)(~readSector[b] & 0xFF);

                    deinterleavedSectors.Add((ulong)p, readSector);
                }
            }

            int                       blockSize        = 128 << dpb.bsh;
            var                       blockMs          = new MemoryStream();
            ulong                     blockNo          = 0;
            int                       sectorsPerBlock  = 0;
            Dictionary<ulong, byte[]> allocationBlocks = new Dictionary<ulong, byte[]>();

            AaruConsole.DebugWriteLine("CP/M Plugin", "Creating allocation blocks.");

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

            AaruConsole.DebugWriteLine("CP/M Plugin", "Reading directory.");

            int dirOff;
            int dirSectors = ((dpb.drm + 1) * 32) / workingDefinition.bytesPerSector;

            if(workingDefinition.sofs > 0)
                dirOff = workingDefinition.sofs;
            else
                dirOff = workingDefinition.ofs * workingDefinition.sectorsPerTrack;

            // Read the whole directory blocks
            var dirMs = new MemoryStream();

            for(int d = 0; d < dirSectors; d++)
            {
                deinterleavedSectors.TryGetValue((ulong)(d + dirOff), out byte[] sector);
                dirMs.Write(sector, 0, sector.Length);
            }

            byte[] directory = dirMs.ToArray();

            if(directory == null)
                return Errno.InvalidArgument;

            int    dirCnt = 0;
            string file1  = null;
            string file2  = null;
            string file3  = null;

            Dictionary<string, Dictionary<int, List<ushort>>> fileExtents =
                new Dictionary<string, Dictionary<int, List<ushort>>>();

            statCache = new Dictionary<string, FileEntryInfo>();
            cpmStat   = new FileSystemInfo();
            bool atime = false;
            dirList           = new List<string>();
            labelCreationDate = null;
            labelUpdateDate   = null;
            passwordCache     = new Dictionary<string, byte[]>();

            AaruConsole.DebugWriteLine("CP/M Plugin", "Traversing directory.");

            // For each directory entry
            for(int dOff = 0; dOff < directory.Length; dOff += 32)

                // Describes a file (does not support PDOS entries with user >= 16, because they're identical to password entries
                if((directory[dOff] & 0x7F) < 0x10)
                    if(allocationBlocks.Count > 256)
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

                        int entryNo = ((32 * entry.extentCounter) + entry.extentCounterHigh) / (dpb.exm + 1);

                        // Do we have a stat for the file already?
                        if(statCache.TryGetValue(filename, out FileEntryInfo fInfo))
                            statCache.Remove(filename);
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
                        statCache.Add(filename, fInfo);

                        // Add the file to the directory listing
                        if(!dirList.Contains(filename))
                            dirList.Add(filename);

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
                    }
                    else
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

                        int entryNo = ((32 * entry.extentCounterHigh) + entry.extentCounter) / (dpb.exm + 1);

                        // Do we have a stat for the file already?
                        if(statCache.TryGetValue(filename, out FileEntryInfo fInfo))
                            statCache.Remove(filename);
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
                        statCache.Add(filename, fInfo);

                        // Add the file to the directory listing
                        if(!dirList.Contains(filename))
                            dirList.Add(filename);

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
                    }

                // A password entry (or a file entry in PDOS, but this does not handle that case)
                else if((directory[dOff] & 0x7F) >= 0x10 &&
                        (directory[dOff] & 0x7F) < 0x20)
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

                    // Do not repeat passwords
                    if(passwordCache.ContainsKey(filename))
                        passwordCache.Remove(filename);

                    // Copy whole password entry
                    byte[] tmp = new byte[32];
                    Array.Copy(directory, dOff, tmp, 0, 32);
                    passwordCache.Add(filename, tmp);

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
                }

                // Volume label and password entry. Volume password is ignored.
                else
                    switch(directory[dOff] & 0x7F)
                    {
                        case 0x20:
                            LabelEntry labelEntry =
                                Marshal.ByteArrayToStructureLittleEndian<LabelEntry>(directory, dOff, 32);

                            // The volume label defines if one of the fields in CP/M 3 timestamp is a creation or an
                            // access time
                            atime |= (labelEntry.flags & 0x40) == 0x40;

                            label             = Encoding.ASCII.GetString(directory, dOff + 1, 11).Trim();
                            labelCreationDate = new byte[4];
                            labelUpdateDate   = new byte[4];
                            Array.Copy(directory, dOff + 24, labelCreationDate, 0, 4);
                            Array.Copy(directory, dOff + 28, labelUpdateDate, 0, 4);

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
                                    if(statCache.TryGetValue(file1, out fInfo))
                                        statCache.Remove(file1);
                                    else
                                        fInfo = new FileEntryInfo();

                                    if(atime)
                                        fInfo.AccessTime = DateHandlers.CpmToDateTime(dateEntry.date1);
                                    else
                                        fInfo.CreationTime = DateHandlers.CpmToDateTime(dateEntry.date1);

                                    fInfo.LastWriteTime = DateHandlers.CpmToDateTime(dateEntry.date2);

                                    statCache.Add(file1, fInfo);
                                }

                                if(!string.IsNullOrEmpty(file2))
                                {
                                    if(statCache.TryGetValue(file2, out fInfo))
                                        statCache.Remove(file2);
                                    else
                                        fInfo = new FileEntryInfo();

                                    if(atime)
                                        fInfo.AccessTime = DateHandlers.CpmToDateTime(dateEntry.date3);
                                    else
                                        fInfo.CreationTime = DateHandlers.CpmToDateTime(dateEntry.date3);

                                    fInfo.LastWriteTime = DateHandlers.CpmToDateTime(dateEntry.date4);

                                    statCache.Add(file2, fInfo);
                                }

                                if(!string.IsNullOrEmpty(file3))
                                {
                                    if(statCache.TryGetValue(file3, out fInfo))
                                        statCache.Remove(file3);
                                    else
                                        fInfo = new FileEntryInfo();

                                    if(atime)
                                        fInfo.AccessTime = DateHandlers.CpmToDateTime(dateEntry.date5);
                                    else
                                        fInfo.CreationTime = DateHandlers.CpmToDateTime(dateEntry.date5);

                                    fInfo.LastWriteTime = DateHandlers.CpmToDateTime(dateEntry.date6);

                                    statCache.Add(file3, fInfo);
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
                                    if(statCache.TryGetValue(file1, out fInfo))
                                        statCache.Remove(file1);
                                    else
                                        fInfo = new FileEntryInfo();

                                    byte[] ctime = new byte[4];
                                    ctime[0] = trdPartyDateEntry.create1[0];
                                    ctime[1] = trdPartyDateEntry.create1[1];

                                    fInfo.AccessTime    = DateHandlers.CpmToDateTime(trdPartyDateEntry.access1);
                                    fInfo.CreationTime  = DateHandlers.CpmToDateTime(ctime);
                                    fInfo.LastWriteTime = DateHandlers.CpmToDateTime(trdPartyDateEntry.modify1);

                                    statCache.Add(file1, fInfo);
                                }

                                if(!string.IsNullOrEmpty(file2))
                                {
                                    if(statCache.TryGetValue(file2, out fInfo))
                                        statCache.Remove(file2);
                                    else
                                        fInfo = new FileEntryInfo();

                                    byte[] ctime = new byte[4];
                                    ctime[0] = trdPartyDateEntry.create2[0];
                                    ctime[1] = trdPartyDateEntry.create2[1];

                                    fInfo.AccessTime    = DateHandlers.CpmToDateTime(trdPartyDateEntry.access2);
                                    fInfo.CreationTime  = DateHandlers.CpmToDateTime(ctime);
                                    fInfo.LastWriteTime = DateHandlers.CpmToDateTime(trdPartyDateEntry.modify2);

                                    statCache.Add(file2, fInfo);
                                }

                                if(!string.IsNullOrEmpty(file3))
                                {
                                    if(statCache.TryGetValue(file1, out fInfo))
                                        statCache.Remove(file3);
                                    else
                                        fInfo = new FileEntryInfo();

                                    byte[] ctime = new byte[4];
                                    ctime[0] = trdPartyDateEntry.create3[0];
                                    ctime[1] = trdPartyDateEntry.create3[1];

                                    fInfo.AccessTime    = DateHandlers.CpmToDateTime(trdPartyDateEntry.access3);
                                    fInfo.CreationTime  = DateHandlers.CpmToDateTime(ctime);
                                    fInfo.LastWriteTime = DateHandlers.CpmToDateTime(trdPartyDateEntry.modify3);

                                    statCache.Add(file3, fInfo);
                                }

                                file1  = null;
                                file2  = null;
                                file3  = null;
                                dirCnt = 0;
                            }

                            break;
                    }

            // Cache all files. As CP/M maximum volume size is 8 Mib
            // this should not be a problem
            AaruConsole.DebugWriteLine("CP/M Plugin", "Reading files.");
            long usedBlocks = 0;
            fileCache = new Dictionary<string, byte[]>();

            foreach(string filename in dirList)
            {
                var fileMs = new MemoryStream();

                if(statCache.TryGetValue(filename, out FileEntryInfo fInfo))
                    statCache.Remove(filename);

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
                cpmStat.Files++;
                usedBlocks += fInfo.Blocks;

                statCache.Add(filename, fInfo);
                fileCache.Add(filename, fileMs.ToArray());
            }

            decodedPasswordCache = new Dictionary<string, byte[]>();

            // For each stored password, store a decoded version of it
            if(passwordCache.Count > 0)
                foreach(KeyValuePair<string, byte[]> kvp in passwordCache)
                {
                    byte[] tmp = new byte[8];
                    Array.Copy(kvp.Value, 16, tmp, 0, 8);

                    for(int t = 0; t < 8; t++)
                        tmp[t] ^= kvp.Value[13];

                    decodedPasswordCache.Add(kvp.Key, tmp);
                }

            // Generate statfs.
            cpmStat.Blocks         = (ulong)(dpb.dsm + 1);
            cpmStat.FilenameLength = 11;
            cpmStat.Files          = (ulong)fileCache.Count;
            cpmStat.FreeBlocks     = cpmStat.Blocks - (ulong)usedBlocks;
            cpmStat.PluginId       = Id;
            cpmStat.Type           = "CP/M filesystem";

            // Generate XML info
            XmlFsType = new FileSystemType
            {
                Clusters              = cpmStat.Blocks,
                ClusterSize           = (uint)blockSize,
                Files                 = (ulong)fileCache.Count,
                FilesSpecified        = true,
                FreeClusters          = cpmStat.FreeBlocks,
                FreeClustersSpecified = true,
                Type                  = "CP/M filesystem"
            };

            if(labelCreationDate != null)
            {
                XmlFsType.CreationDate          = DateHandlers.CpmToDateTime(labelCreationDate);
                XmlFsType.CreationDateSpecified = true;
            }

            if(labelUpdateDate != null)
            {
                XmlFsType.ModificationDate          = DateHandlers.CpmToDateTime(labelUpdateDate);
                XmlFsType.ModificationDateSpecified = true;
            }

            if(!string.IsNullOrEmpty(label))
                XmlFsType.VolumeName = label;

            mounted = true;

            return Errno.NoError;
        }

        /// <inheritdoc />
        /// <summary>Gets information about the mounted volume.</summary>
        /// <param name="stat">Information about the mounted volume.</param>
        public Errno StatFs(out FileSystemInfo stat)
        {
            stat = null;

            if(!mounted)
                return Errno.AccessDenied;

            stat = cpmStat;

            return Errno.NoError;
        }

        public Errno Unmount()
        {
            mounted              = false;
            definitions          = null;
            cpmFound             = false;
            workingDefinition    = null;
            dpb                  = null;
            sectorMask           = null;
            label                = null;
            thirdPartyTimestamps = false;
            standardTimestamps   = false;
            labelCreationDate    = null;
            labelUpdateDate      = null;

            return Errno.NoError;
        }
    }
}