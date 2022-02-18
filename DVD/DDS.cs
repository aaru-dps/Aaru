// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DDS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes DVD Disc Definition Structure.
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
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Aaru.Decoders.DVD
{
    // Information from the following standards:
    // ANSI X3.304-1997
    // T10/1048-D revision 9.0
    // T10/1048-D revision 10a
    // T10/1228-D revision 7.0c
    // T10/1228-D revision 11a
    // T10/1363-D revision 10g
    // T10/1545-D revision 1d
    // T10/1545-D revision 5
    // T10/1545-D revision 5a
    // T10/1675-D revision 2c
    // T10/1675-D revision 4
    // T10/1836-D revision 2g
    // ECMA 272: 120 mm DVD Rewritable Disk (DVD-RAM)
    // ECMA 330: 120 mm (4,7 Gbytes per side) and 80 mm (1,46 Gbytes per side) DVD Rewritable Disk (DVD-RAM)
    [SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
     SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), SuppressMessage("ReSharper", "NotAccessedField.Global")]
    public static class DDS
    {
        public static DiscDefinitionStructure? Decode(byte[] response)
        {
            if(response?.Length != 2052)
                return null;

            var dds = new DiscDefinitionStructure
            {
                Identifier = (ushort)((response[4] << 8) + response[5])
            };

            if(dds.Identifier != 0x0A0A)
                return null;

            // Common to both DVD-RAM versions
            dds.DataLength                =  (ushort)((response[0] << 8) + response[1]);
            dds.Reserved1                 =  response[2];
            dds.Reserved2                 =  response[3];
            dds.Reserved3                 =  response[6];
            dds.InProcess                 |= (response[7] & 0x80) == 0x80;
            dds.UserCertification         |= (response[7] & 0x02) == 0x02;
            dds.ManufacturerCertification |= (response[7] & 0x01) == 0x01;

            dds.UpdateCount = (uint)((response[8] << 24) + (response[9] << 16) + (response[10] << 8) + response[11]);

            dds.Groups = (ushort)((response[12] << 8) + response[13]);

            // ECMA-272
            if(dds.Groups == 24)
            {
                dds.PartialCertification |= (response[7] & 0x40) == 0x40;
                dds.FormattingOnlyAGroup |= (response[7] & 0x20) == 0x20;
                dds.Reserved4            =  (byte)((response[7] & 0x1C) >> 2);
                dds.Reserved             =  new byte[6];
                Array.Copy(response, 14, dds.Reserved, 0, 6);
                dds.GroupCertificationFlags = new GroupCertificationFlag[24];

                for(int i = 0; i < 24; i++)
                {
                    dds.GroupCertificationFlags[i].InProcess            |= (response[20 + i] & 0x80) == 0x80;
                    dds.GroupCertificationFlags[i].PartialCertification |= (response[20 + i] & 0x40) == 0x40;
                    dds.GroupCertificationFlags[i].Reserved1            =  (byte)((response[20 + i] & 0x3C) >> 2);
                    dds.GroupCertificationFlags[i].UserCertification    |= (response[20 + i] & 0x02) == 0x02;
                    dds.GroupCertificationFlags[i].Reserved2            |= (response[20 + i] & 0x01) == 0x01;
                }
            }

            // ECMA-330
            if(dds.Groups != 1)
                return dds;

            {
                dds.Reserved4 = (byte)((response[7] & 0x7C) >> 2);
                dds.Reserved  = new byte[68];
                Array.Copy(response, 16, dds.Reserved, 0, 68);
                dds.Zones             = (ushort)((response[14] << 8)  + response[15]);
                dds.SpareAreaFirstPSN = (uint)((response[85]   << 16) + (response[86] << 8) + response[87]);
                dds.SpareAreaLastPSN  = (uint)((response[89]   << 16) + (response[90] << 8) + response[91]);
                dds.LSN0Location      = (uint)((response[93]   << 16) + (response[94] << 8) + response[95]);
                dds.StartLSNForZone   = new uint[dds.Zones];

                for(int i = 0; i < dds.Zones; i++)
                    dds.StartLSNForZone[i] = (uint)((response[260 + (i * 4) + 1] << 16) +
                                                    (response[260 + (i * 4) + 2] << 8)  + response[260 + (i * 4) + 3]);
            }

            return dds;
        }

        public static string Prettify(DiscDefinitionStructure? dds)
        {
            if(dds == null)
                return null;

            DiscDefinitionStructure decoded = dds.Value;
            var                     sb      = new StringBuilder();

            if(decoded.InProcess)
            {
                sb.AppendLine("Formatting in progress.");

                if(decoded.Groups == 24)
                {
                    if(decoded.PartialCertification)
                        sb.AppendLine("Formatting is only using partial certification");

                    if(decoded.FormattingOnlyAGroup)
                        sb.AppendLine("Only a group is being formatted");
                }
            }

            if(decoded.UserCertification)
                sb.AppendLine("Disc has been certified by an user");

            if(decoded.ManufacturerCertification)
                sb.AppendLine("Disc has been certified by a manufacturer");

            sb.AppendFormat("DDS has been updated {0} times", decoded.UpdateCount).AppendLine();

            if(decoded.Groups == 24)
                for(int i = 0; i < decoded.GroupCertificationFlags.Length; i++)
                {
                    if(decoded.GroupCertificationFlags[i].InProcess)
                    {
                        sb.AppendFormat("Group {0} is being formatted", i).AppendLine();

                        if(decoded.GroupCertificationFlags[i].PartialCertification)
                            sb.AppendFormat("Group {0} is being certified partially", i).AppendLine();
                    }

                    if(decoded.GroupCertificationFlags[i].UserCertification)
                        sb.AppendFormat("Group {0} has been certified by an user", i).AppendLine();
                }

            if(decoded.Groups != 1)
                return sb.ToString();

            {
                sb.AppendFormat("Disc has {0} zones", decoded.Zones).AppendLine();

                sb.AppendFormat("Primary Spare Area stats at PSN {0:X}h and ends at PSN {1:X}h, inclusively",
                                decoded.SpareAreaFirstPSN, decoded.SpareAreaLastPSN).AppendLine();

                sb.AppendFormat("LSN 0 is at PSN {0:X}h", decoded.LSN0Location).AppendLine();

                for(int i = 0; i < decoded.StartLSNForZone.Length; i++)
                    sb.AppendFormat("Zone {0} starts at LSN {1}", i, decoded.StartLSNForZone[i]).AppendLine();
            }

            return sb.ToString();
        }

        public static string Prettify(byte[] response) => Prettify(Decode(response));

        public struct DiscDefinitionStructure
        {
            /// <summary>Bytes 0 to 1 Data length</summary>
            public ushort DataLength;
            /// <summary>Byte 2 Reserved</summary>
            public byte Reserved1;
            /// <summary>Byte 3 Reserved</summary>
            public byte Reserved2;

            /// <summary>Bytes 4 to 5 DDS Identifier = 0x0A0A</summary>
            public ushort Identifier;
            /// <summary>Byte 6 Reserved</summary>
            public byte Reserved3;
            /// <summary>Byte 7, bit 7 If set, formatting is in process</summary>
            public bool InProcess;
            /// <summary>Byte 7, bit 6 If set, formatting is using partial certification Only in ECMA-272</summary>
            public bool PartialCertification;
            /// <summary>Byte 7, bit 5 If set, only a group is being formatted Only in ECMA-272</summary>
            public bool FormattingOnlyAGroup;
            /// <summary>Byte 7, bits 4 to 2 Reserved</summary>
            public byte Reserved4;
            /// <summary>Byte 7, bit 1 If set, disk has been certified by a user</summary>
            public bool UserCertification;
            /// <summary>Byte 7, bit 0 If set, disk has been certified by a manufacturer</summary>
            public bool ManufacturerCertification;
            /// <summary>Bytes 8 to 11 How many times the DDS has been updated</summary>
            public uint UpdateCount;
            /// <summary>Bytes 12 to 13 How many groups the disk has 24 for ECMA-272 1 for ECMA-330</summary>
            public ushort Groups;
            /// <summary>Bytes 14 to 15 How many zones the disk has Only in ECMA-330</summary>
            public ushort Zones;
            /// <summary>Bytes 14 to 19 in ECMA-272 Bytes 16 to 83 in ECMA-330 Reserved</summary>
            public byte[] Reserved;
            /// <summary>Bytes 20 to 43 Group certification flags</summary>
            public GroupCertificationFlag[] GroupCertificationFlags;

            /// <summary>Bytes 85 to 87 Location of first sector in the Primary Spare Area</summary>
            public uint SpareAreaFirstPSN;
            /// <summary>Bytes 89 to 91 Location of first sector in the Primary Spare Area</summary>
            public uint SpareAreaLastPSN;
            /// <summary>Bytes 93 to 95 PSN for LSN 0</summary>
            public uint LSN0Location;
            /// <summary>The starting LSN of each zone</summary>
            public uint[] StartLSNForZone;
        }

        public struct GroupCertificationFlag
        {
            /// <summary>Bit 7 If set, formatting of this group is in process</summary>
            public bool InProcess;
            /// <summary>Bit 6 If set, formatting is using partial certification</summary>
            public bool PartialCertification;
            /// <summary>Bits 5 to 2 Reserved</summary>
            public byte Reserved1;
            /// <summary>Bit 1 If set, this group has been certified by user</summary>
            public bool UserCertification;
            /// <summary>Bit 0 Reserved</summary>
            public bool Reserved2;
        }
    }
}