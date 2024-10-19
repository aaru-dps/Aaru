// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Xattr.cs
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.Helpers;

namespace Aaru.Filesystems;

public sealed partial class FAT
{
    Dictionary<string, Dictionary<string, byte[]>> _eaCache;

#region IReadOnlyFilesystem Members

    /// <inheritdoc />
    public ErrorNumber ListXAttr(string path, out List<string> xattrs)
    {
        xattrs = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // No other xattr recognized yet
        if(_cachedEaData is null && !_fat32) return ErrorNumber.NotSupported;

        if(path[0] == '/') path = path[1..];

        if(_eaCache.TryGetValue(path.ToLower(_cultureInfo), out Dictionary<string, byte[]> eas))
        {
            xattrs = eas.Keys.ToList();

            return ErrorNumber.NoError;
        }

        ErrorNumber err = GetFileEntry(path, out CompleteDirectoryEntry entry);

        if(err != ErrorNumber.NoError || entry is null) return err;

        xattrs = [];

        if(!_fat32)
        {
            if(entry.Dirent.ea_handle == 0) return ErrorNumber.NoError;

            eas = GetEas(entry.Dirent.ea_handle);
        }
        else
        {
            if(entry.Fat32Ea.start_cluster == 0) return ErrorNumber.NoError;

            eas = GetEas(entry.Fat32Ea);
        }

        if(eas is null) return ErrorNumber.NoError;

        _eaCache.Add(path.ToLower(_cultureInfo), eas);
        xattrs = eas.Keys.ToList();

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetXattr(string path, string xattr, ref byte[] buf)
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        ErrorNumber err = ListXAttr(path, out List<string> xattrs);

        if(err != ErrorNumber.NoError) return err;

        if(path[0] == '/') path = path[1..];

        if(!xattrs.Contains(xattr.ToLower(_cultureInfo))) return ErrorNumber.NoSuchExtendedAttribute;

        if(!_eaCache.TryGetValue(path.ToLower(_cultureInfo), out Dictionary<string, byte[]> eas))
            return ErrorNumber.InvalidArgument;

        if(!eas.TryGetValue(xattr.ToLower(_cultureInfo), out byte[] data)) return ErrorNumber.InvalidArgument;

        buf = new byte[data.Length];
        data.CopyTo(buf, 0);

        return ErrorNumber.NoError;
    }

#endregion

    Dictionary<string, byte[]> GetEas(DirectoryEntry entryFat32Ea)
    {
        var    eaMs                  = new MemoryStream();
        uint[] rootDirectoryClusters = GetClusters(entryFat32Ea.start_cluster);

        foreach(uint cluster in rootDirectoryClusters)
        {
            ErrorNumber errno = _image.ReadSectors(_firstClusterSector + cluster * _sectorsPerCluster,
                                                   _sectorsPerCluster,
                                                   out byte[] buffer);

            if(errno != ErrorNumber.NoError) return null;

            eaMs.Write(buffer, 0, buffer.Length);
        }

        byte[] full = eaMs.ToArray();
        var    size = BitConverter.ToUInt16(full, 0);
        var    eas  = new byte[size];
        Array.Copy(full, 0, eas, 0, size);

        eaMs.Close();

        return GetEas(eas);
    }

    Dictionary<string, byte[]> GetEas(ushort eaHandle)
    {
        int aIndex = eaHandle >> 7;

        // First 0x20 bytes are the magic number and unused words
        var a = BitConverter.ToUInt16(_cachedEaData, aIndex * 2 + 0x20);

        var b = BitConverter.ToUInt16(_cachedEaData, eaHandle * 2 + 0x200);

        var eaCluster = (uint)(a + b);

        if(b == EA_UNUSED) return null;

        EaHeader header =
            Marshal.ByteArrayToStructureLittleEndian<EaHeader>(_cachedEaData,
                                                               (int)(eaCluster * _bytesPerCluster),
                                                               Marshal.SizeOf<EaHeader>());

        if(header.magic != 0x4145) return null;

        var eaLen = BitConverter.ToUInt32(_cachedEaData,
                                          (int)(eaCluster * _bytesPerCluster) + Marshal.SizeOf<EaHeader>());

        var eaData = new byte[eaLen];

        Array.Copy(_cachedEaData, (int)(eaCluster * _bytesPerCluster) + Marshal.SizeOf<EaHeader>(), eaData, 0, eaLen);

        return GetEas(eaData);
    }

    Dictionary<string, byte[]> GetEas(byte[] eaData)
    {
        if(eaData is null || eaData.Length < 4) return null;

        Dictionary<string, byte[]> eas = new();

        if(_debug) eas.Add("com.microsoft.os2.fea", eaData);

        var pos = 4;

        while(pos < eaData.Length)
        {
            pos++; // Skip fEA
            byte cbName  = eaData[pos++];
            var  cbValue = BitConverter.ToUInt16(eaData, pos);
            pos += 2;

            string name = Encoding.ASCII.GetString(eaData, pos, cbName);
            pos += cbName;
            pos++;
            var data = new byte[cbValue];

            Array.Copy(eaData, pos, data, 0, cbValue);
            pos += cbValue;

            // OS/2 System Attributes
            if(name[0] == '.')
            {
                // This is WorkPlace System information so it's IBM
                if(name == ".CLASSINFO")
                    name = "com.ibm.os2.classinfo";
                else
                    name = "com.microsoft.os2" + name.ToLower();
            }

            eas.Add(name, data);
        }

        return eas;
    }

    void CacheEaData()
    {
        if(_eaDirEntry.start_cluster == 0) return;

        var eaDataMs = new MemoryStream();

        foreach(uint cluster in GetClusters(_eaDirEntry.start_cluster))
        {
            ErrorNumber errno = _image.ReadSectors(_firstClusterSector + cluster * _sectorsPerCluster,
                                                   _sectorsPerCluster,
                                                   out byte[] buffer);

            if(errno != ErrorNumber.NoError) break;

            eaDataMs.Write(buffer, 0, buffer.Length);
        }

        _cachedEaData = eaDataMs.ToArray();
    }
}