// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Identify.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies VMware disk images.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes.Interfaces;

namespace DiscImageChef.DiscImages
{
    public partial class VMware
    {
        public bool Identify(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            byte[] ddfMagic = new byte[0x15];

            if(stream.Length > Marshal.SizeOf(vmEHdr))
            {
                stream.Seek(0, SeekOrigin.Begin);
                byte[] vmEHdrB = new byte[Marshal.SizeOf(vmEHdr)];
                stream.Read(vmEHdrB, 0, Marshal.SizeOf(vmEHdr));
                vmEHdr = Helpers.Marshal.ByteArrayToStructureLittleEndian<VMwareExtentHeader>(vmEHdrB);

                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(ddfMagic, 0, 0x15);

                vmCHdr = new VMwareCowHeader();
                if(stream.Length <= Marshal.SizeOf(vmCHdr))
                    return ddfMagicBytes.SequenceEqual(ddfMagic) || vmEHdr.magic == VMWARE_EXTENT_MAGIC ||
                           vmCHdr.magic                                          == VMWARE_COW_MAGIC;

                stream.Seek(0, SeekOrigin.Begin);
                byte[] vmCHdrB = new byte[Marshal.SizeOf(vmCHdr)];
                stream.Read(vmCHdrB, 0, Marshal.SizeOf(vmCHdr));
                vmCHdr = Helpers.Marshal.ByteArrayToStructureLittleEndian<VMwareCowHeader>(vmCHdrB);

                return ddfMagicBytes.SequenceEqual(ddfMagic) || vmEHdr.magic == VMWARE_EXTENT_MAGIC ||
                       vmCHdr.magic                                          == VMWARE_COW_MAGIC;
            }

            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(ddfMagic, 0, 0x15);

            return ddfMagicBytes.SequenceEqual(ddfMagic);
        }
    }
}