// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 24_IBM.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes IBM MODE PAGE 24h: Drive Capabilities Control Mode page.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Aaru.Decoders.SCSI;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static partial class Modes
{
#region IBM Mode Page 0x24: Drive Capabilities Control Mode page

    public struct IBM_ModePage_24
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        public byte ModeControl;
        public byte VelocitySetting;
        public bool EncryptionEnabled;
        public bool EncryptionCapable;
    }

    public static IBM_ModePage_24? DecodeIBMModePage_24(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40) return null;

        if((pageResponse?[0] & 0x3F) != 0x24) return null;

        if(pageResponse[1] + 2 != pageResponse.Length) return null;

        if(pageResponse.Length != 8) return null;

        var decoded = new IBM_ModePage_24();

        decoded.PS                |= (pageResponse[0] & 0x80) == 0x80;
        decoded.ModeControl       =  pageResponse[2];
        decoded.VelocitySetting   =  pageResponse[3];
        decoded.EncryptionEnabled |= (pageResponse[7] & 0x08) == 0x08;
        decoded.EncryptionCapable |= (pageResponse[7] & 0x01) == 0x01;

        return decoded;
    }

    public static string PrettifyIBMModePage_24(byte[] pageResponse) =>
        PrettifyIBMModePage_24(DecodeIBMModePage_24(pageResponse));

    public static string PrettifyIBMModePage_24(IBM_ModePage_24? modePage)
    {
        if(!modePage.HasValue) return null;

        IBM_ModePage_24 page = modePage.Value;
        var             sb   = new StringBuilder();

        sb.AppendLine(Localization.IBM_Vendor_Specific_Control_Mode_Page);

        if(page.PS) sb.AppendLine("\t" + Localization.Parameters_can_be_saved);

        sb.AppendFormat("\t" + Localization.Vendor_specific_mode_control_0,     page.ModeControl);
        sb.AppendFormat("\t" + Localization.Vendor_specific_velocity_setting_0, page.VelocitySetting);

        if(!page.EncryptionCapable) return sb.ToString();

        sb.AppendLine("\t" + Localization.Drive_supports_encryption);

        if(page.EncryptionEnabled) sb.AppendLine("\t" + Localization.Drive_has_encryption_enabled);

        return sb.ToString();
    }

#endregion IBM Mode Page 0x24: Drive Capabilities Control Mode page
}