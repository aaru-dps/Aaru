// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DecodeMediaTagsViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model and code for the media tag decoding window.
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

using System;
using System.Collections.ObjectModel;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Decoders.ATA;
using Aaru.Decoders.Bluray;
using Aaru.Decoders.CD;
using Aaru.Decoders.DVD;
using Aaru.Decoders.SCSI;
using Aaru.Decoders.SCSI.MMC;
using Aaru.Decoders.Xbox;
using Aaru.Gui.Models;
using Aaru.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;
using BCA = Aaru.Decoders.Bluray.BCA;
using Cartridge = Aaru.Decoders.DVD.Cartridge;
using DDS = Aaru.Decoders.DVD.DDS;
using DMI = Aaru.Decoders.Xbox.DMI;
using Inquiry = Aaru.Decoders.SCSI.Inquiry;
using Spare = Aaru.Decoders.DVD.Spare;

namespace Aaru.Gui.ViewModels.Windows;

public sealed partial class DecodeMediaTagsViewModel : ViewModelBase
{
    readonly MediaType _mediaType;
    [ObservableProperty]
    string _decodedText;
    [ObservableProperty]
    bool _decodedVisible;
    [ObservableProperty]
    byte[] _hexData;

    public DecodeMediaTagsViewModel([NotNull] IMediaImage inputFormat)
    {
        TagsList = [];

        _mediaType = inputFormat.Info.MediaType;

        foreach(MediaTagType tag in inputFormat.Info.ReadableMediaTags)
        {
            ErrorNumber errno = inputFormat.ReadMediaTag(tag, out byte[] data);

            if(errno == ErrorNumber.NoError)
            {
                TagsList.Add(new MediaTagModel
                {
                    Tag  = tag,
                    Data = data
                });
            }
        }
    }

    public ObservableCollection<MediaTagModel> TagsList { get; }

    public MediaTagModel SelectedTag
    {
        get;
        set
        {
            SetProperty(ref field, value);

            if(value is null) return;

            // TODO: Decoders should be able to handle tags with/without length header
            HexData        = value.Data;
            DecodedVisible = true;

            if(value.Decoded != null)
            {
                DecodedText = value.Decoded;

                return;
            }

            byte[] data = value.Data;

            uint dataLen;

            switch(value.Tag)
            {
                case MediaTagType.CD_TOC:
                    dataLen = data.Length >= 2 ? Swapping.Swap(BitConverter.ToUInt16(data, 0)) : 0u;

                    if(dataLen + 2 != data.Length)
                    {
                        var tmp = new byte[data.Length + 2];
                        Array.Copy(data, 0, tmp, 2, data.Length);
                        tmp[0] = (byte)((data.Length & 0xFF00) >> 8);
                        tmp[1] = (byte)(data.Length & 0xFF);
                        data   = tmp;
                    }

                    DecodedText = TOC.Prettify(data);

                    if(string.IsNullOrEmpty(DecodedText)) DecodedVisible = false;

                    break;
                case MediaTagType.CD_SessionInfo:
                    DecodedText = Session.Prettify(value.Data);
                    if(string.IsNullOrEmpty(DecodedText)) DecodedVisible = false;

                    break;
                case MediaTagType.CD_FullTOC:
                    dataLen = data.Length >= 2 ? Swapping.Swap(BitConverter.ToUInt16(data, 0)) : 0u;

                    if(dataLen + 2 != data.Length)
                    {
                        var tmp = new byte[data.Length + 2];
                        Array.Copy(data, 0, tmp, 2, data.Length);
                        tmp[0] = (byte)((data.Length & 0xFF00) >> 8);
                        tmp[1] = (byte)(data.Length & 0xFF);
                        data   = tmp;
                    }

                    DecodedText = FullTOC.Prettify(data);
                    if(string.IsNullOrEmpty(DecodedText)) DecodedVisible = false;

                    break;
                case MediaTagType.CD_PMA:
                    dataLen = data.Length >= 2 ? Swapping.Swap(BitConverter.ToUInt16(data, 0)) : 0u;

                    if(dataLen + 2 != data.Length)
                    {
                        var tmp = new byte[data.Length + 2];
                        Array.Copy(data, 0, tmp, 2, data.Length);
                        tmp[0] = (byte)((data.Length & 0xFF00) >> 8);
                        tmp[1] = (byte)(data.Length & 0xFF);
                        data   = tmp;
                    }

                    DecodedText = PMA.Prettify(data);
                    if(string.IsNullOrEmpty(DecodedText)) DecodedVisible = false;

                    break;
                case MediaTagType.CD_ATIP:
                    dataLen = data.Length >= 4 ? Swapping.Swap(BitConverter.ToUInt32(data, 0)) : 0u;

                    if(dataLen + 4 != data.Length)
                    {
                        var tmp = new byte[data.Length + 4];
                        Array.Copy(data, 0, tmp, 4, data.Length);
                        tmp[0] = (byte)((data.Length & 0xFF000000) >> 24);
                        tmp[1] = (byte)((data.Length & 0xFF0000)   >> 16);
                        tmp[2] = (byte)((data.Length & 0xFF00)     >> 8);
                        tmp[3] = (byte)(data.Length & 0xFF);
                        data   = tmp;
                    }

                    DecodedText = ATIP.Prettify(data);
                    if(string.IsNullOrEmpty(DecodedText)) DecodedVisible = false;

                    break;
                case MediaTagType.CD_TEXT:
                    dataLen = data.Length >= 2 ? Swapping.Swap(BitConverter.ToUInt16(data, 0)) : 0u;

                    if(dataLen + 2 != data.Length)
                    {
                        var tmp = new byte[data.Length + 2];
                        Array.Copy(data, 0, tmp, 2, data.Length);
                        tmp[0] = (byte)((data.Length & 0xFF00) >> 8);
                        tmp[1] = (byte)(data.Length & 0xFF);
                        data   = tmp;
                    }

                    DecodedText = CDTextOnLeadIn.Prettify(data);
                    if(string.IsNullOrEmpty(DecodedText)) DecodedVisible = false;

                    break;
                case MediaTagType.CD_MCN:
                    DecodedText = Encoding.UTF8.GetString(value.Data);
                    if(string.IsNullOrEmpty(DecodedText)) DecodedVisible = false;

                    break;
                case MediaTagType.DVD_PFI:
                case MediaTagType.DVD_PFI_2ndLayer:
                    DecodedText = PFI.Prettify(value.Data, _mediaType);
                    if(string.IsNullOrEmpty(DecodedText)) DecodedVisible = false;

                    break;
                case MediaTagType.DVD_CMI:
                    DecodedText = CSS_CPRM.PrettifyLeadInCopyright(value.Data);
                    if(string.IsNullOrEmpty(DecodedText)) DecodedVisible = false;

                    break;
                case MediaTagType.DVDRAM_DDS:
                    DecodedText = DDS.Prettify(value.Data);
                    if(string.IsNullOrEmpty(DecodedText)) DecodedVisible = false;

                    break;
                case MediaTagType.DVDRAM_SpareArea:
                    DecodedText = Spare.Prettify(value.Data);
                    if(string.IsNullOrEmpty(DecodedText)) DecodedVisible = false;

                    break;
                case MediaTagType.DVDR_PFI:
                    DecodedText = PFI.Prettify(value.Data, _mediaType);
                    if(string.IsNullOrEmpty(DecodedText)) DecodedVisible = false;

                    break;
                case MediaTagType.BD_DI:
                    if(value.Data.Length == 4096)
                    {
                        var tmp = new byte[4100];
                        Array.Copy(value.Data, 0, tmp, 4, 4096);
                        value.Data = tmp;
                    }

                    DecodedText = DI.Prettify(value.Data);
                    if(string.IsNullOrEmpty(DecodedText)) DecodedVisible = false;

                    break;
                case MediaTagType.BD_BCA:
                    if(value.Data.Length == 64)
                    {
                        var tmp = new byte[68];
                        Array.Copy(value.Data, 0, tmp, 4, 64);
                        value.Data = tmp;
                    }

                    DecodedText = BCA.Prettify(value.Data);
                    if(string.IsNullOrEmpty(DecodedText)) DecodedVisible = false;

                    break;
                case MediaTagType.BD_DDS:
                    if(value.Data.Length == 96)
                    {
                        var tmp = new byte[100];
                        Array.Copy(value.Data, 0, tmp, 4, 96);
                        value.Data = tmp;
                    }

                    DecodedText = Decoders.Bluray.DDS.Prettify(value.Data);
                    if(string.IsNullOrEmpty(DecodedText)) DecodedVisible = false;

                    break;
                case MediaTagType.BD_CartridgeStatus:
                    if(value.Data.Length == 4)
                    {
                        var tmp = new byte[8];
                        Array.Copy(value.Data, 0, tmp, 4, 4);
                        value.Data = tmp;
                    }

                    DecodedText = Cartridge.Prettify(value.Data);
                    if(string.IsNullOrEmpty(DecodedText)) DecodedVisible = false;

                    break;
                case MediaTagType.BD_SpareArea:
                    if(value.Data.Length == 12)
                    {
                        var tmp = new byte[16];
                        Array.Copy(value.Data, 0, tmp, 4, 12);
                        value.Data = tmp;
                    }

                    DecodedText = Decoders.Bluray.Spare.Prettify(value.Data);
                    if(string.IsNullOrEmpty(DecodedText)) DecodedVisible = false;

                    break;
                case MediaTagType.MMC_WriteProtection:
                    DecodedText = WriteProtect.PrettifyWriteProtectionStatus(value.Data);
                    if(string.IsNullOrEmpty(DecodedText)) DecodedVisible = false;

                    break;
                case MediaTagType.MMC_DiscInformation:
                    DecodedText = DiscInformation.Prettify(value.Data);
                    if(string.IsNullOrEmpty(DecodedText)) DecodedVisible = false;

                    break;
                case MediaTagType.SCSI_INQUIRY:
                    DecodedText = Inquiry.Prettify(value.Data);
                    if(string.IsNullOrEmpty(DecodedText)) DecodedVisible = false;

                    break;
                case MediaTagType.SCSI_MODEPAGE_2A:
                    DecodedText = Modes.PrettifyModePage_2A(value.Data);
                    if(string.IsNullOrEmpty(DecodedText)) DecodedVisible = false;

                    break;
                case MediaTagType.ATA_IDENTIFY:
                case MediaTagType.ATAPI_IDENTIFY:
                    DecodedText = Identify.Prettify(value.Data);
                    if(string.IsNullOrEmpty(DecodedText)) DecodedVisible = false;

                    break;
                case MediaTagType.Xbox_SecuritySector:
                    DecodedText = SS.Prettify(value.Data);
                    if(string.IsNullOrEmpty(DecodedText)) DecodedVisible = false;

                    break;
                case MediaTagType.SCSI_MODESENSE_6:
                    DecodedText = Modes.PrettifyModeHeader6(value.Data, PeripheralDeviceTypes.DirectAccess);
                    if(string.IsNullOrEmpty(DecodedText)) DecodedVisible = false;

                    break;
                case MediaTagType.SCSI_MODESENSE_10:
                    DecodedText = Modes.PrettifyModeHeader10(value.Data, PeripheralDeviceTypes.DirectAccess);
                    if(string.IsNullOrEmpty(DecodedText)) DecodedVisible = false;

                    break;
                case MediaTagType.Xbox_DMI:
                    DecodedText = DMI.IsXbox360(value.Data)
                                      ? DMI.PrettifyXbox360(value.Data)
                                      : DMI.PrettifyXbox(value.Data);

                    if(string.IsNullOrEmpty(DecodedText)) DecodedVisible = false;

                    break;
                default:
                    DecodedVisible = false;

                    break;
            }

            if(DecodedText != null) value.Decoded = DecodedText;
        }
    }
}