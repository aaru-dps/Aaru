// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SCR.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SecureDigital SCR.
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

namespace Aaru.Decoders.SecureDigital;

[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), SuppressMessage("ReSharper", "NotAccessedField.Global")]
public class SCR
{
    public BusWidth       BusWidth;
    public CommandSupport CommandSupport;
    public bool           DataStatusAfterErase;
    public byte           ExtendedSecurity;
    public byte[]         ManufacturerReserved;
    public byte           Security;
    public byte           Spec;
    public bool           Spec3;
    public bool           Spec4;
    public byte           SpecX;
    public byte           Structure;
}

[Flags]
public enum BusWidth : byte
{
    OneBit = 1 << 0, FourBit = 1 << 2
}

[Flags]
public enum CommandSupport : byte
{
    SpeedClassControl           = 1 << 0, SetBlockCount = 1 << 1, ExtensionRegisterSingleBlock = 1 << 2,
    ExtensionRegisterMultiBlock = 1 << 3
}

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), SuppressMessage("ReSharper", "InconsistentNaming")]
public static partial class Decoders
{
    public static SCR DecodeSCR(uint[] response)
    {
        if(response?.Length != 2)
            return null;

        byte[] data = new byte[8];

        byte[] tmp = BitConverter.GetBytes(response[0]);
        Array.Copy(tmp, 0, data, 0, 4);
        tmp = BitConverter.GetBytes(response[1]);
        Array.Copy(tmp, 0, data, 4, 4);

        return DecodeSCR(data);
    }

    public static SCR DecodeSCR(byte[] response)
    {
        if(response?.Length != 8)
            return null;

        var scr = new SCR
        {
            Structure            = (byte)((response[0] & 0xF0) >> 4),
            Spec                 = (byte)(response[0] & 0x0F),
            DataStatusAfterErase = (response[1]       & 0x80) == 0x80,
            Security             = (byte)((response[1] & 0x70) >> 4),
            BusWidth             = (BusWidth)(response[1] & 0x0F),
            Spec3                = (response[2]           & 0x80) == 0x80,
            ExtendedSecurity     = (byte)((response[2] & 0x78) >> 3),
            Spec4                = (response[2] & 0x04) == 0x04,
            SpecX                = (byte)(((response[2] & 0x03) << 2) + ((response[3] & 0xC0) >> 6)),
            CommandSupport       = (CommandSupport)(response[3] & 0x0F),
            ManufacturerReserved = new byte[4]
        };

        Array.Copy(response, 4, scr.ManufacturerReserved, 0, 4);

        return scr;
    }

    public static string PrettifySCR(SCR scr)
    {
        if(scr == null)
            return null;

        var sb = new StringBuilder();
        sb.AppendLine(Localization.SecureDigital_Device_Configuration_Register);

        if(scr.Structure != 0)
            sb.AppendFormat("\t" + Localization.Unknown_register_version_0, scr.Structure).AppendLine();

        switch(scr.Spec)
        {
            case 0 when scr.Spec3 == false && scr.Spec4 == false && scr.SpecX == 0:
                sb.AppendLine("\t" + Localization.
                                  Device_follows_SecureDigital_Physical_Layer_Specification_version_1_0x);

                break;
            case 1 when scr.Spec3 == false && scr.Spec4 == false && scr.SpecX == 0:
                sb.AppendLine("\t" + Localization.
                                  Device_follows_SecureDigital_Physical_Layer_Specification_version_1_10);

                break;
            case 2 when scr.Spec3 == false && scr.Spec4 == false && scr.SpecX == 0:
                sb.AppendLine("\t" + Localization.
                                  Device_follows_SecureDigital_Physical_Layer_Specification_version_2_00);

                break;
            case 2 when scr.Spec3 && scr.Spec4 == false && scr.SpecX == 0:
                sb.AppendLine("\t" + Localization.
                                  Device_follows_SecureDigital_Physical_Layer_Specification_version_3_0x);

                break;
            case 2 when scr.Spec3 && scr.Spec4 && scr.SpecX == 0:
                sb.AppendLine("\t" + Localization.
                                  Device_follows_SecureDigital_Physical_Layer_Specification_version_4_xx);

                break;
            case 2 when scr.Spec3:
                switch(scr.SpecX)
                {
                    case 1:
                        sb.AppendLine("\t" + Localization.
                                          Device_follows_SecureDigital_Physical_Layer_Specification_version_5_xx);

                        break;
                    case 2:
                        sb.AppendLine("\t" + Localization.
                                          Device_follows_SecureDigital_Physical_Layer_Specification_version_6_xx);

                        break;
                    case 3:
                        sb.AppendLine("\t" + Localization.
                                          Device_follows_SecureDigital_Physical_Layer_Specification_version_7_xx);

                        break;
                    case 4:
                        sb.AppendLine("\t" + Localization.
                                          Device_follows_SecureDigital_Physical_Layer_Specification_version_8_xx);

                        break;
                }

                break;
            default:
                sb.
                    AppendFormat("\t" + Localization.Device_follows_SecureDigital_Physical_Layer_Specification_with_unknown_version_0_1_2_3,
                                 scr.Spec, scr.Spec3, scr.Spec4, scr.SpecX).AppendLine();

                break;
        }

        switch(scr.Security)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Device_does_not_support_CPRM);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.Device_does_not_use_CPRM);

                break;
            case 2:
                sb.AppendLine("\t" + Localization.Device_uses_CPRM_according_to_specification_version_1_01);

                break;
            case 3:
                sb.AppendLine("\t" + Localization.Device_uses_CPRM_according_to_specification_version_2_00);

                break;
            case 4:
                sb.AppendLine("\t" + Localization.Device_uses_CPRM_according_to_specification_version_3_xx);

                break;
            default:
                sb.AppendFormat("\t" + Localization.Device_uses_unknown_CPRM_specification_with_code_0, scr.Security).
                   AppendLine();

                break;
        }

        if(scr.BusWidth.HasFlag(BusWidth.OneBit))
            sb.AppendLine("\t" + Localization.Device_supports_1_bit_data_bus);

        if(scr.BusWidth.HasFlag(BusWidth.FourBit))
            sb.AppendLine("\t" + Localization.Device_supports_4_bit_data_bus);

        if(scr.ExtendedSecurity != 0)
            sb.AppendLine("\t" + Localization.Device_supports_extended_security);

        if(scr.CommandSupport.HasFlag(CommandSupport.ExtensionRegisterMultiBlock))
            sb.AppendLine("\t" + Localization.Device_supports_extension_register_multi_block_commands);

        if(scr.CommandSupport.HasFlag(CommandSupport.ExtensionRegisterSingleBlock))
            sb.AppendLine("\t" + Localization.Device_supports_extension_register_single_block_commands);

        if(scr.CommandSupport.HasFlag(CommandSupport.SetBlockCount))
            sb.AppendLine("\t" + Localization.Device_supports_set_block_count_command);

        if(scr.CommandSupport.HasFlag(CommandSupport.SpeedClassControl))
            sb.AppendLine("\t" + Localization.Device_supports_speed_class_control_command);

        return sb.ToString();
    }

    public static string PrettifySCR(uint[] response) => PrettifySCR(DecodeSCR(response));

    public static string PrettifySCR(byte[] response) => PrettifySCR(DecodeSCR(response));
}