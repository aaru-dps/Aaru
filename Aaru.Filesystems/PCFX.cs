// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : PCEngine.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : NEC PC-FX plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the NEC PC-FX track header and shows information.
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
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

// Not a filesystem, more like an executable header
/// <inheritdoc />
/// <summary>Implements detection of NEC PC-FX headers</summary>
public sealed class PCFX : IFilesystem
{
    const string IDENTIFIER = "PC-FX:Hu_CD-ROM ";

    const string FS_TYPE = "pcfx";
    /// <inheritdoc />
    public FileSystemType XmlFsType { get; private set; }
    /// <inheritdoc />
    public Encoding Encoding { get; private set; }
    /// <inheritdoc />
    public string Name => Localization.PCFX_Name;
    /// <inheritdoc />
    public Guid Id => new("8BC27CCE-D9E9-48F8-BA93-C66A86EB565A");
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(2 + partition.Start           >= partition.End ||
           imagePlugin.Info.XmlMediaType != XmlMediaType.OpticalDisc)
            return false;

        ErrorNumber errno = imagePlugin.ReadSectors(partition.Start, 2, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        var encoding = Encoding.GetEncoding("shift_jis");

        return encoding.GetString(sector, 0, 16) == IDENTIFIER;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        // Always Shift-JIS
        Encoding    = Encoding.GetEncoding("shift_jis");
        information = "";

        ErrorNumber errno = imagePlugin.ReadSectors(partition.Start, 2, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return;

        Header header = Marshal.ByteArrayToStructureLittleEndian<Header>(sector);

        string   date;
        DateTime dateTime = DateTime.MinValue;

        try
        {
            date = Encoding.GetString(header.date);
            int year  = int.Parse(date[..4]);
            int month = int.Parse(date.Substring(4, 2));
            int day   = int.Parse(date.Substring(6, 2));
            dateTime = new DateTime(year, month, day);
        }
        catch
        {
            date = null;
        }

        var sb = new StringBuilder();
        sb.AppendLine(Localization.PC_FX_executable);
        sb.AppendFormat(Localization.Identifier_0, StringHandlers.CToString(header.signature, Encoding)).AppendLine();
        sb.AppendFormat(Localization.Copyright_0, StringHandlers.CToString(header.copyright, Encoding)).AppendLine();
        sb.AppendFormat(Localization.Title_0, StringHandlers.CToString(header.title, Encoding)).AppendLine();
        sb.AppendFormat(Localization.Maker_ID_0, StringHandlers.CToString(header.makerId, Encoding)).AppendLine();
        sb.AppendFormat(Localization.Maker_name_0, StringHandlers.CToString(header.makerName, Encoding)).AppendLine();
        sb.AppendFormat(Localization.Volume_number_0, header.volumeNumber).AppendLine();
        sb.AppendFormat(Localization.Country_code_0, header.country).AppendLine();
        sb.AppendFormat(Localization.Version_0_1, header.minorVersion, header.majorVersion).AppendLine();

        if(date != null)
            sb.AppendFormat(Localization.Dated_0, dateTime).AppendLine();

        sb.AppendFormat(Localization.Load_0_sectors_from_sector_1, header.loadCount, header.loadOffset).AppendLine();

        sb.AppendFormat(Localization.Load_at_0_and_jump_to_1, header.loadAddress, header.entryPoint).AppendLine();

        information = sb.ToString();

        XmlFsType = new FileSystemType
        {
            Type                  = FS_TYPE,
            Clusters              = partition.Length,
            ClusterSize           = 2048,
            Bootable              = true,
            CreationDate          = dateTime,
            CreationDateSpecified = date != null,
            PublisherIdentifier   = StringHandlers.CToString(header.makerName, Encoding),
            VolumeName            = StringHandlers.CToString(header.title, Encoding),
            SystemIdentifier      = "PC-FX"
        };
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Header
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] signature;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0xE0)]
        public readonly byte[] copyright;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x710)]
        public readonly byte[] unknown;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] title;
        public readonly uint loadOffset;
        public readonly uint loadCount;
        public readonly uint loadAddress;
        public readonly uint entryPoint;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly byte[] makerId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
        public readonly byte[] makerName;
        public readonly uint   volumeNumber;
        public readonly byte   majorVersion;
        public readonly byte   minorVersion;
        public readonly ushort country;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] date;
    }
}