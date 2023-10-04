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
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.Filesystems;

public sealed partial class FAT
{
#region IReadOnlyFilesystem Members

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
    public ErrorNumber OpenDir(string path, out IDirNode node)
    {
        node = null;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        if(string.IsNullOrWhiteSpace(path) || path == "/")
        {
            node = new FatDirNode
            {
                Path      = path,
                _position = 0,
                _entries  = _rootDirectoryCache.Values.ToArray()
            };

            return ErrorNumber.NoError;
        }

        string cutPath = path.StartsWith("/", StringComparison.Ordinal)
                             ? path[1..].ToLower(_cultureInfo)
                             : path.ToLower(_cultureInfo);

        if(_directoryCache.TryGetValue(cutPath, out Dictionary<string, CompleteDirectoryEntry> currentDirectory))
        {
            node = new FatDirNode
            {
                Path      = path,
                _position = 0,
                _entries  = currentDirectory.Values.ToArray()
            };

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

        for(var p = 0; p < pieces.Length; p++)
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

                node = new FatDirNode
                {
                    Path      = path,
                    _position = 0,
                    _entries  = Array.Empty<CompleteDirectoryEntry>()
                };

                return ErrorNumber.NoError;
            }

            uint[] clusters = GetClusters(currentCluster);

            if(clusters is null)
                return ErrorNumber.InvalidArgument;

            var directoryBuffer = new byte[_bytesPerCluster * clusters.Length];

            for(var i = 0; i < clusters.Length; i++)
            {
                ErrorNumber errno = _image.ReadSectors(_firstClusterSector + clusters[i] * _sectorsPerCluster,
                                                       _sectorsPerCluster, out byte[] buffer);

                if(errno != ErrorNumber.NoError)
                    return errno;

                Array.Copy(buffer, 0, directoryBuffer, i * _bytesPerCluster, _bytesPerCluster);
            }

            currentDirectory = new Dictionary<string, CompleteDirectoryEntry>();
            byte[] lastLfnName     = null;
            byte   lastLfnChecksum = 0;

            for(var pos = 0; pos < directoryBuffer.Length; pos += Marshal.SizeOf<DirectoryEntry>())
            {
                DirectoryEntry dirent =
                    Marshal.ByteArrayToStructureLittleEndian<DirectoryEntry>(directoryBuffer, pos,
                                                                             Marshal.SizeOf<DirectoryEntry>());

                if(dirent.filename[0] == DIRENT_FINISHED)
                    break;

                if(dirent.attributes.HasFlag(FatAttributes.LFN))
                {
                    if(_namespace != Namespace.Lfn && _namespace != Namespace.Ecs)
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

                    Array.Copy(lfnEntry.name1, 0, lastLfnName, lfnSequence * 26,      10);
                    Array.Copy(lfnEntry.name2, 0, lastLfnName, lfnSequence * 26 + 10, 12);
                    Array.Copy(lfnEntry.name3, 0, lastLfnName, lfnSequence * 26 + 22, 4);

                    continue;
                }

                // Not a correct entry
                if(dirent.filename[0] < DIRENT_MIN && dirent.filename[0] != DIRENT_E5)
                    continue;

                // Self
                if(_encoding.GetString(dirent.filename).TrimEnd() == ".")
                    continue;

                // Parent
                if(_encoding.GetString(dirent.filename).TrimEnd() == "..")
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

                if(_namespace is Namespace.Lfn or Namespace.Ecs && lastLfnName != null)
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

                string name      = _encoding.GetString(dirent.filename).TrimEnd();
                string extension = _encoding.GetString(dirent.extension).TrimEnd();

                if(name == "" && extension == "")
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_empty_filename_in_0, path);

                    if(!_debug || dirent is { size: > 0, start_cluster: 0 })
                        continue; // Skip invalid name

                    // If debug, add it
                    name = ":{EMPTYNAME}:";

                    // Try to create a unique filename with an extension from 000 to 999
                    for(var uniq = 0; uniq < 1000; uniq++)
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

                    name      = StringHandlers.CToString(humanEntry.name1,     _encoding).TrimEnd();
                    extension = StringHandlers.CToString(humanEntry.extension, _encoding).TrimEnd();
                    string name2 = StringHandlers.CToString(humanEntry.name2, _encoding).TrimEnd();

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
            if(_eaCache != null && _namespace is Namespace.Os2 or Namespace.Ecs && !_fat32)
            {
                var filesWithEas = currentDirectory.Where(t => t.Value.Dirent.ea_handle != 0).ToList();

                foreach(KeyValuePair<string, CompleteDirectoryEntry> fileWithEa in filesWithEas)
                {
                    Dictionary<string, byte[]> eas = GetEas(fileWithEa.Value.Dirent.ea_handle);

                    if(eas is null)
                        continue;

                    if(!eas.TryGetValue("com.microsoft.os2.longname", out byte[] longnameEa))
                        continue;

                    if(BitConverter.ToUInt16(longnameEa, 0) != EAT_ASCII)
                        continue;

                    var longnameSize = BitConverter.ToUInt16(longnameEa, 2);

                    if(longnameSize + 4 > longnameEa.Length)
                        continue;

                    var longnameBytes = new byte[longnameSize];

                    Array.Copy(longnameEa, 4, longnameBytes, 0, longnameSize);

                    string longname = StringHandlers.CToString(longnameBytes, _encoding);

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
                var fat32EaSidecars = currentDirectory.Where(t => t.Key.EndsWith(FAT32_EA_TAIL, true, _cultureInfo)).
                                                       ToList();

                foreach(KeyValuePair<string, CompleteDirectoryEntry> sidecar in fat32EaSidecars)
                {
                    // No real file this sidecar accompanies
                    if(!currentDirectory.TryGetValue(sidecar.Key[..^FAT32_EA_TAIL.Length],
                                                     out CompleteDirectoryEntry fileWithEa))
                        continue;

                    // If not in debug mode we will consider the lack of EA bitflags to mean the EAs are corrupted or not real
                    if(!_debug)
                    {
                        if(!fileWithEa.Dirent.caseinfo.HasFlag(CaseInfo.NormalEaOld) &&
                           !fileWithEa.Dirent.caseinfo.HasFlag(CaseInfo.CriticalEa)  &&
                           !fileWithEa.Dirent.caseinfo.HasFlag(CaseInfo.NormalEa)    &&
                           !fileWithEa.Dirent.caseinfo.HasFlag(CaseInfo.CriticalEa))
                            continue;
                    }

                    fileWithEa.Fat32Ea = sidecar.Value.Dirent;

                    if(!_debug)
                        currentDirectory.Remove(sidecar.Key);
                }
            }

            _directoryCache.Add(currentPath, currentDirectory);
        }

        if(currentDirectory is null)
            return ErrorNumber.NoSuchFile;

        node = new FatDirNode
        {
            Path      = path,
            _position = 0,
            _entries  = currentDirectory.Values.ToArray()
        };

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadDir(IDirNode node, out string filename)
    {
        filename = null;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        if(node is not FatDirNode mynode)
            return ErrorNumber.InvalidArgument;

        if(mynode._position < 0)
            return ErrorNumber.InvalidArgument;

        if(mynode._position >= mynode._entries.Length)
            return ErrorNumber.NoError;

        CompleteDirectoryEntry entry = mynode._entries[mynode._position];

        filename = _namespace switch
                   {
                       Namespace.Ecs when entry.Longname is not null                      => entry.Longname,
                       Namespace.Ecs when entry.Longname is null && entry.Lfn is not null => entry.Lfn,
                       Namespace.Lfn when entry.Lfn is not null                           => entry.Lfn,
                       Namespace.Human when entry.HumanName is not null                   => entry.HumanName,
                       Namespace.Os2 when entry.Longname is not null                      => entry.Longname,
                       _                                                                  => entry.Shortname
                   };

        mynode._position++;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseDir(IDirNode node)
    {
        if(node is not FatDirNode mynode)
            return ErrorNumber.InvalidArgument;

        mynode._position = -1;
        mynode._entries  = null;

        return ErrorNumber.NoError;
    }

#endregion
}