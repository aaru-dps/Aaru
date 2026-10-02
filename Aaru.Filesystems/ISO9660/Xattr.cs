// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Xattr.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Handles extended attributes
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
// In the loving memory of Facunda "Tata" Suárez Domínguez, R.I.P. 2019/07/24
// ****************************************************************************/

using System;
using System.Collections.Generic;
using Aaru.CommonTypes.Enums;
using Aaru.Helpers;

namespace Aaru.Filesystems;

public sealed partial class ISO9660
{
#region IReadOnlyFilesystem Members

    /// <inheritdoc />
    public ErrorNumber ListXAttr(string path, out List<string> xattrs)
    {
        xattrs = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        ErrorNumber err = GetFileEntry(path, out DecodedDirectoryEntry entry);

        if(err != ErrorNumber.NoError) return err;

        xattrs = [];

        if(entry.XattrLength > 0) xattrs.Add(Xattrs.XATTR_ISO9660_EA);

        if(entry.AssociatedFile != null) xattrs.Add(Xattrs.XATTR_ISO9660_ASSOCIATED_FILE);

        if(entry.AppleDosType is not null) xattrs.Add(Xattrs.XATTR_APPLE_DOS_TYPE);

        if(entry.AppleProDosType is not null) xattrs.Add(Xattrs.XATTR_APPLE_PRODOS_TYPE);

        if(entry.ResourceFork != null) xattrs.Add(Xattrs.XATTR_APPLE_RESOURCE_FORK);

        if(entry.FinderInfo != null) xattrs.Add(Xattrs.XATTR_APPLE_FINDER_INFO);

        if(entry.AppleIcon != null) xattrs.Add(Xattrs.XATTR_APPLE_ICON);

        if(entry.AmigaComment != null) xattrs.Add(Xattrs.XATTR_AMIGA_COMMENTS);

        if(entry.AmigaProtection != null) xattrs.Add(Xattrs.XATTR_AMIGA_PROTECTION);

        if(entry.AcornSystemArea?.Filetype != null) xattrs.Add(Xattrs.XATTR_ACORN_RISCOS_FILETYPE);

        if(entry.Flags.HasFlag(FileFlags.Directory) || entry.Extents == null || entry.Extents.Count == 0)
            return ErrorNumber.NoError;

        ErrorNumber errno = _image.ReadSectorTag(entry.Extents[0].extent * _blockSize / 2048,
                                                 false,
                                                 SectorTagType.CdSectorSubHeader,
                                                 out _);

        // Only Mode 2 sectors have a subheader, the other attributes are still there without it
        if(errno != ErrorNumber.NoError) return ErrorNumber.NoError;

        xattrs.Add(Xattrs.XATTR_ISO9660_MODE2_SUBHEADER);
        xattrs.Add(Xattrs.XATTR_ISO9660_MODE2_SUBHEADER_COPY);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetXattr(string path, string xattr, ref byte[] buf)
    {
        buf = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        ErrorNumber err = GetFileEntry(path, out DecodedDirectoryEntry entry);

        if(err != ErrorNumber.NoError) return err;

        switch(xattr)
        {
            case Xattrs.XATTR_ISO9660_EA:
                if(entry.XattrLength == 0) return ErrorNumber.NoSuchExtendedAttribute;

                if(entry.Extents is null) return ErrorNumber.InvalidArgument;

                if(entry.Extents.Count > 1)
                    return ReadSingleExtent(entry.XattrLength * _blockSize, entry.Extents[0].extent, out buf);

                buf = [];

                return ErrorNumber.NoError;

            case Xattrs.XATTR_ISO9660_ASSOCIATED_FILE:
                if(entry.AssociatedFile is null) return ErrorNumber.NoSuchExtendedAttribute;

                if(entry.AssociatedFile.Extents is null) return ErrorNumber.InvalidArgument;

                if(entry.AssociatedFile.Size != 0)
                {
                    return ReadWithExtents(0,
                                           (long)entry.AssociatedFile.Size,
                                           entry.AssociatedFile.Extents,
                                           entry.AssociatedFile.XA?.signature == XA_MAGIC &&
                                           entry.AssociatedFile.XA?.attributes.HasFlag(XaAttributes.Interleaved) ==
                                           true,
                                           entry.AssociatedFile.XA?.filenumber ?? 0,
                                           out buf);
                }

                buf = [];

                return ErrorNumber.NoError;

            case Xattrs.XATTR_APPLE_DOS_TYPE:
                if(entry.AppleDosType is null) return ErrorNumber.NoSuchExtendedAttribute;

                buf    = new byte[1];
                buf[0] = entry.AppleDosType.Value;

                return ErrorNumber.NoError;
            case Xattrs.XATTR_APPLE_PRODOS_TYPE:
                if(entry.AppleProDosType is null) return ErrorNumber.NoSuchExtendedAttribute;

                buf = BitConverter.GetBytes(entry.AppleProDosType.Value);

                return ErrorNumber.NoError;
            case Xattrs.XATTR_APPLE_RESOURCE_FORK:
                if(entry.ResourceFork is null) return ErrorNumber.NoSuchExtendedAttribute;

                if(entry.ResourceFork.Extents is null) return ErrorNumber.InvalidArgument;

                if(entry.ResourceFork.Size != 0)
                {
                    return ReadWithExtents(0,
                                           (long)entry.ResourceFork.Size,
                                           entry.ResourceFork.Extents,
                                           entry.ResourceFork.XA?.signature == XA_MAGIC &&
                                           entry.ResourceFork.XA?.attributes.HasFlag(XaAttributes.Interleaved) == true,
                                           entry.ResourceFork.XA?.filenumber ?? 0,
                                           out buf);
                }

                buf = [];

                return ErrorNumber.NoError;

            case Xattrs.XATTR_APPLE_FINDER_INFO:
                if(entry.FinderInfo is null) return ErrorNumber.NoSuchExtendedAttribute;

                buf = Marshal.StructureToByteArrayBigEndian(entry.FinderInfo.Value);

                return ErrorNumber.NoError;
            case Xattrs.XATTR_APPLE_ICON:
                if(entry.AppleIcon is null) return ErrorNumber.NoSuchExtendedAttribute;

                buf = new byte[entry.AppleIcon.Length];
                Array.Copy(entry.AppleIcon, 0, buf, 0, entry.AppleIcon.Length);

                return ErrorNumber.NoError;
            case Xattrs.XATTR_AMIGA_COMMENTS:
                if(entry.AmigaComment is null) return ErrorNumber.NoSuchExtendedAttribute;

                buf = new byte[entry.AmigaComment.Length];
                Array.Copy(entry.AmigaComment, 0, buf, 0, entry.AmigaComment.Length);

                return ErrorNumber.NoError;
            case Xattrs.XATTR_AMIGA_PROTECTION:
                if(entry.AmigaProtection is null) return ErrorNumber.NoSuchExtendedAttribute;

                // Recorded in the same order as the big endian value AmigaOS uses, all bits preserved
                buf =
                [
                    entry.AmigaProtection.Value.User, entry.AmigaProtection.Value.Reserved,
                    (byte)entry.AmigaProtection.Value.Multiuser, (byte)entry.AmigaProtection.Value.Protection
                ];

                return ErrorNumber.NoError;
            case Xattrs.XATTR_ACORN_RISCOS_FILETYPE:
                if(entry.AcornSystemArea?.Filetype is null) return ErrorNumber.NoSuchExtendedAttribute;

                buf = BitConverter.GetBytes(entry.AcornSystemArea.Value.Filetype.Value);

                return ErrorNumber.NoError;
            case Xattrs.XATTR_ISO9660_MODE2_SUBHEADER:
                buf = ReadSubheaderWithExtents(entry.Extents, false);

                return ErrorNumber.NoError;
            case Xattrs.XATTR_ISO9660_MODE2_SUBHEADER_COPY:
                buf = ReadSubheaderWithExtents(entry.Extents, true);

                return ErrorNumber.NoError;
            default:
                return ErrorNumber.NoSuchExtendedAttribute;
        }
    }

#endregion
}