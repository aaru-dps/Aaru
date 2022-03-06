// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 0E.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MODE PAGE 0Eh: CD-ROM audio control parameters page.
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
    #region Mode Page 0x0E: CD-ROM audio control parameters page
    /// <summary>CD-ROM audio control parameters Page code 0x0E 16 bytes in SCSI-2, MMC-1, MMC-2, MMC-3</summary>
    public struct ModePage_0E
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        /// <summary>Return status as soon as playback operation starts</summary>
        public bool Immed;
        /// <summary>Stop on track crossing</summary>
        public bool SOTC;
        /// <summary>Indicates <see cref="BlocksPerSecondOfAudio" /> is valid</summary>
        public bool APRVal;
        /// <summary>Multiplier for <see cref="BlocksPerSecondOfAudio" /></summary>
        public byte LBAFormat;
        /// <summary>LBAs per second of audio</summary>
        public ushort BlocksPerSecondOfAudio;
        /// <summary>Channels output on this port</summary>
        public byte OutputPort0ChannelSelection;
        /// <summary>Volume level for this port</summary>
        public byte OutputPort0Volume;
        /// <summary>Channels output on this port</summary>
        public byte OutputPort1ChannelSelection;
        /// <summary>Volume level for this port</summary>
        public byte OutputPort1Volume;
        /// <summary>Channels output on this port</summary>
        public byte OutputPort2ChannelSelection;
        /// <summary>Volume level for this port</summary>
        public byte OutputPort2Volume;
        /// <summary>Channels output on this port</summary>
        public byte OutputPort3ChannelSelection;
        /// <summary>Volume level for this port</summary>
        public byte OutputPort3Volume;
    }

    public static ModePage_0E? DecodeModePage_0E(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40)
            return null;

        if((pageResponse?[0] & 0x3F) != 0x0E)
            return null;

        if(pageResponse[1] + 2 != pageResponse.Length)
            return null;

        if(pageResponse.Length < 16)
            return null;

        var decoded = new ModePage_0E();

        decoded.PS                          |= (pageResponse[0]       & 0x80) == 0x80;
        decoded.Immed                       |= (pageResponse[2]       & 0x04) == 0x04;
        decoded.SOTC                        |= (pageResponse[2]       & 0x02) == 0x02;
        decoded.APRVal                      |= (pageResponse[5]       & 0x80) == 0x80;
        decoded.LBAFormat                   =  (byte)(pageResponse[5] & 0x0F);
        decoded.BlocksPerSecondOfAudio      =  (ushort)((pageResponse[6] << 8) + pageResponse[7]);
        decoded.OutputPort0ChannelSelection =  (byte)(pageResponse[8] & 0x0F);
        decoded.OutputPort0Volume           =  pageResponse[9];
        decoded.OutputPort1ChannelSelection =  (byte)(pageResponse[10] & 0x0F);
        decoded.OutputPort1Volume           =  pageResponse[11];
        decoded.OutputPort2ChannelSelection =  (byte)(pageResponse[12] & 0x0F);
        decoded.OutputPort2Volume           =  pageResponse[13];
        decoded.OutputPort3ChannelSelection =  (byte)(pageResponse[14] & 0x0F);
        decoded.OutputPort3Volume           =  pageResponse[15];

        return decoded;
    }

    public static string PrettifyModePage_0E(byte[] pageResponse) =>
        PrettifyModePage_0E(DecodeModePage_0E(pageResponse));

    public static string PrettifyModePage_0E(ModePage_0E? modePage)
    {
        if(!modePage.HasValue)
            return null;

        ModePage_0E page = modePage.Value;
        var         sb   = new StringBuilder();

        sb.AppendLine("SCSI CD-ROM audio control parameters page:");

        if(page.PS)
            sb.AppendLine("\tParameters can be saved");

        sb.AppendLine(page.Immed ? "\tDrive will return from playback command immediately"
                          : "\tDrive will return from playback command when playback ends");

        if(page.SOTC)
            sb.AppendLine("\tDrive will stop playback on track end");

        if(page.APRVal)
        {
            double blocks;

            if(page.LBAFormat == 8)
                blocks = page.BlocksPerSecondOfAudio * (1 / 256);
            else
                blocks = page.BlocksPerSecondOfAudio;

            sb.AppendFormat("\tThere are {0} blocks per each second of audio", blocks).AppendLine();
        }

        if(page.OutputPort0ChannelSelection > 0)
        {
            sb.Append("\tOutput port 0 has channels ");

            if((page.OutputPort0ChannelSelection & 0x01) == 0x01)
                sb.Append("0 ");

            if((page.OutputPort0ChannelSelection & 0x02) == 0x02)
                sb.Append("1 ");

            if((page.OutputPort0ChannelSelection & 0x04) == 0x04)
                sb.Append("2 ");

            if((page.OutputPort0ChannelSelection & 0x08) == 0x08)
                sb.Append("3 ");

            switch(page.OutputPort0Volume)
            {
                case 0:
                    sb.AppendLine("muted");

                    break;
                case 0xFF:
                    sb.AppendLine("at maximum volume");

                    break;
                default:
                    sb.AppendFormat("at volume {0}", page.OutputPort0Volume).AppendLine();

                    break;
            }
        }

        if(page.OutputPort1ChannelSelection > 0)
        {
            sb.Append("\tOutput port 1 has channels ");

            if((page.OutputPort1ChannelSelection & 0x01) == 0x01)
                sb.Append("0 ");

            if((page.OutputPort1ChannelSelection & 0x02) == 0x02)
                sb.Append("1 ");

            if((page.OutputPort1ChannelSelection & 0x04) == 0x04)
                sb.Append("2 ");

            if((page.OutputPort1ChannelSelection & 0x08) == 0x08)
                sb.Append("3 ");

            switch(page.OutputPort1Volume)
            {
                case 0:
                    sb.AppendLine("muted");

                    break;
                case 0xFF:
                    sb.AppendLine("at maximum volume");

                    break;
                default:
                    sb.AppendFormat("at volume {0}", page.OutputPort1Volume).AppendLine();

                    break;
            }
        }

        if(page.OutputPort2ChannelSelection > 0)
        {
            sb.Append("\tOutput port 2 has channels ");

            if((page.OutputPort2ChannelSelection & 0x01) == 0x01)
                sb.Append("0 ");

            if((page.OutputPort2ChannelSelection & 0x02) == 0x02)
                sb.Append("1 ");

            if((page.OutputPort2ChannelSelection & 0x04) == 0x04)
                sb.Append("2 ");

            if((page.OutputPort2ChannelSelection & 0x08) == 0x08)
                sb.Append("3 ");

            switch(page.OutputPort2Volume)
            {
                case 0:
                    sb.AppendLine("muted");

                    break;
                case 0xFF:
                    sb.AppendLine("at maximum volume");

                    break;
                default:
                    sb.AppendFormat("at volume {0}", page.OutputPort2Volume).AppendLine();

                    break;
            }
        }

        if(page.OutputPort3ChannelSelection <= 0)
            return sb.ToString();

        sb.Append("\tOutput port 3 has channels ");

        if((page.OutputPort3ChannelSelection & 0x01) == 0x01)
            sb.Append("0 ");

        if((page.OutputPort3ChannelSelection & 0x02) == 0x02)
            sb.Append("1 ");

        if((page.OutputPort3ChannelSelection & 0x04) == 0x04)
            sb.Append("2 ");

        if((page.OutputPort3ChannelSelection & 0x08) == 0x08)
            sb.Append("3 ");

        switch(page.OutputPort3Volume)
        {
            case 0:
                sb.AppendLine("muted");

                break;
            case 0xFF:
                sb.AppendLine("at maximum volume");

                break;
            default:
                sb.AppendFormat("at volume {0}", page.OutputPort3Volume).AppendLine();

                break;
        }

        return sb.ToString();
    }
    #endregion Mode Page 0x0E: CD-ROM audio control parameters page
}