// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Xattr.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Lisa filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Methods to handle Apple Lisa extended attributes (label, tags, serial,
//     etc).
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes.Structs;
using Aaru.Decoders;
using Aaru.Helpers;

namespace Aaru.Filesystems.LisaFS
{
    public sealed partial class LisaFS
    {
        /// <inheritdoc />
        public Errno ListXAttr(string path, out List<string> xattrs)
        {
            xattrs = null;
            Errno error = LookupFileId(path, out short fileId, out bool isDir);

            if(error != Errno.NoError)
                return error;

            return isDir ? Errno.InvalidArgument : ListXAttr(fileId, out xattrs);
        }

        /// <inheritdoc />
        public Errno GetXattr(string path, string xattr, ref byte[] buf)
        {
            Errno error = LookupFileId(path, out short fileId, out bool isDir);

            if(error != Errno.NoError)
                return error;

            return isDir ? Errno.InvalidArgument : GetXattr(fileId, xattr, out buf);
        }

        /// <summary>Lists special Apple Lisa filesystem features as extended attributes</summary>
        /// <returns>Error number.</returns>
        /// <param name="fileId">File identifier.</param>
        /// <param name="xattrs">Extended attributes.</param>
        Errno ListXAttr(short fileId, out List<string> xattrs)
        {
            xattrs = null;

            if(!_mounted)
                return Errno.AccessDenied;

            // System files
            if(fileId < 4)
            {
                if(!_debug ||
                   fileId == 0)
                    return Errno.InvalidArgument;

                xattrs = new List<string>();

                // Only MDDF contains an extended attributes
                if(fileId == FILEID_MDDF)
                {
                    byte[] buf = Encoding.ASCII.GetBytes(_mddf.password);

                    // If the MDDF contains a password, show it
                    if(buf.Length > 0)
                        xattrs.Add("com.apple.lisa.password");
                }
            }
            else
            {
                // Search for the file
                Errno error = ReadExtentsFile(fileId, out ExtentFile file);

                if(error != Errno.NoError)
                    return error;

                xattrs = new List<string>();

                // Password field is never emptied, check if valid
                if(file.password_valid > 0)
                    xattrs.Add("com.apple.lisa.password");

                // Check for a valid copy-protection serial number
                if(file.serial > 0)
                    xattrs.Add("com.apple.lisa.serial");

                // Check if the label contains something or is empty
                if(!ArrayHelpers.ArrayIsNullOrEmpty(file.LisaInfo))
                    xattrs.Add("com.apple.lisa.label");
            }

            // On debug mode allow sector tags to be accessed as an xattr
            if(_debug)
                xattrs.Add("com.apple.lisa.tags");

            xattrs.Sort();

            return Errno.NoError;
        }

        /// <summary>Lists special Apple Lisa filesystem features as extended attributes</summary>
        /// <returns>Error number.</returns>
        /// <param name="fileId">File identifier.</param>
        /// <param name="xattr">Extended attribute name.</param>
        /// <param name="buf">Buffer where the extended attribute will be stored.</param>
        Errno GetXattr(short fileId, string xattr, out byte[] buf)
        {
            buf = null;

            if(!_mounted)
                return Errno.AccessDenied;

            // System files
            if(fileId < 4)
            {
                if(!_debug ||
                   fileId == 0)
                    return Errno.InvalidArgument;

                // Only MDDF contains an extended attributes
                if(fileId == FILEID_MDDF)
                    if(xattr == "com.apple.lisa.password")
                    {
                        buf = Encoding.ASCII.GetBytes(_mddf.password);

                        return Errno.NoError;
                    }

                // But on debug mode even system files contain tags
                if(_debug && xattr == "com.apple.lisa.tags")
                    return ReadSystemFile(fileId, out buf, true);

                return Errno.NoSuchExtendedAttribute;
            }

            // Search for the file
            Errno error = ReadExtentsFile(fileId, out ExtentFile file);

            if(error != Errno.NoError)
                return error;

            switch(xattr)
            {
                case "com.apple.lisa.password" when file.password_valid > 0:
                    buf = new byte[8];
                    Array.Copy(file.password, 0, buf, 0, 8);

                    return Errno.NoError;
                case "com.apple.lisa.serial" when file.serial > 0:
                    buf = Encoding.ASCII.GetBytes(file.serial.ToString());

                    return Errno.NoError;
            }

            if(!ArrayHelpers.ArrayIsNullOrEmpty(file.LisaInfo) &&
               xattr == "com.apple.lisa.label")
            {
                buf = new byte[128];
                Array.Copy(file.LisaInfo, 0, buf, 0, 128);

                return Errno.NoError;
            }

            if(_debug && xattr == "com.apple.lisa.tags")
                return ReadFile(fileId, out buf, true);

            return Errno.NoSuchExtendedAttribute;
        }

        /// <summary>Decodes a sector tag. Not tested with 24-byte tags.</summary>
        /// <returns>Error number.</returns>
        /// <param name="tag">Sector tag.</param>
        /// <param name="decoded">Decoded sector tag.</param>
        static Errno DecodeTag(byte[] tag, out LisaTag.PriamTag decoded)
        {
            decoded = new LisaTag.PriamTag();
            LisaTag.PriamTag? pmTag = LisaTag.DecodeTag(tag);

            if(!pmTag.HasValue)
                return Errno.InvalidArgument;

            decoded = pmTag.Value;

            return Errno.NoError;
        }
    }
}