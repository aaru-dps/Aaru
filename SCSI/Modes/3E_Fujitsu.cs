// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 3E_Fujitsu.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes Fujitsu MODE PAGE 3Eh: Verify Control page.
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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.CommonTypes.Structs.Devices.SCSI;

namespace Aaru.Decoders.SCSI;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
public static partial class Modes
{
#region Fujitsu Mode Page 0x3E: Verify Control page

    public enum Fujitsu_VerifyModes : byte
    {
        /// <summary>Always verify after writing</summary>
        Always = 0,
        /// <summary>Never verify after writing</summary>
        Never = 1,
        /// <summary>Verify after writing depending on condition</summary>
        Depends = 2,
        Reserved = 4
    }

    public struct Fujitsu_ModePage_3E
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        /// <summary>If set, AV data support mode is applied</summary>
        public bool audioVisualMode;
        /// <summary>If set the test write operation is restricted</summary>
        public bool streamingMode;
        public byte Reserved1;
        /// <summary>Verify mode for WRITE commands</summary>
        public Fujitsu_VerifyModes verifyMode;
        public byte Reserved2;
        /// <summary>Device type provided in response to INQUIRY</summary>
        public PeripheralDeviceTypes devType;
        public byte[] Reserved3;
    }

    public static Fujitsu_ModePage_3E? DecodeFujitsuModePage_3E(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40)
            return null;

        if((pageResponse?[0] & 0x3F) != 0x3E)
            return null;

        if(pageResponse[1] + 2 != pageResponse.Length)
            return null;

        if(pageResponse.Length != 8)
            return null;

        var decoded = new Fujitsu_ModePage_3E();

        decoded.PS |= (pageResponse[0] & 0x80) == 0x80;

        decoded.audioVisualMode |= (pageResponse[2] & 0x80) == 0x80;
        decoded.streamingMode   |= (pageResponse[2] & 0x40) == 0x40;
        decoded.Reserved1       =  (byte)((pageResponse[2] & 0x3C) >> 2);
        decoded.verifyMode      =  (Fujitsu_VerifyModes)(pageResponse[2] & 0x03);

        decoded.Reserved2 = (byte)((pageResponse[3] & 0xE0) >> 5);
        decoded.devType   = (PeripheralDeviceTypes)(pageResponse[3] & 0x1F);

        decoded.Reserved3 = new byte[4];
        Array.Copy(pageResponse, 4, decoded.Reserved3, 0, 4);

        return decoded;
    }

    public static string PrettifyFujitsuModePage_3E(byte[] pageResponse) =>
        PrettifyFujitsuModePage_3E(DecodeFujitsuModePage_3E(pageResponse));

    public static string PrettifyFujitsuModePage_3E(Fujitsu_ModePage_3E? modePage)
    {
        if(!modePage.HasValue)
            return null;

        Fujitsu_ModePage_3E page = modePage.Value;
        var                 sb   = new StringBuilder();

        sb.AppendLine(Localization.Fujitsu_Verify_Control_Page);

        if(page.PS)
            sb.AppendLine("\t" + Localization.Parameters_can_be_saved);

        if(page.audioVisualMode)
            sb.AppendLine("\t" + Localization.Audio_Visual_data_support_mode_is_applied);

        if(page.streamingMode)
            sb.AppendLine("\t" + Localization.Test_write_operation_is_restricted_during_read_or_write_operations);

        switch(page.verifyMode)
        {
            case Fujitsu_VerifyModes.Always:
                sb.AppendLine("\t" + Localization.Always_apply_the_verify_operation);

                break;
            case Fujitsu_VerifyModes.Never:
                sb.AppendLine("\t" + Localization.Never_apply_the_verify_operation);

                break;
            case Fujitsu_VerifyModes.Depends:
                sb.AppendLine("\t" + Localization.Apply_the_verify_operation_depending_on_the_condition);

                break;
        }

        sb.AppendFormat("\t" + Localization.The_device_type_that_would_be_provided_in_the_INQUIRY_response_is_0,
                        page.devType).AppendLine();

        return sb.ToString();
    }

#endregion Fujitsu Mode Page 0x3E: Verify Control page
}