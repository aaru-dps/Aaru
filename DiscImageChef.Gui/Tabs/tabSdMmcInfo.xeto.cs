// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : tabSdMmcInfo.xeto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device information.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the SecureDigital/MultiMediaCard device information.
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

using DiscImageChef.CommonTypes.Enums;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace DiscImageChef.Gui.Tabs
{
    public class tabSdMmcInfo : TabPage
    {
        public tabSdMmcInfo()
        {
            XamlReader.Load(this);
        }

        internal void LoadData(DeviceType deviceType, byte[] cid, byte[] csd, byte[] ocr, byte[] extendedCsd,
                               byte[]     scr)
        {
            switch(deviceType)
            {
                case DeviceType.MMC:
                {
                    Text = "MultiMediaCard";
                    if(cid != null)
                    {
                        tabCid.Visible = true;
                        txtCid.Text    = Decoders.MMC.Decoders.PrettifyCID(cid);
                    }

                    if(csd != null)
                    {
                        tabCsd.Visible = true;
                        txtCid.Text    = Decoders.MMC.Decoders.PrettifyCSD(csd);
                    }

                    if(ocr != null)
                    {
                        tabOcr.Visible = true;
                        txtCid.Text    = Decoders.MMC.Decoders.PrettifyOCR(ocr);
                    }

                    if(extendedCsd != null)
                    {
                        tabExtendedCsd.Visible = true;
                        txtCid.Text            = Decoders.MMC.Decoders.PrettifyExtendedCSD(extendedCsd);
                    }
                }
                    break;
                case DeviceType.SecureDigital:
                {
                    Text = "SecureDigital";
                    if(cid != null)
                    {
                        tabCid.Visible = true;

                        txtCid.Text = Decoders.SecureDigital.Decoders.PrettifyCID(cid);
                    }

                    if(csd != null)
                    {
                        tabCsd.Visible = true;

                        txtCid.Text = Decoders.SecureDigital.Decoders.PrettifyCSD(csd);
                    }

                    if(ocr != null)
                    {
                        tabOcr.Visible = true;
                        txtCid.Text    = Decoders.SecureDigital.Decoders.PrettifyOCR(ocr);
                    }

                    if(scr != null)
                    {
                        tabScr.Visible = true;
                        txtCid.Text    = Decoders.SecureDigital.Decoders.PrettifySCR(scr);
                    }
                }
                    break;
            }

            Visible = tabCid.Visible || tabCsd.Visible || tabOcr.Visible || tabExtendedCsd.Visible || tabScr.Visible;
        }

        #region XAML controls
        #pragma warning disable 169
        #pragma warning disable 649
        TabPage  tabCid;
        TextArea txtCid;
        TabPage  tabCsd;
        TextArea txtCsd;
        TabPage  tabOcr;
        TextArea txtOcr;
        TabPage  tabExtendedCsd;
        TextArea txtExtendedCsd;
        TabPage  tabScr;
        TextArea txtScr;
        #pragma warning restore 169
        #pragma warning restore 649
        #endregion
    }
}