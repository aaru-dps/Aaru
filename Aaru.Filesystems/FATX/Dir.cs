// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dir.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : FATX filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Methods to show the FATX directories.
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
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;

namespace Aaru.Filesystems
{
    public sealed partial class XboxFatPlugin
    {
        /// <inheritdoc />
        public Errno ReadDir(string path, out List<string> contents)
        {
            contents = null;

            if(!_mounted)
                return Errno.AccessDenied;

            if(string.IsNullOrWhiteSpace(path) ||
               path == "/")
            {
                contents = _rootDirectory.Keys.ToList();

                return Errno.NoError;
            }

            string cutPath = path.StartsWith('/') ? path.Substring(1).ToLower(_cultureInfo)
                                 : path.ToLower(_cultureInfo);

            if(_directoryCache.TryGetValue(cutPath, out Dictionary<string, DirectoryEntry> currentDirectory))
            {
                contents = currentDirectory.Keys.ToList();

                return Errno.NoError;
            }

            string[] pieces = cutPath.Split(new[]
            {
                '/'
            }, StringSplitOptions.RemoveEmptyEntries);

            KeyValuePair<string, DirectoryEntry> entry =
                _rootDirectory.FirstOrDefault(t => t.Key.ToLower(_cultureInfo) == pieces[0]);

            if(string.IsNullOrEmpty(entry.Key))
                return Errno.NoSuchFile;

            if(!entry.Value.attributes.HasFlag(Attributes.Directory))
                return Errno.NotDirectory;

            string currentPath = pieces[0];

            currentDirectory = _rootDirectory;

            for(int p = 0; p < pieces.Length; p++)
            {
                entry = currentDirectory.FirstOrDefault(t => t.Key.ToLower(_cultureInfo) == pieces[p]);

                if(string.IsNullOrEmpty(entry.Key))
                    return Errno.NoSuchFile;

                if(!entry.Value.attributes.HasFlag(Attributes.Directory))
                    return Errno.NotDirectory;

                currentPath = p == 0 ? pieces[0] : $"{currentPath}/{pieces[p]}";
                uint currentCluster = entry.Value.firstCluster;

                if(_directoryCache.TryGetValue(currentPath, out currentDirectory))
                    continue;

                uint[] clusters = GetClusters(currentCluster);

                if(clusters is null)
                    return Errno.InvalidArgument;

                byte[] directoryBuffer = new byte[_bytesPerCluster * clusters.Length];

                for(int i = 0; i < clusters.Length; i++)
                {
                    byte[] buffer =
                        _imagePlugin.ReadSectors(_firstClusterSector + ((clusters[i] - 1) * _sectorsPerCluster),
                                                 _sectorsPerCluster);

                    Array.Copy(buffer, 0, directoryBuffer, i * _bytesPerCluster, _bytesPerCluster);
                }

                currentDirectory = new Dictionary<string, DirectoryEntry>();

                int pos = 0;

                while(pos < directoryBuffer.Length)
                {
                    DirectoryEntry dirent = _littleEndian
                                                ? Marshal.
                                                    ByteArrayToStructureLittleEndian<
                                                        DirectoryEntry>(directoryBuffer, pos,
                                                                        Marshal.SizeOf<DirectoryEntry>())
                                                : Marshal.ByteArrayToStructureBigEndian<DirectoryEntry>(directoryBuffer,
                                                    pos, Marshal.SizeOf<DirectoryEntry>());

                    pos += Marshal.SizeOf<DirectoryEntry>();

                    if(dirent.filenameSize == UNUSED_DIRENTRY ||
                       dirent.filenameSize == FINISHED_DIRENTRY)
                        break;

                    if(dirent.filenameSize == DELETED_DIRENTRY ||
                       dirent.filenameSize > MAX_FILENAME)
                        continue;

                    string filename = Encoding.GetString(dirent.filename, 0, dirent.filenameSize);

                    currentDirectory.Add(filename, dirent);
                }

                _directoryCache.Add(currentPath, currentDirectory);
            }

            contents = currentDirectory?.Keys.ToList();

            return Errno.NoError;
        }
    }
}