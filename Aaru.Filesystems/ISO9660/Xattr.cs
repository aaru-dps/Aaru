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
// Copyright © 2011-2023 Natalia Portillo
// In the loving memory of Facunda "Tata" Suárez Domínguez, R.I.P. 2019/07/24
// ****************************************************************************/

using System;
using System.Collections.Generic;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;

namespace Aaru.Filesystems
{
    public sealed partial class ISO9660
    {
        /// <inheritdoc />
        public Errno ListXAttr(string path, out List<string> xattrs)
        {
            xattrs = null;

            if(!_mounted)
                return Errno.AccessDenied;

            Errno err = GetFileEntry(path, out DecodedDirectoryEntry entry);

            if(err != Errno.NoError)
                return err;

            xattrs = new List<string>();

            if(entry.XattrLength > 0)
                xattrs.Add("org.iso.9660.ea");

            if(entry.AssociatedFile != null)
                xattrs.Add("org.iso.9660.AssociatedFile");

            if(entry.AppleDosType != null)
                xattrs.Add("com.apple.dos.type");

            if(entry.AppleProDosType != null)
                xattrs.Add("com.apple.prodos.type");

            if(entry.ResourceFork != null)
                xattrs.Add("com.apple.ResourceFork");

            if(entry.FinderInfo != null)
                xattrs.Add("com.apple.FinderInfo");

            if(entry.AppleIcon != null)
                xattrs.Add("com.apple.Macintosh.Icon");

            if(entry.AmigaComment != null)
                xattrs.Add("com.amiga.comments");

            if(entry.Flags.HasFlag(FileFlags.Directory) ||
               entry.Extents       == null              ||
               entry.Extents.Count == 0)
                return Errno.NoError;

            // TODO: No more exceptions
            try
            {
                byte[] sector = _image.ReadSectorLong(entry.Extents[0].extent * _blockSize / 2048);

                if(sector[15] != 2)
                    return Errno.NoError;
            }
            catch
            {
                return Errno.NoError;
            }

            xattrs.Add("org.iso.mode2.subheader");
            xattrs.Add("org.iso.mode2.subheader.copy");

            return Errno.NoError;
        }

        /// <inheritdoc />
        public Errno GetXattr(string path, string xattr, ref byte[] buf)
        {
            buf = null;

            if(!_mounted)
                return Errno.AccessDenied;

            Errno err = GetFileEntry(path, out DecodedDirectoryEntry entry);

            if(err != Errno.NoError)
                return err;

            switch(xattr)
            {
                case "org.iso.9660.ea":
                    if(entry.XattrLength == 0)
                        return Errno.NoSuchExtendedAttribute;

                    if(entry.Extents is null)
                        return Errno.InvalidArgument;

                    buf = ReadSingleExtent(entry.XattrLength * _blockSize, entry.Extents[0].extent);

                    return Errno.NoError;
                case "org.iso.9660.AssociatedFile":
                    if(entry.AssociatedFile is null)
                        return Errno.NoSuchExtendedAttribute;

                    if(entry.AssociatedFile.Extents is null)
                        return Errno.InvalidArgument;

                    if(entry.AssociatedFile.Size == 0)
                    {
                        buf = Array.Empty<byte>();

                        return Errno.NoError;
                    }

                    buf = ReadWithExtents(0, (long)entry.AssociatedFile.Size, entry.AssociatedFile.Extents,
                                          entry.AssociatedFile.XA?.signature == XA_MAGIC &&
                                          entry.AssociatedFile.XA?.attributes.HasFlag(XaAttributes.Interleaved) == true,
                                          entry.AssociatedFile.XA?.filenumber ?? 0);

                    return Errno.NoError;
                case "com.apple.dos.type":
                    if(entry.AppleDosType is null)
                        return Errno.NoSuchExtendedAttribute;

                    buf    = new byte[1];
                    buf[0] = entry.AppleDosType.Value;

                    return Errno.NoError;
                case "com.apple.prodos.type":
                    if(entry.AppleProDosType is null)
                        return Errno.NoSuchExtendedAttribute;

                    buf = BitConverter.GetBytes(entry.AppleProDosType.Value);

                    return Errno.NoError;
                case "com.apple.ResourceFork":
                    if(entry.ResourceFork is null)
                        return Errno.NoSuchExtendedAttribute;

                    if(entry.ResourceFork.Extents is null)
                        return Errno.InvalidArgument;

                    if(entry.ResourceFork.Size == 0)
                    {
                        buf = Array.Empty<byte>();

                        return Errno.NoError;
                    }

                    buf = ReadWithExtents(0, (long)entry.ResourceFork.Size, entry.ResourceFork.Extents,
                                          entry.ResourceFork.XA?.signature == XA_MAGIC &&
                                          entry.ResourceFork.XA?.attributes.HasFlag(XaAttributes.Interleaved) == true,
                                          entry.ResourceFork.XA?.filenumber ?? 0);

                    return Errno.NoError;
                case "com.apple.FinderInfo":
                    if(entry.FinderInfo is null)
                        return Errno.NoSuchExtendedAttribute;

                    buf = Marshal.StructureToByteArrayBigEndian(entry.FinderInfo.Value);

                    return Errno.NoError;
                case "com.apple.Macintosh.Icon":
                    if(entry.AppleIcon is null)
                        return Errno.NoSuchExtendedAttribute;

                    buf = new byte[entry.AppleIcon.Length];
                    Array.Copy(entry.AppleIcon, 0, buf, 0, entry.AppleIcon.Length);

                    return Errno.NoError;
                case "com.amiga.comments":
                    if(entry.AmigaComment is null)
                        return Errno.NoSuchExtendedAttribute;

                    buf = new byte[entry.AmigaComment.Length];
                    Array.Copy(entry.AmigaComment, 0, buf, 0, entry.AmigaComment.Length);

                    return Errno.NoError;
                case "org.iso.mode2.subheader":
                    buf = ReadSubheaderWithExtents(entry.Extents, false);

                    return Errno.NoError;
                case "org.iso.mode2.subheader.copy":
                    buf = ReadSubheaderWithExtents(entry.Extents, true);

                    return Errno.NoError;
                default: return Errno.NoSuchExtendedAttribute;
            }
        }
    }
}