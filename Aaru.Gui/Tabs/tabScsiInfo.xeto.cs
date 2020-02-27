// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : tabScsiInfo.xeto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device information.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the SCSI device information.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General public License for more details.
//
//     You should have received a copy of the GNU General public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Console;
using Aaru.Decoders.SCSI;
using Aaru.Decoders.SCSI.MMC;
using Eto.Forms;
using Eto.Serialization.Xaml;
using Inquiry = Aaru.Decoders.SCSI.Inquiry;

namespace Aaru.Gui.Tabs
{
    public class tabScsiInfo : TabPage
    {
        private byte[] configuration;
        private Dictionary<byte, byte[]> evpdPages;
        private CommonTypes.Structs.Devices.SCSI.Inquiry? inquiry;
        private byte[] inquiryData;
        private Modes.DecodedMode? mode;
        private byte[] modeSense10;
        private byte[] modeSense6;
        private PeripheralDeviceTypes type;

        public tabScsiInfo()
        {
            XamlReader.Load(this);
        }

        internal void LoadData(byte[] scsiInquiryData, CommonTypes.Structs.Devices.SCSI.Inquiry? scsiInquiry,
            Dictionary<byte, byte[]> scsiEvpdPages, Modes.DecodedMode? scsiMode,
            PeripheralDeviceTypes scsiType, byte[] scsiModeSense6,
            byte[] scsiModeSense10,
            byte[] mmcConfiguration)
        {
            inquiryData = scsiInquiryData;
            inquiry = scsiInquiry;
            evpdPages = scsiEvpdPages;
            mode = scsiMode;
            type = scsiType;
            modeSense6 = scsiModeSense6;
            modeSense10 = scsiModeSense10;
            configuration = mmcConfiguration;

            if (inquiryData == null || !inquiry.HasValue) return;

            Visible = true;
            txtScsiInquiry.Text = Inquiry.Prettify(inquiry);

            if (mode.HasValue)
            {
                tabScsiModeSense.Visible = true;

                var modePagesList = new TreeGridItemCollection();

                treeModeSensePages.Columns.Add(new GridColumn {HeaderText = "Page", DataCell = new TextBoxCell(0)});

                treeModeSensePages.AllowMultipleSelection = false;
                treeModeSensePages.ShowHeader = false;
                treeModeSensePages.DataStore = modePagesList;

                modePagesList.Add(new TreeGridItem
                {
                    Values = new object[]
                    {
                        "Header", Modes.PrettifyModeHeader(mode.Value.Header, type)
                    }
                });

                if (mode.Value.Pages != null)
                    foreach (var page in mode.Value.Pages.OrderBy(t => t.Page).ThenBy(t => t.Subpage))
                    {
                        var pageNumberText = page.Subpage == 0
                            ? $"MODE {page.Page:X2}h"
                            : $"MODE {page.Page:X2} Subpage {page.Subpage:X2}";
                        string decodedText;

                        switch (page.Page)
                        {
                            case 0x00:
                            {
                                if (type == PeripheralDeviceTypes.MultiMediaDevice && page.Subpage == 0)
                                    decodedText = Modes.PrettifyModePage_00_SFF(page.PageResponse);
                                else decodedText = "Undecoded";

                                break;
                            }
                            case 0x01:
                            {
                                if (page.Subpage == 0)
                                    decodedText = type == PeripheralDeviceTypes.MultiMediaDevice
                                        ? Modes.PrettifyModePage_01_MMC(page.PageResponse)
                                        : Modes.PrettifyModePage_01(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x02:
                            {
                                if (page.Subpage == 0) decodedText = Modes.PrettifyModePage_02(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x03:
                            {
                                if (page.Subpage == 0) decodedText = Modes.PrettifyModePage_03(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x04:
                            {
                                if (page.Subpage == 0) decodedText = Modes.PrettifyModePage_04(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x05:
                            {
                                if (page.Subpage == 0) decodedText = Modes.PrettifyModePage_05(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x06:
                            {
                                if (page.Subpage == 0) decodedText = Modes.PrettifyModePage_06(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x07:
                            {
                                if (page.Subpage == 0)
                                    decodedText = type == PeripheralDeviceTypes.MultiMediaDevice
                                        ? Modes.PrettifyModePage_07_MMC(page.PageResponse)
                                        : Modes.PrettifyModePage_07(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x08:
                            {
                                if (page.Subpage == 0) decodedText = Modes.PrettifyModePage_08(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x0A:
                            {
                                if (page.Subpage == 0) decodedText = Modes.PrettifyModePage_0A(page.PageResponse);
                                else if (page.Subpage == 1)
                                    decodedText = Modes.PrettifyModePage_0A_S01(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x0B:
                            {
                                if (page.Subpage == 0) decodedText = Modes.PrettifyModePage_0B(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x0D:
                            {
                                if (page.Subpage == 0) decodedText = Modes.PrettifyModePage_0D(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x0E:
                            {
                                if (page.Subpage == 0) decodedText = Modes.PrettifyModePage_0E(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x0F:
                            {
                                if (page.Subpage == 0) decodedText = Modes.PrettifyModePage_0F(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x10:
                            {
                                if (page.Subpage == 0)
                                    decodedText = type == PeripheralDeviceTypes.SequentialAccess
                                        ? Modes.PrettifyModePage_10_SSC(page.PageResponse)
                                        : Modes.PrettifyModePage_10(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x11:
                            {
                                if (page.Subpage == 0) decodedText = Modes.PrettifyModePage_11(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x12:
                            case 0x13:
                            case 0x14:
                            {
                                if (page.Subpage == 0) decodedText = Modes.PrettifyModePage_12_13_14(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x1A:
                            {
                                if (page.Subpage == 0) decodedText = Modes.PrettifyModePage_1A(page.PageResponse);
                                else if (page.Subpage == 1)
                                    decodedText = Modes.PrettifyModePage_1A_S01(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x1B:
                            {
                                if (page.Subpage == 0) decodedText = Modes.PrettifyModePage_1B(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x1C:
                            {
                                if (page.Subpage == 0)
                                    decodedText = type == PeripheralDeviceTypes.MultiMediaDevice
                                        ? Modes.PrettifyModePage_1C_SFF(page.PageResponse)
                                        : Modes.PrettifyModePage_1C(page.PageResponse);
                                else if (page.Subpage == 1)
                                    decodedText = Modes.PrettifyModePage_1C_S01(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x1D:
                            {
                                if (page.Subpage == 0) decodedText = Modes.PrettifyModePage_1D(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x21:
                            {
                                if (StringHandlers.CToString(inquiry?.VendorIdentification).Trim() == "CERTANCE")
                                    decodedText = Modes.PrettifyCertanceModePage_21(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x22:
                            {
                                if (StringHandlers.CToString(inquiry?.VendorIdentification).Trim() == "CERTANCE")
                                    decodedText = Modes.PrettifyCertanceModePage_22(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x24:
                            {
                                if (StringHandlers.CToString(inquiry?.VendorIdentification).Trim() == "IBM")
                                    decodedText = Modes.PrettifyIBMModePage_24(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x2A:
                            {
                                if (page.Subpage == 0) decodedText = Modes.PrettifyModePage_2A(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x2F:
                            {
                                if (StringHandlers.CToString(inquiry?.VendorIdentification).Trim() == "IBM")
                                    decodedText = Modes.PrettifyIBMModePage_2F(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x30:
                            {
                                if (Modes.IsAppleModePage_30(page.PageResponse))
                                    decodedText = "Drive identifies as Apple OEM drive";
                                else goto default;

                                break;
                            }
                            case 0x3B:
                            {
                                if (StringHandlers.CToString(inquiry?.VendorIdentification).Trim() == "HP")
                                    decodedText = Modes.PrettifyHPModePage_3B(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x3C:
                            {
                                if (StringHandlers.CToString(inquiry?.VendorIdentification).Trim() == "HP")
                                    decodedText = Modes.PrettifyHPModePage_3C(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x3D:
                            {
                                if (StringHandlers.CToString(inquiry?.VendorIdentification).Trim() == "IBM")
                                    decodedText = Modes.PrettifyIBMModePage_3D(page.PageResponse);
                                else if (StringHandlers.CToString(inquiry?.VendorIdentification).Trim() == "HP")
                                    decodedText = Modes.PrettifyHPModePage_3D(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x3E:
                            {
                                if (StringHandlers.CToString(inquiry?.VendorIdentification).Trim() == "FUJITSU")
                                    decodedText = Modes.PrettifyFujitsuModePage_3E(page.PageResponse);
                                else if (StringHandlers.CToString(inquiry?.VendorIdentification).Trim() == "HP")
                                    decodedText = Modes.PrettifyHPModePage_3E(page.PageResponse);
                                else goto default;

                                break;
                            }
                            default:
                            {
                                decodedText = "Undecoded";
                                break;
                            }
                        }

                        // TODO: Automatic error reporting
                        if (decodedText == null) decodedText = "Error decoding page, please open an issue.";
                        modePagesList.Add(new TreeGridItem {Values = new object[] {pageNumberText, decodedText}});
                    }
            }

            if (evpdPages != null)
            {
                tabScsiEvpd.Visible = true;
                treeEvpdPages.ShowHeader = false;

                var evpdPagesList = new TreeGridItemCollection();

                treeEvpdPages.Columns.Add(new GridColumn {HeaderText = "Page", DataCell = new TextBoxCell(0)});

                treeEvpdPages.AllowMultipleSelection = false;
                treeEvpdPages.ShowHeader = false;
                treeEvpdPages.DataStore = evpdPagesList;

                foreach (var page in evpdPages.OrderBy(t => t.Key))
                {
                    var evpdPageTitle = "";
                    var evpdDecodedPage = "";
                    if (page.Key >= 0x01 && page.Key <= 0x7F)
                    {
                        evpdPageTitle = $"ASCII Page {page.Key:X2}h";
                        evpdDecodedPage = EVPD.DecodeASCIIPage(page.Value);
                    }
                    else if (page.Key == 0x80)
                    {
                        evpdPageTitle = "Unit Serial Number";
                        evpdDecodedPage = EVPD.DecodePage80(page.Value);
                    }
                    else if (page.Key == 0x81)
                    {
                        evpdPageTitle = "SCSI Implemented operating definitions";
                        evpdDecodedPage = EVPD.PrettifyPage_81(page.Value);
                    }
                    else if (page.Key == 0x82)
                    {
                        evpdPageTitle = "ASCII implemented operating definitions";
                        evpdDecodedPage = EVPD.DecodePage82(page.Value);
                    }
                    else if (page.Key == 0x83)
                    {
                        evpdPageTitle = "SCSI Device identification";
                        evpdDecodedPage = EVPD.PrettifyPage_83(page.Value);
                    }
                    else if (page.Key == 0x84)
                    {
                        evpdPageTitle = "SCSI Software Interface Identifiers";
                        evpdDecodedPage = EVPD.PrettifyPage_84(page.Value);
                    }
                    else if (page.Key == 0x85)
                    {
                        evpdPageTitle = "SCSI Management Network Addresses";
                        evpdDecodedPage = EVPD.PrettifyPage_85(page.Value);
                    }
                    else if (page.Key == 0x86)
                    {
                        evpdPageTitle = "SCSI Extended INQUIRY Data";
                        evpdDecodedPage = EVPD.PrettifyPage_86(page.Value);
                    }
                    else if (page.Key == 0x89)
                    {
                        evpdPageTitle = "SCSI to ATA Translation Layer Data";
                        evpdDecodedPage = EVPD.PrettifyPage_89(page.Value);
                    }
                    else if (page.Key == 0xB0)
                    {
                        evpdPageTitle = "SCSI Sequential-access Device Capabilities";
                        evpdDecodedPage = EVPD.PrettifyPage_B0(page.Value);
                    }
                    else if (page.Key == 0xB1)
                    {
                        evpdPageTitle = "Manufacturer-assigned Serial Number";
                        evpdDecodedPage = EVPD.DecodePageB1(page.Value);
                    }
                    else if (page.Key == 0xB2)
                    {
                        evpdPageTitle = "TapeAlert Supported Flags Bitmap";
                        evpdDecodedPage = $"0x{EVPD.DecodePageB2(page.Value):X16}";
                    }
                    else if (page.Key == 0xB3)
                    {
                        evpdPageTitle = "Automation Device Serial Number";
                        evpdDecodedPage = EVPD.DecodePageB3(page.Value);
                    }
                    else if (page.Key == 0xB4)
                    {
                        evpdPageTitle = "Data Transfer Device Element Address";
                        evpdDecodedPage = EVPD.DecodePageB4(page.Value);
                    }
                    else if (page.Key == 0xC0 &&
                             StringHandlers.CToString(inquiry.Value.VendorIdentification).ToLowerInvariant().Trim() ==
                             "quantum")
                    {
                        evpdPageTitle = "Quantum Firmware Build Information page";
                        evpdDecodedPage = EVPD.PrettifyPage_C0_Quantum(page.Value);
                    }
                    else if (page.Key == 0xC0 &&
                             StringHandlers.CToString(inquiry.Value.VendorIdentification).ToLowerInvariant().Trim() ==
                             "seagate")
                    {
                        evpdPageTitle = "Seagate Firmware Numbers page";
                        evpdDecodedPage = EVPD.PrettifyPage_C0_Seagate(page.Value);
                    }
                    else if (page.Key == 0xC0 &&
                             StringHandlers.CToString(inquiry.Value.VendorIdentification).ToLowerInvariant().Trim() ==
                             "ibm")
                    {
                        evpdPageTitle = "IBM Drive Component Revision Levels page";
                        evpdDecodedPage = EVPD.PrettifyPage_C0_IBM(page.Value);
                    }
                    else if (page.Key == 0xC1 &&
                             StringHandlers.CToString(inquiry.Value.VendorIdentification).ToLowerInvariant().Trim() ==
                             "ibm")
                    {
                        evpdPageTitle = "IBM Drive Serial Numbers page";
                        evpdDecodedPage = EVPD.PrettifyPage_C1_IBM(page.Value);
                    }
                    else if ((page.Key == 0xC0 || page.Key == 0xC1) &&
                             StringHandlers.CToString(inquiry.Value.VendorIdentification).ToLowerInvariant().Trim() ==
                             "certance")
                    {
                        evpdPageTitle = "Certance Drive Component Revision Levels page";
                        evpdDecodedPage = EVPD.PrettifyPage_C0_C1_Certance(page.Value);
                    }
                    else if ((page.Key == 0xC2 || page.Key == 0xC3 || page.Key == 0xC4 || page.Key == 0xC5 ||
                              page.Key == 0xC6) &&
                             StringHandlers.CToString(inquiry.Value.VendorIdentification).ToLowerInvariant().Trim() ==
                             "certance")
                    {
                        switch (page.Key)
                        {
                            case 0xC2:
                                evpdPageTitle = "Head Assembly Serial Number";
                                break;
                            case 0xC3:
                                evpdPageTitle = "Reel Motor 1 Serial Number";
                                break;
                            case 0xC4:
                                evpdPageTitle = "Reel Motor 2 Serial Number";
                                break;
                            case 0xC5:
                                evpdPageTitle = "Board Serial Number";
                                break;
                            case 0xC6:
                                evpdPageTitle = "Base Mechanical Serial Number";
                                break;
                        }

                        evpdDecodedPage = EVPD.PrettifyPage_C2_C3_C4_C5_C6_Certance(page.Value);
                    }
                    else if ((page.Key == 0xC0 || page.Key == 0xC1 || page.Key == 0xC2 || page.Key == 0xC3 ||
                              page.Key == 0xC4 || page.Key == 0xC5) && StringHandlers
                                 .CToString(inquiry.Value.VendorIdentification)
                                 .ToLowerInvariant().Trim() == "hp")
                    {
                        switch (page.Key)
                        {
                            case 0xC0:
                                evpdPageTitle = "HP Drive Firmware Revision Levels page:";
                                break;
                            case 0xC1:
                                evpdPageTitle = "HP Drive Hardware Revision Levels page:";
                                break;
                            case 0xC2:
                                evpdPageTitle = "HP Drive PCA Revision Levels page:";
                                break;
                            case 0xC3:
                                evpdPageTitle = "HP Drive Mechanism Revision Levels page:";
                                break;
                            case 0xC4:
                                evpdPageTitle = "HP Drive Head Assembly Revision Levels page:";
                                break;
                            case 0xC5:
                                evpdPageTitle = "HP Drive ACI Revision Levels page:";
                                break;
                        }

                        evpdDecodedPage = EVPD.PrettifyPage_C0_to_C5_HP(page.Value);
                    }
                    else if (page.Key == 0xDF &&
                             StringHandlers.CToString(inquiry.Value.VendorIdentification).ToLowerInvariant().Trim() ==
                             "certance")
                    {
                        evpdPageTitle = "Certance drive status page";
                        evpdDecodedPage = EVPD.PrettifyPage_DF_Certance(page.Value);
                    }
                    else
                    {
                        if (page.Key == 0x00) continue;

                        evpdPageTitle = $"Page {page.Key:X2}h";
                        evpdDecodedPage = "Undecoded";
                        AaruConsole.DebugWriteLine("Device-Info command", "Found undecoded SCSI VPD page 0x{0:X2}",
                            page.Key);
                    }

                    evpdPagesList.Add(new TreeGridItem
                    {
                        Values = new object[] {evpdPageTitle, evpdDecodedPage, page.Value}
                    });
                }
            }

            if (configuration != null)
            {
                tabMmcFeatures.Visible = true;

                var featuresList = new TreeGridItemCollection();

                treeMmcFeatures.Columns.Add(new GridColumn {HeaderText = "Feature", DataCell = new TextBoxCell(0)});

                treeMmcFeatures.AllowMultipleSelection = false;
                treeMmcFeatures.ShowHeader = false;
                treeMmcFeatures.DataStore = featuresList;

                var ftr = Features.Separate(configuration);

                AaruConsole.DebugWriteLine("Device-Info command", "GET CONFIGURATION length is {0} bytes",
                    ftr.DataLength);
                AaruConsole.DebugWriteLine("Device-Info command", "GET CONFIGURATION current profile is {0:X4}h",
                    ftr.CurrentProfile);
                if (ftr.Descriptors != null)
                    foreach (var desc in ftr.Descriptors)
                    {
                        var featureNumber = $"Feature {desc.Code:X4}h";
                        string featureDescription;
                        AaruConsole.DebugWriteLine("Device-Info command", "Feature {0:X4}h", desc.Code);

                        switch (desc.Code)
                        {
                            case 0x0000:
                                featureDescription = Features.Prettify_0000(desc.Data);
                                break;
                            case 0x0001:
                                featureDescription = Features.Prettify_0001(desc.Data);
                                break;
                            case 0x0002:
                                featureDescription = Features.Prettify_0002(desc.Data);
                                break;
                            case 0x0003:
                                featureDescription = Features.Prettify_0003(desc.Data);
                                break;
                            case 0x0004:
                                featureDescription = Features.Prettify_0004(desc.Data);
                                break;
                            case 0x0010:
                                featureDescription = Features.Prettify_0010(desc.Data);
                                break;
                            case 0x001D:
                                featureDescription = Features.Prettify_001D(desc.Data);
                                break;
                            case 0x001E:
                                featureDescription = Features.Prettify_001E(desc.Data);
                                break;
                            case 0x001F:
                                featureDescription = Features.Prettify_001F(desc.Data);
                                break;
                            case 0x0020:
                                featureDescription = Features.Prettify_0020(desc.Data);
                                break;
                            case 0x0021:
                                featureDescription = Features.Prettify_0021(desc.Data);
                                break;
                            case 0x0022:
                                featureDescription = Features.Prettify_0022(desc.Data);
                                break;
                            case 0x0023:
                                featureDescription = Features.Prettify_0023(desc.Data);
                                break;
                            case 0x0024:
                                featureDescription = Features.Prettify_0024(desc.Data);
                                break;
                            case 0x0025:
                                featureDescription = Features.Prettify_0025(desc.Data);
                                break;
                            case 0x0026:
                                featureDescription = Features.Prettify_0026(desc.Data);
                                break;
                            case 0x0027:
                                featureDescription = Features.Prettify_0027(desc.Data);
                                break;
                            case 0x0028:
                                featureDescription = Features.Prettify_0028(desc.Data);
                                break;
                            case 0x0029:
                                featureDescription = Features.Prettify_0029(desc.Data);
                                break;
                            case 0x002A:
                                featureDescription = Features.Prettify_002A(desc.Data);
                                break;
                            case 0x002B:
                                featureDescription = Features.Prettify_002B(desc.Data);
                                break;
                            case 0x002C:
                                featureDescription = Features.Prettify_002C(desc.Data);
                                break;
                            case 0x002D:
                                featureDescription = Features.Prettify_002D(desc.Data);
                                break;
                            case 0x002E:
                                featureDescription = Features.Prettify_002E(desc.Data);
                                break;
                            case 0x002F:
                                featureDescription = Features.Prettify_002F(desc.Data);
                                break;
                            case 0x0030:
                                featureDescription = Features.Prettify_0030(desc.Data);
                                break;
                            case 0x0031:
                                featureDescription = Features.Prettify_0031(desc.Data);
                                break;
                            case 0x0032:
                                featureDescription = Features.Prettify_0032(desc.Data);
                                break;
                            case 0x0033:
                                featureDescription = Features.Prettify_0033(desc.Data);
                                break;
                            case 0x0035:
                                featureDescription = Features.Prettify_0035(desc.Data);
                                break;
                            case 0x0037:
                                featureDescription = Features.Prettify_0037(desc.Data);
                                break;
                            case 0x0038:
                                featureDescription = Features.Prettify_0038(desc.Data);
                                break;
                            case 0x003A:
                                featureDescription = Features.Prettify_003A(desc.Data);
                                break;
                            case 0x003B:
                                featureDescription = Features.Prettify_003B(desc.Data);
                                break;
                            case 0x0040:
                                featureDescription = Features.Prettify_0040(desc.Data);
                                break;
                            case 0x0041:
                                featureDescription = Features.Prettify_0041(desc.Data);
                                break;
                            case 0x0042:
                                featureDescription = Features.Prettify_0042(desc.Data);
                                break;
                            case 0x0050:
                                featureDescription = Features.Prettify_0050(desc.Data);
                                break;
                            case 0x0051:
                                featureDescription = Features.Prettify_0051(desc.Data);
                                break;
                            case 0x0080:
                                featureDescription = Features.Prettify_0080(desc.Data);
                                break;
                            case 0x0100:
                                featureDescription = Features.Prettify_0100(desc.Data);
                                break;
                            case 0x0101:
                                featureDescription = Features.Prettify_0101(desc.Data);
                                break;
                            case 0x0102:
                                featureDescription = Features.Prettify_0102(desc.Data);
                                break;
                            case 0x0103:
                                featureDescription = Features.Prettify_0103(desc.Data);
                                break;
                            case 0x0104:
                                featureDescription = Features.Prettify_0104(desc.Data);
                                break;
                            case 0x0105:
                                featureDescription = Features.Prettify_0105(desc.Data);
                                break;
                            case 0x0106:
                                featureDescription = Features.Prettify_0106(desc.Data);
                                break;
                            case 0x0107:
                                featureDescription = Features.Prettify_0107(desc.Data);
                                break;
                            case 0x0108:
                                featureDescription = Features.Prettify_0108(desc.Data);
                                break;
                            case 0x0109:
                                featureDescription = Features.Prettify_0109(desc.Data);
                                break;
                            case 0x010A:
                                featureDescription = Features.Prettify_010A(desc.Data);
                                break;
                            case 0x010B:
                                featureDescription = Features.Prettify_010B(desc.Data);
                                break;
                            case 0x010C:
                                featureDescription = Features.Prettify_010C(desc.Data);
                                break;
                            case 0x010D:
                                featureDescription = Features.Prettify_010D(desc.Data);
                                break;
                            case 0x010E:
                                featureDescription = Features.Prettify_010E(desc.Data);
                                break;
                            case 0x0110:
                                featureDescription = Features.Prettify_0110(desc.Data);
                                break;
                            case 0x0113:
                                featureDescription = Features.Prettify_0113(desc.Data);
                                break;
                            case 0x0142:
                                featureDescription = Features.Prettify_0142(desc.Data);
                                break;
                            default:
                                featureDescription = "Unknown feature";
                                break;
                        }

                        featuresList.Add(new TreeGridItem {Values = new object[] {featureNumber, featureDescription}});
                    }
                else
                    AaruConsole.DebugWriteLine("Device-Info command",
                        "GET CONFIGURATION returned no feature descriptors");
            }

            Invalidate();
        }

        protected void OnBtnSaveInquiryBinary(object sender, EventArgs e)
        {
            var dlgSaveBinary = new SaveFileDialog();
            dlgSaveBinary.Filters.Add(new FileFilter {Extensions = new[] {"*.bin"}, Name = "Binary"});
            var result = dlgSaveBinary.ShowDialog(this);

            if (result != DialogResult.Ok) return;

            var saveFs = new FileStream(dlgSaveBinary.FileName, FileMode.Create);
            saveFs.Write(inquiryData, 0, inquiryData.Length);

            saveFs.Close();
        }

        protected void OnBtnSaveInquiryText(object sender, EventArgs e)
        {
            var dlgSaveText = new SaveFileDialog();
            dlgSaveText.Filters.Add(new FileFilter {Extensions = new[] {"*.txt"}, Name = "Text"});
            var result = dlgSaveText.ShowDialog(this);

            if (result != DialogResult.Ok) return;

            var saveFs = new FileStream(dlgSaveText.FileName, FileMode.Create);
            var saveSw = new StreamWriter(saveFs);
            saveSw.Write(txtScsiInquiry.Text);
            saveFs.Close();
        }

        protected void OnBtnSaveMode6(object sender, EventArgs e)
        {
            var dlgSaveBinary = new SaveFileDialog();
            dlgSaveBinary.Filters.Add(new FileFilter {Extensions = new[] {"*.bin"}, Name = "Binary"});
            var result = dlgSaveBinary.ShowDialog(this);

            if (result != DialogResult.Ok) return;

            var saveFs = new FileStream(dlgSaveBinary.FileName, FileMode.Create);
            saveFs.Write(modeSense6, 0, modeSense6.Length);

            saveFs.Close();
        }

        protected void OnBtnSaveMode10(object sender, EventArgs e)
        {
            var dlgSaveBinary = new SaveFileDialog();
            dlgSaveBinary.Filters.Add(new FileFilter {Extensions = new[] {"*.bin"}, Name = "Binary"});
            var result = dlgSaveBinary.ShowDialog(this);

            if (result != DialogResult.Ok) return;

            var saveFs = new FileStream(dlgSaveBinary.FileName, FileMode.Create);
            saveFs.Write(modeSense10, 0, modeSense10.Length);

            saveFs.Close();
        }

        protected void OnTreeModePagesSelectedItemChanged(object sender, EventArgs e)
        {
            if (!(treeModeSensePages.SelectedItem is TreeGridItem item)) return;

            txtModeSensePage.Text = item.Values[1] as string;
        }

        protected void OnTreeEvpdPagesSelectedItemChanged(object sender, EventArgs e)
        {
            if (!(treeEvpdPages.SelectedItem is TreeGridItem item)) return;

            txtEvpdPage.Text = item.Values[1] as string;
        }

        protected void OnBtnSaveEvpd(object sender, EventArgs e)
        {
            if (!(treeModeSensePages.SelectedItem is TreeGridItem item)) return;
            if (!(item.Values[2] is byte[] data)) return;

            var dlgSaveBinary = new SaveFileDialog();
            dlgSaveBinary.Filters.Add(new FileFilter {Extensions = new[] {"*.bin"}, Name = "Binary"});
            var result = dlgSaveBinary.ShowDialog(this);

            if (result != DialogResult.Ok) return;

            var saveFs = new FileStream(dlgSaveBinary.FileName, FileMode.Create);
            saveFs.Write(data, 0, data.Length);

            saveFs.Close();
        }

        protected void OnTreeMmcFeaturesSelectedItemChanged(object sender, EventArgs e)
        {
            if (!(treeMmcFeatures.SelectedItem is TreeGridItem item)) return;

            txtMmcFeature.Text = item.Values[1] as string;
        }

        protected void OnBtnSaveMmcFeatures(object sender, EventArgs e)
        {
            var dlgSaveBinary = new SaveFileDialog();
            dlgSaveBinary.Filters.Add(new FileFilter {Extensions = new[] {"*.bin"}, Name = "Binary"});
            var result = dlgSaveBinary.ShowDialog(this);

            if (result != DialogResult.Ok) return;

            var saveFs = new FileStream(dlgSaveBinary.FileName, FileMode.Create);
            saveFs.Write(configuration, 0, configuration.Length);

            saveFs.Close();
        }

        #region XAML controls

#pragma warning disable 169
#pragma warning disable 649
        private TabPage tabScsiInquiry;
        private Label lblScsiInquiry;
        private TextArea txtScsiInquiry;
        private Button btnSaveInquiryBinary;
        private Button btnSaveInquiryText;
        private TabPage tabScsiModeSense;
        private TreeGridView treeModeSensePages;
        private TextArea txtModeSensePage;
        private Button btnSaveMode6;
        private Button btnSaveMode10;
        private TabPage tabScsiEvpd;
        private TreeGridView treeEvpdPages;
        private TextArea txtEvpdPage;
        private Button btnSaveEvpd;
        private TabPage tabMmcFeatures;
        private TreeGridView treeMmcFeatures;
        private TextArea txtMmcFeature;
        private Button btnSaveMmcFeatures;
#pragma warning restore 169
#pragma warning restore 649

        #endregion
    }
}