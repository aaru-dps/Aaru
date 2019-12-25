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
using System.Threading;
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

// ReSharper disable UnusedMember.Local

namespace DiscImageChef.Gui.Forms
{
    public class frmDump : Form
    {
        readonly string  _devicePath;
        Device           _dev;
        Dump             _dumper;
        string           _outputPrefix;
        Resume           _resume;
        CICMMetadataType _sidecar;

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

            if(scsiInfo != null)
                mediaType = scsiInfo.MediaType;
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
                        if(deviceInfo.IsPcmcia)
                            mediaType = MediaType.PCCardTypeII;
                        else if(deviceInfo.IsCompactFlash)
                            mediaType = MediaType.CompactFlash;
                        else
                            mediaType = MediaType.GENERIC_HDD;

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

            List<CommonEncodingInfo> encodings = Encoding.GetEncodings().Select(info => new CommonEncodingInfo
            {
                Name = info.Name, DisplayName = info.GetEncoding().EncodingName
            }).ToList();

            encodings.AddRange(Claunia.Encoding.Encoding.GetEncodings().Select(info => new CommonEncodingInfo
            {
                Name = info.Name, DisplayName = info.DisplayName
            }));

            ObservableCollection<CommonEncodingInfo> lstEncodings = new ObservableCollection<CommonEncodingInfo>();

            foreach(CommonEncodingInfo info in encodings.OrderBy(t => t.DisplayName))
                lstEncodings.Add(info);

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

            _devicePath = devicePath;
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

            var stkOptions = new StackLayout
            {
                Orientation = Orientation.Vertical
            };

            foreach((string name, Type type, string description, object @default) option in plugin.SupportedOptions)
                switch(option.type.ToString())
                {
                    case"System.Boolean":
                        var optBoolean = new CheckBox();
                        optBoolean.ID      = "opt" + option.name;
                        optBoolean.Text    = option.description;
                        optBoolean.Checked = (bool)option.@default;
                        stkOptions.Items.Add(optBoolean);

                        break;
                    case"System.SByte":
                    case"System.Int16":
                    case"System.Int32":
                    case"System.Int64":
                        var stkNumber = new StackLayout();
                        stkNumber.Orientation = Orientation.Horizontal;
                        var optNumber = new NumericStepper();
                        optNumber.ID    = "opt" + option.name;
                        optNumber.Value = Convert.ToDouble(option.@default);
                        stkNumber.Items.Add(optNumber);
                        var lblNumber = new Label();
                        lblNumber.Text = option.description;
                        stkNumber.Items.Add(lblNumber);
                        stkOptions.Items.Add(stkNumber);

                        break;
                    case"System.Byte":
                    case"System.UInt16":
                    case"System.UInt32":
                    case"System.UInt64":
                        var stkUnsigned = new StackLayout();
                        stkUnsigned.Orientation = Orientation.Horizontal;
                        var optUnsigned = new NumericStepper();
                        optUnsigned.ID       = "opt" + option.name;
                        optUnsigned.MinValue = 0;
                        optUnsigned.Value    = Convert.ToDouble(option.@default);
                        stkUnsigned.Items.Add(optUnsigned);
                        var lblUnsigned = new Label();
                        lblUnsigned.Text = option.description;
                        stkUnsigned.Items.Add(lblUnsigned);
                        stkOptions.Items.Add(stkUnsigned);

                        break;
                    case"System.Single":
                    case"System.Double":
                        var stkFloat = new StackLayout();
                        stkFloat.Orientation = Orientation.Horizontal;
                        var optFloat = new NumericStepper();
                        optFloat.ID            = "opt" + option.name;
                        optFloat.DecimalPlaces = 2;
                        optFloat.Value         = Convert.ToDouble(option.@default);
                        stkFloat.Items.Add(optFloat);
                        var lblFloat = new Label();
                        lblFloat.Text = option.description;
                        stkFloat.Items.Add(lblFloat);
                        stkOptions.Items.Add(stkFloat);

                        break;
                    case"System.Guid":
                        // TODO
                        break;
                    case"System.String":
                        var stkString = new StackLayout();
                        stkString.Orientation = Orientation.Horizontal;
                        var lblString = new Label();
                        lblString.Text = option.description;
                        stkString.Items.Add(lblString);
                        var optString = new TextBox();
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
            if(!(cmbFormat.SelectedValue is IWritableImage plugin))
                return;

            var dlgDestination = new SaveFileDialog
            {
                Title = "Choose destination file"
            };

            dlgDestination.Filters.Add(new FileFilter(plugin.Name, plugin.KnownExtensions.ToArray()));

            DialogResult result = dlgDestination.ShowDialog(this);

            if(result != DialogResult.Ok)
            {
                txtDestination.Text = "";
                _outputPrefix       = null;

                return;
            }

            if(string.IsNullOrEmpty(Path.GetExtension(dlgDestination.FileName)))
                dlgDestination.FileName += plugin.KnownExtensions.First();

            txtDestination.Text = dlgDestination.FileName;

            _outputPrefix = Path.Combine(Path.GetDirectoryName(dlgDestination.FileName),
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
                _sidecar = null;

                return;
            }

            var dlgMetadata = new OpenFileDialog
            {
                Title = "Choose existing metadata sidecar", CheckFileExists = true
            };

            dlgMetadata.Filters.Add(new FileFilter("CICM XML metadata", ".xml"));

            DialogResult result = dlgMetadata.ShowDialog(this);

            if(result != DialogResult.Ok)
            {
                chkExistingMetadata.Checked = false;

                return;
            }

            var sidecarXs = new XmlSerializer(typeof(CICMMetadataType));

            try
            {
                var sr = new StreamReader(dlgMetadata.FileName);
                _sidecar = (CICMMetadataType)sidecarXs.Deserialize(sr);
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
            if(chkResume.Checked == false)
                return;

            if(_outputPrefix != null)
                CheckResumeFile();
        }

        void CheckResumeFile()
        {
            _resume = null;
            var xs = new XmlSerializer(typeof(Resume));

            try
            {
                var sr = new StreamReader(_outputPrefix + ".resume.xml");
                _resume = (Resume)xs.Deserialize(sr);
                sr.Close();
            }
            catch
            {
                MessageBox.Show("Incorrect resume file, cannot use it...", MessageBoxType.Error);
                chkResume.Checked = false;

                return;
            }

            if(_resume           == null              ||
               _resume.NextBlock <= _resume.LastBlock ||
               (_resume.BadBlocks.Count != 0 && !_resume.Tape))
                return;

            MessageBox.Show("Media already dumped correctly, please choose another destination...",
                            MessageBoxType.Warning);

            chkResume.Checked = false;
        }

        void OnBtnCloseClick(object sender, EventArgs e) => Close();

        void OnBtnStopClick(object sender, EventArgs e)
        {
            btnStop.Enabled = false;
            _dumper.Abort();
        }

        void OnBtnDumpClick(object sender, EventArgs e)
        {
            txtLog.Text            = "";
            btnClose.Visible       = false;
            btnStart.Visible       = false;
            btnStop.Visible        = true;
            btnStop.Enabled        = true;
            stkProgress.Visible    = true;
            btnDestination.Visible = false;
            stkOptions.Visible     = false;

            UpdateStatus("Opening device...");

            try
            {
                _dev = new Device(_devicePath);

                if(_dev.IsRemote)
                    Statistics.AddRemote(_dev.RemoteApplication, _dev.RemoteVersion, _dev.RemoteOperatingSystem,
                                         _dev.RemoteOperatingSystemVersion, _dev.RemoteArchitecture);

                if(_dev.Error)
                {
                    StoppingErrorMessage($"Error {_dev.LastError} opening device.");

                    return;
                }
            }
            catch(Exception exception)
            {
                StoppingErrorMessage($"Exception {exception.Message} opening device.");

                return;
            }

            Statistics.AddDevice(_dev);
            Statistics.AddCommand("dump-media");

            if(!(cmbFormat.SelectedValue is IWritableImage outputFormat))
            {
                StoppingErrorMessage("Cannot open output plugin.");

                return;
            }

            Encoding encoding = null;

            if(cmbEncoding.SelectedValue is CommonEncodingInfo encodingInfo)
                try
                {
                    encoding = Claunia.Encoding.Encoding.GetEncoding(encodingInfo.Name);
                }
                catch(ArgumentException)
                {
                    StoppingErrorMessage("Specified encoding is not supported.");

                    return;
                }

            Dictionary<string, string> parsedOptions = new Dictionary<string, string>();

            if(grpOptions.Content is StackLayout stkFormatOptions)
                foreach(Control option in stkFormatOptions.Children)
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

            var dumpLog = new DumpLog(_outputPrefix + ".log", _dev);

            dumpLog.WriteLine("Output image format: {0}.", outputFormat.Name);

            _dumper = new Dump(chkResume.Checked == true, _dev, _devicePath, outputFormat,
                               (ushort)stpRetries.Value,
                               chkForce.Checked       == true, false, chkPersistent.Checked == true,
                               chkStopOnError.Checked == true, _resume, dumpLog, encoding, _outputPrefix,
                               txtDestination.Text, parsedOptions, _sidecar, (uint)stpSkipped.Value,
                               chkExistingMetadata.Checked == false, chkTrim.Checked == false,
                               chkTrack1Pregap.Checked     == true, true);

            new Thread(DoWork).Start();
        }

        void DoWork()
        {
            _dumper.UpdateStatus         += UpdateStatus;
            _dumper.ErrorMessage         += ErrorMessage;
            _dumper.StoppingErrorMessage += StoppingErrorMessage;
            _dumper.PulseProgress        += PulseProgress;
            _dumper.InitProgress         += InitProgress;
            _dumper.UpdateProgress       += UpdateProgress;
            _dumper.EndProgress          += EndProgress;
            _dumper.InitProgress2        += InitProgress2;
            _dumper.UpdateProgress2      += UpdateProgress2;
            _dumper.EndProgress2         += EndProgress2;

            _dumper.Start();

            _dev.Close();

            WorkFinished();
        }

        void WorkFinished() => Application.Instance.Invoke(() =>
        {
            btnClose.Visible     = true;
            btnStop.Visible      = false;
            stkProgress1.Visible = false;
            stkProgress2.Visible = false;
        });

        void EndProgress2() => Application.Instance.Invoke(() =>
        {
            stkProgress2.Visible = false;
        });

        void UpdateProgress2(string text, long current, long maximum) => Application.Instance.Invoke(() =>
        {
            lblProgress2.Text          = text;
            prgProgress2.Indeterminate = false;
            prgProgress2.MinValue      = 0;

            if(maximum > int.MaxValue)
            {
                prgProgress2.MaxValue = (int)(maximum / int.MaxValue);
                prgProgress2.Value    = (int)(current / int.MaxValue);
            }
            else
            {
                prgProgress2.MaxValue = (int)maximum;
                prgProgress2.Value    = (int)current;
            }
        });

        void InitProgress2() => Application.Instance.Invoke(() =>
        {
            stkProgress2.Visible = true;
        });

        void EndProgress() => Application.Instance.Invoke(() =>
        {
            stkProgress1.Visible = false;
        });

        void UpdateProgress(string text, long current, long maximum) => Application.Instance.Invoke(() =>
        {
            lblProgress.Text          = text;
            prgProgress.Indeterminate = false;
            prgProgress.MinValue      = 0;

            if(maximum > int.MaxValue)
            {
                prgProgress.MaxValue = (int)(maximum / int.MaxValue);
                prgProgress.Value    = (int)(current / int.MaxValue);
            }
            else
            {
                prgProgress.MaxValue = (int)maximum;
                prgProgress.Value    = (int)current;
            }
        });

        void InitProgress() => Application.Instance.Invoke(() =>
        {
            stkProgress1.Visible = true;
        });

        void PulseProgress(string text) => Application.Instance.Invoke(() =>
        {
            lblProgress.Text          = text;
            prgProgress.Indeterminate = true;
        });

        void StoppingErrorMessage(string text) => Application.Instance.Invoke(() =>
        {
            ErrorMessage(text);
            MessageBox.Show(text, MessageBoxType.Error);
            WorkFinished();
        });

        void ErrorMessage(string text) => Application.Instance.Invoke(() =>
        {
            txtLog.Append(text + Environment.NewLine, true);
        });

        void UpdateStatus(string text) => Application.Instance.Invoke(() =>
        {
            txtLog.Append(text + Environment.NewLine, true);
        });

        class CommonEncodingInfo
        {
            public string Name        { get; set; }
            public string DisplayName { get; set; }
        }

        #region XAML IDs
        // ReSharper disable InconsistentNaming
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
        Button         btnClose;
        Button         btnStart;
        Button         btnStop;
        StackLayout    stkProgress;
        Label          lblDestinationLabel;
        Label          lblDestination;
        TextArea       txtLog;
        StackLayout    stkProgress1;
        Label          lblProgress;
        ProgressBar    prgProgress;
        StackLayout    stkProgress2;
        Label          lblProgress2;
        ProgressBar    prgProgress2;
        StackLayout    stkOptions;

        // ReSharper restore InconsistentNaming
        #endregion
    }
}