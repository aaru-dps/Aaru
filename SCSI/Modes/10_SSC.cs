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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Aaru.Decoders.SCSI;

[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
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

        sb.AppendLine("SCSI Device configuration page:");

        if(page.PS)
            sb.AppendLine("\tParameters can be saved");

        sb.AppendFormat("\tActive format: {0}", page.ActiveFormat).AppendLine();
        sb.AppendFormat("\tActive partition: {0}", page.ActivePartition).AppendLine();

        sb.AppendFormat("\tWrite buffer shall have a full ratio of {0} before being flushed to medium",
                        page.WriteBufferFullRatio).AppendLine();

        sb.AppendFormat("\tRead buffer shall have an empty ratio of {0} before more data is read from medium",
                        page.ReadBufferEmptyRatio).AppendLine();

        sb.
            AppendFormat("\tDrive will delay {0} ms before buffered data is forcefully written to the medium even before buffer is full",
                         page.WriteDelayTime * 100).AppendLine();

        if(page.DBR)
        {
            sb.AppendLine("\tDrive supports recovering data from buffer");

            sb.AppendLine(page.RBO ? "\tRecovered buffer data comes in LIFO order"
                              : "\tRecovered buffer data comes in FIFO order");
        }

        if(page.BIS)
            sb.AppendLine("\tMedium supports block IDs");

        if(page.RSmk)
            sb.AppendLine("\tDrive reports setmarks");

        switch(page.SOCF)
        {
            case 0:
                sb.AppendLine("\tDrive will pre-read until buffer is full");

                break;
            case 1:
                sb.AppendLine("\tDrive will pre-read until one filemark is detected");

                break;
            case 2:
                sb.AppendLine("\tDrive will pre-read until two filemark is detected");

                break;
            case 3:
                sb.AppendLine("\tDrive will pre-read until three filemark is detected");

                break;
        }

        if(page.REW)
        {
            sb.AppendLine("\tDrive reports early warnings");

            if(page.SEW)
                sb.AppendLine("\tDrive will synchronize buffer to medium on early warnings");
        }

        switch(page.GapSize)
        {
            case 0: break;
            case 1:
                sb.AppendLine("\tInter-block gap is long enough to support update in place");

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
                sb.AppendFormat("\tInter-block gap is {0} times the device's defined gap size", page.GapSize).
                   AppendLine();

                break;
            default:
                sb.AppendFormat("\tInter-block gap is unknown value {0}", page.GapSize).AppendLine();

                break;
        }

        if(page.EEG)
            sb.AppendLine("\tDrive generates end-of-data");

        switch(page.SelectedCompression)
        {
            case 0:
                sb.AppendLine("\tDrive does not use compression");

                break;
            case 1:
                sb.AppendLine("\tDrive uses default compression");

                break;
            default:
                sb.AppendFormat("\tDrive uses unknown compression {0}", page.SelectedCompression).AppendLine();

                break;
        }

        if(page.SWP)
            sb.AppendLine("\tSoftware write protect is enabled");

        if(page.ASOCWP)
            sb.AppendLine("\tAssociated write protect is enabled");

        if(page.PERSWP)
            sb.AppendLine("\tPersistent write protect is enabled");

        if(page.PRMWP)
            sb.AppendLine("\tPermanent write protect is enabled");

        if(page.BAML)
            sb.AppendLine(page.BAM ? "\tDrive operates using explicit address mode"
                              : "\tDrive operates using implicit address mode");

        switch(page.RewindOnReset)
        {
            case 1:
                sb.AppendLine("\tDrive shall position to beginning of default data partition on reset");

                break;
            case 2:
                sb.AppendLine("\tDrive shall maintain its position on reset");

                break;
        }

        switch(page.WTRE)
        {
            case 1:
                sb.AppendLine("\tDrive will do nothing on WORM tampered medium");

                break;
            case 2:
                sb.AppendLine("\tDrive will return CHECK CONDITION on WORM tampered medium");

                break;
        }

        if(page.OIR)
            sb.AppendLine("\tDrive will only respond to commands if it has received a reservation");

        return sb.ToString();
    }
    #endregion Mode Page 0x10: Device configuration page
}