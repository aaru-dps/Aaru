// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ScsiInfoViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model and code for the SCSI information tab.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Gui.ViewModels.Tabs;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Console;
using Aaru.Decoders.SCSI;
using Aaru.Decoders.SCSI.MMC;
using Aaru.Gui.Models;
using Aaru.Helpers;
using Avalonia.Controls;
using ReactiveUI;
using Inquiry = Aaru.CommonTypes.Structs.Devices.SCSI.Inquiry;

public sealed class ScsiInfoViewModel : ViewModelBase
{
    readonly byte[] _configuration;
    readonly byte[] _scsiModeSense10;
    readonly byte[] _scsiModeSense6;
    readonly Window _view;
    string          _evpdPageText;
    string          _mmcFeatureText;
    string          _scsiModeSensePageText;
    object          _selectedEvpdPage;
    object          _selectedMmcFeature;
    object          _selectedModeSensePage;

    public ScsiInfoViewModel(byte[] scsiInquiryData, Inquiry? scsiInquiry, Dictionary<byte, byte[]> scsiEvpdPages,
                             Modes.DecodedMode? scsiMode, PeripheralDeviceTypes scsiType, byte[] scsiModeSense6,
                             byte[] scsiModeSense10, byte[] mmcConfiguration, Window view)
    {
        InquiryData              = scsiInquiryData;
        _scsiModeSense6          = scsiModeSense6;
        _scsiModeSense10         = scsiModeSense10;
        _configuration           = mmcConfiguration;
        _view                    = view;
        ModeSensePages           = new ObservableCollection<ScsiPageModel>();
        EvpdPages                = new ObservableCollection<ScsiPageModel>();
        MmcFeatures              = new ObservableCollection<ScsiPageModel>();
        SaveInquiryBinaryCommand = ReactiveCommand.Create(ExecuteSaveInquiryBinaryCommand);
        SaveInquiryTextCommand   = ReactiveCommand.Create(ExecuteSaveInquiryTextCommand);
        SaveModeSense6Command    = ReactiveCommand.Create(ExecuteSaveModeSense6Command);
        SaveModeSense10Command   = ReactiveCommand.Create(ExecuteSaveModeSense10Command);
        SaveEvpdPageCommand      = ReactiveCommand.Create(ExecuteSaveEvpdPageCommand);
        SaveMmcFeaturesCommand   = ReactiveCommand.Create(ExecuteSaveMmcFeaturesCommand);

        if(InquiryData == null ||
           !scsiInquiry.HasValue)
            return;

        ScsiInquiryText = Decoders.SCSI.Inquiry.Prettify(scsiInquiry);

        if(scsiMode.HasValue)
        {
            ModeSensePages.Add(new ScsiPageModel
            {
                Page        = "Header",
                Description = Modes.PrettifyModeHeader(scsiMode.Value.Header, scsiType)
            });

            if(scsiMode.Value.Pages != null)
                foreach(Modes.ModePage page in scsiMode.Value.Pages.OrderBy(t => t.Page).ThenBy(t => t.Subpage))
                {
                    string pageNumberText = page.Subpage == 0 ? $"MODE {page.Page:X2}h"
                                                : $"MODE {page.Page:X2} Subpage {page.Subpage:X2}";

                    string decodedText;

                    switch(page.Page)
                    {
                        case 0x00:
                        {
                            if(scsiType     == PeripheralDeviceTypes.MultiMediaDevice &&
                               page.Subpage == 0)
                                decodedText = Modes.PrettifyModePage_00_SFF(page.PageResponse);
                            else
                                decodedText = "Undecoded";

                            break;
                        }
                        case 0x01:
                        {
                            if(page.Subpage == 0)
                                decodedText = scsiType == PeripheralDeviceTypes.MultiMediaDevice
                                                  ? Modes.PrettifyModePage_01_MMC(page.PageResponse)
                                                  : Modes.PrettifyModePage_01(page.PageResponse);
                            else
                                goto default;

                            break;
                        }
                        case 0x02:
                        {
                            if(page.Subpage == 0)
                                decodedText = Modes.PrettifyModePage_02(page.PageResponse);
                            else
                                goto default;

                            break;
                        }
                        case 0x03:
                        {
                            if(page.Subpage == 0)
                                decodedText = Modes.PrettifyModePage_03(page.PageResponse);
                            else
                                goto default;

                            break;
                        }
                        case 0x04:
                        {
                            if(page.Subpage == 0)
                                decodedText = Modes.PrettifyModePage_04(page.PageResponse);
                            else
                                goto default;

                            break;
                        }
                        case 0x05:
                        {
                            if(page.Subpage == 0)
                                decodedText = Modes.PrettifyModePage_05(page.PageResponse);
                            else
                                goto default;

                            break;
                        }
                        case 0x06:
                        {
                            if(page.Subpage == 0)
                                decodedText = Modes.PrettifyModePage_06(page.PageResponse);
                            else
                                goto default;

                            break;
                        }
                        case 0x07:
                        {
                            if(page.Subpage == 0)
                                decodedText = scsiType == PeripheralDeviceTypes.MultiMediaDevice
                                                  ? Modes.PrettifyModePage_07_MMC(page.PageResponse)
                                                  : Modes.PrettifyModePage_07(page.PageResponse);
                            else
                                goto default;

                            break;
                        }
                        case 0x08:
                        {
                            if(page.Subpage == 0)
                                decodedText = Modes.PrettifyModePage_08(page.PageResponse);
                            else
                                goto default;

                            break;
                        }
                        case 0x0A:
                        {
                            if(page.Subpage == 0)
                                decodedText = Modes.PrettifyModePage_0A(page.PageResponse);
                            else if(page.Subpage == 1)
                                decodedText = Modes.PrettifyModePage_0A_S01(page.PageResponse);
                            else
                                goto default;

                            break;
                        }
                        case 0x0B:
                        {
                            if(page.Subpage == 0)
                                decodedText = Modes.PrettifyModePage_0B(page.PageResponse);
                            else
                                goto default;

                            break;
                        }
                        case 0x0D:
                        {
                            if(page.Subpage == 0)
                                decodedText = Modes.PrettifyModePage_0D(page.PageResponse);
                            else
                                goto default;

                            break;
                        }
                        case 0x0E:
                        {
                            if(page.Subpage == 0)
                                decodedText = Modes.PrettifyModePage_0E(page.PageResponse);
                            else
                                goto default;

                            break;
                        }
                        case 0x0F:
                        {
                            if(page.Subpage == 0)
                                decodedText = Modes.PrettifyModePage_0F(page.PageResponse);
                            else
                                goto default;

                            break;
                        }
                        case 0x10:
                        {
                            if(page.Subpage == 0)
                                decodedText = scsiType == PeripheralDeviceTypes.SequentialAccess
                                                  ? Modes.PrettifyModePage_10_SSC(page.PageResponse)
                                                  : Modes.PrettifyModePage_10(page.PageResponse);
                            else
                                goto default;

                            break;
                        }
                        case 0x11:
                        {
                            if(page.Subpage == 0)
                                decodedText = Modes.PrettifyModePage_11(page.PageResponse);
                            else
                                goto default;

                            break;
                        }
                        case 0x12:
                        case 0x13:
                        case 0x14:
                        {
                            if(page.Subpage == 0)
                                decodedText = Modes.PrettifyModePage_12_13_14(page.PageResponse);
                            else
                                goto default;

                            break;
                        }
                        case 0x1A:
                        {
                            if(page.Subpage == 0)
                                decodedText = Modes.PrettifyModePage_1A(page.PageResponse);
                            else if(page.Subpage == 1)
                                decodedText = Modes.PrettifyModePage_1A_S01(page.PageResponse);
                            else
                                goto default;

                            break;
                        }
                        case 0x1B:
                        {
                            if(page.Subpage == 0)
                                decodedText = Modes.PrettifyModePage_1B(page.PageResponse);
                            else
                                goto default;

                            break;
                        }
                        case 0x1C:
                        {
                            if(page.Subpage == 0)
                                decodedText = scsiType == PeripheralDeviceTypes.MultiMediaDevice
                                                  ? Modes.PrettifyModePage_1C_SFF(page.PageResponse)
                                                  : Modes.PrettifyModePage_1C(page.PageResponse);
                            else if(page.Subpage == 1)
                                decodedText = Modes.PrettifyModePage_1C_S01(page.PageResponse);
                            else
                                goto default;

                            break;
                        }
                        case 0x1D:
                        {
                            if(page.Subpage == 0)
                                decodedText = Modes.PrettifyModePage_1D(page.PageResponse);
                            else
                                goto default;

                            break;
                        }
                        case 0x21:
                        {
                            if(StringHandlers.CToString(scsiInquiry.Value.VendorIdentification).Trim() == "CERTANCE")
                                decodedText = Modes.PrettifyCertanceModePage_21(page.PageResponse);
                            else
                                goto default;

                            break;
                        }
                        case 0x22:
                        {
                            if(StringHandlers.CToString(scsiInquiry.Value.VendorIdentification).Trim() == "CERTANCE")
                                decodedText = Modes.PrettifyCertanceModePage_22(page.PageResponse);
                            else
                                goto default;

                            break;
                        }
                        case 0x24:
                        {
                            if(StringHandlers.CToString(scsiInquiry.Value.VendorIdentification).Trim() == "IBM")
                                decodedText = Modes.PrettifyIBMModePage_24(page.PageResponse);
                            else
                                goto default;

                            break;
                        }
                        case 0x2A:
                        {
                            if(page.Subpage == 0)
                                decodedText = Modes.PrettifyModePage_2A(page.PageResponse);
                            else
                                goto default;

                            break;
                        }
                        case 0x2F:
                        {
                            if(StringHandlers.CToString(scsiInquiry.Value.VendorIdentification).Trim() == "IBM")
                                decodedText = Modes.PrettifyIBMModePage_2F(page.PageResponse);
                            else
                                goto default;

                            break;
                        }
                        case 0x30:
                        {
                            if(Modes.IsAppleModePage_30(page.PageResponse))
                                decodedText = "Drive identifies as Apple OEM drive";
                            else
                                goto default;

                            break;
                        }
                        case 0x3B:
                        {
                            if(StringHandlers.CToString(scsiInquiry.Value.VendorIdentification).Trim() == "HP")
                                decodedText = Modes.PrettifyHPModePage_3B(page.PageResponse);
                            else
                                goto default;

                            break;
                        }
                        case 0x3C:
                        {
                            if(StringHandlers.CToString(scsiInquiry.Value.VendorIdentification).Trim() == "HP")
                                decodedText = Modes.PrettifyHPModePage_3C(page.PageResponse);
                            else
                                goto default;

                            break;
                        }
                        case 0x3D:
                        {
                            if(StringHandlers.CToString(scsiInquiry.Value.VendorIdentification).Trim() == "IBM")
                                decodedText = Modes.PrettifyIBMModePage_3D(page.PageResponse);
                            else if(StringHandlers.CToString(scsiInquiry.Value.VendorIdentification).Trim() == "HP")
                                decodedText = Modes.PrettifyHPModePage_3D(page.PageResponse);
                            else
                                goto default;

                            break;
                        }
                        case 0x3E:
                        {
                            if(StringHandlers.CToString(scsiInquiry.Value.VendorIdentification).Trim() == "FUJITSU")
                                decodedText = Modes.PrettifyFujitsuModePage_3E(page.PageResponse);
                            else if(StringHandlers.CToString(scsiInquiry.Value.VendorIdentification).Trim() == "HP")
                                decodedText = Modes.PrettifyHPModePage_3E(page.PageResponse);
                            else
                                goto default;

                            break;
                        }
                        default:
                        {
                            decodedText = "Undecoded";

                            break;
                        }
                    }

                    // TODO: Automatic error reporting
                    if(decodedText == null)
                        decodedText = "Error decoding page, please open an issue.";

                    ModeSensePages.Add(new ScsiPageModel
                    {
                        Page        = pageNumberText,
                        Description = decodedText
                    });
                }
        }

        if(scsiEvpdPages != null)
            foreach(KeyValuePair<byte, byte[]> page in scsiEvpdPages.OrderBy(t => t.Key))
            {
                var    evpdPageTitle = "";
                string evpdDecodedPage;

                switch(page.Key)
                {
                    case >= 0x01 and <= 0x7F:
                        evpdPageTitle   = $"ASCII Page {page.Key:X2}h";
                        evpdDecodedPage = EVPD.DecodeASCIIPage(page.Value);

                        break;
                    case 0x80:
                        evpdPageTitle   = "Unit Serial Number";
                        evpdDecodedPage = EVPD.DecodePage80(page.Value);

                        break;
                    case 0x81:
                        evpdPageTitle   = "SCSI Implemented operating definitions";
                        evpdDecodedPage = EVPD.PrettifyPage_81(page.Value);

                        break;
                    case 0x82:
                        evpdPageTitle   = "ASCII implemented operating definitions";
                        evpdDecodedPage = EVPD.DecodePage82(page.Value);

                        break;
                    case 0x83:
                        evpdPageTitle   = "SCSI Device identification";
                        evpdDecodedPage = EVPD.PrettifyPage_83(page.Value);

                        break;
                    case 0x84:
                        evpdPageTitle   = "SCSI Software Interface Identifiers";
                        evpdDecodedPage = EVPD.PrettifyPage_84(page.Value);

                        break;
                    case 0x85:
                        evpdPageTitle   = "SCSI Management Network Addresses";
                        evpdDecodedPage = EVPD.PrettifyPage_85(page.Value);

                        break;
                    case 0x86:
                        evpdPageTitle   = "SCSI Extended INQUIRY Data";
                        evpdDecodedPage = EVPD.PrettifyPage_86(page.Value);

                        break;
                    case 0x89:
                        evpdPageTitle   = "SCSI to ATA Translation Layer Data";
                        evpdDecodedPage = EVPD.PrettifyPage_89(page.Value);

                        break;
                    case 0xB0:
                        evpdPageTitle   = "SCSI Sequential-access Device Capabilities";
                        evpdDecodedPage = EVPD.PrettifyPage_B0(page.Value);

                        break;
                    case 0xB1:
                        evpdPageTitle   = "Manufacturer-assigned Serial Number";
                        evpdDecodedPage = EVPD.DecodePageB1(page.Value);

                        break;
                    case 0xB2:
                        evpdPageTitle   = "TapeAlert Supported Flags Bitmap";
                        evpdDecodedPage = $"0x{EVPD.DecodePageB2(page.Value):X16}";

                        break;
                    case 0xB3:
                        evpdPageTitle   = "Automation Device Serial Number";
                        evpdDecodedPage = EVPD.DecodePageB3(page.Value);

                        break;
                    case 0xB4:
                        evpdPageTitle   = "Data Transfer Device Element Address";
                        evpdDecodedPage = EVPD.DecodePageB4(page.Value);

                        break;
                    case 0xC0 when StringHandlers.CToString(scsiInquiry.Value.VendorIdentification).ToLowerInvariant().
                                                  Trim() == "quantum":
                        evpdPageTitle   = "Quantum Firmware Build Information page";
                        evpdDecodedPage = EVPD.PrettifyPage_C0_Quantum(page.Value);

                        break;
                    case 0xC0 when StringHandlers.CToString(scsiInquiry.Value.VendorIdentification).ToLowerInvariant().
                                                  Trim() == "seagate":
                        evpdPageTitle   = "Seagate Firmware Numbers page";
                        evpdDecodedPage = EVPD.PrettifyPage_C0_Seagate(page.Value);

                        break;
                    case 0xC0 when StringHandlers.CToString(scsiInquiry.Value.VendorIdentification).ToLowerInvariant().
                                                  Trim() == "ibm":
                        evpdPageTitle   = "IBM Drive Component Revision Levels page";
                        evpdDecodedPage = EVPD.PrettifyPage_C0_IBM(page.Value);

                        break;
                    case 0xC1 when StringHandlers.CToString(scsiInquiry.Value.VendorIdentification).ToLowerInvariant().
                                                  Trim() == "ibm":
                        evpdPageTitle   = "IBM Drive Serial Numbers page";
                        evpdDecodedPage = EVPD.PrettifyPage_C1_IBM(page.Value);

                        break;
                    case 0xC0 or 0xC1
                        when StringHandlers.CToString(scsiInquiry.Value.VendorIdentification).ToLowerInvariant().
                                            Trim() == "certance":
                        evpdPageTitle   = "Certance Drive Component Revision Levels page";
                        evpdDecodedPage = EVPD.PrettifyPage_C0_C1_Certance(page.Value);

                        break;
                    case 0xC2 or 0xC3 or 0xC4 or 0xC5 or 0xC6
                        when StringHandlers.CToString(scsiInquiry.Value.VendorIdentification).ToLowerInvariant().
                                            Trim() == "certance":
                        evpdPageTitle = page.Key switch
                                        {
                                            0xC2 => "Head Assembly Serial Number",
                                            0xC3 => "Reel Motor 1 Serial Number",
                                            0xC4 => "Reel Motor 2 Serial Number",
                                            0xC5 => "Board Serial Number",
                                            0xC6 => "Base Mechanical Serial Number",
                                            _    => evpdPageTitle
                                        };

                        evpdDecodedPage = EVPD.PrettifyPage_C2_C3_C4_C5_C6_Certance(page.Value);

                        break;
                    case 0xC0 or 0xC1 or 0xC2 or 0xC3 or 0xC4 or 0xC5
                        when StringHandlers.CToString(scsiInquiry.Value.VendorIdentification).ToLowerInvariant().
                                            Trim() == "hp":
                        evpdPageTitle = page.Key switch
                                        {
                                            0xC0 => "HP Drive Firmware Revision Levels page:",
                                            0xC1 => "HP Drive Hardware Revision Levels page:",
                                            0xC2 => "HP Drive PCA Revision Levels page:",
                                            0xC3 => "HP Drive Mechanism Revision Levels page:",
                                            0xC4 => "HP Drive Head Assembly Revision Levels page:",
                                            0xC5 => "HP Drive ACI Revision Levels page:",
                                            _    => evpdPageTitle
                                        };

                        evpdDecodedPage = EVPD.PrettifyPage_C0_to_C5_HP(page.Value);

                        break;
                    case 0xDF when StringHandlers.CToString(scsiInquiry.Value.VendorIdentification).ToLowerInvariant().
                                                  Trim() == "certance":
                        evpdPageTitle   = "Certance drive status page";
                        evpdDecodedPage = EVPD.PrettifyPage_DF_Certance(page.Value);

                        break;
                    default:
                    {
                        if(page.Key == 0x00)
                            continue;

                        evpdPageTitle   = $"Page {page.Key:X2}h";
                        evpdDecodedPage = "Undecoded";

                        AaruConsole.DebugWriteLine("Device-Info command", "Found undecoded SCSI VPD page 0x{0:X2}",
                                                   page.Key);

                        break;
                    }
                }

                EvpdPages.Add(new ScsiPageModel
                {
                    Page        = evpdPageTitle,
                    Data        = page.Value,
                    Description = evpdDecodedPage
                });
            }

        if(_configuration != null)
        {
            Features.SeparatedFeatures ftr = Features.Separate(_configuration);

            AaruConsole.DebugWriteLine("Device-Info command", "GET CONFIGURATION length is {0} bytes", ftr.DataLength);

            AaruConsole.DebugWriteLine("Device-Info command", "GET CONFIGURATION current profile is {0:X4}h",
                                       ftr.CurrentProfile);

            if(ftr.Descriptors != null)
                foreach(Features.FeatureDescriptor desc in ftr.Descriptors)
                {
                    string featureNumber = $"Feature {desc.Code:X4}h";
                    AaruConsole.DebugWriteLine("Device-Info command", "Feature {0:X4}h", desc.Code);

                    string featureDescription = desc.Code switch
                                                {
                                                    0x0000 => Features.Prettify_0000(desc.Data),
                                                    0x0001 => Features.Prettify_0001(desc.Data),
                                                    0x0002 => Features.Prettify_0002(desc.Data),
                                                    0x0003 => Features.Prettify_0003(desc.Data),
                                                    0x0004 => Features.Prettify_0004(desc.Data),
                                                    0x0010 => Features.Prettify_0010(desc.Data),
                                                    0x001D => Features.Prettify_001D(desc.Data),
                                                    0x001E => Features.Prettify_001E(desc.Data),
                                                    0x001F => Features.Prettify_001F(desc.Data),
                                                    0x0020 => Features.Prettify_0020(desc.Data),
                                                    0x0021 => Features.Prettify_0021(desc.Data),
                                                    0x0022 => Features.Prettify_0022(desc.Data),
                                                    0x0023 => Features.Prettify_0023(desc.Data),
                                                    0x0024 => Features.Prettify_0024(desc.Data),
                                                    0x0025 => Features.Prettify_0025(desc.Data),
                                                    0x0026 => Features.Prettify_0026(desc.Data),
                                                    0x0027 => Features.Prettify_0027(desc.Data),
                                                    0x0028 => Features.Prettify_0028(desc.Data),
                                                    0x0029 => Features.Prettify_0029(desc.Data),
                                                    0x002A => Features.Prettify_002A(desc.Data),
                                                    0x002B => Features.Prettify_002B(desc.Data),
                                                    0x002C => Features.Prettify_002C(desc.Data),
                                                    0x002D => Features.Prettify_002D(desc.Data),
                                                    0x002E => Features.Prettify_002E(desc.Data),
                                                    0x002F => Features.Prettify_002F(desc.Data),
                                                    0x0030 => Features.Prettify_0030(desc.Data),
                                                    0x0031 => Features.Prettify_0031(desc.Data),
                                                    0x0032 => Features.Prettify_0032(desc.Data),
                                                    0x0033 => Features.Prettify_0033(desc.Data),
                                                    0x0035 => Features.Prettify_0035(desc.Data),
                                                    0x0037 => Features.Prettify_0037(desc.Data),
                                                    0x0038 => Features.Prettify_0038(desc.Data),
                                                    0x003A => Features.Prettify_003A(desc.Data),
                                                    0x003B => Features.Prettify_003B(desc.Data),
                                                    0x0040 => Features.Prettify_0040(desc.Data),
                                                    0x0041 => Features.Prettify_0041(desc.Data),
                                                    0x0042 => Features.Prettify_0042(desc.Data),
                                                    0x0050 => Features.Prettify_0050(desc.Data),
                                                    0x0051 => Features.Prettify_0051(desc.Data),
                                                    0x0080 => Features.Prettify_0080(desc.Data),
                                                    0x0100 => Features.Prettify_0100(desc.Data),
                                                    0x0101 => Features.Prettify_0101(desc.Data),
                                                    0x0102 => Features.Prettify_0102(desc.Data),
                                                    0x0103 => Features.Prettify_0103(desc.Data),
                                                    0x0104 => Features.Prettify_0104(desc.Data),
                                                    0x0105 => Features.Prettify_0105(desc.Data),
                                                    0x0106 => Features.Prettify_0106(desc.Data),
                                                    0x0107 => Features.Prettify_0107(desc.Data),
                                                    0x0108 => Features.Prettify_0108(desc.Data),
                                                    0x0109 => Features.Prettify_0109(desc.Data),
                                                    0x010A => Features.Prettify_010A(desc.Data),
                                                    0x010B => Features.Prettify_010B(desc.Data),
                                                    0x010C => Features.Prettify_010C(desc.Data),
                                                    0x010D => Features.Prettify_010D(desc.Data),
                                                    0x010E => Features.Prettify_010E(desc.Data),
                                                    0x0110 => Features.Prettify_0110(desc.Data),
                                                    0x0113 => Features.Prettify_0113(desc.Data),
                                                    0x0142 => Features.Prettify_0142(desc.Data),
                                                    _      => "Unknown feature"
                                                };

                    MmcFeatures.Add(new ScsiPageModel
                    {
                        Page        = featureNumber,
                        Description = featureDescription
                    });
                }
            else
                AaruConsole.DebugWriteLine("Device-Info command", "GET CONFIGURATION returned no feature descriptors");
        }
    }

    public byte[]                              InquiryData              { get; }
    public string                              ScsiInquiryText          { get; }
    public ObservableCollection<ScsiPageModel> ModeSensePages           { get; }
    public ObservableCollection<ScsiPageModel> EvpdPages                { get; }
    public ObservableCollection<ScsiPageModel> MmcFeatures              { get; }
    public ReactiveCommand<Unit, Task>         SaveInquiryBinaryCommand { get; }
    public ReactiveCommand<Unit, Task>         SaveInquiryTextCommand   { get; }
    public ReactiveCommand<Unit, Task>         SaveModeSense6Command    { get; }
    public ReactiveCommand<Unit, Task>         SaveModeSense10Command   { get; }
    public ReactiveCommand<Unit, Task>         SaveEvpdPageCommand      { get; }
    public ReactiveCommand<Unit, Task>         SaveMmcFeaturesCommand   { get; }

    public object SelectedModeSensePage
    {
        get => _selectedModeSensePage;
        set
        {
            if(value == _selectedModeSensePage)
                return;

            if(value is ScsiPageModel pageModel)
                ModeSensePageText = pageModel.Description;

            this.RaiseAndSetIfChanged(ref _selectedModeSensePage, value);
        }
    }

    public string ModeSensePageText
    {
        get => _scsiModeSensePageText;
        set => this.RaiseAndSetIfChanged(ref _scsiModeSensePageText, value);
    }

    public object SelectedEvpdPage
    {
        get => _selectedEvpdPage;
        set
        {
            if(value == _selectedEvpdPage)
                return;

            if(value is ScsiPageModel pageModel)
                EvpdPageText = pageModel.Description;

            this.RaiseAndSetIfChanged(ref _selectedEvpdPage, value);
        }
    }

    public string EvpdPageText
    {
        get => _evpdPageText;
        set => this.RaiseAndSetIfChanged(ref _evpdPageText, value);
    }

    public object SelectedMmcFeature
    {
        get => _selectedMmcFeature;
        set
        {
            if(value == _selectedMmcFeature)
                return;

            if(value is ScsiPageModel pageModel)
                MmcFeatureText = pageModel.Description;

            this.RaiseAndSetIfChanged(ref _selectedMmcFeature, value);
        }
    }

    public string MmcFeatureText
    {
        get => _mmcFeatureText;
        set => this.RaiseAndSetIfChanged(ref _mmcFeatureText, value);
    }

    async Task ExecuteSaveInquiryBinaryCommand()
    {
        var dlgSaveBinary = new SaveFileDialog();

        dlgSaveBinary.Filters.Add(new FileDialogFilter
        {
            Extensions = new List<string>(new[]
            {
                "*.bin"
            }),
            Name = "Binary"
        });

        string result = await dlgSaveBinary.ShowAsync(_view);

        if(result is null)
            return;

        var saveFs = new FileStream(result, FileMode.Create);
        saveFs.Write(InquiryData, 0, InquiryData.Length);

        saveFs.Close();
    }

    async Task ExecuteSaveInquiryTextCommand()
    {
        var dlgSaveText = new SaveFileDialog();

        dlgSaveText.Filters.Add(new FileDialogFilter
        {
            Extensions = new List<string>(new[]
            {
                "*.txt"
            }),
            Name = "Text"
        });

        string result = await dlgSaveText.ShowAsync(_view);

        if(result is null)
            return;

        var saveFs = new FileStream(result, FileMode.Create);
        var saveSw = new StreamWriter(saveFs);
        saveSw.Write(ScsiInquiryText);
        saveFs.Close();
    }

    async Task ExecuteSaveModeSense6Command()
    {
        var dlgSaveBinary = new SaveFileDialog();

        dlgSaveBinary.Filters.Add(new FileDialogFilter
        {
            Extensions = new List<string>(new[]
            {
                "*.bin"
            }),
            Name = "Binary"
        });

        string result = await dlgSaveBinary.ShowAsync(_view);

        if(result is null)
            return;

        var saveFs = new FileStream(result, FileMode.Create);
        saveFs.Write(_scsiModeSense6, 0, _scsiModeSense6.Length);

        saveFs.Close();
    }

    async Task ExecuteSaveModeSense10Command()
    {
        var dlgSaveBinary = new SaveFileDialog();

        dlgSaveBinary.Filters.Add(new FileDialogFilter
        {
            Extensions = new List<string>(new[]
            {
                "*.bin"
            }),
            Name = "Binary"
        });

        string result = await dlgSaveBinary.ShowAsync(_view);

        if(result is null)
            return;

        var saveFs = new FileStream(result, FileMode.Create);
        saveFs.Write(_scsiModeSense10, 0, _scsiModeSense10.Length);

        saveFs.Close();
    }

    async Task ExecuteSaveEvpdPageCommand()
    {
        if(!(SelectedEvpdPage is ScsiPageModel pageModel))
            return;

        var dlgSaveBinary = new SaveFileDialog();

        dlgSaveBinary.Filters.Add(new FileDialogFilter
        {
            Extensions = new List<string>(new[]
            {
                "*.bin"
            }),
            Name = "Binary"
        });

        string result = await dlgSaveBinary.ShowAsync(_view);

        if(result is null)
            return;

        var saveFs = new FileStream(result, FileMode.Create);
        saveFs.Write(pageModel.Data, 0, pageModel.Data.Length);

        saveFs.Close();
    }

    async Task ExecuteSaveMmcFeaturesCommand()
    {
        var dlgSaveBinary = new SaveFileDialog();

        dlgSaveBinary.Filters.Add(new FileDialogFilter
        {
            Extensions = new List<string>(new[]
            {
                "*.bin"
            }),
            Name = "Binary"
        });

        string result = await dlgSaveBinary.ShowAsync(_view);

        if(result is null)
            return;

        var saveFs = new FileStream(result, FileMode.Create);
        saveFs.Write(_configuration, 0, _configuration.Length);

        saveFs.Close();
    }
}