// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Xattr.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : High Performance Optical File System plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads OS/2 extended attributes, stored in the second subfile of a file.
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
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using Aaru.Logging;

namespace Aaru.Filesystems;

public sealed partial class HPOFS
{
    /// <inheritdoc />
    public ErrorNumber ListXAttr(string path, out List<string> xattrs)
    {
        xattrs = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        ErrorNumber errno = GetEntry(path, out string _, out Entry entry);

        if(errno != ErrorNumber.NoError) return errno;

        errno = ReadExtendedAttributes(entry, out List<(string name, byte[] value)> eas);

        if(errno != ErrorNumber.NoError) return errno;

        xattrs = eas.Select(static ea => ConvertEaNameToXattr(ea.name)).ToList();

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetXattr(string path, string xattr, ref byte[] buf)
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        ErrorNumber errno = GetEntry(path, out string _, out Entry entry);

        if(errno != ErrorNumber.NoError) return errno;

        errno = ReadExtendedAttributes(entry, out List<(string name, byte[] value)> eas);

        if(errno != ErrorNumber.NoError) return errno;

        foreach((string name, byte[] value) in eas)
        {
            if(!ConvertEaNameToXattr(name).Equals(xattr, StringComparison.Ordinal)) continue;

            buf = value;

            return ErrorNumber.NoError;
        }

        return ErrorNumber.NoSuchExtendedAttribute;
    }

    /// <summary>Reads all extended attributes of an entry</summary>
    ErrorNumber ReadExtendedAttributes(Entry entry, out List<(string name, byte[] value)> eas)
    {
        eas = [];

        if(entry is null || entry.Subfiles.Length < SUBFILE_EA) return ErrorNumber.NoError;

        SubfileDescriptor descriptor = entry.Subfiles[SUBFILE_EA - 1];

        ErrorNumber errno = GetExtents(descriptor, SUBFILE_EA, out Extent[] extents);

        if(errno != ErrorNumber.NoError) return errno;

        if(extents.Length == 0) return ErrorNumber.NoError;

        uint size = Math.Max(descriptor.size, _bytesPerSector);

        errno = ReadFromExtents(extents, 0, size, out byte[] data);

        if(errno != ErrorNumber.NoError) return errno;

        if(!data.Take(8).SequenceEqual(_eaSignature))
        {
            AaruLogging.Debug(MODULE_NAME, "Extended attributes of file {0} have no signature", entry.Record.fileId);

            return ErrorNumber.InvalidArgument;
        }

        // First descriptor's value is right after the last descriptor
        uint descriptorsEnd = BigEndianBitConverter.ToUInt32(data, EA_HEADER_SIZE);
        int  position       = EA_HEADER_SIZE;

        while(position + EA_DESCRIPTOR_SIZE <= descriptorsEnd && position + EA_DESCRIPTOR_SIZE <= data.Length)
        {
            EaDescriptor ea = Marshal.ByteArrayToStructureBigEndian<EaDescriptor>(data, position, EA_DESCRIPTOR_SIZE);

            if(ea.valueOffset == 0 || ea.nameLength == 0) break;

            int nameStart = position + EA_DESCRIPTOR_SIZE;

            if(nameStart + ea.nameLength > data.Length || (ulong)ea.valueOffset + ea.valueLength > (ulong)data.Length)
                return ErrorNumber.InvalidArgument;

            byte[] rawName = new byte[ea.nameLength];
            Array.Copy(data, nameStart, rawName, 0, ea.nameLength);

            byte[] value = new byte[ea.valueLength];
            Array.Copy(data, ea.valueOffset, value, 0, ea.valueLength);

            eas.Add((StringHandlers.CToString(rawName, GetEaEncoding(ea)), value));

            position = nameStart + ea.nameLength;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Gets the encoding of an extended attribute name</summary>
    Encoding GetEaEncoding(EaDescriptor ea)
    {
        try
        {
            if(ea.codepage > 0) return Encoding.GetEncoding(ea.codepage);
        }
        catch(Exception ex) when(ex is ArgumentException or NotSupportedException)
        {
            AaruLogging.Debug(MODULE_NAME, "Codepage {0} is not supported", ea.codepage);
        }

        return _encoding;
    }

    /// <summary>Converts an OS/2 EA name to an xattr name, the same way the HPFS plugin does</summary>
    static string ConvertEaNameToXattr(string eaName)
    {
        if(string.IsNullOrEmpty(eaName)) return eaName;

        if(eaName.Equals(".CLASSINFO", StringComparison.OrdinalIgnoreCase)) return Xattrs.XATTR_WORKPLACE_CLASSINFO;

        return eaName.StartsWith(".", StringComparison.Ordinal) ? $"{EA_PREFIX_OS2}{eaName}" : eaName;
    }
}