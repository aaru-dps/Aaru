// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : frmDump.xeto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Media dump window.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the media dump GUI window.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Core;
using DiscImageChef.Core.Devices.Dumping;
using DiscImageChef.Core.Logging;
using DiscImageChef.Core.Media.Info;
using DiscImageChef.Devices;
using Eto.Forms;
using Eto.Serialization.Xaml;
using Schemas;
using DeviceInfo = DiscImageChef.Core.Devices.Info.DeviceInfo;
using MediaType = DiscImageChef.CommonTypes.MediaType;

namespace DiscImageChef.Gui.Forms
{
    public class frmDump : Form
    {
        readonly string devicePath;

        Dump             dumper;
        string           outputPrefix;
        Resume           resume;
        CICMMetadataType sidecar;

        public frmDump(string devicePath, DeviceInfo deviceInfo, ScsiInfo scsiInfo = null)
        {
            MediaType mediaType;
            XamlReader.Load(this);

            // Defaults
            chkStopOnError.Checked      = false;
            chkForce.Checked            = false;
            chkPersistent.Checked       = true;
            chkResume.Checked           = true;
            chkTrack1Pregap.Checked     = false;
            chkSidecar.Checked          = true;
            chkTrim.Checked             = true;
            chkExistingMetadata.Checked = false;
            stpRetries.Value            = 5;
            stpSkipped.Value            = 512;

            if(scsiInfo != null) mediaType = scsiInfo.MediaType;
            else
                switch(deviceInfo.Type)
                {
                    case DeviceType.SecureDigital:
                        mediaType = MediaType.SecureDigital;
                        break;
                    case DeviceType.MMC:
                        mediaType = MediaType.MMC;
                        break;
                    default:
                        if(deviceInfo.IsPcmcia) mediaType            = MediaType.PCCardTypeII;
                        else if(deviceInfo.IsCompactFlash) mediaType = MediaType.CompactFlash;
                        else mediaType                               = MediaType.GENERIC_HDD;
                        break;
                }

            ObservableCollection<IWritableImage> lstPlugins = new ObservableCollection<IWritableImage>();
            PluginBase                           plugins    = GetPluginBase.Instance;
            foreach(IWritableImage plugin in
                plugins.WritableImages.Values.Where(p => p.SupportedMediaTypes.Contains(mediaType)))
                lstPlugins.Add(plugin);
            cmbFormat.ItemTextBinding = Binding.Property((IWritableImage p) => p.Name);
            cmbFormat.ItemKeyBinding  = Binding.Property((IWritableImage p) => p.Id.ToString());
            cmbFormat.DataStore       = lstPlugins;

            List<CommonEncodingInfo> encodings = Encoding
                                                .GetEncodings().Select(info => new CommonEncodingInfo
                                                 {
                                                     Name = info.Name,
                                                     DisplayName =
                                                         info.GetEncoding().EncodingName
                                                 }).ToList();
            encodings.AddRange(Claunia.Encoding.Encoding.GetEncodings()
                                      .Select(info => new CommonEncodingInfo
                                       {
                                           Name = info.Name, DisplayName = info.DisplayName
                                       }));

            ObservableCollection<CommonEncodingInfo> lstEncodings = new ObservableCollection<CommonEncodingInfo>();
            foreach(CommonEncodingInfo info in encodings.OrderBy(t => t.DisplayName)) lstEncodings.Add(info);
            cmbEncoding.ItemTextBinding = Binding.Property((CommonEncodingInfo p) => p.DisplayName);
            cmbEncoding.ItemKeyBinding  = Binding.Property((CommonEncodingInfo p) => p.Name);
            cmbEncoding.DataStore       = lstEncodings;

            switch(mediaType)
            {
                case MediaType.CD:
                case MediaType.CDDA:
                case MediaType.CDG:
                case MediaType.CDEG:
                case MediaType.CDI:
                case MediaType.CDROM:
                case MediaType.CDROMXA:
                case MediaType.CDPLUS:
                case MediaType.CDMO:
                case MediaType.CDR:
                case MediaType.CDRW:
                case MediaType.CDMRW:
                case MediaType.VCD:
                case MediaType.SVCD:
                case MediaType.PCD:
                case MediaType.DDCD:
                case MediaType.DDCDR:
                case MediaType.DDCDRW:
                case MediaType.DTSCD:
                case MediaType.CDMIDI:
                case MediaType.CDV:
                case MediaType.CDIREADY:
                case MediaType.FMTOWNS:
                case MediaType.PS1CD:
                case MediaType.PS2CD:
                case MediaType.MEGACD:
                case MediaType.SATURNCD:
                case MediaType.GDROM:
                case MediaType.GDR:
                case MediaType.MilCD:
                case MediaType.SuperCDROM2:
                case MediaType.JaguarCD:
                case MediaType.ThreeDO:
                case MediaType.PCFX:
                case MediaType.NeoGeoCD:
                case MediaType.CDTV:
                case MediaType.CD32:
                case MediaType.Playdia:
                case MediaType.Pippin:
                case MediaType.VideoNow:
                case MediaType.VideoNowColor:
                case MediaType.VideoNowXp:
                    chkTrack1Pregap.Visible = true;
                    break;
                default:
                    chkTrack1Pregap.Visible = false;
                    break;
            }

            this.devicePath = devicePath;
        }

        void OnCmbFormatSelectedIndexChanged(object sender, EventArgs e)
        {
            txtDestination.Text = "";

            if(!(cmbFormat.SelectedValue is IWritableImage plugin))
            {
                grpOptions.Visible     = false;
                btnDestination.Enabled = false;
                return;
            }

            btnDestination.Enabled = true;

            if(!plugin.SupportedOptions.Any())
            {
                grpOptions.Content = null;
                grpOptions.Visible = false;
                return;
            }

            grpOptions.Visible = true;

            StackLayout stkOptions = new StackLayout {Orientation = Orientation.Vertical};

            foreach((string name, Type type, string description, object @default) option in plugin.SupportedOptions)
                switch(option.type.ToString())
                {
                    case "System.Boolean":
                        CheckBox optBoolean = new CheckBox();
                        optBoolean.ID      = "opt" + option.name;
                        optBoolean.Text    = option.description;
                        optBoolean.Checked = (bool)option.@default;
                        stkOptions.Items.Add(optBoolean);
                        break;
                    case "System.SByte":
                    case "System.Int16":
                    case "System.Int32":
                    case "System.Int64":
                        StackLayout stkNumber = new StackLayout();
                        stkNumber.Orientation = Orientation.Horizontal;
                        NumericStepper optNumber = new NumericStepper();
                        optNumber.ID    = "opt" + option.name;
                        optNumber.Value = Convert.ToDouble(option.@default);
                        stkNumber.Items.Add(optNumber);
                        Label lblNumber = new Label();
                        lblNumber.Text = option.description;
                        stkNumber.Items.Add(lblNumber);
                        stkOptions.Items.Add(stkNumber);
                        break;
                    case "System.Byte":
                    case "System.UInt16":
                    case "System.UInt32":
                    case "System.UInt64":
                        StackLayout stkUnsigned = new StackLayout();
                        stkUnsigned.Orientation = Orientation.Horizontal;
                        NumericStepper optUnsigned = new NumericStepper();
                        optUnsigned.ID       = "opt" + option.name;
                        optUnsigned.MinValue = 0;
                        optUnsigned.Value    = Convert.ToDouble(option.@default);
                        stkUnsigned.Items.Add(optUnsigned);
                        Label lblUnsigned = new Label();
                        lblUnsigned.Text = option.description;
                        stkUnsigned.Items.Add(lblUnsigned);
                        stkOptions.Items.Add(stkUnsigned);
                        break;
                    case "System.Single":
                    case "System.Double":
                        StackLayout stkFloat = new StackLayout();
                        stkFloat.Orientation = Orientation.Horizontal;
                        NumericStepper optFloat = new NumericStepper();
                        optFloat.ID            = "opt" + option.name;
                        optFloat.DecimalPlaces = 2;
                        optFloat.Value         = Convert.ToDouble(option.@default);
                        stkFloat.Items.Add(optFloat);
                        Label lblFloat = new Label();
                        lblFloat.Text = option.description;
                        stkFloat.Items.Add(lblFloat);
                        stkOptions.Items.Add(stkFloat);
                        break;
                    case "System.Guid":
                        // TODO
                        break;
                    case "System.String":
                        StackLayout stkString = new StackLayout();
                        stkString.Orientation = Orientation.Horizontal;
                        Label lblString = new Label();
                        lblString.Text = option.description;
                        stkString.Items.Add(lblString);
                        TextBox optString = new TextBox();
                        optString.ID   = "opt" + option.name;
                        optString.Text = (string)option.@default;
                        stkString.Items.Add(optString);
                        stkOptions.Items.Add(stkString);
                        break;
                }

            grpOptions.Content = stkOptions;
        }

        void OnBtnDestinationClick(object sender, EventArgs e)
        {
            if(!(cmbFormat.SelectedValue is IWritableImage plugin)) return;

            SaveFileDialog dlgDestination = new SaveFileDialog {Title = "Choose destination file"};
            dlgDestination.Filters.Add(new FileFilter(plugin.Name, plugin.KnownExtensions.ToArray()));

            DialogResult result = dlgDestination.ShowDialog(this);

            if(result != DialogResult.Ok)
            {
                txtDestination.Text = "";
                outputPrefix        = null;
                return;
            }

            if(string.IsNullOrEmpty(Path.GetExtension(dlgDestination.FileName)))
                dlgDestination.FileName += plugin.KnownExtensions.First();

            txtDestination.Text = dlgDestination.FileName;
            outputPrefix = Path.Combine(Path.GetDirectoryName(dlgDestination.FileName),
                                        Path.GetFileNameWithoutExtension(dlgDestination.FileName));
            chkResume.Checked = true;
        }

        void OnChkSidecarCheckedChanged(object sender, EventArgs e)
        {
            cmbEncoding.Visible = chkSidecar.Checked.Value;
            lblEncoding.Visible = chkSidecar.Checked.Value;
        }

        void OnChkExistingMetadataCheckedChanged(object sender, EventArgs e)
        {
            if(chkExistingMetadata.Checked == false)
            {
                sidecar = null;
                return;
            }

            OpenFileDialog dlgMetadata =
                new OpenFileDialog {Title = "Choose existing metadata sidecar", CheckFileExists = true};
            dlgMetadata.Filters.Add(new FileFilter("CICM XML metadata", ".xml"));

            DialogResult result = dlgMetadata.ShowDialog(this);

            if(result != DialogResult.Ok)
            {
                chkExistingMetadata.Checked = false;
                return;
            }

            XmlSerializer sidecarXs = new XmlSerializer(typeof(CICMMetadataType));
            try
            {
                StreamReader sr = new StreamReader(dlgMetadata.FileName);
                sidecar = (CICMMetadataType)sidecarXs.Deserialize(sr);
                sr.Close();
            }
            catch
            {
                MessageBox.Show("Incorrect metadata sidecar file...", MessageBoxType.Error);
                chkExistingMetadata.Checked = false;
            }
        }

        void OnChkResumeCheckedChanged(object sender, EventArgs e)
        {
            if(chkResume.Checked == false) return;

            if(outputPrefix != null) CheckResumeFile();
        }

        void CheckResumeFile()
        {
            Resume        resume = null;
            XmlSerializer xs     = new XmlSerializer(typeof(Resume));
            try
            {
                StreamReader sr = new StreamReader(outputPrefix + ".resume.xml");
                resume = (Resume)xs.Deserialize(sr);
                sr.Close();
            }
            catch
            {
                MessageBox.Show("Incorrect resume file, cannot use it...", MessageBoxType.Error);
                chkResume.Checked = false;
                return;
            }

            if(resume == null || resume.NextBlock <= resume.LastBlock || resume.BadBlocks.Count != 0) return;

            MessageBox.Show("Media already dumped correctly, please choose another destination...",
                            MessageBoxType.Warning);
            chkResume.Checked = false;
        }

        void OnBtnCancelClick(object sender, EventArgs e)
        {
            Close();
        }

        void OnBtnAbortClick(object sender, EventArgs e)
        {
            dumper.Abort();
        }

        void OnBtnDumpClick(object sender, EventArgs e)
        {
            Device dev;
            try
            {
                dev = new Device(devicePath);

                if(dev.Error)
                {
                    MessageBox.Show($"Error {dev.LastError} opening device.", MessageBoxType.Error);
                    return;
                }
            }
            catch(Exception exception)
            {
                MessageBox.Show($"Exception {exception.Message} opening device.", MessageBoxType.Error);
                return;
            }

            Statistics.AddDevice(dev);
            Statistics.AddCommand("dump-media");

            if(!(cmbFormat.SelectedValue is IWritableImage outputFormat))
            {
                MessageBox.Show("Cannot open output plugin.", MessageBoxType.Error);
                return;
            }

            Encoding encoding = null;

            if(cmbEncoding.SelectedValue is CommonEncodingInfo encodingInfo)
                try { encoding = Claunia.Encoding.Encoding.GetEncoding(encodingInfo.Name); }
                catch(ArgumentException)
                {
                    MessageBox.Show("Specified encoding is not supported.", MessageBoxType.Error);
                    return;
                }

            DumpLog dumpLog = new DumpLog(outputPrefix + ".log", dev);

            dumpLog.WriteLine("Output image format: {0}.", outputFormat.Name);

            Dictionary<string, string> parsedOptions = new Dictionary<string, string>();

            if(grpOptions.Content is StackLayout stkOptions)
                foreach(Control option in stkOptions.Children)
                {
                    string value;

                    switch(option)
                    {
                        case CheckBox optBoolean:
                            value = optBoolean.Checked?.ToString();
                            break;
                        case NumericStepper optNumber:
                            value = optNumber.Value.ToString(CultureInfo.CurrentCulture);
                            break;
                        case TextBox optString:
                            value = optString.Text;
                            break;
                        default: continue;
                    }

                    string key = option.ID.Substring(3);

                    parsedOptions.Add(key, value);
                }

            dumper = new Dump(chkResume.Checked      == true, dev, devicePath, outputFormat, (ushort)stpRetries.Value,
                              chkForce.Checked       == true, false, chkPersistent.Checked == true,
                              chkStopOnError.Checked == true, resume, dumpLog, encoding, outputPrefix,
                              txtDestination.Text, parsedOptions, sidecar, (uint)stpSkipped.Value,
                              chkExistingMetadata.Checked == false, chkTrim.Checked == false,
                              chkTrack1Pregap.Checked     == true);

            /*dumper.UpdateStatus         += Progress.UpdateStatus;
            dumper.ErrorMessage         += Progress.ErrorMessage;
            dumper.StoppingErrorMessage += Progress.ErrorMessage;
            dumper.UpdateProgress       += Progress.UpdateProgress;
            dumper.PulseProgress        += Progress.PulseProgress;
            dumper.InitProgress         += Progress.InitProgress;
            dumper.EndProgress          += Progress.EndProgress;*/

            dumper.Start();

            dev.Close();
        }

        class CommonEncodingInfo
        {
            public string Name        { get; set; }
            public string DisplayName { get; set; }
        }

        #region XAML IDs
        ComboBox       cmbFormat;
        TextBox        txtDestination;
        Button         btnDestination;
        CheckBox       chkStopOnError;
        CheckBox       chkForce;
        CheckBox       chkPersistent;
        CheckBox       chkResume;
        CheckBox       chkTrack1Pregap;
        CheckBox       chkSidecar;
        CheckBox       chkTrim;
        CheckBox       chkExistingMetadata;
        ComboBox       cmbEncoding;
        GroupBox       grpOptions;
        NumericStepper stpRetries;
        NumericStepper stpSkipped;
        Label          lblEncoding;
        Button         btnCancel;
        Button         btnDump;
        #endregion
    }
}