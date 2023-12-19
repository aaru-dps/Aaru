// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 04.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MODE PAGE 04h: Rigid disk drive geometry page.
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
#region Mode Page 0x04: Rigid disk drive geometry page

    /// <summary>Disconnect-reconnect page Page code 0x04 24 bytes in SCSI-2, SBC-1</summary>
    public struct ModePage_04
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        /// <summary>Cylinders used for data storage</summary>
        public uint Cylinders;
        /// <summary>Heads for reading and/or writing</summary>
        public byte Heads;
        /// <summary>Cylinder where write precompensation starts</summary>
        public uint WritePrecompCylinder;
        /// <summary>Cylinder where write current reduction starts</summary>
        public uint WriteReduceCylinder;
        /// <summary>Step rate in 100 ns units</summary>
        public ushort DriveStepRate;
        /// <summary>Cylinder where the heads park</summary>
        public int LandingCylinder;
        /// <summary>Rotational position locking</summary>
        public byte RPL;
        /// <summary>Rotational skew to apply when synchronized</summary>
        public byte RotationalOffset;
        /// <summary>Medium speed in rpm</summary>
        public ushort MediumRotationRate;
    }

    public static ModePage_04? DecodeModePage_04(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40)
            return null;

        if((pageResponse?[0] & 0x3F) != 0x04)
            return null;

        if(pageResponse[1] + 2 != pageResponse.Length)
            return null;

        if(pageResponse.Length < 20)
            return null;

        var decoded = new ModePage_04();

        decoded.PS                   |= (pageResponse[0] & 0x80) == 0x80;
        decoded.Cylinders            =  (uint)((pageResponse[2] << 16) + (pageResponse[3] << 8) + pageResponse[4]);
        decoded.Heads                =  pageResponse[5];
        decoded.WritePrecompCylinder =  (uint)((pageResponse[6] << 16) + (pageResponse[7] << 8) + pageResponse[8]);

        decoded.WriteReduceCylinder = (uint)((pageResponse[9] << 16) + (pageResponse[10] << 8) + pageResponse[11]);

        decoded.DriveStepRate = (ushort)((pageResponse[12] << 8) + pageResponse[13]);

        decoded.LandingCylinder  = (pageResponse[14] << 16) + (pageResponse[15] << 8) + pageResponse[16];
        decoded.RPL              = (byte)(pageResponse[17] & 0x03);
        decoded.RotationalOffset = pageResponse[18];

        if(pageResponse.Length >= 22)
            decoded.MediumRotationRate = (ushort)((pageResponse[20] << 8) + pageResponse[21]);

        return decoded;
    }

    public static string PrettifyModePage_04(byte[] pageResponse) =>
        PrettifyModePage_04(DecodeModePage_04(pageResponse));

    public static string PrettifyModePage_04(ModePage_04? modePage)
    {
        if(!modePage.HasValue)
            return null;

        ModePage_04 page = modePage.Value;
        var         sb   = new StringBuilder();

        sb.AppendLine(Localization.SCSI_Rigid_disk_drive_geometry_page);

        if(page.PS)
            sb.AppendLine("\t" + Localization.Parameters_can_be_saved);

        sb.AppendFormat("\t" + Localization._0_heads,     page.Heads).AppendLine();
        sb.AppendFormat("\t" + Localization._0_cylinders, page.Cylinders).AppendLine();

        if(page.WritePrecompCylinder < page.Cylinders)
        {
            sb.AppendFormat("\t" + Localization.Write_pre_compensation_starts_at_cylinder_0, page.WritePrecompCylinder).
               AppendLine();
        }

        if(page.WriteReduceCylinder < page.Cylinders)
        {
            sb.AppendFormat("\t" + Localization.Write_current_reduction_starts_at_cylinder_0, page.WriteReduceCylinder).
               AppendLine();
        }

        if(page.DriveStepRate > 0)
            sb.AppendFormat("\t" + Localization.Drive_steps_in_0_ns, (uint)page.DriveStepRate * 100).AppendLine();

        sb.AppendFormat("\t" + Localization.Heads_park_in_cylinder_0, page.LandingCylinder).AppendLine();

        if(page.MediumRotationRate > 0)
            sb.AppendFormat("\t" + Localization.Medium_rotates_at_0_rpm, page.MediumRotationRate).AppendLine();

        switch(page.RPL)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Spindle_synchronization_is_disabled_or_unsupported);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.Target_operates_as_a_synchronized_spindle_slave);

                break;
            case 2:
                sb.AppendLine("\t" + Localization.Target_operates_as_a_synchronized_spindle_master);

                break;
            case 3:
                sb.AppendLine("\t" + Localization.Target_operates_as_a_synchronized_spindle_master_control);

                break;
        }

        return sb.ToString();
    }

#endregion Mode Page 0x04: Rigid disk drive geometry page
}