// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ODS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Files-11 On-Disk Structure plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Files-11 On-Disk Structure and shows information.
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

using System;
using System.Text;
using Aaru.CommonTypes.Interfaces;
using Schemas;

namespace Aaru.Filesystems;

// Information from VMS File System Internals by Kirby McCoy
// ISBN: 1-55558-056-4
// With some hints from http://www.decuslib.com/DECUS/vmslt97b/gnusoftware/gccaxp/7_1/vms/hm2def.h
// Expects the home block to be always in sector #1 (does not check deltas)
// Assumes a sector size of 512 bytes (VMS does on HDDs and optical drives, dunno about M.O.)
// Book only describes ODS-2. Need to test ODS-1 and ODS-5
// There is an ODS with signature "DECFILES11A", yet to be seen
// Time is a 64 bit unsigned integer, tenths of microseconds since 1858/11/17 00:00:00.
// TODO: Implement checksum
/// <inheritdoc />
/// <summary>Implements detection of DEC's On-Disk Structure, aka the ODS filesystem</summary>
public sealed partial class ODS : IFilesystem
{
    /// <inheritdoc />
    public FileSystemType XmlFsType { get; private set; }
    /// <inheritdoc />
    public Encoding Encoding { get; private set; }
    /// <inheritdoc />
    public string Name => Localization.ODS_Name;
    /// <inheritdoc />
    public Guid Id => new("de20633c-8021-4384-aeb0-83b0df14491f");
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;
}