// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 10_SSC.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MODE PAGE 10h: Device configuration page.
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
using System.Text;

namespace Aaru.Decoders.SCSI;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static partial class Modes
{
#region Mode Page 0x10: Device configuration page

    /// <summary>Device configuration page Page code 0x10 16 bytes in SCSI-2, SSC-1, SSC-2, SSC-3</summary>
    public struct ModePage_10_SSC
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        /// <summary>Used in mode select to change partition to one specified in <see cref="ActivePartition" /></summary>
        public bool CAP;
        /// <summary>Used in mode select to change format to one specified in <see cref="ActiveFormat" /></summary>
        public bool CAF;
        /// <summary>Active format, vendor-specific</summary>
        public byte ActiveFormat;
        /// <summary>Current logical partition</summary>
        public byte ActivePartition;
        /// <summary>How full the buffer shall be before writing to medium</summary>
        public byte WriteBufferFullRatio;
        /// <summary>How empty the buffer shall be before reading more data from the medium</summary>
        public byte ReadBufferEmptyRatio;
        /// <summary>Delay in 100 ms before buffered data is forcefully written to the medium even before buffer is full</summary>
        public ushort WriteDelayTime;
        /// <summary>Drive supports recovering data from buffer</summary>
        public bool DBR;
        /// <summary>Medium has block IDs</summary>
        public bool BIS;
        /// <summary>Drive recognizes and reports setmarks</summary>
        public bool RSmk;
        /// <summary>Drive selects best speed</summary>
        public bool AVC;
        /// <summary>If drive should stop pre-reading on filemarks</summary>
        public byte SOCF;
        /// <summary>If set, recovered buffer data is LIFO, otherwise, FIFO</summary>
        public bool RBO;
        /// <summary>Report early warnings</summary>
        public bool REW;
        /// <summary>Inter-block gap</summary>
        public byte GapSize;
        /// <summary>End-of-Data format</summary>
        public byte EODDefined;
        /// <summary>EOD generation enabled</summary>
        public bool EEG;
        /// <summary>Synchronize data to medium on early warning</summary>
        public bool SEW;
        /// <summary>Bytes to reduce buffer size on early warning</summary>
        public uint BufferSizeEarlyWarning;
        /// <summary>Selected data compression algorithm</summary>
        public byte SelectedCompression;

        /// <summary>Soft write protect</summary>
        public bool SWP;
        /// <summary>Associated write protect</summary>
        public bool ASOCWP;
        /// <summary>Persistent write protect</summary>
        public bool PERSWP;
        /// <summary>Permanent write protect</summary>
        public bool PRMWP;

        public bool BAML;
        public bool BAM;
        public byte RewindOnReset;

        /// <summary>How drive shall respond to detection of compromised WORM medium integrity</summary>
        public byte WTRE;
        /// <summary>Respond to commands only if a reservation exists</summary>
        public bool OIR;
    }

    public static ModePage_10_SSC? DecodeModePage_10_SSC(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40)
            return null;

        if((pageResponse?[0] & 0x3F) != 0x10)
            return null;

        if(pageResponse[1] + 2 != pageResponse.Length)
            return null;

        if(pageResponse.Length < 16)
            return null;

        var decoded = new ModePage_10_SSC();

        decoded.PS                   |= (pageResponse[0]       & 0x80) == 0x80;
        decoded.CAP                  |= (pageResponse[2]       & 0x40) == 0x40;
        decoded.CAF                  |= (pageResponse[2]       & 0x20) == 0x20;
        decoded.ActiveFormat         =  (byte)(pageResponse[2] & 0x1F);
        decoded.ActivePartition      =  pageResponse[3];
        decoded.WriteBufferFullRatio =  pageResponse[4];
        decoded.ReadBufferEmptyRatio =  pageResponse[5];
        decoded.WriteDelayTime       =  (ushort)((pageResponse[6] << 8) + pageResponse[7]);
        decoded.DBR                  |= (pageResponse[8]  & 0x80) == 0x80;
        decoded.BIS                  |= (pageResponse[8]  & 0x40) == 0x40;
        decoded.RSmk                 |= (pageResponse[8]  & 0x20) == 0x20;
        decoded.AVC                  |= (pageResponse[8]  & 0x10) == 0x10;
        decoded.RBO                  |= (pageResponse[8]  & 0x02) == 0x02;
        decoded.REW                  |= (pageResponse[8]  & 0x01) == 0x01;
        decoded.EEG                  |= (pageResponse[10] & 0x10) == 0x10;
        decoded.SEW                  |= (pageResponse[10] & 0x08) == 0x08;
        decoded.SOCF                 =  (byte)((pageResponse[8] & 0x0C) >> 2);

        decoded.BufferSizeEarlyWarning = (uint)((pageResponse[11] << 16) + (pageResponse[12] << 8) + pageResponse[13]);

        decoded.SelectedCompression = pageResponse[14];

        decoded.SWP    |= (pageResponse[10] & 0x04) == 0x04;
        decoded.ASOCWP |= (pageResponse[15] & 0x04) == 0x04;
        decoded.PERSWP |= (pageResponse[15] & 0x02) == 0x02;
        decoded.PRMWP  |= (pageResponse[15] & 0x01) == 0x01;

        decoded.BAML |= (pageResponse[10] & 0x02) == 0x02;
        decoded.BAM  |= (pageResponse[10] & 0x01) == 0x01;

        decoded.RewindOnReset = (byte)((pageResponse[15] & 0x18) >> 3);

        decoded.OIR  |= (pageResponse[15] & 0x20) == 0x20;
        decoded.WTRE =  (byte)((pageResponse[15] & 0xC0) >> 6);

        return decoded;
    }

    public static string PrettifyModePage_10_SSC(byte[] pageResponse) =>
        PrettifyModePage_10_SSC(DecodeModePage_10_SSC(pageResponse));

    public static string PrettifyModePage_10_SSC(ModePage_10_SSC? modePage)
    {
        if(!modePage.HasValue)
            return null;

        ModePage_10_SSC page = modePage.Value;
        var             sb   = new StringBuilder();

        sb.AppendLine(Localization.SCSI_Device_configuration_page);

        if(page.PS)
            sb.AppendLine("\t" + Localization.Parameters_can_be_saved);

        sb.AppendFormat("\t" + Localization.Active_format_0,    page.ActiveFormat).AppendLine();
        sb.AppendFormat("\t" + Localization.Active_partition_0, page.ActivePartition).AppendLine();

        sb.AppendFormat("\t" + Localization.Write_buffer_shall_have_a_full_ratio_of_0_before_being_flushed_to_medium,
                        page.WriteBufferFullRatio).
           AppendLine();

        sb.
            AppendFormat("\t" + Localization.Read_buffer_shall_have_an_empty_ratio_of_0_before_more_data_is_read_from_medium,
                         page.ReadBufferEmptyRatio).
            AppendLine();

        sb.
            AppendFormat("\t" + Localization.Drive_will_delay_0_ms_before_buffered_data_is_forcefully_written_to_the_medium_even_before_buffer_is_full,
                         page.WriteDelayTime * 100).
            AppendLine();

        if(page.DBR)
        {
            sb.AppendLine("\t" + Localization.Drive_supports_recovering_data_from_buffer);

            sb.AppendLine(page.RBO
                              ? "\t" + Localization.Recovered_buffer_data_comes_in_LIFO_order
                              : "\t" + Localization.Recovered_buffer_data_comes_in_FIFO_order);
        }

        if(page.BIS)
            sb.AppendLine("\t" + Localization.Medium_supports_block_IDs);

        if(page.RSmk)
            sb.AppendLine("\t" + Localization.Drive_reports_setmarks);

        switch(page.SOCF)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Drive_will_pre_read_until_buffer_is_full);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.Drive_will_pre_read_until_one_filemark_is_detected);

                break;
            case 2:
                sb.AppendLine("\t" + Localization.Drive_will_pre_read_until_two_filemark_is_detected);

                break;
            case 3:
                sb.AppendLine("\t" + Localization.Drive_will_pre_read_until_three_filemark_is_detected);

                break;
        }

        if(page.REW)
        {
            sb.AppendLine("\t" + Localization.Drive_reports_early_warnings);

            if(page.SEW)
                sb.AppendLine("\t" + Localization.Drive_will_synchronize_buffer_to_medium_on_early_warnings);
        }

        switch(page.GapSize)
        {
            case 0:
                break;
            case 1:
                sb.AppendLine("\t" + Localization.Inter_block_gap_is_long_enough_to_support_update_in_place);

                break;
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
            case 8:
            case 9:
            case 10:
            case 11:
            case 12:
            case 13:
            case 14:
            case 15:
                sb.AppendFormat("\t" + Localization.Inter_block_gap_is_0_times_the_device_defined_gap_size,
                                page.GapSize).
                   AppendLine();

                break;
            default:
                sb.AppendFormat("\t" + Localization.Inter_block_gap_is_unknown_value_0, page.GapSize).AppendLine();

                break;
        }

        if(page.EEG)
            sb.AppendLine("\t" + Localization.Drive_generates_end_of_data);

        switch(page.SelectedCompression)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Drive_does_not_use_compression);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.Drive_uses_default_compression);

                break;
            default:
                sb.AppendFormat("\t" + Localization.Drive_uses_unknown_compression_0, page.SelectedCompression).
                   AppendLine();

                break;
        }

        if(page.SWP)
            sb.AppendLine("\t" + Localization.Software_write_protect_is_enabled);

        if(page.ASOCWP)
            sb.AppendLine("\t" + Localization.Associated_write_protect_is_enabled);

        if(page.PERSWP)
            sb.AppendLine("\t" + Localization.Persistent_write_protect_is_enabled);

        if(page.PRMWP)
            sb.AppendLine("\t" + Localization.Permanent_write_protect_is_enabled);

        if(page.BAML)
        {
            sb.AppendLine(page.BAM
                              ? "\t" + Localization.Drive_operates_using_explicit_address_mode
                              : "\t" + Localization.Drive_operates_using_implicit_address_mode);
        }

        switch(page.RewindOnReset)
        {
            case 1:
                sb.AppendLine("\t" + Localization.Drive_shall_position_to_beginning_of_default_data_partition_on_reset);

                break;
            case 2:
                sb.AppendLine("\t" + Localization.Drive_shall_maintain_its_position_on_reset);

                break;
        }

        switch(page.WTRE)
        {
            case 1:
                sb.AppendLine("\t" + Localization.Drive_will_do_nothing_on_WORM_tampered_medium);

                break;
            case 2:
                sb.AppendLine("\t" + Localization.Drive_will_return_CHECK_CONDITION_on_WORM_tampered_medium);

                break;
        }

        if(page.OIR)
            sb.AppendLine("\t" + Localization.Drive_will_only_respond_to_commands_if_it_has_received_a_reservation);

        return sb.ToString();
    }

#endregion Mode Page 0x10: Device configuration page
}