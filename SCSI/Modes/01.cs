// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 01.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes and encodes SCSI MODE PAGE 01h: Read-write error recovery page.
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

namespace Aaru.Decoders.SCSI
{
    [SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
     SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static partial class Modes
    {
        public static byte[] EncodeModePage_01(ModePage_01 page)
        {
            byte[] pg = new byte[8];

            pg[0] = 0x01;
            pg[1] = 6;

            if(page.PS)
                pg[0] += 0x80;

            if(page.AWRE)
                pg[2] += 0x80;

            if(page.ARRE)
                pg[2] += 0x40;

            if(page.TB)
                pg[2] += 0x20;

            if(page.RC)
                pg[2] += 0x10;

            if(page.EER)
                pg[2] += 0x08;

            if(page.PER)
                pg[2] += 0x04;

            if(page.DTE)
                pg[2] += 0x02;

            if(page.DCR)
                pg[2] += 0x01;

            pg[3] = page.ReadRetryCount;
            pg[4] = page.CorrectionSpan;
            pg[5] = (byte)page.HeadOffsetCount;
            pg[6] = (byte)page.DataStrobeOffsetCount;

            // This is from a newer version of SCSI unknown what happen for drives expecting an 8 byte page
            /*
            pg[8] = page.WriteRetryCount;
            if (page.LBPERE)
                pg[7] += 0x80;
            pg[10] = (byte)((page.RecoveryTimeLimit & 0xFF00) << 8);
            pg[11] = (byte)(page.RecoveryTimeLimit & 0xFF);*/

            return pg;
        }

        #region Mode Page 0x01: Read-write error recovery page
        /// <summary>Disconnect-reconnect page Page code 0x01 12 bytes in SCSI-2, SBC-1, SBC-2</summary>
        public struct ModePage_01
        {
            /// <summary>Parameters can be saved</summary>
            public bool PS;
            /// <summary>Automatic Write Reallocation Enabled</summary>
            public bool AWRE;
            /// <summary>Automatic Read Reallocation Enabled</summary>
            public bool ARRE;
            /// <summary>Transfer block</summary>
            public bool TB;
            /// <summary>Read continuous</summary>
            public bool RC;
            /// <summary>Enable early recovery</summary>
            public bool EER;
            /// <summary>Post error reporting</summary>
            public bool PER;
            /// <summary>Disable transfer on error</summary>
            public bool DTE;
            /// <summary>Disable correction</summary>
            public bool DCR;
            /// <summary>How many times to retry a read operation</summary>
            public byte ReadRetryCount;
            /// <summary>How many bits of largest data burst error is maximum to apply error correction on it</summary>
            public byte CorrectionSpan;
            /// <summary>Offset to move the heads</summary>
            public sbyte HeadOffsetCount;
            /// <summary>Incremental position to which the recovered data strobe shall be adjusted</summary>
            public sbyte DataStrobeOffsetCount;
            /// <summary>How many times to retry a write operation</summary>
            public byte WriteRetryCount;
            /// <summary>Maximum time in ms to use in data error recovery procedures</summary>
            public ushort RecoveryTimeLimit;

            /// <summary>Logical block provisioning error reporting is enabled</summary>
            public bool LBPERE;
        }

        public static ModePage_01? DecodeModePage_01(byte[] pageResponse)
        {
            if((pageResponse?[0] & 0x40) == 0x40)
                return null;

            if((pageResponse?[0] & 0x3F) != 0x01)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 8)
                return null;

            var decoded = new ModePage_01();

            decoded.PS   |= (pageResponse[0] & 0x80) == 0x80;
            decoded.AWRE |= (pageResponse[2] & 0x80) == 0x80;
            decoded.ARRE |= (pageResponse[2] & 0x40) == 0x40;
            decoded.TB   |= (pageResponse[2] & 0x20) == 0x20;
            decoded.RC   |= (pageResponse[2] & 0x10) == 0x10;
            decoded.EER  |= (pageResponse[2] & 0x08) == 0x08;
            decoded.PER  |= (pageResponse[2] & 0x04) == 0x04;
            decoded.DTE  |= (pageResponse[2] & 0x02) == 0x02;
            decoded.DCR  |= (pageResponse[2] & 0x01) == 0x01;

            decoded.ReadRetryCount        = pageResponse[3];
            decoded.CorrectionSpan        = pageResponse[4];
            decoded.HeadOffsetCount       = (sbyte)pageResponse[5];
            decoded.DataStrobeOffsetCount = (sbyte)pageResponse[6];

            if(pageResponse.Length < 12)
                return decoded;

            decoded.WriteRetryCount   =  pageResponse[8];
            decoded.RecoveryTimeLimit =  (ushort)((pageResponse[10] << 8) + pageResponse[11]);
            decoded.LBPERE            |= (pageResponse[7] & 0x80) == 0x80;

            return decoded;
        }

        public static string PrettifyModePage_01(byte[] pageResponse) =>
            PrettifyModePage_01(DecodeModePage_01(pageResponse));

        public static string PrettifyModePage_01(ModePage_01? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_01 page = modePage.Value;
            var         sb   = new StringBuilder();

            sb.AppendLine("SCSI Read-write error recovery page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            if(page.AWRE)
                sb.AppendLine("\tAutomatic write reallocation is enabled");

            if(page.ARRE)
                sb.AppendLine("\tAutomatic read reallocation is enabled");

            if(page.TB)
                sb.AppendLine("\tData not recovered within limits shall be transferred back before a CHECK CONDITION");

            if(page.RC)
                sb.AppendLine("\tDrive will transfer the entire requested length without delaying to perform error recovery");

            if(page.EER)
                sb.AppendLine("\tDrive will use the most expedient form of error recovery first");

            if(page.PER)
                sb.AppendLine("\tDrive shall report recovered errors");

            if(page.DTE)
                sb.AppendLine("\tTransfer will be terminated upon error detection");

            if(page.DCR)
                sb.AppendLine("\tError correction is disabled");

            if(page.ReadRetryCount > 0)
                sb.AppendFormat("\tDrive will repeat read operations {0} times", page.ReadRetryCount).AppendLine();

            if(page.WriteRetryCount > 0)
                sb.AppendFormat("\tDrive will repeat write operations {0} times", page.WriteRetryCount).AppendLine();

            if(page.RecoveryTimeLimit > 0)
                sb.AppendFormat("\tDrive will employ a maximum of {0} ms to recover data", page.RecoveryTimeLimit).
                   AppendLine();

            if(page.LBPERE)
                sb.AppendLine("Logical block provisioning error reporting is enabled");

            return sb.ToString();
        }
        #endregion Mode Page 0x01: Read-write error recovery page
    }
}