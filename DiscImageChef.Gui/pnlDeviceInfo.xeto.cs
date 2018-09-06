// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : pnlDeviceInfo.xeto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device information.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the device information panel.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiscImageChef.Console;
using DiscImageChef.Decoders.ATA;
using DiscImageChef.Decoders.PCMCIA;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Decoders.SCSI.MMC;
using DiscImageChef.Decoders.SCSI.SSC;
using DiscImageChef.Devices;
using Eto.Forms;
using Eto.Serialization.Xaml;
using DeviceInfo = DiscImageChef.Core.Devices.Info.DeviceInfo;
using Tuple = DiscImageChef.Decoders.PCMCIA.Tuple;

namespace DiscImageChef.Gui
{
    public class pnlDeviceInfo : Panel
    {
        DeviceInfo devInfo;

        public pnlDeviceInfo(DeviceInfo devInfo)
        {
            XamlReader.Load(this);

            this.devInfo = devInfo;

            txtType.Text         = devInfo.Type.ToString();
            txtManufacturer.Text = devInfo.Manufacturer;
            txtModel.Text        = devInfo.Model;
            txtRevision.Text     = devInfo.Revision;
            txtSerial.Text       = devInfo.Serial;
            txtScsiType.Text     = devInfo.ScsiType.ToString();
            chkRemovable.Checked = devInfo.IsRemovable;
            chkUsb.Checked       = devInfo.IsUsb;

            if(devInfo.IsUsb)
            {
                tabUsb.Visible                = true;
                btnSaveUsbDescriptors.Enabled = devInfo.UsbDescriptors != null;
                txtUsbVendorId.Text           = $"{devInfo.UsbVendorId:X4}";
                txtUsbProductId.Text          = $"{devInfo.UsbProductId:X4}";
                txtUsbManufacturer.Text       = devInfo.UsbManufacturerString;
                txtUsbProduct.Text            = devInfo.UsbProductString;
                txtUsbSerial.Text             = devInfo.UsbSerialString;
            }

            if(devInfo.IsFireWire)
            {
                tabFirewire.Visible          = true;
                txtFirewireVendorId.Text     = $"{devInfo.FireWireVendor:X4}";
                txtFirewireModelId.Text      = $"{devInfo.FireWireModel:X4}";
                txtFirewireManufacturer.Text = devInfo.FireWireVendorName;
                txtFirewireModel.Text        = devInfo.FireWireModelName;
                txtFirewireGuid.Text         = $"{devInfo.FireWireGuid:X16}";
            }

            if(devInfo.IsPcmcia)
            {
                tabPcmcia.Visible = true;

                TreeGridItemCollection cisList = new TreeGridItemCollection();

                treeMmcFeatures.Columns.Add(new GridColumn {HeaderText = "CIS", DataCell = new TextBoxCell(0)});

                treeMmcFeatures.AllowMultipleSelection = false;
                treeMmcFeatures.ShowHeader             = false;
                treeMmcFeatures.DataStore              = cisList;

                Tuple[] tuples = CIS.GetTuples(devInfo.Cis);
                if(tuples != null)
                    foreach(Tuple tuple in tuples)
                    {
                        string tupleCode;
                        string tupleDescription;

                        switch(tuple.Code)
                        {
                            case TupleCodes.CISTPL_NULL:
                            case TupleCodes.CISTPL_END: continue;
                            case TupleCodes.CISTPL_DEVICEGEO:
                            case TupleCodes.CISTPL_DEVICEGEO_A:
                                tupleCode        = "Device Geometry Tuples";
                                tupleDescription = CIS.PrettifyDeviceGeometryTuple(tuple);
                                break;
                            case TupleCodes.CISTPL_MANFID:
                                tupleCode        = "Manufacturer Identification Tuple";
                                tupleDescription = CIS.PrettifyManufacturerIdentificationTuple(tuple);
                                break;
                            case TupleCodes.CISTPL_VERS_1:
                                tupleCode        = "Level 1 Version / Product Information Tuple";
                                tupleDescription = CIS.PrettifyLevel1VersionTuple(tuple);
                                break;
                            case TupleCodes.CISTPL_ALTSTR:
                            case TupleCodes.CISTPL_BAR:
                            case TupleCodes.CISTPL_BATTERY:
                            case TupleCodes.CISTPL_BYTEORDER:
                            case TupleCodes.CISTPL_CFTABLE_ENTRY:
                            case TupleCodes.CISTPL_CFTABLE_ENTRY_CB:
                            case TupleCodes.CISTPL_CHECKSUM:
                            case TupleCodes.CISTPL_CONFIG:
                            case TupleCodes.CISTPL_CONFIG_CB:
                            case TupleCodes.CISTPL_DATE:
                            case TupleCodes.CISTPL_DEVICE:
                            case TupleCodes.CISTPL_DEVICE_A:
                            case TupleCodes.CISTPL_DEVICE_OA:
                            case TupleCodes.CISTPL_DEVICE_OC:
                            case TupleCodes.CISTPL_EXTDEVIC:
                            case TupleCodes.CISTPL_FORMAT:
                            case TupleCodes.CISTPL_FORMAT_A:
                            case TupleCodes.CISTPL_FUNCE:
                            case TupleCodes.CISTPL_FUNCID:
                            case TupleCodes.CISTPL_GEOMETRY:
                            case TupleCodes.CISTPL_INDIRECT:
                            case TupleCodes.CISTPL_JEDEC_A:
                            case TupleCodes.CISTPL_JEDEC_C:
                            case TupleCodes.CISTPL_LINKTARGET:
                            case TupleCodes.CISTPL_LONGLINK_A:
                            case TupleCodes.CISTPL_LONGLINK_C:
                            case TupleCodes.CISTPL_LONGLINK_CB:
                            case TupleCodes.CISTPL_LONGLINK_MFC:
                            case TupleCodes.CISTPL_NO_LINK:
                            case TupleCodes.CISTPL_ORG:
                            case TupleCodes.CISTPL_PWR_MGMNT:
                            case TupleCodes.CISTPL_SPCL:
                            case TupleCodes.CISTPL_SWIL:
                            case TupleCodes.CISTPL_VERS_2:
                                tupleCode        = $"Undecoded tuple ID {tuple.Code}";
                                tupleDescription = $"Undecoded tuple ID {tuple.Code}";
                                break;
                            default:
                                tupleCode        = $"0x{(byte)tuple.Code:X2}";
                                tupleDescription = $"Found unknown tuple ID 0x{(byte)tuple.Code:X2}";
                                break;
                        }

                        cisList.Add(new TreeGridItem {Values = new object[] {tupleCode, tupleDescription}});
                    }
                else DicConsole.DebugWriteLine("Device-Info command", "PCMCIA CIS returned no tuples");
            }

            if(devInfo.AtaIdentify != null || devInfo.AtapiIdentify != null)
            {
                tabAta.Visible = true;

                if(devInfo.AtaIdentify != null)
                {
                    stkAtaMcpt.Visible  = false;
                    chkAtaMcpt.Checked  = devInfo.AtaMcptError.HasValue;
                    lblAtaMcpt.Visible  = devInfo.AtaMcptError.HasValue;
                    lblAtaIdentify.Text = "ATA IDENTIFY DEVICE";

                    if(devInfo.AtaMcptError.HasValue)
                    {
                        switch(devInfo.AtaMcptError.Value.DeviceHead & 0x7)
                        {
                            case 0:
                                lblAtaMcpt.Text = "Device reports incorrect media card type";
                                break;
                            case 1:
                                lblAtaMcpt.Text = "Device contains a Secure Digital card";
                                break;
                            case 2:
                                lblAtaMcpt.Text = "Device contains a MultiMediaCard ";
                                break;
                            case 3:
                                lblAtaMcpt.Text = "Device contains a Secure Digital I/O card";
                                break;
                            case 4:
                                lblAtaMcpt.Text = "Device contains a Smart Media card";
                                break;
                            default:
                                lblAtaMcpt.Text =
                                    $"Device contains unknown media card type {devInfo.AtaMcptError.Value.DeviceHead & 0x07}";
                                break;
                        }

                        chkAtaMcptWriteProtection.Checked = (devInfo.AtaMcptError.Value.DeviceHead & 0x08) == 0x08;

                        ushort specificData = (ushort)(devInfo.AtaMcptError.Value.CylinderHigh * 0x100 +
                                                       devInfo.AtaMcptError.Value.CylinderLow);
                        if(specificData != 0)
                        {
                            lblAtaMcptSpecificData.Visible = true;
                            lblAtaMcptSpecificData.Text    = $"Card specific data: 0x{specificData:X4}";
                        }
                    }

                    tabAta.Text         = "ATA";
                    txtAtaIdentify.Text = Identify.Prettify(this.devInfo.AtaIdentify);
                }
                else if(devInfo.AtapiIdentify != null)
                {
                    lblAtaIdentify.Text = "ATA PACKET IDENTIFY DEVICE";
                    stkAtaMcpt.Visible  = false;
                    tabAta.Text         = "ATAPI";
                    txtAtaIdentify.Text = Identify.Prettify(this.devInfo.AtapiIdentify);
                }
            }

            if(devInfo.ScsiInquiryData != null)
            {
                tabSCSI.Visible     = true;
                txtScsiInquiry.Text = Inquiry.Prettify(devInfo.ScsiInquiry);

                if(devInfo.ScsiMode.HasValue)
                {
                    tabScsiModeSense.Visible = true;

                    TreeGridItemCollection modePagesList = new TreeGridItemCollection();

                    treeModeSensePages.Columns.Add(new GridColumn {HeaderText = "Page", DataCell = new TextBoxCell(0)});

                    treeModeSensePages.AllowMultipleSelection = false;
                    treeModeSensePages.ShowHeader             = false;
                    treeModeSensePages.DataStore              = modePagesList;

                    modePagesList.Add(new TreeGridItem
                    {
                        Values = new object[]
                        {
                            "Header",
                            Modes.PrettifyModeHeader(devInfo.ScsiMode.Value.Header,
                                                     devInfo.ScsiType)
                        }
                    });

                    foreach(Modes.ModePage page in devInfo
                                                  .ScsiMode.Value.Pages.OrderBy(t => t.Page).ThenBy(t => t.Subpage))
                    {
                        string pageNumberText = page.Subpage == 0
                                                    ? $"MODE {page.Page:X2}h"
                                                    : $"MODE {page.Page:X2} Subpage {page.Subpage:X2}";
                        string decodedText;

                        switch(page.Page)
                        {
                            case 0x00:
                            {
                                if(devInfo.ScsiType == PeripheralDeviceTypes.MultiMediaDevice && page.Subpage == 0)
                                    decodedText  = Modes.PrettifyModePage_00_SFF(page.PageResponse);
                                else decodedText = "Undecoded";

                                break;
                            }
                            case 0x01:
                            {
                                if(page.Subpage == 0)
                                    decodedText = devInfo.ScsiType == PeripheralDeviceTypes.MultiMediaDevice
                                                      ? Modes.PrettifyModePage_01_MMC(page.PageResponse)
                                                      : Modes.PrettifyModePage_01(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x02:
                            {
                                if(page.Subpage == 0) decodedText = Modes.PrettifyModePage_02(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x03:
                            {
                                if(page.Subpage == 0) decodedText = Modes.PrettifyModePage_03(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x04:
                            {
                                if(page.Subpage == 0) decodedText = Modes.PrettifyModePage_04(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x05:
                            {
                                if(page.Subpage == 0) decodedText = Modes.PrettifyModePage_05(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x06:
                            {
                                if(page.Subpage == 0) decodedText = Modes.PrettifyModePage_06(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x07:
                            {
                                if(page.Subpage == 0)
                                    decodedText = devInfo.ScsiType == PeripheralDeviceTypes.MultiMediaDevice
                                                      ? Modes.PrettifyModePage_07_MMC(page.PageResponse)
                                                      : Modes.PrettifyModePage_07(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x08:
                            {
                                if(page.Subpage == 0) decodedText = Modes.PrettifyModePage_08(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x0A:
                            {
                                if(page.Subpage      == 0) decodedText = Modes.PrettifyModePage_0A(page.PageResponse);
                                else if(page.Subpage == 1)
                                    decodedText = Modes.PrettifyModePage_0A_S01(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x0B:
                            {
                                if(page.Subpage == 0) decodedText = Modes.PrettifyModePage_0B(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x0D:
                            {
                                if(page.Subpage == 0) decodedText = Modes.PrettifyModePage_0D(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x0E:
                            {
                                if(page.Subpage == 0) decodedText = Modes.PrettifyModePage_0E(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x0F:
                            {
                                if(page.Subpage == 0) decodedText = Modes.PrettifyModePage_0F(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x10:
                            {
                                if(page.Subpage == 0)
                                    decodedText = devInfo.ScsiType == PeripheralDeviceTypes.SequentialAccess
                                                      ? Modes.PrettifyModePage_10_SSC(page.PageResponse)
                                                      : Modes.PrettifyModePage_10(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x11:
                            {
                                if(page.Subpage == 0) decodedText = Modes.PrettifyModePage_11(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x12:
                            case 0x13:
                            case 0x14:
                            {
                                if(page.Subpage == 0) decodedText = Modes.PrettifyModePage_12_13_14(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x1A:
                            {
                                if(page.Subpage      == 0) decodedText = Modes.PrettifyModePage_1A(page.PageResponse);
                                else if(page.Subpage == 1)
                                    decodedText = Modes.PrettifyModePage_1A_S01(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x1B:
                            {
                                if(page.Subpage == 0) decodedText = Modes.PrettifyModePage_1B(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x1C:
                            {
                                if(page.Subpage == 0)
                                    decodedText = devInfo.ScsiType == PeripheralDeviceTypes.MultiMediaDevice
                                                      ? Modes.PrettifyModePage_1C_SFF(page.PageResponse)
                                                      : Modes.PrettifyModePage_1C(page.PageResponse);
                                else if(page.Subpage == 1)
                                    decodedText = Modes.PrettifyModePage_1C_S01(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x1D:
                            {
                                if(page.Subpage == 0) decodedText = Modes.PrettifyModePage_1D(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x21:
                            {
                                if(StringHandlers.CToString(devInfo.ScsiInquiry?.VendorIdentification).Trim() ==
                                   "CERTANCE") decodedText = Modes.PrettifyCertanceModePage_21(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x22:
                            {
                                if(StringHandlers.CToString(devInfo.ScsiInquiry?.VendorIdentification).Trim() ==
                                   "CERTANCE") decodedText = Modes.PrettifyCertanceModePage_22(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x24:
                            {
                                if(StringHandlers.CToString(devInfo.ScsiInquiry?.VendorIdentification).Trim() == "IBM")
                                    decodedText = Modes.PrettifyIBMModePage_24(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x2A:
                            {
                                if(page.Subpage == 0) decodedText = Modes.PrettifyModePage_2A(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x2F:
                            {
                                if(StringHandlers.CToString(devInfo.ScsiInquiry?.VendorIdentification).Trim() == "IBM")
                                    decodedText = Modes.PrettifyIBMModePage_2F(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x30:
                            {
                                if(Modes.IsAppleModePage_30(page.PageResponse))
                                    decodedText = "Drive identifies as Apple OEM drive";
                                else goto default;

                                break;
                            }
                            case 0x3B:
                            {
                                if(StringHandlers.CToString(devInfo.ScsiInquiry?.VendorIdentification).Trim() == "HP")
                                    decodedText = Modes.PrettifyHPModePage_3B(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x3C:
                            {
                                if(StringHandlers.CToString(devInfo.ScsiInquiry?.VendorIdentification).Trim() == "HP")
                                    decodedText = Modes.PrettifyHPModePage_3C(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x3D:
                            {
                                if(StringHandlers.CToString(devInfo.ScsiInquiry?.VendorIdentification).Trim() == "IBM")
                                    decodedText = Modes.PrettifyIBMModePage_3D(page.PageResponse);
                                else if(StringHandlers.CToString(devInfo.ScsiInquiry?.VendorIdentification).Trim() ==
                                        "HP") decodedText = Modes.PrettifyHPModePage_3D(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x3E:
                            {
                                if(StringHandlers.CToString(devInfo.ScsiInquiry?.VendorIdentification).Trim() ==
                                   "FUJITSU") decodedText = Modes.PrettifyFujitsuModePage_3E(page.PageResponse);
                                else if(StringHandlers.CToString(devInfo.ScsiInquiry?.VendorIdentification).Trim() ==
                                        "HP") decodedText = Modes.PrettifyHPModePage_3E(page.PageResponse);
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
                        if(decodedText == null) decodedText = "Error decoding page, please open an issue.";
                        modePagesList.Add(new TreeGridItem {Values = new object[] {pageNumberText, decodedText}});
                    }
                }

                if(devInfo.ScsiEvpdPages != null)
                {
                    tabScsiEvpd.Visible      = true;
                    treeEvpdPages.ShowHeader = false;

                    TreeGridItemCollection evpdPagesList = new TreeGridItemCollection();

                    treeEvpdPages.Columns.Add(new GridColumn {HeaderText = "Page", DataCell = new TextBoxCell(0)});

                    treeEvpdPages.AllowMultipleSelection = false;
                    treeEvpdPages.ShowHeader             = false;
                    treeEvpdPages.DataStore              = evpdPagesList;

                    foreach(KeyValuePair<byte, byte[]> page in devInfo.ScsiEvpdPages.OrderBy(t => t.Key))
                    {
                        string evpdPageTitle   = "";
                        string evpdDecodedPage = "";
                        if(page.Key >= 0x01 && page.Key <= 0x7F)
                        {
                            evpdPageTitle   = $"ASCII Page {page.Key:X2}h";
                            evpdDecodedPage = EVPD.DecodeASCIIPage(page.Value);
                        }
                        else if(page.Key == 0x80)
                        {
                            evpdPageTitle   = "Unit Serial Number";
                            evpdDecodedPage = EVPD.DecodePage80(page.Value);
                        }
                        else if(page.Key == 0x81)
                        {
                            evpdPageTitle   = "SCSI Implemented operating definitions";
                            evpdDecodedPage = EVPD.PrettifyPage_81(page.Value);
                        }
                        else if(page.Key == 0x82)
                        {
                            evpdPageTitle   = "ASCII implemented operating definitions";
                            evpdDecodedPage = EVPD.DecodePage82(page.Value);
                        }
                        else if(page.Key == 0x83)
                        {
                            evpdPageTitle   = "SCSI Device identification";
                            evpdDecodedPage = EVPD.PrettifyPage_83(page.Value);
                        }
                        else if(page.Key == 0x84)
                        {
                            evpdPageTitle   = "SCSI Software Interface Identifiers";
                            evpdDecodedPage = EVPD.PrettifyPage_84(page.Value);
                        }
                        else if(page.Key == 0x85)
                        {
                            evpdPageTitle   = "SCSI Management Network Addresses";
                            evpdDecodedPage = EVPD.PrettifyPage_85(page.Value);
                        }
                        else if(page.Key == 0x86)
                        {
                            evpdPageTitle   = "SCSI Extended INQUIRY Data";
                            evpdDecodedPage = EVPD.PrettifyPage_86(page.Value);
                        }
                        else if(page.Key == 0x89)
                        {
                            evpdPageTitle   = "SCSI to ATA Translation Layer Data";
                            evpdDecodedPage = EVPD.PrettifyPage_89(page.Value);
                        }
                        else if(page.Key == 0xB0)
                        {
                            evpdPageTitle   = "SCSI Sequential-access Device Capabilities";
                            evpdDecodedPage = EVPD.PrettifyPage_B0(page.Value);
                        }
                        else if(page.Key == 0xB1)
                        {
                            evpdPageTitle   = "Manufacturer-assigned Serial Number";
                            evpdDecodedPage = EVPD.DecodePageB1(page.Value);
                        }
                        else if(page.Key == 0xB2)
                        {
                            evpdPageTitle   = "TapeAlert Supported Flags Bitmap";
                            evpdDecodedPage = $"0x{EVPD.DecodePageB2(page.Value):X16}";
                        }
                        else if(page.Key == 0xB3)
                        {
                            evpdPageTitle   = "Automation Device Serial Number";
                            evpdDecodedPage = EVPD.DecodePageB3(page.Value);
                        }
                        else if(page.Key == 0xB4)
                        {
                            evpdPageTitle   = "Data Transfer Device Element Address";
                            evpdDecodedPage = EVPD.DecodePageB4(page.Value);
                        }
                        else if(page.Key == 0xC0 &&
                                StringHandlers.CToString(devInfo.ScsiInquiry.Value.VendorIdentification)
                                              .ToLowerInvariant().Trim() == "quantum")
                        {
                            evpdPageTitle   = "Quantum Firmware Build Information page";
                            evpdDecodedPage = EVPD.PrettifyPage_C0_Quantum(page.Value);
                        }
                        else if(page.Key == 0xC0 &&
                                StringHandlers.CToString(devInfo.ScsiInquiry.Value.VendorIdentification)
                                              .ToLowerInvariant().Trim() == "seagate")
                        {
                            evpdPageTitle   = "Seagate Firmware Numbers page";
                            evpdDecodedPage = EVPD.PrettifyPage_C0_Seagate(page.Value);
                        }
                        else if(page.Key == 0xC0 &&
                                StringHandlers.CToString(devInfo.ScsiInquiry.Value.VendorIdentification)
                                              .ToLowerInvariant().Trim() == "ibm")
                        {
                            evpdPageTitle   = "IBM Drive Component Revision Levels page";
                            evpdDecodedPage = EVPD.PrettifyPage_C0_IBM(page.Value);
                        }
                        else if(page.Key == 0xC1 &&
                                StringHandlers.CToString(devInfo.ScsiInquiry.Value.VendorIdentification)
                                              .ToLowerInvariant().Trim() == "ibm")
                        {
                            evpdPageTitle   = "IBM Drive Serial Numbers page";
                            evpdDecodedPage = EVPD.PrettifyPage_C1_IBM(page.Value);
                        }
                        else if((page.Key == 0xC0 || page.Key == 0xC1) &&
                                StringHandlers.CToString(devInfo.ScsiInquiry.Value.VendorIdentification)
                                              .ToLowerInvariant().Trim() == "certance")
                        {
                            evpdPageTitle   = "Certance Drive Component Revision Levels page";
                            evpdDecodedPage = EVPD.PrettifyPage_C0_C1_Certance(page.Value);
                        }
                        else if((page.Key == 0xC2 || page.Key == 0xC3 || page.Key == 0xC4 || page.Key == 0xC5 ||
                                 page.Key == 0xC6) &&
                                StringHandlers.CToString(devInfo.ScsiInquiry.Value.VendorIdentification)
                                              .ToLowerInvariant().Trim() == "certance")
                        {
                            switch(page.Key)
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
                        else if((page.Key == 0xC0 || page.Key == 0xC1 || page.Key == 0xC2 || page.Key == 0xC3 ||
                                 page.Key == 0xC4 || page.Key == 0xC5) && StringHandlers
                                                                         .CToString(devInfo.ScsiInquiry.Value
                                                                                           .VendorIdentification)
                                                                         .ToLowerInvariant().Trim() == "hp")
                        {
                            switch(page.Key)
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
                        else if(page.Key == 0xDF &&
                                StringHandlers.CToString(devInfo.ScsiInquiry.Value.VendorIdentification)
                                              .ToLowerInvariant().Trim() == "certance")
                        {
                            evpdPageTitle   = "Certance drive status page";
                            evpdDecodedPage = EVPD.PrettifyPage_DF_Certance(page.Value);
                        }
                        else
                        {
                            if(page.Key == 0x00) continue;

                            evpdPageTitle   = $"Page {page.Key:X2}h";
                            evpdDecodedPage = "Undecoded";
                            DicConsole.DebugWriteLine("Device-Info command", "Found undecoded SCSI VPD page 0x{0:X2}",
                                                      page.Key);
                        }

                        evpdPagesList.Add(new TreeGridItem
                        {
                            Values = new object[] {evpdPageTitle, evpdDecodedPage, page.Value}
                        });
                    }
                }

                if(devInfo.MmcConfiguration != null)
                {
                    tabMmcFeatures.Visible = true;

                    TreeGridItemCollection featuresList = new TreeGridItemCollection();

                    treeMmcFeatures.Columns.Add(new GridColumn {HeaderText = "Feature", DataCell = new TextBoxCell(0)});

                    treeMmcFeatures.AllowMultipleSelection = false;
                    treeMmcFeatures.ShowHeader             = false;
                    treeMmcFeatures.DataStore              = featuresList;

                    Features.SeparatedFeatures ftr = Features.Separate(devInfo.MmcConfiguration);

                    DicConsole.DebugWriteLine("Device-Info command", "GET CONFIGURATION length is {0} bytes",
                                              ftr.DataLength);
                    DicConsole.DebugWriteLine("Device-Info command", "GET CONFIGURATION current profile is {0:X4}h",
                                              ftr.CurrentProfile);
                    if(ftr.Descriptors != null)
                        foreach(Features.FeatureDescriptor desc in ftr.Descriptors)
                        {
                            string featureNumber = $"Feature {desc.Code:X4}h";
                            string featureDescription;
                            DicConsole.DebugWriteLine("Device-Info command", "Feature {0:X4}h", desc.Code);

                            switch(desc.Code)
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

                            featuresList.Add(new TreeGridItem
                            {
                                Values = new object[] {featureNumber, featureDescription}
                            });
                        }
                    else
                        DicConsole.DebugWriteLine("Device-Info command",
                                                  "GET CONFIGURATION returned no feature descriptors");
                }

                if(devInfo.PlextorFeatures != null)
                {
                    tabPlextor.Visible = true;
                    if(devInfo.PlextorFeatures.Eeprom != null)
                    {
                        stkPlextorEeprom.Visible  = true;
                        txtPlextorDiscs.Text      = $"{devInfo.PlextorFeatures.Discs}";
                        txtPlextorCdReadTime.Text = TimeSpan.FromSeconds(devInfo.PlextorFeatures.CdReadTime).ToString();
                        txtPlextorCdWriteTime.Text =
                            TimeSpan.FromSeconds(devInfo.PlextorFeatures.CdWriteTime).ToString();
                        if(devInfo.PlextorFeatures.IsDvd)
                        {
                            stkPlextorDvdTimes.Visible = true;
                            txtPlextorDvdReadTime.Text =
                                TimeSpan.FromSeconds(devInfo.PlextorFeatures.DvdReadTime).ToString();
                            txtPlextorDvdWriteTime.Text =
                                TimeSpan.FromSeconds(devInfo.PlextorFeatures.DvdWriteTime).ToString();
                        }
                    }

                    chkPlextorPoweRec.Checked = devInfo.PlextorFeatures.PoweRec;

                    if(devInfo.PlextorFeatures.PoweRec)
                    {
                        chkPlextorPoweRecEnabled.Visible = true;
                        chkPlextorPoweRecEnabled.Checked = devInfo.PlextorFeatures.PoweRecEnabled;

                        if(devInfo.PlextorFeatures.PoweRecEnabled)
                        {
                            stkPlextorPoweRecEnabled.Visible = true;

                            if(devInfo.PlextorFeatures.PoweRecRecommendedSpeed > 0)
                            {
                                stkPlextorPoweRecRecommended.Visible = true;
                                txtPlextorPoweRecRecommended.Text =
                                    $"{devInfo.PlextorFeatures.PoweRecRecommendedSpeed} Kb/sec.";
                            }

                            if(devInfo.PlextorFeatures.PoweRecSelected > 0)
                            {
                                stkPlextorPoweRecSelected.Visible = true;
                                txtPlextorPoweRecSelected.Text =
                                    $"{devInfo.PlextorFeatures.PoweRecSelected} Kb/sec.";
                            }

                            if(devInfo.PlextorFeatures.PoweRecMax > 0)
                            {
                                stkPlextorPoweRecMax.Visible = true;
                                txtPlextorPoweRecMax.Text    = $"{devInfo.PlextorFeatures.PoweRecMax} Kb/sec.";
                            }

                            if(devInfo.PlextorFeatures.PoweRecLast > 0)
                            {
                                stkPlextorPoweRecLast.Visible = true;
                                txtPlextorPoweRecLast.Text    = $"{devInfo.PlextorFeatures.PoweRecLast} Kb/sec.";
                            }
                        }
                    }

                    chkPlextorSilentMode.Checked = devInfo.PlextorFeatures.SilentMode;

                    if(devInfo.PlextorFeatures.SilentMode)
                    {
                        chkPlextorSilentModeEnabled.Visible = true;
                        chkPlextorSilentModeEnabled.Checked = devInfo.PlextorFeatures.SilentModeEnabled;

                        if(devInfo.PlextorFeatures.SilentModeEnabled)
                        {
                            lblPlextorSilentModeAccessTime.Text = devInfo.PlextorFeatures.AccessTimeLimit == 2
                                                                      ? "\tAccess time is slow"
                                                                      : "\tAccess time is fast";

                            txtPlextorSilentModeCdReadSpeedLimit.Text =
                                devInfo.PlextorFeatures.CdReadSpeedLimit > 0
                                    ? $"{devInfo.PlextorFeatures.CdReadSpeedLimit}x"
                                    : "unlimited";

                            txtPlextorSilentModeCdWriteSpeedLimit.Text =
                                devInfo.PlextorFeatures.CdWriteSpeedLimit > 0
                                    ? $"{devInfo.PlextorFeatures.CdReadSpeedLimit}x"
                                    : "unlimited";

                            if(devInfo.PlextorFeatures.IsDvd)
                            {
                                stkPlextorSilentModeDvdReadSpeedLimit.Visible = true;
                                txtPlextorSilentModeDvdReadSpeedLimit.Text =
                                    devInfo.PlextorFeatures.DvdReadSpeedLimit > 0
                                        ? $"{devInfo.PlextorFeatures.DvdReadSpeedLimit}x"
                                        : "unlimited";
                            }
                        }
                    }

                    chkPlextorGigaRec.Checked   = devInfo.PlextorFeatures.GigaRec;
                    chkPlextorSecuRec.Checked   = devInfo.PlextorFeatures.SecuRec;
                    chkPlextorSpeedRead.Checked = devInfo.PlextorFeatures.SpeedRead;

                    if(devInfo.PlextorFeatures.SpeedRead)
                    {
                        chkPlextorSpeedEnabled.Visible = true;
                        chkPlextorSpeedEnabled.Checked = devInfo.PlextorFeatures.SpeedReadEnabled;
                    }

                    chkPlextorHiding.Checked = devInfo.PlextorFeatures.Hiding;
                    if(devInfo.PlextorFeatures.Hiding)
                    {
                        stkPlextorHiding.Visible           = true;
                        chkPlextorHidesRecordables.Checked = devInfo.PlextorFeatures.HidesRecordables;
                        chkPlextorHidesSessions.Checked    = devInfo.PlextorFeatures.HidesSessions;
                    }

                    chkPlextorVariRec.Checked = devInfo.PlextorFeatures.VariRec;

                    if(devInfo.PlextorFeatures.IsDvd)
                    {
                        stkPlextorDvd.Visible              = true;
                        chkPlextorVariRecDvd.Checked       = devInfo.PlextorFeatures.VariRecDvd;
                        chkPlextorBitSetting.Checked       = devInfo.PlextorFeatures.BitSetting;
                        chkPlextorBitSettingDl.Checked     = devInfo.PlextorFeatures.BitSettingDl;
                        chkPlextorDvdPlusWriteTest.Checked = devInfo.PlextorFeatures.DvdPlusWriteTest;
                    }
                }

                if(devInfo.ScsiInquiry.Value.KreonPresent)
                {
                    tabKreon.Visible                  = true;
                    chkKreonChallengeResponse.Checked = devInfo.KreonFeatures.HasFlag(KreonFeatures.ChallengeResponse);
                    chkKreonDecryptSs.Checked         = devInfo.KreonFeatures.HasFlag(KreonFeatures.DecryptSs);
                    chkKreonXtremeUnlock.Checked      = devInfo.KreonFeatures.HasFlag(KreonFeatures.XtremeUnlock);
                    chkKreonWxripperUnlock.Checked    = devInfo.KreonFeatures.HasFlag(KreonFeatures.WxripperUnlock);
                    chkKreonChallengeResponse360.Checked =
                        devInfo.KreonFeatures.HasFlag(KreonFeatures.ChallengeResponse360);
                    chkKreonDecryptSs360.Checked      = devInfo.KreonFeatures.HasFlag(KreonFeatures.DecryptSs360);
                    chkKreonXtremeUnlock360.Checked   = devInfo.KreonFeatures.HasFlag(KreonFeatures.XtremeUnlock360);
                    chkKreonWxripperUnlock360.Checked = devInfo.KreonFeatures.HasFlag(KreonFeatures.WxripperUnlock360);
                    chkKreonLock.Checked              = devInfo.KreonFeatures.HasFlag(KreonFeatures.Lock);
                    chkKreonErrorSkipping.Checked     = devInfo.KreonFeatures.HasFlag(KreonFeatures.ErrorSkipping);
                }

                if(devInfo.BlockLimits != null)
                {
                    BlockLimits.BlockLimitsData? blockLimits = BlockLimits.Decode(devInfo.BlockLimits);

                    if(blockLimits.HasValue)
                    {
                        tabSsc.Visible = true;
                        if(blockLimits.Value.minBlockLen == blockLimits.Value.maxBlockLen)
                        {
                            lblMinBlockSize.Visible = true;
                            lblMinBlockSize.Text =
                                $"Device's block size is fixed at {blockLimits.Value.minBlockLen} bytes";
                        }
                        else
                        {
                            lblMinBlockSize.Visible = true;
                            lblMaxBlockSize.Visible = true;

                            lblMaxBlockSize.Text = blockLimits.Value.maxBlockLen > 0
                                                       ? $"Device's maximum block size is {blockLimits.Value.maxBlockLen} bytes"
                                                       : "Device does not specify a maximum block size";
                            lblMinBlockSize.Text =
                                $"Device's minimum block size is {blockLimits.Value.minBlockLen} bytes";

                            if(blockLimits.Value.granularity > 0)
                            {
                                lblBlockSizeGranularity.Visible = true;
                                lblBlockSizeGranularity.Text =
                                    $"Device's needs a block size granularity of 2^{blockLimits.Value.granularity} ({Math.Pow(2, blockLimits.Value.granularity)}) bytes";
                            }
                        }
                    }
                }

                if(devInfo.DensitySupport != null)
                    if(devInfo.DensitySupportHeader.HasValue)
                    {
                        stkDensities.Visible = true;
                        txtDensities.Text    = DensitySupport.PrettifyDensity(devInfo.DensitySupportHeader);
                    }

                if(devInfo.MediumDensitySupport != null)
                {
                    if(devInfo.MediaTypeSupportHeader.HasValue)
                    {
                        stkMediaTypes.Visible = true;
                        txtMediumTypes.Text   = DensitySupport.PrettifyMediumType(devInfo.MediaTypeSupportHeader);
                    }

                    txtMediumDensity.Visible = true;
                    txtMediumDensity.Text    = DensitySupport.PrettifyMediumType(devInfo.MediumDensitySupport);
                }
            }

            switch(devInfo.Type)
            {
                case DeviceType.MMC:
                {
                    tabSecureDigital.Text = "MultiMediaCard";
                    if(devInfo.CID != null)
                    {
                        tabCid.Visible = true;
                        txtCid.Text    = Decoders.MMC.Decoders.PrettifyCID(devInfo.CID);
                    }

                    if(devInfo.CSD != null)
                    {
                        tabCsd.Visible = true;
                        txtCid.Text    = Decoders.MMC.Decoders.PrettifyCSD(devInfo.CSD);
                    }

                    if(devInfo.OCR != null)
                    {
                        tabOcr.Visible = true;
                        txtCid.Text    = Decoders.MMC.Decoders.PrettifyOCR(devInfo.OCR);
                    }

                    if(devInfo.ExtendedCSD != null)
                    {
                        tabExtendedCsd.Visible = true;
                        txtCid.Text            = Decoders.MMC.Decoders.PrettifyExtendedCSD(devInfo.ExtendedCSD);
                    }
                }
                    break;
                case DeviceType.SecureDigital:
                {
                    tabSecureDigital.Text = "SecureDigital";
                    if(devInfo.CID != null)
                    {
                        tabCid.Visible = true;

                        txtCid.Text = Decoders.SecureDigital.Decoders.PrettifyCID(devInfo.CID);
                    }

                    if(devInfo.CSD != null)
                    {
                        tabCsd.Visible = true;

                        txtCid.Text = Decoders.SecureDigital.Decoders.PrettifyCSD(devInfo.CSD);
                    }

                    if(devInfo.OCR != null)
                    {
                        tabOcr.Visible = true;
                        txtCid.Text    = Decoders.SecureDigital.Decoders.PrettifyOCR(devInfo.OCR);
                    }

                    if(devInfo.SCR != null)
                    {
                        tabScr.Visible = true;
                        txtCid.Text    = Decoders.SecureDigital.Decoders.PrettifySCR(devInfo.SCR);
                    }
                }
                    break;
            }

            tabSecureDigital.Visible = tabCid.Visible || tabCsd.Visible || tabOcr.Visible || tabExtendedCsd.Visible ||
                                       tabScr.Visible;
        }

        protected void OnBtnSaveAtaBinary(object sender, EventArgs e)
        {
            SaveFileDialog dlgSaveBinary = new SaveFileDialog();
            dlgSaveBinary.Filters.Add(new FileFilter {Extensions = new[] {"*.bin"}, Name = "Binary"});
            DialogResult result = dlgSaveBinary.ShowDialog(this);

            if(result != DialogResult.Ok) return;

            FileStream saveFs = new FileStream(dlgSaveBinary.FileName, FileMode.Create);
            if(devInfo.AtaIdentify        != null) saveFs.Write(devInfo.AtaIdentify,   0, devInfo.AtaIdentify.Length);
            else if(devInfo.AtapiIdentify != null) saveFs.Write(devInfo.AtapiIdentify, 0, devInfo.AtapiIdentify.Length);

            saveFs.Close();
        }

        protected void OnBtnSaveAtaText(object sender, EventArgs e)
        {
            SaveFileDialog dlgSaveText = new SaveFileDialog();
            dlgSaveText.Filters.Add(new FileFilter {Extensions = new[] {"*.txt"}, Name = "Text"});
            DialogResult result = dlgSaveText.ShowDialog(this);

            if(result != DialogResult.Ok) return;

            FileStream   saveFs = new FileStream(dlgSaveText.FileName, FileMode.Create);
            StreamWriter saveSw = new StreamWriter(saveFs);
            saveSw.Write(txtAtaIdentify.Text);
            saveFs.Close();
        }

        protected void OnBtnSaveInquiryBinary(object sender, EventArgs e)
        {
            SaveFileDialog dlgSaveBinary = new SaveFileDialog();
            dlgSaveBinary.Filters.Add(new FileFilter {Extensions = new[] {"*.bin"}, Name = "Binary"});
            DialogResult result = dlgSaveBinary.ShowDialog(this);

            if(result != DialogResult.Ok) return;

            FileStream saveFs = new FileStream(dlgSaveBinary.FileName, FileMode.Create);
            saveFs.Write(devInfo.ScsiInquiryData, 0, devInfo.ScsiInquiryData.Length);

            saveFs.Close();
        }

        protected void OnBtnSaveInquiryText(object sender, EventArgs e)
        {
            SaveFileDialog dlgSaveText = new SaveFileDialog();
            dlgSaveText.Filters.Add(new FileFilter {Extensions = new[] {"*.txt"}, Name = "Text"});
            DialogResult result = dlgSaveText.ShowDialog(this);

            if(result != DialogResult.Ok) return;

            FileStream   saveFs = new FileStream(dlgSaveText.FileName, FileMode.Create);
            StreamWriter saveSw = new StreamWriter(saveFs);
            saveSw.Write(txtScsiInquiry.Text);
            saveFs.Close();
        }

        protected void OnBtnSaveMode6(object sender, EventArgs e)
        {
            SaveFileDialog dlgSaveBinary = new SaveFileDialog();
            dlgSaveBinary.Filters.Add(new FileFilter {Extensions = new[] {"*.bin"}, Name = "Binary"});
            DialogResult result = dlgSaveBinary.ShowDialog(this);

            if(result != DialogResult.Ok) return;

            FileStream saveFs = new FileStream(dlgSaveBinary.FileName, FileMode.Create);
            saveFs.Write(devInfo.ScsiModeSense6, 0, devInfo.ScsiModeSense6.Length);

            saveFs.Close();
        }

        protected void OnBtnSaveMode10(object sender, EventArgs e)
        {
            SaveFileDialog dlgSaveBinary = new SaveFileDialog();
            dlgSaveBinary.Filters.Add(new FileFilter {Extensions = new[] {"*.bin"}, Name = "Binary"});
            DialogResult result = dlgSaveBinary.ShowDialog(this);

            if(result != DialogResult.Ok) return;

            FileStream saveFs = new FileStream(dlgSaveBinary.FileName, FileMode.Create);
            saveFs.Write(devInfo.ScsiModeSense10, 0, devInfo.ScsiModeSense10.Length);

            saveFs.Close();
        }

        protected void OnTreeModePagesSelectedItemChanged(object sender, EventArgs e)
        {
            if(!(treeModeSensePages.SelectedItem is TreeGridItem item)) return;

            txtModeSensePage.Text = item.Values[1] as string;
        }

        protected void OnTreeEvpdPagesSelectedItemChanged(object sender, EventArgs e)
        {
            if(!(treeEvpdPages.SelectedItem is TreeGridItem item)) return;

            txtEvpdPage.Text = item.Values[1] as string;
        }

        protected void OnBtnSaveEvpd(object sender, EventArgs e)
        {
            if(!(treeModeSensePages.SelectedItem is TreeGridItem item)) return;
            if(!(item.Values[2] is byte[] data)) return;

            SaveFileDialog dlgSaveBinary = new SaveFileDialog();
            dlgSaveBinary.Filters.Add(new FileFilter {Extensions = new[] {"*.bin"}, Name = "Binary"});
            DialogResult result = dlgSaveBinary.ShowDialog(this);

            if(result != DialogResult.Ok) return;

            FileStream saveFs = new FileStream(dlgSaveBinary.FileName, FileMode.Create);
            saveFs.Write(data, 0, data.Length);

            saveFs.Close();
        }

        protected void OnTreeMmcFeaturesSelectedItemChanged(object sender, EventArgs e)
        {
            if(!(treeMmcFeatures.SelectedItem is TreeGridItem item)) return;

            txtMmcFeature.Text = item.Values[1] as string;
        }

        protected void OnBtnSaveMmcFeatures(object sender, EventArgs e)
        {
            SaveFileDialog dlgSaveBinary = new SaveFileDialog();
            dlgSaveBinary.Filters.Add(new FileFilter {Extensions = new[] {"*.bin"}, Name = "Binary"});
            DialogResult result = dlgSaveBinary.ShowDialog(this);

            if(result != DialogResult.Ok) return;

            FileStream saveFs = new FileStream(dlgSaveBinary.FileName, FileMode.Create);
            saveFs.Write(devInfo.MmcConfiguration, 0, devInfo.MmcConfiguration.Length);

            saveFs.Close();
        }

        protected void OnBtnSaveUsbDescriptors(object sender, EventArgs e)
        {
            SaveFileDialog dlgSaveBinary = new SaveFileDialog();
            dlgSaveBinary.Filters.Add(new FileFilter {Extensions = new[] {"*.bin"}, Name = "Binary"});
            DialogResult result = dlgSaveBinary.ShowDialog(this);

            if(result != DialogResult.Ok) return;

            FileStream saveFs = new FileStream(dlgSaveBinary.FileName, FileMode.Create);
            saveFs.Write(devInfo.UsbDescriptors, 0, devInfo.UsbDescriptors.Length);

            saveFs.Close();
        }

        protected void OnTreePcmciaSelectedItemChanged(object sender, EventArgs e)
        {
            if(!(treePcmcia.SelectedItem is TreeGridItem item)) return;

            txtPcmciaCis.Text = item.Values[1] as string;
        }

        protected void OnBtnSavePcmciaCis(object sender, EventArgs e)
        {
            SaveFileDialog dlgSaveBinary = new SaveFileDialog();
            dlgSaveBinary.Filters.Add(new FileFilter {Extensions = new[] {"*.bin"}, Name = "Binary"});
            DialogResult result = dlgSaveBinary.ShowDialog(this);

            if(result != DialogResult.Ok) return;

            FileStream saveFs = new FileStream(dlgSaveBinary.FileName, FileMode.Create);
            saveFs.Write(devInfo.Cis, 0, devInfo.Cis.Length);

            saveFs.Close();
        }

        #region XAML controls
        #pragma warning disable 169
        #pragma warning disable 649
        Label        lblDeviceInfo;
        TabControl   tabInfos;
        TabPage      tabGeneral;
        Label        lblType;
        TextBox      txtType;
        Label        lblManufacturer;
        TextBox      txtManufacturer;
        Label        lblModel;
        TextBox      txtModel;
        Label        lblRevision;
        TextBox      txtRevision;
        Label        lblSerial;
        TextBox      txtSerial;
        Label        lblScsiType;
        TextBox      txtScsiType;
        CheckBox     chkRemovable;
        CheckBox     chkUsb;
        TabPage      tabAta;
        Label        lblAtaIdentify;
        TextArea     txtAtaIdentify;
        Button       btnSaveAtaBinary;
        Button       btnSaveAtaText;
        StackLayout  stkAtaMcpt;
        CheckBox     chkAtaMcpt;
        Label        lblAtaMcpt;
        CheckBox     chkAtaMcptWriteProtection;
        Label        lblAtaMcptSpecificData;
        TabPage      tabSCSI;
        TabPage      tabScsiInquiry;
        Label        lblScsiInquiry;
        TextArea     txtScsiInquiry;
        Button       btnSaveInquiryBinary;
        Button       btnSaveInquiryText;
        TabPage      tabScsiModeSense;
        TreeGridView treeModeSensePages;
        TextArea     txtModeSensePage;
        Button       btnSaveMode6;
        Button       btnSaveMode10;
        TabPage      tabScsiEvpd;
        TreeGridView treeEvpdPages;
        TextArea     txtEvpdPage;
        Button       btnSaveEvpd;
        TabPage      tabMmcFeatures;
        TreeGridView treeMmcFeatures;
        TextArea     txtMmcFeature;
        Button       btnSaveMmcFeatures;
        TabPage      tabKreon;
        CheckBox     chkKreonChallengeResponse;
        CheckBox     chkKreonDecryptSs;
        CheckBox     chkKreonXtremeUnlock;
        CheckBox     chkKreonWxripperUnlock;
        CheckBox     chkKreonChallengeResponse360;
        CheckBox     chkKreonDecryptSs360;
        CheckBox     chkKreonXtremeUnlock360;
        CheckBox     chkKreonWxripperUnlock360;
        CheckBox     chkKreonLock;
        CheckBox     chkKreonErrorSkipping;
        TabPage      tabPlextor;
        StackLayout  stkPlextorEeprom;
        Label        lblPlextorDiscs;
        TextBox      txtPlextorDiscs;
        Label        lblPlextorCdReadTime;
        TextBox      txtPlextorCdReadTime;
        Label        lblPlextorCdWriteTime;
        TextBox      txtPlextorCdWriteTime;
        StackLayout  stkPlextorDvdTimes;
        Label        lblPlextorDvdReadTime;
        TextBox      txtPlextorDvdReadTime;
        Label        lblPlextorDvdWriteTime;
        TextBox      txtPlextorDvdWriteTime;
        CheckBox     chkPlextorPoweRec;
        CheckBox     chkPlextorPoweRecEnabled;
        StackLayout  stkPlextorPoweRecEnabled;
        StackLayout  stkPlextorPoweRecRecommended;
        Label        lblPlextorPoweRecRecommended;
        TextBox      txtPlextorPoweRecRecommended;
        StackLayout  stkPlextorPoweRecSelected;
        Label        lblPlextorPoweRecSelected;
        TextBox      txtPlextorPoweRecSelected;
        StackLayout  stkPlextorPoweRecMax;
        Label        lblPlextorPoweRecMax;
        TextBox      txtPlextorPoweRecMax;
        StackLayout  stkPlextorPoweRecLast;
        Label        lblPlextorPoweRecLast;
        TextBox      txtPlextorPoweRecLast;
        CheckBox     chkPlextorSilentMode;
        CheckBox     chkPlextorSilentModeEnabled;
        StackLayout  stkPlextorSilentModeEnabled;
        Label        lblPlextorSilentModeAccessTime;
        Label        lblPlextorSilentModeCdReadSpeedLimit;
        TextBox      txtPlextorSilentModeCdReadSpeedLimit;
        StackLayout  stkPlextorSilentModeDvdReadSpeedLimit;
        Label        lblPlextorSilentModeDvdReadSpeedLimit;
        TextBox      txtPlextorSilentModeDvdReadSpeedLimit;
        Label        lblPlextorSilentModeCdWriteSpeedLimit;
        TextBox      txtPlextorSilentModeCdWriteSpeedLimit;
        CheckBox     chkPlextorGigaRec;
        CheckBox     chkPlextorSecuRec;
        CheckBox     chkPlextorSpeedRead;
        CheckBox     chkPlextorSpeedEnabled;
        CheckBox     chkPlextorHiding;
        StackLayout  stkPlextorHiding;
        CheckBox     chkPlextorHidesRecordables;
        CheckBox     chkPlextorHidesSessions;
        CheckBox     chkPlextorVariRec;
        StackLayout  stkPlextorDvd;
        CheckBox     chkPlextorVariRecDvd;
        CheckBox     chkPlextorBitSetting;
        CheckBox     chkPlextorBitSettingDl;
        CheckBox     chkPlextorDvdPlusWriteTest;
        TabPage      tabSsc;
        StackLayout  Vertical;
        Label        lblMinBlockSize;
        Label        lblMaxBlockSize;
        Label        lblBlockSizeGranularity;
        StackLayout  stkDensities;
        Label        lblDensities;
        TextArea     txtDensities;
        StackLayout  stkMediaTypes;
        Label        lblMediumTypes;
        TextArea     txtMediumTypes;
        TextArea     txtMediumDensity;
        TabPage      tabSecureDigital;
        TabPage      tabCid;
        TextArea     txtCid;
        TabPage      tabCsd;
        TextArea     txtCsd;
        TabPage      tabOcr;
        TextArea     txtOcr;
        TabPage      tabExtendedCsd;
        TextArea     txtExtendedCsd;
        TabPage      tabScr;
        TextArea     txtScr;
        TabPage      tabUsb;
        Label        lblUsbVendorId;
        TextBox      txtUsbVendorId;
        Label        lblUsbProductId;
        TextBox      txtUsbProductId;
        Label        lblUsbManufacturer;
        TextBox      txtUsbManufacturer;
        Label        lblUsbProduct;
        TextBox      txtUsbProduct;
        Label        lblUsbSerial;
        TextBox      txtUsbSerial;
        Button       btnSaveUsbDescriptors;
        TabPage      tabFirewire;
        Label        lblFirewireVendorId;
        TextBox      txtFirewireVendorId;
        Label        lblFirewireModelId;
        TextBox      txtFirewireModelId;
        Label        lblFirewireManufacturer;
        TextBox      txtFirewireManufacturer;
        Label        lblFirewireModel;
        TextBox      txtFirewireModel;
        Label        lblFirewireGuid;
        TextBox      txtFirewireGuid;
        TabPage      tabPcmcia;
        TreeGridView treePcmcia;
        TextArea     txtPcmciaCis;
        Button       btnSavePcmciaCis;
        #pragma warning restore 169
        #pragma warning restore 649
        #endregion
    }
}