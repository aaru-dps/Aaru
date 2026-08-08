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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Decoders.SCSI;
using Aaru.Decoders.SCSI.MMC;
using Aaru.Gui.Models;
using Aaru.Helpers;
using Aaru.Localization;
using Aaru.Logging;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Inquiry = Aaru.CommonTypes.Structs.Devices.SCSI.Inquiry;

namespace Aaru.Gui.ViewModels.Tabs;

public sealed partial class ScsiInfoViewModel : ViewModelBase
{
    const    string MODULE_NAME = "SCSI Information ViewModel";
    readonly byte[] _configuration;
    readonly byte[] _scsiModeSense10;
    readonly byte[] _scsiModeSense6;
    readonly Window _view;
    [ObservableProperty]
    string _evpdPageText;
    [ObservableProperty]
    string _mmcFeatureText;
    [ObservableProperty]
    string _modeSensePageText;

    public ScsiInfoViewModel(byte[] scsiInquiryData, Inquiry? scsiInquiry, Dictionary<byte, byte[]> scsiEvpdPages,
                             Modes.DecodedMode? scsiMode, PeripheralDeviceTypes scsiType, byte[] scsiModeSense6,
                             byte[] scsiModeSense10, byte[] mmcConfiguration, Window view)
    {
        InquiryData              = scsiInquiryData;
        _scsiModeSense6          = scsiModeSense6;
        _scsiModeSense10         = scsiModeSense10;
        _configuration           = mmcConfiguration;
        _view                    = view;
        ModeSensePages           = [];
        EvpdPages                = [];
        MmcFeatures              = [];
        SaveInquiryBinaryCommand = new AsyncRelayCommand(SaveInquiryBinaryAsync);
        SaveInquiryTextCommand   = new AsyncRelayCommand(SaveInquiryTextAsync);
        SaveModeSense6Command    = new AsyncRelayCommand(SaveModeSense6Async);
        SaveModeSense10Command   = new AsyncRelayCommand(SaveModeSense10Async);
        SaveEvpdPageCommand      = new AsyncRelayCommand(SaveEvpdPageAsync);
        SaveMmcFeaturesCommand   = new AsyncRelayCommand(SaveMmcFeaturesAsync);

        if(InquiryData == null || !scsiInquiry.HasValue) return;

        ScsiInquiryText = Decoders.SCSI.Inquiry.Prettify(scsiInquiry);

        if(scsiMode.HasValue)
        {
            ModeSensePages.Add(new ScsiPageModel
            {
                Page        = UI.Title_Header,
                Description = Modes.PrettifyModeHeader(scsiMode.Value.Header, scsiType)
            });

            if(scsiMode.Value.Pages != null)
            {
                foreach(Modes.ModePage page in scsiMode.Value.Pages.OrderBy(static t => t.Page)
                                                       .ThenBy(static t => t.Subpage))
                {
                    string pageNumberText = page.Subpage == 0
                                                ? string.Format(UI.MODE_0,           page.Page)
                                                : string.Format(UI.MODE_0_Subpage_1, page.Page, page.Subpage);

                    string decodedText;

                    switch(page.Page)
                    {
                        case 0x00:
                        {
                            if(scsiType == PeripheralDeviceTypes.MultiMediaDevice && page.Subpage == 0)
                                decodedText = Modes.PrettifyModePage_00_SFF(page.PageResponse);
                            else
                                decodedText = UI.Undecoded;

                            break;
                        }
                        case 0x01:
                        {
                            if(page.Subpage == 0)
                            {
                                decodedText = scsiType == PeripheralDeviceTypes.MultiMediaDevice
                                                  ? Modes.PrettifyModePage_01_MMC(page.PageResponse)
                                                  : Modes.PrettifyModePage_01(page.PageResponse);
                            }
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
                            {
                                decodedText = scsiType == PeripheralDeviceTypes.MultiMediaDevice
                                                  ? Modes.PrettifyModePage_07_MMC(page.PageResponse)
                                                  : Modes.PrettifyModePage_07(page.PageResponse);
                            }
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
                            {
                                decodedText = scsiType == PeripheralDeviceTypes.SequentialAccess
                                                  ? Modes.PrettifyModePage_10_SSC(page.PageResponse)
                                                  : Modes.PrettifyModePage_10(page.PageResponse);
                            }
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
                            {
                                decodedText = scsiType == PeripheralDeviceTypes.MultiMediaDevice
                                                  ? Modes.PrettifyModePage_1C_SFF(page.PageResponse)
                                                  : Modes.PrettifyModePage_1C(page.PageResponse);
                            }
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
                                decodedText = Aaru.Localization.Core.Drive_identifies_as_Apple_OEM_drive;
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
                            decodedText = UI.Undecoded;

                            break;
                        }
                    }

                    // TODO: Automatic error reporting
                    decodedText ??= UI.Error_decoding_page_please_open_an_issue;

                    ModeSensePages.Add(new ScsiPageModel
                    {
                        Page        = pageNumberText,
                        Description = decodedText
                    });
                }
            }
        }

        if(scsiEvpdPages != null)
        {
            foreach(KeyValuePair<byte, byte[]> page in scsiEvpdPages.OrderBy(static t => t.Key))
            {
                var    evpdPageTitle = "";
                string evpdDecodedPage;

                switch(page.Key)
                {
                    case >= 0x01 and <= 0x7F:
                        evpdPageTitle   = string.Format(UI.ASCII_Page_0, page.Key);
                        evpdDecodedPage = EVPD.DecodeASCIIPage(page.Value);

                        break;
                    case 0x80:
                        evpdPageTitle   = UI.Unit_Serial_Number;
                        evpdDecodedPage = EVPD.DecodePage80(page.Value);

                        break;
                    case 0x81:
                        evpdPageTitle   = UI.SCSI_Implemented_operating_definitions;
                        evpdDecodedPage = EVPD.PrettifyPage_81(page.Value);

                        break;
                    case 0x82:
                        evpdPageTitle   = UI.ASCII_implemented_operating_definitions;
                        evpdDecodedPage = EVPD.DecodePage82(page.Value);

                        break;
                    case 0x83:
                        evpdPageTitle   = UI.SCSI_Device_identification;
                        evpdDecodedPage = EVPD.PrettifyPage_83(page.Value);

                        break;
                    case 0x84:
                        evpdPageTitle   = UI.SCSI_Software_Interface_Identifiers;
                        evpdDecodedPage = EVPD.PrettifyPage_84(page.Value);

                        break;
                    case 0x85:
                        evpdPageTitle   = UI.SCSI_Management_Network_Addresses;
                        evpdDecodedPage = EVPD.PrettifyPage_85(page.Value);

                        break;
                    case 0x86:
                        evpdPageTitle   = UI.SCSI_Extended_INQUIRY_Data;
                        evpdDecodedPage = EVPD.PrettifyPage_86(page.Value);

                        break;
                    case 0x89:
                        evpdPageTitle   = UI.SCSI_to_ATA_Translation_Layer_Data;
                        evpdDecodedPage = EVPD.PrettifyPage_89(page.Value);

                        break;
                    case 0xB0:
                        evpdPageTitle   = UI.SCSI_Sequential_access_Device_Capabilities;
                        evpdDecodedPage = EVPD.PrettifyPage_B0(page.Value);

                        break;
                    case 0xB1:
                        evpdPageTitle   = UI.Manufacturer_assigned_Serial_Number;
                        evpdDecodedPage = EVPD.DecodePageB1(page.Value);

                        break;
                    case 0xB2:
                        evpdPageTitle   = UI.TapeAlert_Supported_Flags_Bitmap;
                        evpdDecodedPage = $"0x{EVPD.DecodePageB2(page.Value):X16}";

                        break;
                    case 0xB3:
                        evpdPageTitle   = UI.Automation_Device_Serial_Number;
                        evpdDecodedPage = EVPD.DecodePageB3(page.Value);

                        break;
                    case 0xB4:
                        evpdPageTitle   = UI.Data_Transfer_Device_Element_Address;
                        evpdDecodedPage = EVPD.DecodePageB4(page.Value);

                        break;
                    case 0xC0 when StringHandlers.CToString(scsiInquiry.Value.VendorIdentification)
                                                 .ToLowerInvariant()
                                                 .Trim() ==
                                   "quantum":
                        evpdPageTitle   = UI.Quantum_Firmware_Build_Information_page;
                        evpdDecodedPage = EVPD.PrettifyPage_C0_Quantum(page.Value);

                        break;
                    case 0xC0 when StringHandlers.CToString(scsiInquiry.Value.VendorIdentification)
                                                 .ToLowerInvariant()
                                                 .Trim() ==
                                   "seagate":
                        evpdPageTitle   = UI.Seagate_Firmware_Numbers_page;
                        evpdDecodedPage = EVPD.PrettifyPage_C0_Seagate(page.Value);

                        break;
                    case 0xC0 when StringHandlers.CToString(scsiInquiry.Value.VendorIdentification)
                                                 .ToLowerInvariant()
                                                 .Trim() ==
                                   "ibm":
                        evpdPageTitle   = UI.IBM_Drive_Component_Revision_Levels_page;
                        evpdDecodedPage = EVPD.PrettifyPage_C0_IBM(page.Value);

                        break;
                    case 0xC1 when StringHandlers.CToString(scsiInquiry.Value.VendorIdentification)
                                                 .ToLowerInvariant()
                                                 .Trim() ==
                                   "ibm":
                        evpdPageTitle   = UI.IBM_Drive_Serial_Numbers_page;
                        evpdDecodedPage = EVPD.PrettifyPage_C1_IBM(page.Value);

                        break;
                    case 0xC0 or 0xC1
                        when StringHandlers.CToString(scsiInquiry.Value.VendorIdentification)
                                           .ToLowerInvariant()
                                           .Trim() ==
                             "certance":
                        evpdPageTitle   = UI.Certance_Drive_Component_Revision_Levels_page;
                        evpdDecodedPage = EVPD.PrettifyPage_C0_C1_Certance(page.Value);

                        break;
                    case 0xC2 or 0xC3 or 0xC4 or 0xC5 or 0xC6
                        when StringHandlers.CToString(scsiInquiry.Value.VendorIdentification)
                                           .ToLowerInvariant()
                                           .Trim() ==
                             "certance":
                        evpdPageTitle = page.Key switch
                                        {
                                            0xC2 => UI.Head_Assembly_Serial_Number,
                                            0xC3 => UI.Reel_Motor_1_Serial_Number,
                                            0xC4 => UI.Reel_Motor_2_Serial_Number,
                                            0xC5 => UI.Board_Serial_Number,
                                            0xC6 => UI.Base_Mechanical_Serial_Number,
                                            _    => evpdPageTitle
                                        };

                        evpdDecodedPage = EVPD.PrettifyPage_C2_C3_C4_C5_C6_Certance(page.Value);

                        break;
                    case 0xC0 or 0xC1 or 0xC2 or 0xC3 or 0xC4 or 0xC5
                        when StringHandlers.CToString(scsiInquiry.Value.VendorIdentification)
                                           .ToLowerInvariant()
                                           .Trim() ==
                             "hp":
                        evpdPageTitle = page.Key switch
                                        {
                                            0xC0 => UI.HP_Drive_Firmware_Revision_Levels_page,
                                            0xC1 => UI.HP_Drive_Hardware_Revision_Levels_page,
                                            0xC2 => UI.HP_Drive_PCA_Revision_Levels_page,
                                            0xC3 => UI.HP_Drive_Mechanism_Revision_Levels_page,
                                            0xC4 => UI.HP_Drive_Head_Assembly_Revision_Levels_page,
                                            0xC5 => UI.HP_Drive_ACI_Revision_Levels_page,
                                            _    => evpdPageTitle
                                        };

                        evpdDecodedPage = EVPD.PrettifyPage_C0_to_C5_HP(page.Value);

                        break;
                    case 0xDF when StringHandlers.CToString(scsiInquiry.Value.VendorIdentification)
                                                 .ToLowerInvariant()
                                                 .Trim() ==
                                   "certance":
                        evpdPageTitle   = UI.Certance_drive_status_page;
                        evpdDecodedPage = EVPD.PrettifyPage_DF_Certance(page.Value);

                        break;
                    default:
                    {
                        if(page.Key == 0x00) continue;

                        evpdPageTitle   = string.Format(UI.Page_0_h, page.Key);
                        evpdDecodedPage = UI.Undecoded;

                        AaruLogging.Debug(MODULE_NAME,
                                          Aaru.Localization.Core.Found_undecoded_SCSI_VPD_page_0,
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
        }

        if(_configuration == null) return;

        Features.SeparatedFeatures ftr = Features.Separate(_configuration);

        AaruLogging.Debug(MODULE_NAME, Aaru.Localization.Core.GET_CONFIGURATION_length_is_0, ftr.DataLength);

        AaruLogging.Debug(MODULE_NAME,
                          Aaru.Localization.Core.GET_CONFIGURATION_current_profile_is_0,
                          ftr.CurrentProfile);

        if(ftr.Descriptors != null)
        {
            foreach(Features.FeatureDescriptor desc in ftr.Descriptors)
            {
                var featureNumber = string.Format(Aaru.Localization.Core.Feature_0, desc.Code);
                AaruLogging.Debug(MODULE_NAME, Aaru.Localization.Core.Feature_0, desc.Code);

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
                                                _      => UI.Unknown_feature
                                            };

                MmcFeatures.Add(new ScsiPageModel
                {
                    Page        = featureNumber,
                    Description = featureDescription
                });
            }
        }
        else
            AaruLogging.Debug(MODULE_NAME, Aaru.Localization.Core.GET_CONFIGURATION_returned_no_feature_descriptors);
    }

    public byte[]                              InquiryData              { get; }
    public string                              ScsiInquiryText          { get; }
    public ObservableCollection<ScsiPageModel> ModeSensePages           { get; }
    public ObservableCollection<ScsiPageModel> EvpdPages                { get; }
    public ObservableCollection<ScsiPageModel> MmcFeatures              { get; }
    public ICommand                            SaveInquiryBinaryCommand { get; }
    public ICommand                            SaveInquiryTextCommand   { get; }
    public ICommand                            SaveModeSense6Command    { get; }
    public ICommand                            SaveModeSense10Command   { get; }
    public ICommand                            SaveEvpdPageCommand      { get; }
    public ICommand                            SaveMmcFeaturesCommand   { get; }

    public object SelectedModeSensePage
    {
        get;
        set
        {
            if(value == field) return;

            if(value is ScsiPageModel pageModel) ModeSensePageText = pageModel.Description;

            SetProperty(ref field, value);
        }
    }

    public object SelectedEvpdPage
    {
        get;
        set
        {
            if(value == field) return;

            if(value is ScsiPageModel pageModel) EvpdPageText = pageModel.Description;

            SetProperty(ref field, value);
        }
    }

    public object SelectedMmcFeature
    {
        get;
        set
        {
            if(value == field) return;

            if(value is ScsiPageModel pageModel) MmcFeatureText = pageModel.Description;

            SetProperty(ref field, value);
        }
    }

    async Task SaveInquiryBinaryAsync()
    {
        IStorageFile result = await _view.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            FileTypeChoices = new List<FilePickerFileType>
            {
                FilePickerFileTypes.Binary
            }
        });

        if(result is null) return;

        await using var saveFs = new FileStream(result.Path.LocalPath, FileMode.Create);
        await saveFs.WriteAsync(InquiryData);
    }

    async Task SaveInquiryTextAsync()
    {
        IStorageFile result = await _view.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            FileTypeChoices = new List<FilePickerFileType>
            {
                FilePickerFileTypes.PlainText
            }
        });

        if(result is null) return;

        await using var saveFs = new FileStream(result.Path.LocalPath, FileMode.Create);
        await using var saveSw = new StreamWriter(saveFs);
        await saveSw.WriteAsync(ScsiInquiryText);
    }

    async Task SaveModeSense6Async()
    {
        if(_scsiModeSense6 is null) return;

        IStorageFile result = await _view.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            FileTypeChoices = new List<FilePickerFileType>
            {
                FilePickerFileTypes.Binary
            }
        });

        if(result is null) return;

        await using var saveFs = new FileStream(result.Path.LocalPath, FileMode.Create);
        await saveFs.WriteAsync(_scsiModeSense6);
    }

    async Task SaveModeSense10Async()
    {
        if(_scsiModeSense10 is null) return;

        IStorageFile result = await _view.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            FileTypeChoices = new List<FilePickerFileType>
            {
                FilePickerFileTypes.Binary
            }
        });

        if(result is null) return;

        await using var saveFs = new FileStream(result.Path.LocalPath, FileMode.Create);
        await saveFs.WriteAsync(_scsiModeSense10);
    }

    async Task SaveEvpdPageAsync()
    {
        if(SelectedEvpdPage is not ScsiPageModel pageModel) return;

        IStorageFile result = await _view.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            FileTypeChoices = new List<FilePickerFileType>
            {
                FilePickerFileTypes.Binary
            }
        });

        if(result is null) return;

        await using var saveFs = new FileStream(result.Path.LocalPath, FileMode.Create);
        await saveFs.WriteAsync(pageModel.Data);
    }

    async Task SaveMmcFeaturesAsync()
    {
        if(_configuration is null) return;

        IStorageFile result = await _view.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            FileTypeChoices = new List<FilePickerFileType>
            {
                FilePickerFileTypes.Binary
            }
        });

        if(result is null) return;

        await using var saveFs = new FileStream(result.Path.LocalPath, FileMode.Create);
        await saveFs.WriteAsync(_configuration);
    }
}