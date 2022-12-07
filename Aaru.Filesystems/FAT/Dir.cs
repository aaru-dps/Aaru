// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dir.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Microsoft FAT filesystem plugin.
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
using System.Globalization;
using System.Linq;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.Filesystems;

public sealed partial class FAT
{
    /// <inheritdoc />
    /// <summary>Solves a symbolic link.</summary>
    /// <param name="path">Link path.</param>
    /// <param name="dest">Link destination.</param>
    public ErrorNumber ReadLink(string path, out string dest)
    {
        dest = null;

        return ErrorNumber.NotSupported;
    }

    /// <inheritdoc />
    /// <summary>Lists contents from a directory.</summary>
    /// <param name="path">Directory path.</param>
    /// <param name="contents">Directory contents.</param>
    public ErrorNumber ReadDir(string path, out List<string> contents)
    {
        contents = null;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        if(string.IsNullOrWhiteSpace(path) ||
           path == "/")
        {
            contents = _rootDirectoryCache.Keys.ToList();

            return ErrorNumber.NoError;
        }

        string cutPath = path.StartsWith("/", StringComparison.Ordinal) ? path[1..].ToLower(_cultureInfo)
                             : path.ToLower(_cultureInfo);

        if(_directoryCache.TryGetValue(cutPath, out Dictionary<string, CompleteDirectoryEntry> currentDirectory))
        {
            contents = currentDirectory.Keys.ToList();

            return ErrorNumber.NoError;
        }

        string[] pieces = cutPath.Split(new[]
        {
            '/'
        }, StringSplitOptions.RemoveEmptyEntries);

        KeyValuePair<string, CompleteDirectoryEntry> entry =
            _rootDirectoryCache.FirstOrDefault(t => t.Key.ToLower(_cultureInfo) == pieces[0]);

        if(string.IsNullOrEmpty(entry.Key))
            return ErrorNumber.NoSuchFile;

        if(!entry.Value.Dirent.attributes.HasFlag(FatAttributes.Subdirectory))
            return ErrorNumber.NotDirectory;

        string currentPath = pieces[0];

        currentDirectory = _rootDirectoryCache;

        for(int p = 0; p < pieces.Length; p++)
        {
            entry = currentDirectory.FirstOrDefault(t => t.Key.ToLower(_cultureInfo) == pieces[p]);

            if(string.IsNullOrEmpty(entry.Key))
                return ErrorNumber.NoSuchFile;

            if(!entry.Value.Dirent.attributes.HasFlag(FatAttributes.Subdirectory))
                return ErrorNumber.NotDirectory;

            currentPath = p == 0 ? pieces[0] : $"{currentPath}/{pieces[p]}";
            uint currentCluster = entry.Value.Dirent.start_cluster;

            if(_fat32)
                currentCluster += (uint)(entry.Value.Dirent.ea_handle << 16);

            if(_directoryCache.TryGetValue(currentPath, out currentDirectory))
                continue;

            // Reserved unallocated directory, seen in Atari ST
            if(currentCluster == 0)
            {
                _directoryCache[currentPath] = new Dictionary<string, CompleteDirectoryEntry>();
                contents                     = new List<string>();

                return ErrorNumber.NoError;
            }

            uint[] clusters = GetClusters(currentCluster);

            if(clusters is null)
                return ErrorNumber.InvalidArgument;

            byte[] directoryBuffer = new byte[_bytesPerCluster * clusters.Length];

            for(int i = 0; i < clusters.Length; i++)
            {
                ErrorNumber errno = _image.ReadSectors(_firstClusterSector + (clusters[i] * _sectorsPerCluster),
                                                       _sectorsPerCluster, out byte[] buffer);

                if(errno != ErrorNumber.NoError)
                    return errno;

                Array.Copy(buffer, 0, directoryBuffer, i * _bytesPerCluster, _bytesPerCluster);
            }

            currentDirectory = new Dictionary<string, CompleteDirectoryEntry>();
            byte[] lastLfnName     = null;
            byte   lastLfnChecksum = 0;

            for(int pos = 0; pos < directoryBuffer.Length; pos += Marshal.SizeOf<DirectoryEntry>())
            {
                DirectoryEntry dirent =
                    Marshal.ByteArrayToStructureLittleEndian<DirectoryEntry>(directoryBuffer, pos,
                                                                             Marshal.SizeOf<DirectoryEntry>());

                if(dirent.filename[0] == DIRENT_FINISHED)
                    break;

                if(dirent.attributes.HasFlag(FatAttributes.LFN))
                {
                    if(_namespace != Namespace.Lfn &&
                       _namespace != Namespace.Ecs)
                        continue;

                    LfnEntry lfnEntry =
                        Marshal.ByteArrayToStructureLittleEndian<LfnEntry>(directoryBuffer, pos,
                                                                           Marshal.SizeOf<LfnEntry>());

                    int lfnSequence = lfnEntry.sequence & LFN_MASK;

                    if((lfnEntry.sequence & LFN_ERASED) > 0)
                        continue;

                    if((lfnEntry.sequence & LFN_LAST) > 0)
                    {
                        lastLfnName     = new byte[lfnSequence * 26];
                        lastLfnChecksum = lfnEntry.checksum;
                    }

                    if(lastLfnName is null)
                        continue;

                    if(lfnEntry.checksum != lastLfnChecksum)
                        continue;

                    lfnSequence--;

                    Array.Copy(lfnEntry.name1, 0, lastLfnName, lfnSequence * 26, 10);
                    Array.Copy(lfnEntry.name2, 0, lastLfnName, (lfnSequence * 26) + 10, 12);
                    Array.Copy(lfnEntry.name3, 0, lastLfnName, (lfnSequence * 26) + 22, 4);

                    continue;
                }

                // Not a correct entry
                if(dirent.filename[0] < DIRENT_MIN &&
                   dirent.filename[0] != DIRENT_E5)
                    continue;

                // Self
                if(Encoding.GetString(dirent.filename).TrimEnd() == ".")
                    continue;

                // Parent
                if(Encoding.GetString(dirent.filename).TrimEnd() == "..")
                    continue;

                // Deleted
                if(dirent.filename[0] == DIRENT_DELETED)
                    continue;

                string filename;

                if(dirent.attributes.HasFlag(FatAttributes.VolumeLabel))
                    continue;

                var completeEntry = new CompleteDirectoryEntry
                {
                    Dirent = dirent
                };

                if(_namespace is Namespace.Lfn or Namespace.Ecs &&
                   lastLfnName != null)
                {
                    byte calculatedLfnChecksum = LfnChecksum(dirent.filename, dirent.extension);

                    if(calculatedLfnChecksum == lastLfnChecksum)
                    {
                        filename = StringHandlers.CToString(lastLfnName, Encoding.Unicode, true);

                        completeEntry.Lfn = filename;
                        lastLfnName       = null;
                        lastLfnChecksum   = 0;
                    }
                }

                if(dirent.filename[0] == DIRENT_E5)
                    dirent.filename[0] = DIRENT_DELETED;

                string name      = Encoding.GetString(dirent.filename).TrimEnd();
                string extension = Encoding.GetString(dirent.extension).TrimEnd();

                if(name      == "" &&
                   extension == "")
                {
                    AaruConsole.DebugWriteLine("FAT filesystem", Localization.Found_empty_filename_in_0, path);

                    if(!_debug ||
                       dirent is { size: > 0, start_cluster: 0 })
                        continue; // Skip invalid name

                    // If debug, add it
                    name = ":{EMPTYNAME}:";

                    // Try to create a unique filename with an extension from 000 to 999
                    for(int uniq = 0; uniq < 1000; uniq++)
                    {
                        extension = $"{uniq:D03}";

                        if(!currentDirectory.ContainsKey($"{name}.{extension}"))
                            break;
                    }

                    // If we couldn't find it, just skip over
                    if(currentDirectory.ContainsKey($"{name}.{extension}"))
                        continue;
                }

                if(_namespace == Namespace.Nt)
                {
                    if(dirent.caseinfo.HasFlag(CaseInfo.LowerCaseExtension))
                        extension = extension.ToLower(CultureInfo.CurrentCulture);

                    if(dirent.caseinfo.HasFlag(CaseInfo.LowerCaseBasename))
                        name = name.ToLower(CultureInfo.CurrentCulture);
                }

                if(extension != "")
                    filename = name + "." + extension;
                else
                    filename = name;

                if(_namespace == Namespace.Human)
                {
                    HumanDirectoryEntry humanEntry =
                        Marshal.ByteArrayToStructureLittleEndian<HumanDirectoryEntry>(directoryBuffer, pos,
                            Marshal.SizeOf<HumanDirectoryEntry>());

                    completeEntry.HumanDirent = humanEntry;

                    name      = StringHandlers.CToString(humanEntry.name1, Encoding).TrimEnd();
                    extension = StringHandlers.CToString(humanEntry.extension, Encoding).TrimEnd();
                    string name2 = StringHandlers.CToString(humanEntry.name2, Encoding).TrimEnd();

                    if(extension != "")
                        filename = name + name2 + "." + extension;
                    else
                        filename = name + name2;

                    completeEntry.HumanName = filename;
                }

                // Atari ST allows slash AND colon so cannot simply substitute one for the other like in Mac filesystems
                filename = filename.Replace('/', '\u2215');

                // Using array accessor ensures that repeated entries just get substituted.
                // Repeated entries are not allowed but some bad implementations (e.g. FAT32.IFS)allow to create them
                // when using spaces
                completeEntry.Shortname                    = filename;
                currentDirectory[completeEntry.ToString()] = completeEntry;
            }

            // Check OS/2 .LONGNAME
            if(_eaCache != null                             &&
               _namespace is Namespace.Os2 or Namespace.Ecs &&
               !_fat32)
            {
                List<KeyValuePair<string, CompleteDirectoryEntry>> filesWithEas =
                    currentDirectory.Where(t => t.Value.Dirent.ea_handle != 0).ToList();

                foreach(KeyValuePair<string, CompleteDirectoryEntry> fileWithEa in filesWithEas)
                {
                    Dictionary<string, byte[]> eas = GetEas(fileWithEa.Value.Dirent.ea_handle);

                    if(eas is null)
                        continue;

                    if(!eas.TryGetValue("com.microsoft.os2.longname", out byte[] longnameEa))
                        continue;

                    if(BitConverter.ToUInt16(longnameEa, 0) != EAT_ASCII)
                        continue;

                    ushort longnameSize = BitConverter.ToUInt16(longnameEa, 2);

                    if(longnameSize + 4 > longnameEa.Length)
                        continue;

                    byte[] longnameBytes = new byte[longnameSize];

                    Array.Copy(longnameEa, 4, longnameBytes, 0, longnameSize);

                    string longname = StringHandlers.CToString(longnameBytes, Encoding);

                    if(string.IsNullOrWhiteSpace(longname))
                        continue;

                    // Forward slash is allowed in .LONGNAME, so change it to visually similar division slash
                    longname = longname.Replace('/', '\u2215');

                    fileWithEa.Value.Longname = longname;
                    currentDirectory.Remove(fileWithEa.Key);
                    currentDirectory[fileWithEa.Value.ToString()] = fileWithEa.Value;
                }
            }

            // Check FAT32.IFS EAs
            if(_fat32 || _debug)
            {
                List<KeyValuePair<string, CompleteDirectoryEntry>> fat32EaSidecars =
                    currentDirectory.Where(t => t.Key.EndsWith(FAT32_EA_TAIL, true, _cultureInfo)).ToList();

                foreach(KeyValuePair<string, CompleteDirectoryEntry> sidecar in fat32EaSidecars)
                {
                    // No real file this sidecar accompanies
                    if(!currentDirectory.TryGetValue(sidecar.Key[..^FAT32_EA_TAIL.Length],
                                                     out CompleteDirectoryEntry fileWithEa))
                        continue;

                    // If not in debug mode we will consider the lack of EA bitflags to mean the EAs are corrupted or not real
                    if(!_debug)
                        if(!fileWithEa.Dirent.caseinfo.HasFlag(CaseInfo.NormalEaOld) &&
                           !fileWithEa.Dirent.caseinfo.HasFlag(CaseInfo.CriticalEa)  &&
                           !fileWithEa.Dirent.caseinfo.HasFlag(CaseInfo.NormalEa)    &&
                           !fileWithEa.Dirent.caseinfo.HasFlag(CaseInfo.CriticalEa))
                            continue;

                    fileWithEa.Fat32Ea = sidecar.Value.Dirent;

                    if(!_debug)
                        currentDirectory.Remove(sidecar.Key);
                }
            }

            _directoryCache.Add(currentPath, currentDirectory);
        }

        contents = currentDirectory?.Keys.ToList();

        return ErrorNumber.NoError;
    }
}