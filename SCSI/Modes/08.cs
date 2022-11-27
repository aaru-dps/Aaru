// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 08.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MODE PAGE 08h: Caching page.
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

using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Aaru.Decoders.SCSI;

[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static partial class Modes
{
    #region Mode Page 0x08: Caching page
    /// <summary>Disconnect-reconnect page Page code 0x08 12 bytes in SCSI-2 20 bytes in SBC-1, SBC-2, SBC-3</summary>
    public struct ModePage_08
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        /// <summary><c>true</c> if write cache is enabled</summary>
        public bool WCE;
        /// <summary>Multiplication factor</summary>
        public bool MF;
        /// <summary><c>true</c> if read cache is enabled</summary>
        public bool RCD;
        /// <summary>Advices on reading-cache retention priority</summary>
        public byte DemandReadRetentionPrio;
        /// <summary>Advices on writing-cache retention priority</summary>
        public byte WriteRetentionPriority;
        /// <summary>If requested read blocks are more than this, no pre-fetch is done</summary>
        public ushort DisablePreFetch;
        /// <summary>Minimum pre-fetch</summary>
        public ushort MinimumPreFetch;
        /// <summary>Maximum pre-fetch</summary>
        public ushort MaximumPreFetch;
        /// <summary>Upper limit on maximum pre-fetch value</summary>
        public ushort MaximumPreFetchCeiling;

        /// <summary>Manual cache controlling</summary>
        public bool IC;
        /// <summary>Abort pre-fetch</summary>
        public bool ABPF;
        /// <summary>Caching analysis permitted</summary>
        public bool CAP;
        /// <summary>Pre-fetch over discontinuities</summary>
        public bool Disc;
        /// <summary><see cref="CacheSegmentSize" /> is to be used to control caching segmentation</summary>
        public bool Size;
        /// <summary>Force sequential write</summary>
        public bool FSW;
        /// <summary>Logical block cache segment size</summary>
        public bool LBCSS;
        /// <summary>Disable read-ahead</summary>
        public bool DRA;
        /// <summary>How many segments should the cache be divided upon</summary>
        public byte CacheSegments;
        /// <summary>How many bytes should the cache be divided upon</summary>
        public ushort CacheSegmentSize;
        /// <summary>How many bytes should be used as a buffer when all other cached data cannot be evicted</summary>
        public uint NonCacheSegmentSize;

        public bool NV_DIS;
    }

    public static ModePage_08? DecodeModePage_08(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40)
            return null;

        if((pageResponse?[0] & 0x3F) != 0x08)
            return null;

        if(pageResponse[1] + 2 != pageResponse.Length)
            return null;

        if(pageResponse.Length < 12)
            return null;

        var decoded = new ModePage_08();

        decoded.PS  |= (pageResponse[0] & 0x80) == 0x80;
        decoded.WCE |= (pageResponse[2] & 0x04) == 0x04;
        decoded.MF  |= (pageResponse[2] & 0x02) == 0x02;
        decoded.RCD |= (pageResponse[2] & 0x01) == 0x01;

        decoded.DemandReadRetentionPrio = (byte)((pageResponse[3] & 0xF0) >> 4);
        decoded.WriteRetentionPriority  = (byte)(pageResponse[3] & 0x0F);
        decoded.DisablePreFetch         = (ushort)((pageResponse[4]  << 8) + pageResponse[5]);
        decoded.MinimumPreFetch         = (ushort)((pageResponse[6]  << 8) + pageResponse[7]);
        decoded.MaximumPreFetch         = (ushort)((pageResponse[8]  << 8) + pageResponse[9]);
        decoded.MaximumPreFetchCeiling  = (ushort)((pageResponse[10] << 8) + pageResponse[11]);

        if(pageResponse.Length < 20)
            return decoded;

        decoded.IC   |= (pageResponse[2] & 0x80) == 0x80;
        decoded.ABPF |= (pageResponse[2] & 0x40) == 0x40;
        decoded.CAP  |= (pageResponse[2] & 0x20) == 0x20;
        decoded.Disc |= (pageResponse[2] & 0x10) == 0x10;
        decoded.Size |= (pageResponse[2] & 0x08) == 0x08;

        decoded.FSW   |= (pageResponse[12] & 0x80) == 0x80;
        decoded.LBCSS |= (pageResponse[12] & 0x40) == 0x40;
        decoded.DRA   |= (pageResponse[12] & 0x20) == 0x20;

        decoded.CacheSegments       = pageResponse[13];
        decoded.CacheSegmentSize    = (ushort)((pageResponse[14] << 8)  + pageResponse[15]);
        decoded.NonCacheSegmentSize = (uint)((pageResponse[17]   << 16) + (pageResponse[18] << 8) + pageResponse[19]);

        decoded.NV_DIS |= (pageResponse[12] & 0x01) == 0x01;

        return decoded;
    }

    public static string PrettifyModePage_08(byte[] pageResponse) =>
        PrettifyModePage_08(DecodeModePage_08(pageResponse));

    public static string PrettifyModePage_08(ModePage_08? modePage)
    {
        if(!modePage.HasValue)
            return null;

        ModePage_08 page = modePage.Value;
        var         sb   = new StringBuilder();

        sb.AppendLine(Localization.SCSI_Caching_mode_page);

        if(page.PS)
            sb.AppendLine("\t" + Localization.Parameters_can_be_saved);

        if(page.RCD)
            sb.AppendLine("\t" + Localization.Read_cache_is_enabled);

        if(page.WCE)
            sb.AppendLine("\t" + Localization.Write_cache_is_enabled);

        switch(page.DemandReadRetentionPrio)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Drive_does_not_distinguish_between_cached_read_data);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.
                                  Data_put_by_READ_commands_should_be_evicted_from_cache_sooner_than_data_put_in_read_cache_by_other_means);

                break;
            case 0xF:
                sb.AppendLine("\t" + Localization.
                                  Data_put_by_READ_commands_should_not_be_evicted_if_there_is_data_cached_by_other_means_that_can_be_evicted);

                break;
            default:
                sb.AppendFormat("\t" + Localization.Unknown_demand_read_retention_priority_value_0,
                                page.DemandReadRetentionPrio).AppendLine();

                break;
        }

        switch(page.WriteRetentionPriority)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Drive_does_not_distinguish_between_cached_write_data);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.
                                  Data_put_by_WRITE_commands_should_be_evicted_from_cache_sooner_than_data_put_in_write_cache_by_other_means);

                break;
            case 0xF:
                sb.AppendLine("\t" + Localization.
                                  Data_put_by_WRITE_commands_should_not_be_evicted_if_there_is_data_cached_by_other_means_that_can_be_evicted);

                break;
            default:
                sb.AppendFormat("\t" + Localization.Unknown_demand_write_retention_priority_value_0,
                                page.DemandReadRetentionPrio).AppendLine();

                break;
        }

        if(page.DRA)
            sb.AppendLine("\t" + Localization.Read_ahead_is_disabled);
        else
        {
            if(page.MF)
                sb.AppendLine("\t" + Localization.Pre_fetch_values_indicate_a_block_multiplier);

            if(page.DisablePreFetch == 0)
                sb.AppendLine("\t" + Localization.No_pre_fetch_will_be_done);
            else
            {
                sb.AppendFormat("\t" + Localization.Pre_fetch_will_be_done_for_READ_commands_of_0_blocks_or_less,
                                page.DisablePreFetch).AppendLine();

                if(page.MinimumPreFetch > 0)
                    sb.AppendFormat(Localization.At_least_0_blocks_will_be_always_pre_fetched, page.MinimumPreFetch).
                       AppendLine();

                if(page.MaximumPreFetch > 0)
                    sb.AppendFormat("\t" + Localization.A_maximum_of_0_blocks_will_be_pre_fetched,
                                    page.MaximumPreFetch).AppendLine();

                if(page.MaximumPreFetchCeiling > 0)
                    sb.
                        AppendFormat("\t" + Localization.A_maximum_of_0_blocks_will_be_pre_fetched_even_if_it_is_commanded_to_pre_fetch_more,
                                     page.MaximumPreFetchCeiling).AppendLine();

                if(page.IC)
                    sb.AppendLine("\t" + Localization.
                                      Device_should_use_number_of_cache_segments_or_cache_segment_size_for_caching);

                if(page.ABPF)
                    sb.AppendLine("\t" + Localization.Pre_fetch_should_be_aborted_upon_receiving_a_new_command);

                if(page.CAP)
                    sb.AppendLine("\t" + Localization.Caching_analysis_is_permitted);

                if(page.Disc)
                    sb.AppendLine("\t" + Localization.
                                      Pre_fetch_can_continue_across_discontinuities_such_as_cylinders_or_tracks);
            }
        }

        if(page.FSW)
            sb.AppendLine("\t" + Localization.Drive_should_not_reorder_the_sequence_of_write_commands_to_be_faster);

        if(page.Size)
        {
            if(page.CacheSegmentSize > 0)
                if(page.LBCSS)
                    sb.AppendFormat("\t" + Localization.Drive_cache_segments_should_be_0_blocks_long,
                                    page.CacheSegmentSize).AppendLine();
                else
                    sb.AppendFormat("\t" + Localization.Drive_cache_segments_should_be_0_bytes_long,
                                    page.CacheSegmentSize).AppendLine();
        }
        else
        {
            if(page.CacheSegments > 0)
                sb.AppendFormat("\t" + Localization.Drive_should_have_0_cache_segments, page.CacheSegments).
                   AppendLine();
        }

        if(page.NonCacheSegmentSize > 0)
            sb.
                AppendFormat("\t" + Localization.Drive_shall_allocate_0_bytes_to_buffer_even_when_all_cached_data_cannot_be_evicted,
                             page.NonCacheSegmentSize).AppendLine();

        if(page.NV_DIS)
            sb.AppendLine("\t" + Localization.Non_Volatile_cache_is_disabled);

        return sb.ToString();
    }
    #endregion Mode Page 0x08: Caching page
}