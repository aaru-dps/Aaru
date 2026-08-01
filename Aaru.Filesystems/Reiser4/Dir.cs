// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dir.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Reiser4 filesystem plugin
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
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Logging;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

public sealed partial class Reiser4
{
    /// <inheritdoc />
    public ErrorNumber OpenDir(string path, out IDirNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Normalize path
        string normalizedPath = path ?? "/";

        if(normalizedPath is "" or ".") normalizedPath = "/";

        // Root directory
        if(normalizedPath is "/")
        {
            if(_rootDirectoryCache.Count == 0) return ErrorNumber.NoSuchFile;

            node = new Reiser4DirNode
            {
                Path     = "/",
                Position = 0,
                Entries  = _rootDirectoryCache.Keys.ToArray()
            };

            return ErrorNumber.NoError;
        }

        // Subdirectory traversal
        string stripped = normalizedPath.StartsWith("/", StringComparison.Ordinal)
                              ? normalizedPath[1..]
                              : normalizedPath;

        string[] components = stripped.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if(components.Length == 0) return ErrorNumber.InvalidArgument;

        AaruLogging.Debug(MODULE_NAME, "OpenDir: traversing {0} components", components.Length);

        // Start from root directory cache
        Dictionary<string, LargeKey> currentEntries = _rootDirectoryCache;

        for(var i = 0; i < components.Length; i++)
        {
            string component = components[i];

            AaruLogging.Debug(MODULE_NAME, "OpenDir: navigating to '{0}'", component);

            if(component is "." or "..") continue;

            if(!currentEntries.TryGetValue(component, out LargeKey target))
            {
                AaruLogging.Debug(MODULE_NAME, "OpenDir: '{0}' not found", component);

                return ErrorNumber.NoSuchFile;
            }

            // Verify it's a directory
            ErrorNumber errno = ReadObjectMode(target, out ushort mode);

            if(errno != ErrorNumber.NoError) return errno;

            if((mode & S_IFMT) != S_IFDIR)
            {
                AaruLogging.Debug(MODULE_NAME, "OpenDir: '{0}' is not a directory (mode=0x{1:X4})", component, mode);

                return ErrorNumber.NotDirectory;
            }

            // Read this subdirectory's entries
            ulong objectId = GetStatDataObjectId(target);

            errno = ReadDirectoryEntries(objectId, out Dictionary<string, LargeKey> subEntries);

            if(errno != ErrorNumber.NoError) return errno;

            // Last component — this is the directory being opened
            if(i == components.Length - 1)
            {
                string[] entryNames = subEntries.Keys.Where(static name => name is not ("." or "..")).ToArray();

                node = new Reiser4DirNode
                {
                    Path     = normalizedPath,
                    Position = 0,
                    Entries  = entryNames
                };

                AaruLogging.Debug(MODULE_NAME,
                                  "OpenDir: opened '{0}' with {1} entries",
                                  normalizedPath,
                                  entryNames.Length);

                return ErrorNumber.NoError;
            }

            // Intermediate component — descend; "." and ".." stay in the cached dictionary
            // harmlessly since the loop above already special-cases them before lookup
            currentEntries = subEntries;
        }

        return ErrorNumber.NoSuchFile;
    }

    /// <inheritdoc />
    public ErrorNumber CloseDir(IDirNode node) => ErrorNumber.NoError;

    /// <inheritdoc />
    public ErrorNumber ReadDir(IDirNode node, out string filename)
    {
        filename = null;

        if(node is not Reiser4DirNode dirNode) return ErrorNumber.InvalidArgument;

        if(dirNode.Position >= dirNode.Entries.Length) return ErrorNumber.NoError;

        filename = dirNode.Entries[dirNode.Position];
        dirNode.Position++;

        return ErrorNumber.NoError;
    }

    /// <summary>Reads the mode field from an object's stat-data given its stat-data key, caching by objectid</summary>
    ErrorNumber ReadObjectMode(LargeKey statDataKey, out ushort mode)
    {
        ulong objectId = GetStatDataObjectId(statDataKey);

        if(_modeCache.TryGetValue(objectId, out mode)) return ErrorNumber.NoError;

        ErrorNumber cacheErrno = ReadObjectModeUncached(statDataKey, out mode);

        if(cacheErrno == ErrorNumber.NoError) _modeCache[objectId] = mode;

        return cacheErrno;
    }

    /// <summary>Reads the mode field from an object's stat-data given its stat-data key</summary>
    ErrorNumber ReadObjectModeUncached(LargeKey statDataKey, out ushort mode)
    {
        mode = 0;

        ErrorNumber errno = SearchByKey(statDataKey, out byte[] leafData, out int itemPos);

        if(errno != ErrorNumber.NoError) return errno;

        if(itemPos < 0) return ErrorNumber.NoSuchFile;

        Node40Header nh = Marshal.ByteArrayToStructureLittleEndian<Node40Header>(leafData);

        if(itemPos >= nh.nr_items) return ErrorNumber.NoSuchFile;

        ReadItemHeader(leafData, itemPos, nh.nr_items, out _, out ushort bodyOff, out _, out ushort pluginId);

        if(pluginId != STATIC_STAT_DATA_ID) return ErrorNumber.NoSuchFile;

        int sdLen = GetItemLength(leafData, itemPos, nh.nr_items, nh.free_space_start);

        if(sdLen < Marshal.SizeOf<StatDataBase>()) return ErrorNumber.InvalidArgument;

        StatDataBase sdBase =
            Marshal.ByteArrayToStructureLittleEndian<StatDataBase>(leafData, bodyOff, Marshal.SizeOf<StatDataBase>());

        int sdOff = bodyOff + Marshal.SizeOf<StatDataBase>();

        if((sdBase.extmask & SD_LIGHT_WEIGHT) != 0 && sdOff + Marshal.SizeOf<LightWeightStat>() <= leafData.Length)
        {
            LightWeightStat lws =
                Marshal.ByteArrayToStructureLittleEndian<LightWeightStat>(leafData,
                                                                          sdOff,
                                                                          Marshal.SizeOf<LightWeightStat>());

            mode = lws.mode;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Extracts the objectid from a stat-data key.
    ///     For large keys the objectid is in el[2] (lower 60 bits).
    ///     For short keys the objectid is in el[1] (lower 60 bits).
    /// </summary>
    ulong GetStatDataObjectId(LargeKey statDataKey) =>
        (_largeKeys ? statDataKey.el2 : statDataKey.el1) & KEY_OBJECTID_MASK;
}