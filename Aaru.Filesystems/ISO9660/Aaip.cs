// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Aaip.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decoding of Arbitrary Attribute Interchange Protocol attributes and ACLs.
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
using System.Linq;
using System.Text;

namespace Aaru.Filesystems;

public sealed partial class ISO9660
{
    /// <summary>Namespaces of AAIP names starting with a reserved byte, indexed by that byte</summary>
    static readonly string[] _aaipNamespaces = [null, "", "system.", "user.", "isofs.", "trusted.", "security."];

    /// <summary>Decodes the component records of an AAIP "AL" entry into the attribute list of the entry</summary>
    /// <param name="data">Buffer holding the entry</param>
    /// <param name="offset">Offset of the entry in the buffer</param>
    /// <param name="length">Length of the entry</param>
    /// <param name="entry">Directory entry receiving the components</param>
    static void DecodeAaipEntry(byte[] data, int offset, int length, DecodedDirectoryEntry entry)
    {
        // A previous "AL" ended the attribute list, so this one starts a new one
        if(!entry.AaipContinues)
        {
            entry.AaipComponents         = [];
            entry.AaipComponentContinues = false;
            entry.AaipPendingBytes       = 0;
        }

        int end          = Math.Min(offset + length, data.Length);
        int componentOff = offset + 5;

        // The last component record of an "AL" can continue, without a new header, at the start of the next one
        if(entry.AaipPendingBytes > 0 && entry.AaipComponents.Count > 0)
        {
            int pending = Math.Min(entry.AaipPendingBytes, end - componentOff);

            AppendToLastComponent(entry.AaipComponents, data, componentOff, pending);

            entry.AaipPendingBytes -= pending;
            componentOff           += pending;
        }

        // Component records are laid out as in "SL" (AAIP 2.0)
        while(componentOff + 2 <= end)
        {
            byte flags           = data[componentOff];
            int  componentLength = Math.Min(data[componentOff + 1], end - componentOff - 2);

            if(entry.AaipComponentContinues && entry.AaipComponents.Count > 0)
                AppendToLastComponent(entry.AaipComponents, data, componentOff + 2, componentLength);
            else
            {
                var component = new byte[componentLength];
                Array.Copy(data, componentOff + 2, component, 0, componentLength);
                entry.AaipComponents.Add(component);
            }

            entry.AaipComponentContinues = (flags & 1) != 0;
            entry.AaipPendingBytes       = data[componentOff + 1] - componentLength;

            componentOff += 2 + componentLength;
        }

        entry.AaipContinues = (data[offset + 4] & (byte)AAIPFlags.Continue) != 0;
    }

    static void AppendToLastComponent(List<byte[]> components, byte[] data, int offset, int length)
    {
        byte[] previous = components[^1];
        var    joined   = new byte[previous.Length + length];
        Array.Copy(previous, 0,      joined, 0,               previous.Length);
        Array.Copy(data,     offset, joined, previous.Length, length);
        components[^1] = joined;
    }

    /// <summary>Gets the attributes recorded with AAIP, the ACLs converted to Linux extended attributes</summary>
    /// <param name="entry">Directory entry</param>
    /// <returns>Attribute names and values</returns>
    static List<(string name, byte[] value)> GetAaipAttributes(DecodedDirectoryEntry entry)
    {
        List<(string name, byte[] value)> attributes = [];

        if(entry.AaipComponents is null) return attributes;

        // Components are pairs of name and value
        for(var i = 0; i + 1 < entry.AaipComponents.Count; i += 2)
        {
            byte[] name  = entry.AaipComponents[i];
            byte[] value = entry.AaipComponents[i + 1];

            // An empty name means the value is the ACL of the file
            if(name.Length == 0)
            {
                ConvertAaipAcl(value, entry, out byte[] access, out byte[] @default);

                if(access != null) attributes.Add((Xattrs.XATTR_POSIX_ACL_ACCESS, access));

                if(@default != null) attributes.Add((Xattrs.XATTR_POSIX_ACL_DEFAULT, @default));

                continue;
            }

            string prefix = "";
            var    start  = 0;

            if(name[0] < _aaipNamespaces.Length && _aaipNamespaces[name[0]] is not null)
            {
                prefix = _aaipNamespaces[name[0]];
                start  = 1;
            }

            attributes.Add((prefix + Encoding.UTF8.GetString(name, start, name.Length - start), value));
        }

        return attributes;
    }

    /// <summary>Converts an AAIP binary ACL to the access and default ACLs Linux uses as extended attributes</summary>
    /// <param name="aaip">AAIP binary ACL</param>
    /// <param name="entry">Directory entry, for the permissions of entries the ACL does not record</param>
    /// <param name="access">Access ACL, or <c>null</c> if not recorded</param>
    /// <param name="default">Default ACL, or <c>null</c> if not recorded</param>
    static void ConvertAaipAcl(byte[] aaip, DecodedDirectoryEntry entry, out byte[] access, out byte[] @default)
    {
        List<(ushort tag, ushort perm, uint id)> accessEntries  = [];
        List<(ushort tag, ushort perm, uint id)> defaultEntries = [];
        List<(ushort tag, ushort perm, uint id)> current        = accessEntries;

        var off = 0;

        while(off < aaip.Length)
        {
            byte flags = aaip[off++];
            int  type  = flags >> 4;
            var  perm  = (ushort)(flags & 7);

            // Qualifiers are split in records of up to 127 bytes, bit 7 of the head telling another one follows
            List<byte> qualifier = [];

            if((flags & 8) != 0)
            {
                while(off < aaip.Length)
                {
                    byte head = aaip[off++];
                    int  len  = Math.Min(head & 0x7F, aaip.Length - off);

                    qualifier.AddRange(aaip.Skip(off).Take(len));
                    off += len;

                    if((head & 0x80) == 0) break;
                }
            }

            // Numeric qualifiers are recorded most significant byte first, in up to 4 bytes
            uint id = qualifier.Count is > 0 and <= 4
                          ? qualifier.Aggregate(0u, static (number, b) => number << 8 | b)
                          : ACL_UNDEFINED_ID;

            switch(type)
            {
                case AAIP_ACL_USER_OBJ:
                    current.Add((ACL_USER_OBJ, perm, ACL_UNDEFINED_ID));

                    break;
                case AAIP_ACL_GROUP_OBJ:
                    current.Add((ACL_GROUP_OBJ, perm, ACL_UNDEFINED_ID));

                    break;
                case AAIP_ACL_MASK:
                    current.Add((ACL_MASK, perm, ACL_UNDEFINED_ID));

                    break;
                case AAIP_ACL_OTHER:
                    current.Add((ACL_OTHER, perm, ACL_UNDEFINED_ID));

                    break;
                case AAIP_ACL_USER_N when id != ACL_UNDEFINED_ID:
                    current.Add((ACL_USER, perm, id));

                    break;
                case AAIP_ACL_GROUP_N when id != ACL_UNDEFINED_ID:
                    current.Add((ACL_GROUP, perm, id));

                    break;
                case AAIP_ACL_SWITCH_MARK:
                    current = defaultEntries;

                    break;

                // TRANSLATE entries and unknown types are not ACL entries of the file
            }
        }

        // The base entries of the access ACL are the "PX" permissions when the ACL does not record them
        if(accessEntries.Count > 0)
        {
            uint mode = entry.PosixAttributes != null
                            ? (uint)entry.PosixAttributes.Value.st_mode
                            : (uint)(entry.PosixAttributesOld?.st_mode ?? 0);

            bool hasMask = accessEntries.Any(static e => e.tag == ACL_MASK);

            if(accessEntries.All(static e => e.tag != ACL_USER_OBJ))
                accessEntries.Add((ACL_USER_OBJ, (ushort)(mode >> 6 & 7), ACL_UNDEFINED_ID));

            // With a mask, the group permissions of the mode are the mask, so the owning group ones are unknown
            if(!hasMask && accessEntries.All(static e => e.tag != ACL_GROUP_OBJ))
                accessEntries.Add((ACL_GROUP_OBJ, (ushort)(mode >> 3 & 7), ACL_UNDEFINED_ID));

            if(accessEntries.All(static e => e.tag != ACL_OTHER))
                accessEntries.Add((ACL_OTHER, (ushort)(mode & 7), ACL_UNDEFINED_ID));
        }

        access   = EncodeLinuxAcl(accessEntries);
        @default = EncodeLinuxAcl(defaultEntries);
    }

    /// <summary>Encodes ACL entries as the system.posix_acl_* extended attributes of Linux</summary>
    /// <returns>The encoded ACL, or <c>null</c> if there are no entries</returns>
    static byte[] EncodeLinuxAcl(List<(ushort tag, ushort perm, uint id)> entries)
    {
        if(entries.Count == 0) return null;

        // Linux requires the entries sorted by tag, and named ones by identifier
        List<(ushort tag, ushort perm, uint id)> sorted = entries.OrderBy(static e => e.tag)
                                                                 .ThenBy(static e => e.id)
                                                                 .ToList();

        var acl = new byte[4 + sorted.Count * 8];
        BitConverter.GetBytes(LINUX_ACL_VERSION).CopyTo(acl, 0);

        for(var i = 0; i < sorted.Count; i++)
        {
            BitConverter.GetBytes(sorted[i].tag).CopyTo(acl, 4  + i * 8);
            BitConverter.GetBytes(sorted[i].perm).CopyTo(acl, 4 + i * 8 + 2);
            BitConverter.GetBytes(sorted[i].id).CopyTo(acl, 4   + i * 8 + 4);
        }

        return acl;
    }
}