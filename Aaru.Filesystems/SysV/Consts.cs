// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : UNIX System V filesystem plugin.
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

// ReSharper disable NotAccessedField.Local

using System.Diagnostics.CodeAnalysis;

namespace Aaru.Filesystems;

// Information from the Linux kernel
/// <inheritdoc />
/// <summary>Implements detection of the UNIX System V filesystem</summary>
[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "UnusedMember.Local"),
 SuppressMessage("ReSharper", "UnusedType.Local")]
public sealed partial class SysVfs
{
    const uint XENIX_MAGIC = 0x002B5544;
    const uint XENIX_CIGAM = 0x44552B00;
    const uint SYSV_MAGIC  = 0xFD187E20;
    const uint SYSV_CIGAM  = 0x207E18FD;

    // Rest have no magic.
    // Per a Linux kernel, Coherent fs has following:
    const string COH_FNAME = "noname";
    const string COH_FPACK = "nopack";
    const string COH_XXXXX = "xxxxx";
    const string COH_XXXXS = "xxxxx ";
    const string COH_XXXXN = "xxxxx\n";

    // SCO AFS
    const ushort SCO_NFREE = 0xFFFF;

    // UNIX 7th Edition has nothing to detect it, so check for a valid filesystem is a must :(
    const ushort V7_NICINOD = 100;
    const ushort V7_NICFREE = 100;
    const uint   V7_MAXSIZE = 0x00FFFFFF;

    const string FS_TYPE_XENIX    = "xenixfs";
    const string FS_TYPE_SVR4     = "sysv_r4";
    const string FS_TYPE_SVR2     = "sysv_r2";
    const string FS_TYPE_COHERENT = "coherent";
    const string FS_TYPE_UNIX7    = "unix7fs";
}