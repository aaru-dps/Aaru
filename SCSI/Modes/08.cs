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

        sb.AppendLine("SCSI Caching mode page:");

        if(page.PS)
            sb.AppendLine("\tParameters can be saved");

        if(page.RCD)
            sb.AppendLine("\tRead-cache is enabled");

        if(page.WCE)
            sb.AppendLine("\tWrite-cache is enabled");

        switch(page.DemandReadRetentionPrio)
        {
            case 0:
                sb.AppendLine("\tDrive does not distinguish between cached read data");

                break;
            case 1:
                sb.AppendLine("\tData put by READ commands should be evicted from cache sooner than data put in read cache by other means");

                break;
            case 0xF:
                sb.AppendLine("\tData put by READ commands should not be evicted if there is data cached by other means that can be evicted");

                break;
            default:
                sb.AppendFormat("\tUnknown demand read retention priority value {0}", page.DemandReadRetentionPrio).
                   AppendLine();

                break;
        }

        switch(page.WriteRetentionPriority)
        {
            case 0:
                sb.AppendLine("\tDrive does not distinguish between cached write data");

                break;
            case 1:
                sb.AppendLine("\tData put by WRITE commands should be evicted from cache sooner than data put in write cache by other means");

                break;
            case 0xF:
                sb.AppendLine("\tData put by WRITE commands should not be evicted if there is data cached by other means that can be evicted");

                break;
            default:
                sb.AppendFormat("\tUnknown demand write retention priority value {0}", page.DemandReadRetentionPrio).
                   AppendLine();

                break;
        }

        if(page.DRA)
            sb.AppendLine("\tRead-ahead is disabled");
        else
        {
            if(page.MF)
                sb.AppendLine("\tPre-fetch values indicate a block multiplier");

            if(page.DisablePreFetch == 0)
                sb.AppendLine("\tNo pre-fetch will be done");
            else
            {
                sb.AppendFormat("\tPre-fetch will be done for READ commands of {0} blocks or less",
                                page.DisablePreFetch).AppendLine();

                if(page.MinimumPreFetch > 0)
                    sb.AppendFormat("At least {0} blocks will be always pre-fetched", page.MinimumPreFetch).
                       AppendLine();

                if(page.MaximumPreFetch > 0)
                    sb.AppendFormat("\tA maximum of {0} blocks will be pre-fetched", page.MaximumPreFetch).AppendLine();

                if(page.MaximumPreFetchCeiling > 0)
                    sb.
                        AppendFormat("\tA maximum of {0} blocks will be pre-fetched even if it is commanded to pre-fetch more",
                                     page.MaximumPreFetchCeiling).AppendLine();

                if(page.IC)
                    sb.AppendLine("\tDevice should use number of cache segments or cache segment size for caching");

                if(page.ABPF)
                    sb.AppendLine("\tPre-fetch should be aborted upon receiving a new command");

                if(page.CAP)
                    sb.AppendLine("\tCaching analysis is permitted");

                if(page.Disc)
                    sb.AppendLine("\tPre-fetch can continue across discontinuities (such as cylinders or tracks)");
            }
        }

        if(page.FSW)
            sb.AppendLine("\tDrive should not reorder the sequence of write commands to be faster");

        if(page.Size)
        {
            if(page.CacheSegmentSize > 0)
                if(page.LBCSS)
                    sb.AppendFormat("\tDrive cache segments should be {0} blocks long", page.CacheSegmentSize).
                       AppendLine();
                else
                    sb.AppendFormat("\tDrive cache segments should be {0} bytes long", page.CacheSegmentSize).
                       AppendLine();
        }
        else
        {
            if(page.CacheSegments > 0)
                sb.AppendFormat("\tDrive should have {0} cache segments", page.CacheSegments).AppendLine();
        }

        if(page.NonCacheSegmentSize > 0)
            sb.AppendFormat("\tDrive shall allocate {0} bytes to buffer even when all cached data cannot be evicted",
                            page.NonCacheSegmentSize).AppendLine();

        if(page.NV_DIS)
            sb.AppendLine("\tNon-Volatile cache is disabled");

        return sb.ToString();
    }
    #endregion Mode Page 0x08: Caching page
}