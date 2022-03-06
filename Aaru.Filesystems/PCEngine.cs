// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : PCEngine.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : NEC PC-Engine CD filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the NEC PC-Engine CD filesystem and shows information.
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
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Schemas;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the PC-Engine CD file headers</summary>
public sealed class PCEnginePlugin : IFilesystem
{
    /// <inheritdoc />
    public FileSystemType XmlFsType { get; private set; }
    /// <inheritdoc />
    public Encoding Encoding { get; private set; }
    /// <inheritdoc />
    public string Name => "PC Engine CD Plugin";
    /// <inheritdoc />
    public Guid Id => new("e5ee6d7c-90fa-49bd-ac89-14ef750b8af3");
    /// <inheritdoc />
    public string Author => "Natalia Portillo";

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(2 + partition.Start >= partition.End)
            return false;

        byte[]      systemDescriptor = new byte[23];
        ErrorNumber errno            = imagePlugin.ReadSector(1 + partition.Start, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        Array.Copy(sector, 0x20, systemDescriptor, 0, 23);

        return Encoding.ASCII.GetString(systemDescriptor) == "PC Engine CD-ROM SYSTEM";
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                               Encoding encoding)
    {
        Encoding    = encoding ?? Encoding.GetEncoding("shift_jis");
        information = "";

        XmlFsType = new FileSystemType
        {
            Type        = "PC Engine filesystem",
            Clusters    = (partition.End - partition.Start + 1) / imagePlugin.Info.SectorSize * 2048,
            ClusterSize = 2048
        };
    }
}