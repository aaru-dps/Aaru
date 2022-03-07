// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 2F_IBM.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes IBM MODE PAGE 2Fh: Behaviour Configuration Mode page.
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

namespace Aaru.Decoders.SCSI;

using System.Diagnostics.CodeAnalysis;
using System.Text;

[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), SuppressMessage("ReSharper", "UnassignedField.Global")]
public static partial class Modes
{
    #region IBM Mode Page 0x2F: Behaviour Configuration Mode page
    public struct IBM_ModePage_2F
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        public byte FenceBehaviour;
        public byte CleanBehaviour;
        public byte WORMEmulation;
        public byte SenseDataBehaviour;
        public bool CCDM;
        public bool DDEOR;
        public bool CLNCHK;
        public byte FirmwareUpdateBehaviour;
        public byte UOE_D;
        public byte UOE_F;
        public byte UOE_C;
    }

    public static IBM_ModePage_2F? DecodeIBMModePage_2F(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40)
            return null;

        if((pageResponse?[0] & 0x3F) != 0x2F)
            return null;

        if(pageResponse[1] + 2 != pageResponse.Length)
            return null;

        if(pageResponse.Length < 8)
            return null;

        return new IBM_ModePage_2F
        {
            PS                      = (pageResponse[0] & 0x80) == 0x80,
            FenceBehaviour          = pageResponse[2],
            CleanBehaviour          = pageResponse[3],
            WORMEmulation           = pageResponse[4],
            SenseDataBehaviour      = pageResponse[5],
            CCDM                    = (pageResponse[6] & 0x04) == 0x04,
            DDEOR                   = (pageResponse[6] & 0x02) == 0x02,
            CLNCHK                  = (pageResponse[6] & 0x01) == 0x01,
            FirmwareUpdateBehaviour = pageResponse[7],
            UOE_C                   = (byte)((pageResponse[8] & 0x30) >> 4),
            UOE_F                   = (byte)((pageResponse[8] & 0x0C) >> 2)
        };
    }

    public static string PrettifyIBMModePage_2F(byte[] pageResponse) =>
        PrettifyIBMModePage_2F(DecodeIBMModePage_2F(pageResponse));

    public static string PrettifyIBMModePage_2F(IBM_ModePage_2F? modePage)
    {
        if(!modePage.HasValue)
            return null;

        IBM_ModePage_2F page = modePage.Value;
        var             sb   = new StringBuilder();

        sb.AppendLine("IBM Behaviour Configuration Mode Page:");

        if(page.PS)
            sb.AppendLine("\tParameters can be saved");

        switch(page.FenceBehaviour)
        {
            case 0:
                sb.AppendLine("\tFence behaviour is normal");

                break;
            case 1:
                sb.AppendLine("\tPanic fence behaviour is enabled");

                break;
            default:
                sb.AppendFormat("\tUnknown fence behaviour code {0}", page.FenceBehaviour).AppendLine();

                break;
        }

        switch(page.CleanBehaviour)
        {
            case 0:
                sb.AppendLine("\tCleaning behaviour is normal");

                break;
            case 1:
                sb.AppendLine("\tDrive will periodically request cleaning");

                break;
            default:
                sb.AppendFormat("\tUnknown cleaning behaviour code {0}", page.CleanBehaviour).AppendLine();

                break;
        }

        switch(page.WORMEmulation)
        {
            case 0:
                sb.AppendLine("\tWORM emulation is disabled");

                break;
            case 1:
                sb.AppendLine("\tWORM emulation is enabled");

                break;
            default:
                sb.AppendFormat("\tUnknown WORM emulation code {0}", page.WORMEmulation).AppendLine();

                break;
        }

        switch(page.SenseDataBehaviour)
        {
            case 0:
                sb.AppendLine("\tUses 35-bytes sense data");

                break;
            case 1:
                sb.AppendLine("\tUses 96-bytes sense data");

                break;
            default:
                sb.AppendFormat("\tUnknown sense data behaviour code {0}", page.WORMEmulation).AppendLine();

                break;
        }

        if(page.CLNCHK)
            sb.AppendLine("\tDrive will set Check Condition when cleaning is needed");

        if(page.DDEOR)
            sb.AppendLine("\tNo deferred error will be reported to a rewind command");

        if(page.CCDM)
            sb.AppendLine("\tDrive will set Check Condition when the criteria for Dead Media is met");

        if(page.FirmwareUpdateBehaviour > 0)
            sb.AppendLine("\tDrive will not accept downlevel firmware via an FMR tape");

        if(page.UOE_C == 1)
            sb.AppendLine("\tDrive will eject cleaning cartridges on error");

        if(page.UOE_F == 1)
            sb.AppendLine("\tDrive will eject firmware cartridges on error");

        if(page.UOE_D == 1)
            sb.AppendLine("\tDrive will eject data cartridges on error");

        return sb.ToString();
    }
    #endregion IBM Mode Page 0x2F: Behaviour Configuration Mode page
}