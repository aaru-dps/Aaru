// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 0D.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MODE PAGE 0Dh: CD-ROM parameteres page.
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
        #region Mode Page 0x0D: CD-ROM parameteres page
        /// <summary>CD-ROM parameteres page Page code 0x0D 8 bytes in SCSI-2, MMC-1, MMC-2, MMC-3</summary>
        public struct ModePage_0D
        {
            /// <summary>Parameters can be saved</summary>
            public bool PS;
            /// <summary>Time the drive shall remain in hold track state after seek or read</summary>
            public byte InactivityTimerMultiplier;
            /// <summary>Seconds per Minute</summary>
            public ushort SecondsPerMinute;
            /// <summary>Frames per Second</summary>
            public ushort FramesPerSecond;
        }

        public static ModePage_0D? DecodeModePage_0D(byte[] pageResponse)
        {
            if((pageResponse?[0] & 0x40) == 0x40)
                return null;

            if((pageResponse?[0] & 0x3F) != 0x0D)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 8)
                return null;

            var decoded = new ModePage_0D();

            decoded.PS                        |= (pageResponse[0]       & 0x80) == 0x80;
            decoded.InactivityTimerMultiplier =  (byte)(pageResponse[3] & 0xF);
            decoded.SecondsPerMinute          =  (ushort)((pageResponse[4] << 8) + pageResponse[5]);
            decoded.FramesPerSecond           =  (ushort)((pageResponse[6] << 8) + pageResponse[7]);

            return decoded;
        }

        public static string PrettifyModePage_0D(byte[] pageResponse) =>
            PrettifyModePage_0D(DecodeModePage_0D(pageResponse));

        public static string PrettifyModePage_0D(ModePage_0D? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_0D page = modePage.Value;
            var         sb   = new StringBuilder();

            sb.AppendLine("SCSI CD-ROM parameters page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            switch(page.InactivityTimerMultiplier)
            {
                case 0:
                    sb.AppendLine("\tDrive will remain in track hold state a vendor-specified time after a seek or read");

                    break;
                case 1:
                    sb.AppendLine("\tDrive will remain in track hold state 125 ms after a seek or read");

                    break;
                case 2:
                    sb.AppendLine("\tDrive will remain in track hold state 250 ms after a seek or read");

                    break;
                case 3:
                    sb.AppendLine("\tDrive will remain in track hold state 500 ms after a seek or read");

                    break;
                case 4:
                    sb.AppendLine("\tDrive will remain in track hold state 1 second after a seek or read");

                    break;
                case 5:
                    sb.AppendLine("\tDrive will remain in track hold state 2 seconds after a seek or read");

                    break;
                case 6:
                    sb.AppendLine("\tDrive will remain in track hold state 4 seconds after a seek or read");

                    break;
                case 7:
                    sb.AppendLine("\tDrive will remain in track hold state 8 seconds after a seek or read");

                    break;
                case 8:
                    sb.AppendLine("\tDrive will remain in track hold state 16 seconds after a seek or read");

                    break;
                case 9:
                    sb.AppendLine("\tDrive will remain in track hold state 32 seconds after a seek or read");

                    break;
                case 10:
                    sb.AppendLine("\tDrive will remain in track hold state 1 minute after a seek or read");

                    break;
                case 11:
                    sb.AppendLine("\tDrive will remain in track hold state 2 minutes after a seek or read");

                    break;
                case 12:
                    sb.AppendLine("\tDrive will remain in track hold state 4 minutes after a seek or read");

                    break;
                case 13:
                    sb.AppendLine("\tDrive will remain in track hold state 8 minutes after a seek or read");

                    break;
                case 14:
                    sb.AppendLine("\tDrive will remain in track hold state 16 minutes after a seek or read");

                    break;
                case 15:
                    sb.AppendLine("\tDrive will remain in track hold state 32 minutes after a seek or read");

                    break;
            }

            if(page.SecondsPerMinute > 0)
                sb.AppendFormat("\tEach minute has {0} seconds", page.SecondsPerMinute).AppendLine();

            if(page.FramesPerSecond > 0)
                sb.AppendFormat("\tEach second has {0} frames", page.FramesPerSecond).AppendLine();

            return sb.ToString();
        }
        #endregion Mode Page 0x0D: CD-ROM parameteres page
    }
}