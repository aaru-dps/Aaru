// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : PrintScsiModePages.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Prints decoded information of SCSI MODE pages to the console.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Console;
using Aaru.Decoders.SCSI;
using Aaru.Helpers;

namespace Aaru.Core;

/// <summary>Prints all SCSI MODE pages</summary>
public static class PrintScsiModePages
{
    /// <summary>Prints all SCSI MODE pages</summary>
    /// <param name="decMode">Decoded SCSI MODE SENSE</param>
    /// <param name="devType">SCSI Peripheral Type</param>
    /// <param name="vendorId">SCSI vendor identification</param>
    public static void Print(Modes.DecodedMode decMode, PeripheralDeviceTypes devType, byte[] vendorId)
    {
        AaruConsole.WriteLine(Modes.PrettifyModeHeader(decMode.Header, devType));

        if(decMode.Pages == null)
            return;

        foreach(Modes.ModePage page in decMode.Pages)

            //AaruConsole.WriteLine("Page {0:X2}h subpage {1:X2}h is {2} bytes long", page.Page, page.Subpage, page.PageResponse.Length);
            switch(page.Page)
            {
                case 0x00:
                {
                    if(devType      == PeripheralDeviceTypes.MultiMediaDevice &&
                       page.Subpage == 0)
                        AaruConsole.WriteLine(Modes.PrettifyModePage_00_SFF(page.PageResponse));
                    else
                    {
                        if(page.Subpage != 0)
                            AaruConsole.WriteLine("Found unknown vendor mode page {0:X2}h subpage {1:X2}h",
                                                  page.Page, page.Subpage);
                        else
                            AaruConsole.WriteLine("Found unknown vendor mode page {0:X2}h", page.Page);
                    }

                    break;
                }
                case 0x01:
                {
                    if(page.Subpage == 0)
                        AaruConsole.WriteLine(devType == PeripheralDeviceTypes.MultiMediaDevice
                                                  ? Modes.PrettifyModePage_01_MMC(page.PageResponse)
                                                  : Modes.PrettifyModePage_01(page.PageResponse));
                    else
                        goto default;

                    break;
                }
                case 0x02:
                {
                    if(page.Subpage == 0)
                        AaruConsole.WriteLine(Modes.PrettifyModePage_02(page.PageResponse));
                    else
                        goto default;

                    break;
                }
                case 0x03:
                {
                    if(page.Subpage == 0)
                        AaruConsole.WriteLine(Modes.PrettifyModePage_03(page.PageResponse));
                    else
                        goto default;

                    break;
                }
                case 0x04:
                {
                    if(page.Subpage == 0)
                        AaruConsole.WriteLine(Modes.PrettifyModePage_04(page.PageResponse));
                    else
                        goto default;

                    break;
                }
                case 0x05:
                {
                    if(page.Subpage == 0)
                        AaruConsole.WriteLine(Modes.PrettifyModePage_05(page.PageResponse));
                    else
                        goto default;

                    break;
                }
                case 0x06:
                {
                    if(page.Subpage == 0)
                        AaruConsole.WriteLine(Modes.PrettifyModePage_06(page.PageResponse));
                    else
                        goto default;

                    break;
                }
                case 0x07:
                {
                    if(page.Subpage == 0)
                        AaruConsole.WriteLine(devType == PeripheralDeviceTypes.MultiMediaDevice
                                                  ? Modes.PrettifyModePage_07_MMC(page.PageResponse)
                                                  : Modes.PrettifyModePage_07(page.PageResponse));
                    else
                        goto default;

                    break;
                }
                case 0x08:
                {
                    if(page.Subpage == 0)
                        AaruConsole.WriteLine(Modes.PrettifyModePage_08(page.PageResponse));
                    else
                        goto default;

                    break;
                }
                case 0x0A:
                {
                    if(page.Subpage == 0)
                        AaruConsole.WriteLine(Modes.PrettifyModePage_0A(page.PageResponse));
                    else if(page.Subpage == 1)
                        AaruConsole.WriteLine(Modes.PrettifyModePage_0A_S01(page.PageResponse));
                    else
                        goto default;

                    break;
                }
                case 0x0B:
                {
                    if(page.Subpage == 0)
                        AaruConsole.WriteLine(Modes.PrettifyModePage_0B(page.PageResponse));
                    else
                        goto default;

                    break;
                }
                case 0x0D:
                {
                    if(page.Subpage == 0)
                        AaruConsole.WriteLine(Modes.PrettifyModePage_0D(page.PageResponse));
                    else
                        goto default;

                    break;
                }
                case 0x0E:
                {
                    if(page.Subpage == 0)
                        AaruConsole.WriteLine(Modes.PrettifyModePage_0E(page.PageResponse));
                    else
                        goto default;

                    break;
                }
                case 0x0F:
                {
                    if(page.Subpage == 0)
                        AaruConsole.WriteLine(Modes.PrettifyModePage_0F(page.PageResponse));
                    else
                        goto default;

                    break;
                }
                case 0x10:
                {
                    if(page.Subpage == 0)
                        AaruConsole.WriteLine(devType == PeripheralDeviceTypes.SequentialAccess
                                                  ? Modes.PrettifyModePage_10_SSC(page.PageResponse)
                                                  : Modes.PrettifyModePage_10(page.PageResponse));
                    else
                        goto default;

                    break;
                }
                case 0x11:
                {
                    if(page.Subpage == 0)
                        AaruConsole.WriteLine(Modes.PrettifyModePage_11(page.PageResponse));
                    else
                        goto default;

                    break;
                }
                case 0x12:
                case 0x13:
                case 0x14:
                {
                    if(page.Subpage == 0)
                        AaruConsole.WriteLine(Modes.PrettifyModePage_12_13_14(page.PageResponse));
                    else
                        goto default;

                    break;
                }
                case 0x1A:
                {
                    if(page.Subpage == 0)
                        AaruConsole.WriteLine(Modes.PrettifyModePage_1A(page.PageResponse));
                    else if(page.Subpage == 1)
                        AaruConsole.WriteLine(Modes.PrettifyModePage_1A_S01(page.PageResponse));
                    else
                        goto default;

                    break;
                }
                case 0x1B:
                {
                    if(page.Subpage == 0)
                        AaruConsole.WriteLine(Modes.PrettifyModePage_1B(page.PageResponse));
                    else
                        goto default;

                    break;
                }
                case 0x1C:
                {
                    if(page.Subpage == 0)
                        AaruConsole.WriteLine(devType == PeripheralDeviceTypes.MultiMediaDevice
                                                  ? Modes.PrettifyModePage_1C_SFF(page.PageResponse)
                                                  : Modes.PrettifyModePage_1C(page.PageResponse));
                    else if(page.Subpage == 1)
                        AaruConsole.WriteLine(Modes.PrettifyModePage_1C_S01(page.PageResponse));
                    else
                        goto default;

                    break;
                }
                case 0x1D:
                {
                    if(page.Subpage == 0)
                        AaruConsole.WriteLine(Modes.PrettifyModePage_1D(page.PageResponse));
                    else
                        goto default;

                    break;
                }
                case 0x21:
                {
                    if(StringHandlers.CToString(vendorId).Trim() == "CERTANCE")
                        AaruConsole.WriteLine(Modes.PrettifyCertanceModePage_21(page.PageResponse));
                    else
                        goto default;

                    break;
                }
                case 0x22:
                {
                    if(StringHandlers.CToString(vendorId).Trim() == "CERTANCE")
                        AaruConsole.WriteLine(Modes.PrettifyCertanceModePage_22(page.PageResponse));
                    else
                        goto default;

                    break;
                }
                case 0x24:
                {
                    if(StringHandlers.CToString(vendorId).Trim() == "IBM")
                        AaruConsole.WriteLine(Modes.PrettifyIBMModePage_24(page.PageResponse));
                    else
                        goto default;

                    break;
                }
                case 0x2A:
                {
                    if(page.Subpage == 0)
                        AaruConsole.WriteLine(Modes.PrettifyModePage_2A(page.PageResponse));
                    else
                        goto default;

                    break;
                }
                case 0x2F:
                {
                    if(StringHandlers.CToString(vendorId).Trim() == "IBM")
                        AaruConsole.WriteLine(Modes.PrettifyIBMModePage_2F(page.PageResponse));
                    else
                        goto default;

                    break;
                }
                case 0x30:
                {
                    if(Modes.IsAppleModePage_30(page.PageResponse))
                        AaruConsole.WriteLine("Drive identifies as Apple OEM drive");
                    else
                        goto default;

                    break;
                }
                case 0x3B:
                {
                    if(StringHandlers.CToString(vendorId).Trim() == "HP")
                        AaruConsole.WriteLine(Modes.PrettifyHPModePage_3B(page.PageResponse));
                    else
                        goto default;

                    break;
                }
                case 0x3C:
                {
                    if(StringHandlers.CToString(vendorId).Trim() == "HP")
                        AaruConsole.WriteLine(Modes.PrettifyHPModePage_3C(page.PageResponse));
                    else
                        goto default;

                    break;
                }
                case 0x3D:
                {
                    if(StringHandlers.CToString(vendorId).Trim() == "IBM")
                        AaruConsole.WriteLine(Modes.PrettifyIBMModePage_3D(page.PageResponse));
                    else if(StringHandlers.CToString(vendorId).Trim() == "HP")
                        AaruConsole.WriteLine(Modes.PrettifyHPModePage_3D(page.PageResponse));
                    else
                        goto default;

                    break;
                }
                case 0x3E:
                {
                    if(StringHandlers.CToString(vendorId).Trim() == "FUJITSU")
                        AaruConsole.WriteLine(Modes.PrettifyFujitsuModePage_3E(page.PageResponse));
                    else if(StringHandlers.CToString(vendorId).Trim() == "HP")
                        AaruConsole.WriteLine(Modes.PrettifyHPModePage_3E(page.PageResponse));
                    else
                        goto default;

                    break;
                }
                default:
                {
                    if(page.Subpage != 0)
                        AaruConsole.WriteLine("Found unknown mode page {0:X2}h subpage {1:X2}h", page.Page,
                                              page.Subpage);
                    else
                        AaruConsole.WriteLine("Found unknown mode page {0:X2}h", page.Page);

                    break;
                }
            }
    }
}