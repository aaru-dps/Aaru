// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : frmDecodeMediaTags.xeto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Form to decode media tags of images.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements media tag decode form.
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
//     along with this program.  If not, see ;http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.ObjectModel;
using System.Text;
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
using Eto.Forms;
using Eto.Serialization.Xaml;
using BCA = Aaru.Decoders.Bluray.BCA;
using Cartridge = Aaru.Decoders.Bluray.Cartridge;
using DDS = Aaru.Decoders.DVD.DDS;
using DMI = Aaru.Decoders.Xbox.DMI;
using Inquiry = Aaru.Decoders.SCSI.Inquiry;
using Spare = Aaru.Decoders.DVD.Spare;

namespace Aaru.Gui.Forms
{
    // TODO: Decode long sector components
    // TODO: Panel with string representation of contents
    public class frmDecodeMediaTags : Form
    {
        const int                              HEX_COLUMNS = 32;
        IMediaImage                            inputFormat;
        ObservableCollection<MediaTagWithData> lstTags;

        public frmDecodeMediaTags(IMediaImage inputFormat)
        {
            this.inputFormat = inputFormat;
            XamlReader.Load(this);

            lstTags                = new ObservableCollection<MediaTagWithData>();
            cmbTag.ItemTextBinding = Binding.Property((MediaTagWithData p) => p.Tag.ToString());
            cmbTag.ItemKeyBinding  = Binding.Property((MediaTagWithData p) => p.Tag.ToString());
            cmbTag.DataStore       = lstTags;
            tabDecoded.Visible     = false;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            foreach(MediaTagType tag in inputFormat.Info.ReadableMediaTags)
                try
                {
                    byte[] data = inputFormat.ReadDiskTag(tag);
                    lstTags.Add(new MediaTagWithData {Tag = tag, Data = data});
                }
                catch
                {
                    //ignore
                }
        }

        // TODO: More graphically aware decoders
        void OnCmbTagSelectedIndexChanged(object sender, EventArgs e)
        {
            if(!(cmbTag.SelectedValue is MediaTagWithData tagWithData)) return;

            // TODO: Decoders should be able to handle tags with/without length header
            txtPrintHex.Text   = PrintHex.ByteArrayToHexArrayString(tagWithData.Data, HEX_COLUMNS);
            tabDecoded.Visible = true;
            switch(tagWithData.Tag)
            {
                case MediaTagType.CD_TOC:
                    txtDecoded.Text = TOC.Prettify(tagWithData.Data);
                    break;
                case MediaTagType.CD_SessionInfo:
                    txtDecoded.Text = Session.Prettify(tagWithData.Data);
                    break;
                case MediaTagType.CD_FullTOC:
                    txtDecoded.Text = FullTOC.Prettify(tagWithData.Data);
                    break;
                case MediaTagType.CD_PMA:
                    txtDecoded.Text = PMA.Prettify(tagWithData.Data);
                    break;
                case MediaTagType.CD_ATIP:
                    txtDecoded.Text = ATIP.Prettify(tagWithData.Data);
                    break;
                case MediaTagType.CD_TEXT:
                    txtDecoded.Text = CDTextOnLeadIn.Prettify(tagWithData.Data);
                    break;
                case MediaTagType.CD_MCN:
                    txtDecoded.Text = Encoding.ASCII.GetString(tagWithData.Data);
                    break;
                case MediaTagType.DVD_PFI:
                    txtDecoded.Text = PFI.Prettify(tagWithData.Data);
                    break;
                case MediaTagType.DVD_CMI:
                    txtDecoded.Text = CSS_CPRM.PrettifyLeadInCopyright(tagWithData.Data);
                    break;
                case MediaTagType.DVDRAM_DDS:
                    txtDecoded.Text = DDS.Prettify(tagWithData.Data);
                    break;
                case MediaTagType.DVDRAM_SpareArea:
                    txtDecoded.Text = Spare.Prettify(tagWithData.Data);
                    break;
                case MediaTagType.DVDR_PFI:
                    txtDecoded.Text = PFI.Prettify(tagWithData.Data);
                    break;
                case MediaTagType.HDDVD_MediumStatus:
                    txtDecoded.Text = PFI.Prettify(tagWithData.Data);
                    break;
                case MediaTagType.BD_DI:
                    txtDecoded.Text = DI.Prettify(tagWithData.Data);
                    break;
                case MediaTagType.BD_BCA:
                    txtDecoded.Text = BCA.Prettify(tagWithData.Data);
                    break;
                case MediaTagType.BD_DDS:
                    txtDecoded.Text = Decoders.Bluray.DDS.Prettify(tagWithData.Data);
                    break;
                case MediaTagType.BD_CartridgeStatus:
                    txtDecoded.Text = Cartridge.Prettify(tagWithData.Data);
                    break;
                case MediaTagType.BD_SpareArea:
                    txtDecoded.Text = Decoders.Bluray.Spare.Prettify(tagWithData.Data);
                    break;
                case MediaTagType.MMC_WriteProtection:
                    txtDecoded.Text = WriteProtect.PrettifyWriteProtectionStatus(tagWithData.Data);
                    break;
                case MediaTagType.MMC_DiscInformation:
                    txtDecoded.Text = DiscInformation.Prettify(tagWithData.Data);
                    break;
                case MediaTagType.SCSI_INQUIRY:
                    txtDecoded.Text = Inquiry.Prettify(tagWithData.Data);
                    break;
                case MediaTagType.SCSI_MODEPAGE_2A:
                    txtDecoded.Text = Modes.PrettifyModePage_2A(tagWithData.Data);
                    break;
                case MediaTagType.ATA_IDENTIFY:
                case MediaTagType.ATAPI_IDENTIFY:
                    txtDecoded.Text = Identify.Prettify(tagWithData.Data);
                    break;
                case MediaTagType.Xbox_SecuritySector:
                    txtDecoded.Text = SS.Prettify(tagWithData.Data);
                    break;
                case MediaTagType.SCSI_MODESENSE_6:
                    txtDecoded.Text = Modes.PrettifyModeHeader6(tagWithData.Data, PeripheralDeviceTypes.DirectAccess);
                    break;
                case MediaTagType.SCSI_MODESENSE_10:
                    txtDecoded.Text = Modes.PrettifyModeHeader10(tagWithData.Data, PeripheralDeviceTypes.DirectAccess);
                    break;
                case MediaTagType.Xbox_DMI:
                    txtDecoded.Text = DMI.IsXbox360(tagWithData.Data)
                                          ? DMI.PrettifyXbox360(tagWithData.Data)
                                          : DMI.PrettifyXbox(tagWithData.Data);
                    break;
                default:
                    tabDecoded.Visible = false;
                    break;
            }
        }

        class MediaTagWithData
        {
            public MediaTagType Tag  { get; set; }
            public byte[]       Data { get; set; }
        }

        #region XAML IDs
        Label    lblTag;
        ComboBox cmbTag;
        TabPage  tabPrintHex;
        TextArea txtPrintHex;
        TabPage  tabDecoded;
        TextArea txtDecoded;
        #endregion
    }
}