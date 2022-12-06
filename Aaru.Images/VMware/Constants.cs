// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Constants.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains constants for VMware disk images.
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
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;

namespace Aaru.DiscImages
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public sealed partial class VMware
    {
        const uint VMWARE_EXTENT_MAGIC = 0x564D444B;
        const uint VMWARE_COW_MAGIC    = 0x44574F43;

        const string VM_TYPE_CUSTOM       = "custom";
        const string VM_TYPE_MONO_SPARSE  = "monolithicSparse";
        const string VM_TYPE_MONO_FLAT    = "monolithicFlat";
        const string VM_TYPE_SPLIT_SPARSE = "twoGbMaxExtentSparse";
        const string VM_TYPE_SPLIT_FLAT   = "twoGbMaxExtentFlat";
        const string VM_TYPE_FULL_DEVICE  = "fullDevice";
        const string VM_TYPE_PART_DEVICE  = "partitionedDevice";
        const string VMFS_TYPE_FLAT       = "vmfsPreallocated";
        const string VMFS_TYPE_ZERO       = "vmfsEagerZeroedThick";
        const string VMFS_TYPE_THIN       = "vmfsThin";
        const string VMFS_TYPE_SPARSE     = "vmfsSparse";
        const string VMFS_TYPE_RDM        = "vmfsRDM";
        const string VMFS_TYPE_RDM_OLD    = "vmfsRawDeviceMap";
        const string VMFS_TYPE_RDMP       = "vmfsRDMP";
        const string VMFS_TYPE_RDMP_OLD   = "vmfsPassthroughRawDeviceMap";
        const string VMFS_TYPE_RAW        = "vmfsRaw";
        const string VMFS_TYPE            = "vmfs";
        const string VM_TYPE_STREAM       = "streamOptimized";

        const string DDF_MAGIC = "# Disk DescriptorFile";

        const string REGEX_VERSION    = @"^\s*version\s*=\s*(?<version>\d+)$";
        const string REGEX_CID        = @"^\s*CID\s*=\s*(?<cid>[0123456789abcdef]{8})$";
        const string REGEX_CID_PARENT = @"^\s*parentCID\s*=\s*(?<cid>[0123456789abcdef]{8})$";
        const string REGEX_TYPE =
            @"^\s*createType\s*=\s*\""(?<type>custom|monolithicSparse|monolithicFlat|twoGbMaxExtentSparse|twoGbMaxExtentFlat|fullDevice|partitionedDevice|vmfs|vmfsPreallocated|vmfsEagerZeroedThick|vmfsThin|vmfsSparse|vmfsRDM|vmfsRawDeviceMap|vmfsRDMP|vmfsPassthroughRawDeviceMap|vmfsRaw|streamOptimized)\""$";
        const string REGEX_EXTENT =
            @"^\s*(?<access>(RW|RDONLY|NOACCESS))\s+(?<sectors>\d+)\s+(?<type>(FLAT|SPARSE|ZERO|VMFS|VMFSSPARSE|VMFSRDM|VMFSRAW))\s+\""(?<filename>.+)\""(\s*(?<offset>\d+))?$";
        const string REGEX_DDB_TYPE = @"^\s*ddb\.adapterType\s*=\s*\""(?<type>ide|buslogic|lsilogic|legacyESX)\""$";
        const string REGEX_DDB_SECTORS = @"^\s*ddb\.geometry\.sectors\s*=\s*\""(?<sectors>\d+)\""$";
        const string REGEX_DDB_HEADS = @"^\s*ddb\.geometry\.heads\s*=\s*\""(?<heads>\d+)\""$";
        const string REGEX_DDB_CYLINDERS = @"^\s*ddb\.geometry\.cylinders\s*=\s*\""(?<cylinders>\d+)\""$";
        const string PARENT_REGEX = @"^\s*parentFileNameHint\s*=\s*\""(?<filename>.+)\""$";

        const uint FLAGS_VALID_NEW_LINE      = 0x01;
        const uint FLAGS_USE_REDUNDANT_TABLE = 0x02;
        const uint FLAGS_ZERO_GRAIN_GTE      = 0x04;
        const uint FLAGS_COMPRESSION         = 0x10000;
        const uint FLAGS_MARKERS             = 0x20000;

        const ushort COMPRESSION_NONE    = 0;
        const ushort COMPRESSION_DEFLATE = 1;

        const uint MAX_CACHE_SIZE     = 16777216;
        const uint SECTOR_SIZE        = 512;
        const uint MAX_CACHED_SECTORS = MAX_CACHE_SIZE / SECTOR_SIZE;
        readonly byte[] _ddfMagicBytes =
        {
            0x23, 0x20, 0x44, 0x69, 0x73, 0x6B, 0x20, 0x44, 0x65, 0x73, 0x63, 0x72, 0x69, 0x70, 0x74, 0x6F, 0x72, 0x46,
            0x69, 0x6C, 0x65
        };
    }
}