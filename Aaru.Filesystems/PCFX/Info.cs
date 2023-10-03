// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

// Not a filesystem, more like an executable header
/// <inheritdoc />
/// <summary>Implements detection of NEC PC-FX headers</summary>
public sealed partial class PCFX
{
#region IFilesystem Members

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(2 + partition.Start                >= partition.End ||
           imagePlugin.Info.MetadataMediaType != MetadataMediaType.OpticalDisc)
            return false;

        ErrorNumber errno = imagePlugin.ReadSectors(partition.Start, 2, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        var encoding = Encoding.GetEncoding("shift_jis");

        return encoding.GetString(sector, 0, 16) == IDENTIFIER;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        // Always Shift-JIS
        encoding    = Encoding.GetEncoding("shift_jis");
        information = "";
        metadata    = new FileSystem();

        ErrorNumber errno = imagePlugin.ReadSectors(partition.Start, 2, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return;

        Header header = Marshal.ByteArrayToStructureLittleEndian<Header>(sector);

        string   date;
        DateTime dateTime = DateTime.MinValue;

        try
        {
            date = encoding.GetString(header.date);
            var year  = int.Parse(date[..4]);
            var month = int.Parse(date.Substring(4, 2));
            var day   = int.Parse(date.Substring(6, 2));
            dateTime = new DateTime(year, month, day);
        }
        catch
        {
            date = null;
        }

        var sb = new StringBuilder();
        sb.AppendLine(Localization.PC_FX_executable);
        sb.AppendFormat(Localization.Identifier_0, StringHandlers.CToString(header.signature, encoding)).AppendLine();
        sb.AppendFormat(Localization.Copyright_0, StringHandlers.CToString(header.copyright, encoding)).AppendLine();
        sb.AppendFormat(Localization.Title_0, StringHandlers.CToString(header.title, encoding)).AppendLine();
        sb.AppendFormat(Localization.Maker_ID_0, StringHandlers.CToString(header.makerId, encoding)).AppendLine();
        sb.AppendFormat(Localization.Maker_name_0, StringHandlers.CToString(header.makerName, encoding)).AppendLine();
        sb.AppendFormat(Localization.Volume_number_0, header.volumeNumber).AppendLine();
        sb.AppendFormat(Localization.Country_code_0, header.country).AppendLine();
        sb.AppendFormat(Localization.Version_0_1, header.minorVersion, header.majorVersion).AppendLine();

        if(date != null)
            sb.AppendFormat(Localization.Dated_0, dateTime).AppendLine();

        sb.AppendFormat(Localization.Load_0_sectors_from_sector_1, header.loadCount, header.loadOffset).AppendLine();

        sb.AppendFormat(Localization.Load_at_0_and_jump_to_1, header.loadAddress, header.entryPoint).AppendLine();

        information = sb.ToString();

        metadata = new FileSystem
        {
            Type                = FS_TYPE,
            Clusters            = partition.Length,
            ClusterSize         = 2048,
            Bootable            = true,
            CreationDate        = date != null ? dateTime : null,
            PublisherIdentifier = StringHandlers.CToString(header.makerName, encoding),
            VolumeName          = StringHandlers.CToString(header.title,     encoding),
            SystemIdentifier    = "PC-FX"
        };
    }

#endregion
}