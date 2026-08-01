// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Xattr.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Reiser filesystem plugin
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
using Aaru.Logging;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

public sealed partial class Reiser
{
    /// <inheritdoc />
    public ErrorNumber ListXAttr(string path, out List<string> xattrs)
    {
        xattrs = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Xattrs require v3.6 format and a valid xattr root
        if(_xattrRootEntries == null) return ErrorNumber.NotSupported;

        AaruLogging.Debug(MODULE_NAME, "ListXAttr: path='{0}'", path);

        // Resolve the target object
        ErrorNumber errno = ResolvePath(path, out uint dirId, out uint objectId);

        if(errno != ErrorNumber.NoError) return errno;

        // Get the xattr directory for this object
        errno = FindXattrDir(dirId, objectId, out Dictionary<string, (uint dirId, uint objectId)> xattrEntries);

        if(errno != ErrorNumber.NoError)
        {
            // No xattr directory is not an error — just means no xattrs
            if(errno != ErrorNumber.NoSuchFile) return errno;

            xattrs = [];

            return ErrorNumber.NoError;
        }

        xattrs = new List<string>(xattrEntries.Count);
        xattrs.AddRange(xattrEntries.Keys.Where(static name => name is not ("." or "..")));

        AaruLogging.Debug(MODULE_NAME, "ListXAttr: found {0} xattrs", xattrs.Count);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetXattr(string path, string xattr, ref byte[] buf)
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        if(_xattrRootEntries == null) return ErrorNumber.NotSupported;

        AaruLogging.Debug(MODULE_NAME, "GetXattr: path='{0}', xattr='{1}'", path, xattr);

        // Resolve the target object
        ErrorNumber errno = ResolvePath(path, out uint dirId, out uint objectId);

        if(errno != ErrorNumber.NoError) return errno;

        // Get the xattr directory for this object
        errno = FindXattrDir(dirId, objectId, out Dictionary<string, (uint dirId, uint objectId)> xattrEntries);

        if(errno != ErrorNumber.NoError) return errno;

        // Find the specific xattr file
        if(!xattrEntries.TryGetValue(xattr, out (uint dirId, uint objectId) xattrFile))
            return ErrorNumber.NoSuchExtendedAttribute;

        // Read the xattr file contents
        errno = ReadFileData(xattrFile.dirId, xattrFile.objectId, out byte[] fileData);

        if(errno != ErrorNumber.NoError) return errno;

        if(fileData == null || fileData.Length < XATTR_HEADER_SIZE) return ErrorNumber.InvalidArgument;

        // Validate the xattr header magic
        XattrHeader header = Marshal.ByteArrayToStructureLittleEndian<XattrHeader>(fileData, 0, XATTR_HEADER_SIZE);

        if(header.h_magic != REISERFS_XATTR_MAGIC)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "GetXattr: invalid xattr magic 0x{0:X8}, expected 0x{1:X8}",
                              header.h_magic,
                              REISERFS_XATTR_MAGIC);

            return ErrorNumber.InvalidArgument;
        }

        // Extract the value (everything after the 8-byte header)
        int valueLen = fileData.Length - XATTR_HEADER_SIZE;
        buf = new byte[valueLen];
        Array.Copy(fileData, XATTR_HEADER_SIZE, buf, 0, valueLen);

        AaruLogging.Debug(MODULE_NAME, "GetXattr: read {0} bytes for '{1}'", valueLen, xattr);

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Finds the per-object xattr directory by looking up
    ///     <c>&lt;OBJECTID_HEX&gt;.&lt;GENERATION_HEX&gt;</c> in the xattr root.
    /// </summary>
    /// <param name="dirId">Directory (packing locality) id of the object</param>
    /// <param name="objectId">Object id</param>
    /// <param name="xattrEntries">Directory entries within the xattr directory (xattr name → (dirId, objectId))</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber FindXattrDir(uint                                                dirId, uint objectId,
                             out Dictionary<string, (uint dirId, uint objectId)> xattrEntries)
    {
        xattrEntries = null;

        // Get the generation number to form the directory name
        ErrorNumber errno = ReadObjectGeneration(dirId, objectId, out uint generation);

        if(errno != ErrorNumber.NoError) return errno;

        // The kernel uses "%X.%X" format (uppercase hex, no leading zeros)
        var xaDirName = $"{objectId:X}.{generation:X}";

        return !_xattrRootEntries.TryGetValue(xaDirName, out (uint dirId, uint objectId) xaDir)
                   ? ErrorNumber.NoSuchFile
                   :

                   // Read the per-object xattr directory
                   GetDirectoryEntries(xaDir.dirId, xaDir.objectId, out xattrEntries);
    }
}