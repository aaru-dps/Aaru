// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MDFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Aaru unit testing.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems;

// Windows volumes record an application identifier with Shift-JIS text, which is not checked here as the names on
// these volumes were written with a Western code page.
[TestFixture]
public class Mdfs() : ReadOnlyFilesystemTest("mdfs")
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "MiniDisc FileSystem");

    public override IFilesystem Plugin     => new MDFS();
    public override bool        Partitions => false;

    public override FileSystemTest[] Tests =>
    [
        new()
        {
            TestFile      = "mdfs_dos_empty.aif",
            MediaType     = MediaType.MDData,
            Sectors       = 70464,
            SectorSize    = 2048,
            Clusters      = 17616,
            ClusterSize   = 8192,
            VolumeName    = "VolumeLabel31",
            EncodingName  = "ibm437",
            ApplicationId = "MD DATA Utility, version 1.94; Sony Corporation, Boulder, Colorado"
        },
        new()
        {
            TestFile      = "mdfs_dos_full.aif",
            MediaType     = MediaType.MDData,
            Sectors       = 70464,
            SectorSize    = 2048,
            Clusters      = 17616,
            ClusterSize   = 8192,
            VolumeName    = "QuickFormatDOS",
            EncodingName  = "ibm437",
            ApplicationId = "MD DATA Utility, version 1.94; Sony Corporation, Boulder, Colorado"
        },
        new()
        {
            TestFile      = "mdfs_dos_setter.aif",
            MediaType     = MediaType.MDData,
            Sectors       = 70464,
            SectorSize    = 2048,
            Clusters      = 17616,
            ClusterSize   = 8192,
            VolumeName    = "VolumeLabel31",
            EncodingName  = "ibm437",
            ApplicationId = "MD DATA Utility, version 1.94; Sony Corporation, Boulder, Colorado"
        },
        new()
        {
            TestFile      = "mdfs_macos_fssetter.aif",
            MediaType     = MediaType.MDData,
            Sectors       = 70464,
            SectorSize    = 2048,
            Clusters      = 17616,
            ClusterSize   = 8192,
            VolumeName    = "Volume label",
            ApplicationId = "MD File System, version1.61; Sony Corporation, Atsugi, Japan"
        },
        new()
        {
            TestFile      = "mdfs_w95_empty.aif",
            MediaType     = MediaType.MDData,
            Sectors       = 70464,
            SectorSize    = 2048,
            Clusters      = 17616,
            ClusterSize   = 8192,
            VolumeName    = "Volume Label 31",
            EncodingName  = "windows-1252"
        },
        new()
        {
            TestFile      = "mdfs_w95_setter.aif",
            MediaType     = MediaType.MDData,
            Sectors       = 70464,
            SectorSize    = 2048,
            Clusters      = 17616,
            ClusterSize   = 8192,
            VolumeName    = "Volume Label 31",
            EncodingName  = "windows-1252"
        },
        new()
        {
            TestFile     = "mdfs_w95_setter.img",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 281856,
            SectorSize   = 512,
            Clusters     = 17616,
            ClusterSize  = 8192,
            VolumeName   = "Volume Label 31",
            EncodingName = "windows-1252"
        },
        new()
        {
            TestFile      = "mdfs_win_bushound_setter.aif",
            MediaType     = MediaType.MDData,
            Sectors       = 70464,
            SectorSize    = 2048,
            Clusters      = 17616,
            ClusterSize   = 8192,
            VolumeName    = "QuickFormat",
            EncodingName  = "windows-1252"
        },
        new()
        {
            TestFile      = "mdfs_win_extvma_fssetter.aif",
            MediaType     = MediaType.MDData,
            Sectors       = 70464,
            SectorSize    = 2048,
            Clusters      = 4404,
            ClusterSize   = 32768,
            VolumeName    = "ExtVMA",
            EncodingName  = "windows-1252"
        },
        new()
        {
            TestFile      = "mdfs_win_extvma_mirror_fssetter.aif",
            MediaType     = MediaType.MDData,
            Sectors       = 70464,
            SectorSize    = 2048,
            Clusters      = 2202,
            ClusterSize   = 65536,
            VolumeName    = "mirror extvma full",
            EncodingName  = "windows-1252"
        },
        new()
        {
            TestFile      = "mdfs_win_full.aif",
            MediaType     = MediaType.MDData,
            Sectors       = 70464,
            SectorSize    = 2048,
            Clusters      = 17616,
            ClusterSize   = 8192,
            VolumeName    = "Quick Format Windows 95",
            EncodingName  = "windows-1252"
        },
        new()
        {
            TestFile      = "mdfs_win_mirror_fssetter.aif",
            MediaType     = MediaType.MDData,
            Sectors       = 70464,
            SectorSize    = 2048,
            Clusters      = 17616,
            ClusterSize   = 8192,
            VolumeName    = "Mirror mode quick",
            EncodingName  = "windows-1252"
        }
    ];
}