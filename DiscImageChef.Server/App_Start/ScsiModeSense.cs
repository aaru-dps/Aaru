// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ScsiModeSense.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef Server.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MODE PAGEs from reports.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using DiscImageChef.Metadata;
using DiscImageChef.Decoders.SCSI;
namespace DiscImageChef.Server.App_Start
{
    public static class ScsiModeSense
    {
        public static void Report(modeType modeSense, string vendor, PeripheralDeviceTypes deviceType, ref List<string> scsiOneValue, ref Dictionary<string, string> modePages)
        {
            if(modeSense.MediumTypeSpecified)
                scsiOneValue.Add(string.Format("Medium type is {0:X2}h", modeSense.MediumType));
            if(modeSense.WriteProtected)
                scsiOneValue.Add("Device is write protected.");
            if(modeSense.BlockDescriptors != null)
            {
                foreach(blockDescriptorType descriptor in modeSense.BlockDescriptors)
                {
                    if(descriptor.BlocksSpecified && descriptor.BlockLengthSpecified)
                        scsiOneValue.Add(string.Format("Density code {0:X2}h has {1} blocks of {2} bytes each",
                                                       descriptor.Density, descriptor.Blocks, descriptor.BlockLength));
                    else
                        scsiOneValue.Add(string.Format("Density code {0:X2}h", descriptor.Density));
                }
            }
            if(modeSense.DPOandFUA)
                scsiOneValue.Add("Drive supports DPO and FUA bits");
            if(modeSense.BlankCheckEnabled)
                scsiOneValue.Add("Blank checking during write is enabled");
            if(modeSense.BufferedModeSpecified)
            {
                switch(modeSense.BufferedMode)
                {
                    case 0:
                        scsiOneValue.Add("Device writes directly to media");
                        break;
                    case 1:
                        scsiOneValue.Add("Device uses a write cache");
                        break;
                    case 2:
                        scsiOneValue.Add("Device uses a write cache but doesn't return until cache is flushed");
                        break;
                    default:
                        scsiOneValue.Add(string.Format("Unknown buffered mode code 0x{0:X2}", modeSense.BufferedMode));
                        break;
                }
            }

            if(modeSense.ModePages != null)
            {
                foreach(modePageType page in modeSense.ModePages)
                {
                    switch(page.page)
                    {
                        case 0x00:
                            {
                                if(deviceType == PeripheralDeviceTypes.MultiMediaDevice && page.subpage == 0)
                                    modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyModePage_00_SFF(page.value));
                                else
                                {
                                    if(page.subpage != 0)
                                        modePages.Add(string.Format("MODE page {0:X2}h subpage {1:X2}h", page.page, page.subpage), "Unknown vendor mode page");
                                    else
                                        modePages.Add(string.Format("MODE page {0:X2}h", page.page), "Unknown vendor mode page");
                                }
                                break;
                            }
                        case 0x01:
                            {
                                if(page.subpage == 0)
                                {
                                    if(deviceType == PeripheralDeviceTypes.MultiMediaDevice)
                                        modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyModePage_01_MMC(page.value));
                                    else
                                        modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyModePage_01(page.value));
                                }
                                else
                                    goto default;

                                break;
                            }
                        case 0x02:
                            {
                                if(page.subpage == 0)
                                    modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyModePage_02(page.value));
                                else
                                    goto default;

                                break;
                            }
                        case 0x03:
                            {
                                if(page.subpage == 0)
                                    modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyModePage_03(page.value));
                                else
                                    goto default;

                                break;
                            }
                        case 0x04:
                            {
                                if(page.subpage == 0)
                                    modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyModePage_04(page.value));
                                else
                                    goto default;

                                break;
                            }
                        case 0x05:
                            {
                                if(page.subpage == 0)
                                    modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyModePage_05(page.value));
                                else
                                    goto default;

                                break;
                            }
                        case 0x06:
                            {
                                if(page.subpage == 0)
                                    modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyModePage_06(page.value));
                                else
                                    goto default;

                                break;
                            }
                        case 0x07:
                            {
                                if(page.subpage == 0)
                                {
                                    if(deviceType == PeripheralDeviceTypes.MultiMediaDevice)
                                        modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyModePage_07_MMC(page.value));
                                    else
                                        modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyModePage_07(page.value));
                                }
                                else
                                    goto default;

                                break;
                            }
                        case 0x08:
                            {
                                if(page.subpage == 0)
                                    modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyModePage_08(page.value));
                                else
                                    goto default;

                                break;
                            }
                        case 0x0A:
                            {
                                if(page.subpage == 0)
                                    modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyModePage_0A(page.value));
                                else if(page.subpage == 1)
                                    modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyModePage_0A_S01(page.value));
                                else
                                    goto default;

                                break;
                            }
                        case 0x0B:
                            {
                                if(page.subpage == 0)
                                    modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyModePage_0B(page.value));
                                else
                                    goto default;

                                break;
                            }
                        case 0x0D:
                            {
                                if(page.subpage == 0)
                                    modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyModePage_0D(page.value));
                                else
                                    goto default;

                                break;
                            }
                        case 0x0E:
                            {
                                if(page.subpage == 0)
                                    modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyModePage_0E(page.value));
                                else
                                    goto default;

                                break;
                            }
                        case 0x0F:
                            {
                                if(page.subpage == 0)
                                    modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyModePage_0F(page.value));
                                else
                                    goto default;

                                break;
                            }
                        case 0x10:
                            {
                                if(page.subpage == 0)
                                {
                                    if(deviceType == Decoders.SCSI.PeripheralDeviceTypes.SequentialAccess)
                                        modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyModePage_10_SSC(page.value));
                                    else
                                        modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyModePage_10(page.value));
                                }
                                else
                                    goto default;

                                break;
                            }
                        case 0x11:
                            {
                                if(page.subpage == 0)
                                    modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyModePage_11(page.value));
                                else
                                    goto default;

                                break;
                            }
                        case 0x12:
                        case 0x13:
                        case 0x14:
                            {
                                if(page.subpage == 0)
                                    modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyModePage_12_13_14(page.value));
                                else
                                    goto default;

                                break;
                            }
                        case 0x1A:
                            {
                                if(page.subpage == 0)
                                    modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyModePage_1A(page.value));
                                else if(page.subpage == 1)
                                    modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyModePage_1A_S01(page.value));
                                else
                                    goto default;

                                break;
                            }
                        case 0x1B:
                            {
                                if(page.subpage == 0)
                                    modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyModePage_1B(page.value));
                                else
                                    goto default;

                                break;
                            }
                        case 0x1C:
                            {
                                if(page.subpage == 0)
                                {
                                    if(deviceType == PeripheralDeviceTypes.MultiMediaDevice)
                                        modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyModePage_1C_SFF(page.value));
                                    else
                                        modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyModePage_1C(page.value));
                                }
                                else if(page.subpage == 1)
                                    modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyModePage_1C_S01(page.value));
                                else
                                    goto default;

                                break;
                            }
                        case 0x1D:
                            {
                                if(page.subpage == 0)
                                    modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyModePage_1D(page.value));
                                else
                                    goto default;

                                break;
                            }
                        case 0x21:
                            {
                                if(vendor == "CERTANCE")
                                    modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyCertanceModePage_21(page.value));
                                else
                                    goto default;

                                break;
                            }
                        case 0x22:
                            {
                                if(vendor == "CERTANCE")
                                    modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyCertanceModePage_22(page.value));
                                else
                                    goto default;

                                break;
                            }
                        case 0x24:
                            {
                                if(vendor == "IBM")
                                    modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyIBMModePage_24(page.value));
                                else
                                    goto default;

                                break;
                            }
                        case 0x2A:
                            {
                                if(page.subpage == 0)
                                    modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyModePage_2A(page.value));
                                else
                                    goto default;

                                break;
                            }
                        case 0x2F:
                            {
                                if(vendor == "IBM")
                                    modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyIBMModePage_2F(page.value));
                                else
                                    goto default;

                                break;
                            }
                        case 0x30:
                            {
                                if(Modes.IsAppleModePage_30(page.value))
                                    modePages.Add("MODE page 30h", "Drive identifies as an Apple OEM drive");
                                else
                                    goto default;

                                break;
                            }
                        case 0x3B:
                            {
                                if(vendor == "HP")
                                    modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyHPModePage_3B(page.value));
                                else
                                    goto default;

                                break;
                            }
                        case 0x3C:
                            {
                                if(vendor == "HP")
                                    modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyHPModePage_3C(page.value));
                                else
                                    goto default;

                                break;
                            }
                        case 0x3D:
                            {
                                if(vendor == "IBM")
                                    modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyIBMModePage_3D(page.value));
                                else if(vendor == "HP")
                                    modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyHPModePage_3D(page.value));
                                else
                                    goto default;

                                break;
                            }
                        case 0x3E:
                            {
                                if(vendor == "FUJITSU")
                                    modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyFujitsuModePage_3E(page.value));
                                else if(vendor == "HP")
                                    modePages.Add(string.Format("MODE page {0:X2}h", page.page), Modes.PrettifyHPModePage_3E(page.value));
                                else
                                    goto default;

                                break;
                            }
                        default:
                            {
                                if(page.subpage != 0)
                                    modePages.Add(string.Format("MODE page {0:X2}h subpage {1:X2}h", page.page, page.subpage), "Unknown mode page");
                                else
                                    modePages.Add(string.Format("MODE page {0:X2}h", page.page), "Unknown mode page");
                            }
                            break;
                    }
                }

                Dictionary<string, string> newModePages = new Dictionary<string, string>();
                foreach(KeyValuePair<string, string> kvp in modePages)
                {
                    if(string.IsNullOrWhiteSpace(kvp.Value))
                        newModePages.Add(kvp.Key, "Undecoded");
                    else
                        newModePages.Add(kvp.Key, kvp.Value.Replace("\n", "<br/>"));
                }
                modePages = newModePages;
            }
        }
    }
}
