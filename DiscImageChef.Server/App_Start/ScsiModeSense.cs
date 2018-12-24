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

using System.Collections.Generic;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Decoders.SCSI;

namespace DiscImageChef.Server
{
    public static class ScsiModeSense
    {
        /// <summary>
        ///     Takes the MODE PAGEs part of a device report and prints it as a list of values and another list of key=value pairs
        ///     to be sequenced by ASP.NET in the rendering
        /// </summary>
        /// <param name="modeSense">MODE PAGEs part of a device report</param>
        /// <param name="vendor">SCSI vendor string</param>
        /// <param name="deviceType">SCSI peripheral device type</param>
        /// <param name="scsiOneValue">List to put values on</param>
        /// <param name="modePages">List to put key=value pairs on</param>
        public static void Report(ScsiMode              modeSense, string vendor,
                                  PeripheralDeviceTypes deviceType,
                                  ref List<string>      scsiOneValue, ref Dictionary<string, string> modePages)
        {
            if(modeSense.MediumType.HasValue) scsiOneValue.Add($"Medium type is {modeSense.MediumType:X2}h");
            if(modeSense.WriteProtected) scsiOneValue.Add("Device is write protected.");
            if(modeSense.BlockDescriptors != null)
                foreach(BlockDescriptor descriptor in modeSense.BlockDescriptors)
                    if(descriptor.Blocks.HasValue && descriptor.BlockLength.HasValue)
                        scsiOneValue
                           .Add($"Density code {descriptor.Density:X2}h has {descriptor.Blocks} blocks of {descriptor.BlockLength} bytes each");
                    else
                        scsiOneValue.Add($"Density code {descriptor.Density:X2}h");

            if(modeSense.DPOandFUA) scsiOneValue.Add("Drive supports DPO and FUA bits");
            if(modeSense.BlankCheckEnabled) scsiOneValue.Add("Blank checking during write is enabled");
            if(modeSense.BufferedMode.HasValue)
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
                        scsiOneValue.Add($"Unknown buffered mode code 0x{modeSense.BufferedMode:X2}");
                        break;
                }

            if(modeSense.ModePages == null) return;

            foreach(ScsiPage page in modeSense.ModePages)
                switch(page.page)
                {
                    case 0x00:
                    {
                        if(deviceType == PeripheralDeviceTypes.MultiMediaDevice && page.subpage == 0)
                            modePages.Add($"MODE page {page.page:X2}h", Modes.PrettifyModePage_00_SFF(page.value));
                        else
                            modePages
                               .Add(page.subpage != 0 ? $"MODE page {page.page:X2}h subpage {page.subpage:X2}h" : $"MODE page {page.page:X2}h",
                                    "Unknown vendor mode page");
                        break;
                    }
                    case 0x01:
                    {
                        if(page.subpage == 0)
                            modePages.Add($"MODE page {page.page:X2}h",
                                          deviceType == PeripheralDeviceTypes.MultiMediaDevice
                                              ? Modes.PrettifyModePage_01_MMC(page.value)
                                              : Modes.PrettifyModePage_01(page.value));
                        else goto default;

                        break;
                    }
                    case 0x02:
                    {
                        if(page.subpage == 0)
                            modePages.Add($"MODE page {page.page:X2}h", Modes.PrettifyModePage_02(page.value));
                        else goto default;

                        break;
                    }
                    case 0x03:
                    {
                        if(page.subpage == 0)
                            modePages.Add($"MODE page {page.page:X2}h", Modes.PrettifyModePage_03(page.value));
                        else goto default;

                        break;
                    }
                    case 0x04:
                    {
                        if(page.subpage == 0)
                            modePages.Add($"MODE page {page.page:X2}h", Modes.PrettifyModePage_04(page.value));
                        else goto default;

                        break;
                    }
                    case 0x05:
                    {
                        if(page.subpage == 0)
                            modePages.Add($"MODE page {page.page:X2}h", Modes.PrettifyModePage_05(page.value));
                        else goto default;

                        break;
                    }
                    case 0x06:
                    {
                        if(page.subpage == 0)
                            modePages.Add($"MODE page {page.page:X2}h", Modes.PrettifyModePage_06(page.value));
                        else goto default;

                        break;
                    }
                    case 0x07:
                    {
                        if(page.subpage == 0)
                            modePages.Add($"MODE page {page.page:X2}h",
                                          deviceType == PeripheralDeviceTypes.MultiMediaDevice
                                              ? Modes.PrettifyModePage_07_MMC(page.value)
                                              : Modes.PrettifyModePage_07(page.value));
                        else goto default;

                        break;
                    }
                    case 0x08:
                    {
                        if(page.subpage == 0)
                            modePages.Add($"MODE page {page.page:X2}h", Modes.PrettifyModePage_08(page.value));
                        else goto default;

                        break;
                    }
                    case 0x0A:
                    {
                        if(page.subpage == 0)
                            modePages.Add($"MODE page {page.page:X2}h", Modes.PrettifyModePage_0A(page.value));
                        else if(page.subpage == 1)
                            modePages.Add($"MODE page {page.page:X2}h subpage {page.subpage:X2}h",
                                          Modes.PrettifyModePage_0A_S01(page.value));
                        else goto default;

                        break;
                    }
                    case 0x0B:
                    {
                        if(page.subpage == 0)
                            modePages.Add($"MODE page {page.page:X2}h", Modes.PrettifyModePage_0B(page.value));
                        else goto default;

                        break;
                    }
                    case 0x0D:
                    {
                        if(page.subpage == 0)
                            modePages.Add($"MODE page {page.page:X2}h", Modes.PrettifyModePage_0D(page.value));
                        else goto default;

                        break;
                    }
                    case 0x0E:
                    {
                        if(page.subpage == 0)
                            modePages.Add($"MODE page {page.page:X2}h", Modes.PrettifyModePage_0E(page.value));
                        else goto default;

                        break;
                    }
                    case 0x0F:
                    {
                        if(page.subpage == 0)
                            modePages.Add($"MODE page {page.page:X2}h", Modes.PrettifyModePage_0F(page.value));
                        else goto default;

                        break;
                    }
                    case 0x10:
                    {
                        if(page.subpage == 0)
                            modePages.Add($"MODE page {page.page:X2}h",
                                          deviceType == PeripheralDeviceTypes.SequentialAccess
                                              ? Modes.PrettifyModePage_10_SSC(page.value)
                                              : Modes.PrettifyModePage_10(page.value));
                        else goto default;

                        break;
                    }
                    case 0x11:
                    {
                        if(page.subpage == 0)
                            modePages.Add($"MODE page {page.page:X2}h", Modes.PrettifyModePage_11(page.value));
                        else goto default;

                        break;
                    }
                    case 0x12:
                    case 0x13:
                    case 0x14:
                    {
                        if(page.subpage == 0)
                            modePages.Add($"MODE page {page.page:X2}h", Modes.PrettifyModePage_12_13_14(page.value));
                        else goto default;

                        break;
                    }
                    case 0x1A:
                    {
                        if(page.subpage == 0)
                            modePages.Add($"MODE page {page.page:X2}h", Modes.PrettifyModePage_1A(page.value));
                        else if(page.subpage == 1)
                            modePages.Add($"MODE page {page.page:X2}h subpage {page.subpage:X2}h",
                                          Modes.PrettifyModePage_1A_S01(page.value));
                        else goto default;

                        break;
                    }
                    case 0x1B:
                    {
                        if(page.subpage == 0)
                            modePages.Add($"MODE page {page.page:X2}h", Modes.PrettifyModePage_1B(page.value));
                        else goto default;

                        break;
                    }
                    case 0x1C:
                    {
                        if(page.subpage == 0)
                            modePages.Add($"MODE page {page.page:X2}h",
                                          deviceType == PeripheralDeviceTypes.MultiMediaDevice
                                              ? Modes.PrettifyModePage_1C_SFF(page.value)
                                              : Modes.PrettifyModePage_1C(page.value));
                        else if(page.subpage == 1)
                            modePages.Add($"MODE page {page.page:X2}h subpage {page.subpage:X2}h",
                                          Modes.PrettifyModePage_1C_S01(page.value));
                        else goto default;

                        break;
                    }
                    case 0x1D:
                    {
                        if(page.subpage == 0)
                            modePages.Add($"MODE page {page.page:X2}h", Modes.PrettifyModePage_1D(page.value));
                        else goto default;

                        break;
                    }
                    case 0x21:
                    {
                        if(vendor == "CERTANCE")
                            modePages.Add($"MODE page {page.page:X2}h", Modes.PrettifyCertanceModePage_21(page.value));
                        else goto default;

                        break;
                    }
                    case 0x22:
                    {
                        if(vendor == "CERTANCE")
                            modePages.Add($"MODE page {page.page:X2}h", Modes.PrettifyCertanceModePage_22(page.value));
                        else goto default;

                        break;
                    }
                    case 0x24:
                    {
                        if(vendor == "IBM")
                            modePages.Add($"MODE page {page.page:X2}h", Modes.PrettifyIBMModePage_24(page.value));
                        else goto default;

                        break;
                    }
                    case 0x2A:
                    {
                        if(page.subpage == 0)
                            modePages.Add($"MODE page {page.page:X2}h", Modes.PrettifyModePage_2A(page.value));
                        else goto default;

                        break;
                    }
                    case 0x2F:
                    {
                        if(vendor == "IBM")
                            modePages.Add($"MODE page {page.page:X2}h", Modes.PrettifyIBMModePage_2F(page.value));
                        else goto default;

                        break;
                    }
                    case 0x30:
                    {
                        if(Modes.IsAppleModePage_30(page.value))
                            modePages.Add("MODE page 30h", "Drive identifies as an Apple OEM drive");
                        else goto default;

                        break;
                    }
                    case 0x3B:
                    {
                        if(vendor == "HP")
                            modePages.Add($"MODE page {page.page:X2}h", Modes.PrettifyHPModePage_3B(page.value));
                        else goto default;

                        break;
                    }
                    case 0x3C:
                    {
                        if(vendor == "HP")
                            modePages.Add($"MODE page {page.page:X2}h", Modes.PrettifyHPModePage_3C(page.value));
                        else goto default;

                        break;
                    }
                    case 0x3D:
                    {
                        if(vendor == "IBM")
                            modePages.Add($"MODE page {page.page:X2}h", Modes.PrettifyIBMModePage_3D(page.value));
                        else if(vendor == "HP")
                            modePages.Add($"MODE page {page.page:X2}h", Modes.PrettifyHPModePage_3D(page.value));
                        else goto default;

                        break;
                    }
                    case 0x3E:
                    {
                        if(vendor == "FUJITSU")
                            modePages.Add($"MODE page {page.page:X2}h", Modes.PrettifyFujitsuModePage_3E(page.value));
                        else if(vendor == "HP")
                            modePages.Add($"MODE page {page.page:X2}h", Modes.PrettifyHPModePage_3E(page.value));
                        else goto default;

                        break;
                    }
                    default:
                    {
                        modePages.Add(page.subpage != 0 ? $"MODE page {page.page:X2}h subpage {page.subpage:X2}h" : $"MODE page {page.page:X2}h",
                                      "Unknown mode page");
                    }
                        break;
                }

            Dictionary<string, string> newModePages = new Dictionary<string, string>();
            foreach(KeyValuePair<string, string> kvp in modePages)
                newModePages.Add(kvp.Key,
                                 string.IsNullOrWhiteSpace(kvp.Value) ? "Undecoded" : kvp.Value.Replace("\n", "<br/>"));

            modePages = newModePages;
        }
    }
}