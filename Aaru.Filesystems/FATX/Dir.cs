// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dir.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : FATX filesystem plugin.
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
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.Filesystems;

public sealed partial class XboxFatPlugin
{
#region IReadOnlyFilesystem Members

    /// <inheritdoc />
    public ErrorNumber OpenDir(string path, out IDirNode node)
    {
        node = null;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        if(string.IsNullOrWhiteSpace(path) || path == "/")
        {
            node = new FatxDirNode
            {
                Path     = path,
                Position = 0,
                Entries  = _rootDirectory.Values.ToArray()
            };

            return ErrorNumber.NoError;
        }

        string cutPath = path.StartsWith('/') ? path[1..].ToLower(_cultureInfo) : path.ToLower(_cultureInfo);

        if(_directoryCache.TryGetValue(cutPath, out Dictionary<string, DirectoryEntry> currentDirectory))
        {
            node = new FatxDirNode
            {
                Path     = path,
                Position = 0,
                Entries  = currentDirectory.Values.ToArray()
            };

            return ErrorNumber.NoError;
        }

        string[] pieces = cutPath.Split(new[]
        {
            '/'
        }, StringSplitOptions.RemoveEmptyEntries);

        KeyValuePair<string, DirectoryEntry> entry =
            _rootDirectory.FirstOrDefault(t => t.Key.ToLower(_cultureInfo) == pieces[0]);

        if(string.IsNullOrEmpty(entry.Key))
            return ErrorNumber.NoSuchFile;

        if(!entry.Value.attributes.HasFlag(Attributes.Directory))
            return ErrorNumber.NotDirectory;

        string currentPath = pieces[0];

        currentDirectory = _rootDirectory;

        for(var p = 0; p < pieces.Length; p++)
        {
            entry = currentDirectory.FirstOrDefault(t => t.Key.ToLower(_cultureInfo) == pieces[p]);

            if(string.IsNullOrEmpty(entry.Key))
                return ErrorNumber.NoSuchFile;

            if(!entry.Value.attributes.HasFlag(Attributes.Directory))
                return ErrorNumber.NotDirectory;

            currentPath = p == 0 ? pieces[0] : $"{currentPath}/{pieces[p]}";
            uint currentCluster = entry.Value.firstCluster;

            if(_directoryCache.TryGetValue(currentPath, out currentDirectory))
                continue;

            uint[] clusters = GetClusters(currentCluster);

            if(clusters is null)
                return ErrorNumber.InvalidArgument;

            var directoryBuffer = new byte[_bytesPerCluster * clusters.Length];

            for(var i = 0; i < clusters.Length; i++)
            {
                ErrorNumber errno =
                    _imagePlugin.ReadSectors(_firstClusterSector + (clusters[i] - 1) * _sectorsPerCluster,
                                             _sectorsPerCluster, out byte[] buffer);

                if(errno != ErrorNumber.NoError)
                    return errno;

                Array.Copy(buffer, 0, directoryBuffer, i * _bytesPerCluster, _bytesPerCluster);
            }

            currentDirectory = new Dictionary<string, DirectoryEntry>();

            var pos = 0;

            while(pos < directoryBuffer.Length)
            {
                DirectoryEntry dirent = _littleEndian
                                            ? Marshal.ByteArrayToStructureLittleEndian<DirectoryEntry>(directoryBuffer,
                                                pos, Marshal.SizeOf<DirectoryEntry>())
                                            : Marshal.ByteArrayToStructureBigEndian<DirectoryEntry>(directoryBuffer,
                                                pos, Marshal.SizeOf<DirectoryEntry>());

                pos += Marshal.SizeOf<DirectoryEntry>();

                if(dirent.filenameSize is UNUSED_DIRENTRY or FINISHED_DIRENTRY)
                    break;

                if(dirent.filenameSize is DELETED_DIRENTRY or > MAX_FILENAME)
                    continue;

                string filename = _encoding.GetString(dirent.filename, 0, dirent.filenameSize);

                currentDirectory.Add(filename, dirent);
            }

            _directoryCache.Add(currentPath, currentDirectory);
        }

        if(currentDirectory is null)
            return ErrorNumber.NoSuchFile;

        node = new FatxDirNode
        {
            Path     = path,
            Position = 0,
            Entries  = currentDirectory.Values.ToArray()
        };

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadDir(IDirNode node, out string filename)
    {
        filename = null;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        if(node is not FatxDirNode mynode)
            return ErrorNumber.InvalidArgument;

        if(mynode.Position < 0)
            return ErrorNumber.InvalidArgument;

        if(mynode.Position >= mynode.Entries.Length)
            return ErrorNumber.NoError;

        filename = _encoding.GetString(mynode.Entries[mynode.Position].filename, 0,
                                       mynode.Entries[mynode.Position].filenameSize);

        mynode.Position++;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseDir(IDirNode node)
    {
        if(node is not FatxDirNode mynode)
            return ErrorNumber.InvalidArgument;

        mynode.Position = -1;
        mynode.Entries  = null;

        return ErrorNumber.NoError;
    }

#endregion
}