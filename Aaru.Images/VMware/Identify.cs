// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.IO;
using System.Linq;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.DiscImages
{
    public sealed partial class VMware
    {
        /// <inheritdoc />
        public bool Identify(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            byte[] ddfMagic = new byte[0x15];

            if(stream.Length > Marshal.SizeOf<ExtentHeader>())
            {
                stream.Seek(0, SeekOrigin.Begin);
                byte[] vmEHdrB = new byte[Marshal.SizeOf<ExtentHeader>()];
                stream.Read(vmEHdrB, 0, Marshal.SizeOf<ExtentHeader>());
                _vmEHdr = Marshal.ByteArrayToStructureLittleEndian<ExtentHeader>(vmEHdrB);

                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(ddfMagic, 0, 0x15);

                _vmCHdr = new CowHeader();

                if(stream.Length <= Marshal.SizeOf<CowHeader>())
                    return _ddfMagicBytes.SequenceEqual(ddfMagic) || _vmEHdr.magic == VMWARE_EXTENT_MAGIC ||
                           _vmCHdr.magic                                           == VMWARE_COW_MAGIC;

                stream.Seek(0, SeekOrigin.Begin);
                byte[] vmCHdrB = new byte[Marshal.SizeOf<CowHeader>()];
                stream.Read(vmCHdrB, 0, Marshal.SizeOf<CowHeader>());
                _vmCHdr = Marshal.ByteArrayToStructureLittleEndian<CowHeader>(vmCHdrB);

                return _ddfMagicBytes.SequenceEqual(ddfMagic) || _vmEHdr.magic == VMWARE_EXTENT_MAGIC ||
                       _vmCHdr.magic                                           == VMWARE_COW_MAGIC;
            }

            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(ddfMagic, 0, 0x15);

            return _ddfMagicBytes.SequenceEqual(ddfMagic);
        }
    }
}