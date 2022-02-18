// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MediaDumpViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model and code for the media dump window.
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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Metadata;
using Aaru.Core;
using Aaru.Core.Devices.Dumping;
using Aaru.Core.Logging;
using Aaru.Core.Media.Info;
using Aaru.Devices;
using Aaru.Gui.Models;
using Avalonia.Controls;
using Avalonia.Threading;
using DynamicData;
using JetBrains.Annotations;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using ReactiveUI;
using Schemas;
using DeviceInfo = Aaru.Core.Devices.Info.DeviceInfo;
using MediaType = Aaru.CommonTypes.MediaType;

namespace Aaru.Gui.ViewModels.Windows
{
    public sealed class MediaDumpViewModel : ViewModelBase
    {
        readonly string  _devicePath;
        readonly Window  _view;
        bool             _closeVisible;
        string           _destination;
        bool             _destinationEnabled;
        Device           _dev;
        Dump             _dumper;
        string           _encodingEnabled;
        bool             _encodingVisible;
        bool             _existingMetadata;
        bool             _force;
        string           _formatReadOnly;
        string           _log;
        bool             _optionsVisible;
        string           _outputPrefix;
        bool             _persistent;
        bool             _progress1Visible;
        bool             _progress2Indeterminate;
        double           _progress2MaxValue;
        string           _progress2Text;
        double           _progress2Value;
        bool             _progress2Visible;
        bool             _progressIndeterminate;
        double           _progressMaxValue;
        string           _progressText;
        double           _progressValue;
        bool             _progressVisible;
        Resume           _resume;
        double           _retries;
        EncodingModel    _selectedEncoding;
        ImagePluginModel _selectedPlugin;
        CICMMetadataType _sidecar;
        double           _skipped;
        bool             _startVisible;
        bool             _stopEnabled;
        bool             _stopOnError;
        bool             _stopVisible;
        bool             _track1Pregap;
        bool             _track1PregapVisible;
        bool             _trim;
        bool             _useResume;
        bool             _useSidecar;

        public MediaDumpViewModel(string devicePath, DeviceInfo deviceInfo, Window view,
                                  [CanBeNull] ScsiInfo scsiInfo = null)
        {
            _view              = view;
            DestinationEnabled = true;
            StartVisible       = true;
            CloseVisible       = true;
            OptionsVisible     = true;
            StartCommand       = ReactiveCommand.Create(ExecuteStartCommand);
            CloseCommand       = ReactiveCommand.Create(ExecuteCloseCommand);
            StopCommand        = ReactiveCommand.Create(ExecuteStopCommand);
            DestinationCommand = ReactiveCommand.Create(ExecuteDestinationCommand);
            PluginsList        = new ObservableCollection<ImagePluginModel>();
            Encodings          = new ObservableCollection<EncodingModel>();

            // Defaults
            StopOnError      = false;
            Force            = false;
            Persistent       = true;
            Resume           = true;
            Track1Pregap     = false;
            Sidecar          = true;
            Trim             = true;
            ExistingMetadata = false;
            Retries          = 5;
            Skipped          = 512;

            MediaType mediaType;

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

            PluginBase plugins = GetPluginBase.Instance;

            foreach(IWritableImage plugin in
                plugins.WritableImages.Values.Where(p => p.SupportedMediaTypes.Contains(mediaType)))
                PluginsList.Add(new ImagePluginModel
                {
                    Plugin = plugin
                });

            Encodings.AddRange(Encoding.GetEncodings().Select(info => new EncodingModel
            {
                Name        = info.Name,
                DisplayName = info.GetEncoding().EncodingName
            }));

            Encodings.AddRange(Claunia.Encoding.Encoding.GetEncodings().Select(info => new EncodingModel
            {
                Name        = info.Name,
                DisplayName = info.DisplayName
            }));

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
                case MediaType.CVD:
                    Track1PregapVisible = true;

                    break;
                default:
                    Track1PregapVisible = false;

                    break;
            }

            _devicePath = devicePath;
        }

        public ReactiveCommand<Unit, Unit> StartCommand       { get; }
        public ReactiveCommand<Unit, Unit> CloseCommand       { get; }
        public ReactiveCommand<Unit, Unit> StopCommand        { get; }
        public ReactiveCommand<Unit, Unit> DestinationCommand { get; }

        public ObservableCollection<ImagePluginModel> PluginsList { get; }
        public ObservableCollection<EncodingModel>    Encodings   { get; }

        public string Title { get; }

        public bool OptionsVisible
        {
            get => _optionsVisible;
            set => this.RaiseAndSetIfChanged(ref _optionsVisible, value);
        }

        public ImagePluginModel SelectedPlugin
        {
            get => _selectedPlugin;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedPlugin, value);

                Destination = "";

                if(value is null)
                {
                    DestinationEnabled = false;

                    return;
                }

                DestinationEnabled = true;

                if(!value.Plugin.SupportedOptions.Any())
                {
                    // Hide options
                }

                /* TODO: Plugin options
                grpOptions.Visible = true;

                var stkOptions = new StackLayout
                {
                    Orientation = Orientation.Vertical
                };

                foreach((string name, Type type, string description, object @default) option in plugin.SupportedOptions)
                    switch(option.type.ToString())
                    {
                        case "System.Boolean":
                            var optBoolean = new CheckBox();
                            optBoolean.ID      = "opt" + option.name;
                            optBoolean.Text    = option.description;
                            optBoolean.Checked = (bool)option.@default;
                            stkOptions.Items.Add(optBoolean);

                            break;
                        case "System.SByte":
                        case "System.Int16":
                        case "System.Int32":
                        case "System.Int64":
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
                        case "System.Byte":
                        case "System.UInt16":
                        case "System.UInt32":
                        case "System.UInt64":
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
                        case "System.Single":
                        case "System.Double":
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
                        case "System.Guid":
                            // TODO
                            break;
                        case "System.String":
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
    */
            }
        }

        public string FormatReadOnly
        {
            get => _formatReadOnly;
            set => this.RaiseAndSetIfChanged(ref _formatReadOnly, value);
        }

        public string Destination
        {
            get => _destination;
            set => this.RaiseAndSetIfChanged(ref _destination, value);
        }

        public bool DestinationEnabled
        {
            get => _destinationEnabled;
            set => this.RaiseAndSetIfChanged(ref _destinationEnabled, value);
        }

        public bool StopOnError
        {
            get => _stopOnError;
            set => this.RaiseAndSetIfChanged(ref _stopOnError, value);
        }

        public bool Force
        {
            get => _force;
            set => this.RaiseAndSetIfChanged(ref _force, value);
        }

        public double Retries
        {
            get => _retries;
            set => this.RaiseAndSetIfChanged(ref _retries, value);
        }

        public bool Persistent
        {
            get => _persistent;
            set => this.RaiseAndSetIfChanged(ref _persistent, value);
        }

        public bool Resume
        {
            get => _useResume;
            set
            {
                this.RaiseAndSetIfChanged(ref _useResume, value);

                if(value == false)
                    return;

                if(_outputPrefix != null)
                    CheckResumeFile();
            }
        }

        public bool Track1Pregap
        {
            get => _track1Pregap;
            set => this.RaiseAndSetIfChanged(ref _track1Pregap, value);
        }

        public bool Track1PregapVisible
        {
            get => _track1PregapVisible;
            set => this.RaiseAndSetIfChanged(ref _track1PregapVisible, value);
        }

        public double Skipped
        {
            get => _skipped;
            set => this.RaiseAndSetIfChanged(ref _skipped, value);
        }

        public bool Sidecar
        {
            get => _useSidecar;
            set
            {
                this.RaiseAndSetIfChanged(ref _useSidecar, value);
                EncodingVisible = value;
            }
        }

        public bool EncodingVisible
        {
            get => _encodingVisible;
            set => this.RaiseAndSetIfChanged(ref _encodingVisible, value);
        }

        public bool Trim
        {
            get => _trim;
            set => this.RaiseAndSetIfChanged(ref _trim, value);
        }

        public bool ExistingMetadata
        {
            get => _existingMetadata;
            set
            {
                this.RaiseAndSetIfChanged(ref _existingMetadata, value);

                if(value == false)
                {
                    _sidecar = null;

                    return;
                }

                var dlgMetadata = new OpenFileDialog
                {
                    Title = "Choose existing metadata sidecar"
                };

                dlgMetadata.Filters.Add(new FileDialogFilter
                {
                    Name = "CICM XML metadata",
                    Extensions = new List<string>(new[]
                    {
                        ".xml"
                    })
                });

                string[] result = dlgMetadata.ShowAsync(_view).Result;

                if(result?.Length != 1)
                {
                    ExistingMetadata = false;

                    return;
                }

                var sidecarXs = new XmlSerializer(typeof(CICMMetadataType));

                try
                {
                    var sr = new StreamReader(result[0]);
                    _sidecar = (CICMMetadataType)sidecarXs.Deserialize(sr);
                    sr.Close();
                }
                catch
                {
                    // ReSharper disable AssignmentIsFullyDiscarded
                    _ = MessageBoxManager.

                        // ReSharper restore AssignmentIsFullyDiscarded
                        GetMessageBoxStandardWindow("Error", "Incorrect metadata sidecar file...", ButtonEnum.Ok,
                                                    Icon.Error).ShowDialog(_view).Result;

                    ExistingMetadata = false;
                }
            }
        }

        public EncodingModel SelectedEncoding
        {
            get => _selectedEncoding;
            set => this.RaiseAndSetIfChanged(ref _selectedEncoding, value);
        }

        public string EncodingEnabled
        {
            get => _encodingEnabled;
            set => this.RaiseAndSetIfChanged(ref _encodingEnabled, value);
        }

        public bool ProgressVisible
        {
            get => _progressVisible;
            set => this.RaiseAndSetIfChanged(ref _progressVisible, value);
        }

        public string Log
        {
            get => _log;
            set => this.RaiseAndSetIfChanged(ref _log, value);
        }

        public bool Progress1Visible
        {
            get => _progress1Visible;
            set => this.RaiseAndSetIfChanged(ref _progress1Visible, value);
        }

        public string ProgressText
        {
            get => _progressText;
            set => this.RaiseAndSetIfChanged(ref _progressText, value);
        }

        public double ProgressValue
        {
            get => _progressValue;
            set => this.RaiseAndSetIfChanged(ref _progressValue, value);
        }

        public double ProgressMaxValue
        {
            get => _progressMaxValue;
            set => this.RaiseAndSetIfChanged(ref _progressMaxValue, value);
        }

        public bool ProgressIndeterminate
        {
            get => _progressIndeterminate;
            set => this.RaiseAndSetIfChanged(ref _progressIndeterminate, value);
        }

        public bool Progress2Visible
        {
            get => _progress2Visible;
            set => this.RaiseAndSetIfChanged(ref _progress2Visible, value);
        }

        public string Progress2Text
        {
            get => _progress2Text;
            set => this.RaiseAndSetIfChanged(ref _progress2Text, value);
        }

        public double Progress2Value
        {
            get => _progress2Value;
            set => this.RaiseAndSetIfChanged(ref _progress2Value, value);
        }

        public double Progress2MaxValue
        {
            get => _progress2MaxValue;
            set => this.RaiseAndSetIfChanged(ref _progress2MaxValue, value);
        }

        public bool Progress2Indeterminate
        {
            get => _progress2Indeterminate;
            set => this.RaiseAndSetIfChanged(ref _progress2Indeterminate, value);
        }

        public bool StartVisible
        {
            get => _startVisible;
            set => this.RaiseAndSetIfChanged(ref _startVisible, value);
        }

        public bool CloseVisible
        {
            get => _closeVisible;
            set => this.RaiseAndSetIfChanged(ref _closeVisible, value);
        }

        public bool StopVisible
        {
            get => _stopVisible;
            set => this.RaiseAndSetIfChanged(ref _stopVisible, value);
        }

        public bool StopEnabled
        {
            get => _stopEnabled;
            set => this.RaiseAndSetIfChanged(ref _stopEnabled, value);
        }

        async void ExecuteDestinationCommand()
        {
            if(SelectedPlugin is null)
                return;

            var dlgDestination = new SaveFileDialog
            {
                Title = "Choose destination file"
            };

            dlgDestination.Filters.Add(new FileDialogFilter
            {
                Name       = SelectedPlugin.Plugin.Name,
                Extensions = SelectedPlugin.Plugin.KnownExtensions.ToList()
            });

            string result = await dlgDestination.ShowAsync(_view);

            if(result is null)
            {
                Destination   = "";
                _outputPrefix = null;

                return;
            }

            if(string.IsNullOrEmpty(Path.GetExtension(result)))
                result += SelectedPlugin.Plugin.KnownExtensions.First();

            Destination = result;

            _outputPrefix = Path.Combine(Path.GetDirectoryName(result) ?? "", Path.GetFileNameWithoutExtension(result));

            Resume = true;
        }

        async void CheckResumeFile()
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
                await MessageBoxManager.
                      GetMessageBoxStandardWindow("Error", "Incorrect resume file, cannot use it...", ButtonEnum.Ok,
                                                  Icon.Error).ShowDialog(_view);

                Resume = false;

                return;
            }

            if(_resume           == null              ||
               _resume.NextBlock <= _resume.LastBlock ||
               (_resume.BadBlocks.Count != 0 && !_resume.Tape))
                return;

            await MessageBoxManager.
                  GetMessageBoxStandardWindow("Warning",
                                              "Media already dumped correctly, please choose another destination...",
                                              ButtonEnum.Ok, Icon.Warning).ShowDialog(_view);

            Resume = false;
        }

        void ExecuteCloseCommand() => _view.Close();

        internal void ExecuteStopCommand()
        {
            StopEnabled = false;
            _dumper?.Abort();
        }

        void ExecuteStartCommand()
        {
            Log                = "";
            CloseVisible       = false;
            StartVisible       = false;
            StopVisible        = true;
            StopEnabled        = true;
            ProgressVisible    = true;
            DestinationEnabled = false;
            OptionsVisible     = false;

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

            if(SelectedPlugin is null)
            {
                StoppingErrorMessage("Cannot open output plugin.");

                return;
            }

            Encoding encoding = null;

            if(!(SelectedEncoding is null))
                try
                {
                    encoding = Claunia.Encoding.Encoding.GetEncoding(SelectedEncoding.Name);
                }
                catch(ArgumentException)
                {
                    StoppingErrorMessage("Specified encoding is not supported.");

                    return;
                }

            Dictionary<string, string> parsedOptions = new Dictionary<string, string>();

            /* TODO: Options
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
            */

            var dumpLog = new DumpLog(_outputPrefix + ".log", _dev, false);

            dumpLog.WriteLine("Output image format: {0}.", SelectedPlugin.Name);

            var errorLog = new ErrorLog(_outputPrefix + ".error.log");

            _dumper = new Dump(Resume, _dev, _devicePath, SelectedPlugin.Plugin, (ushort)Retries, Force, false,
                               Persistent, StopOnError, _resume, dumpLog, encoding, _outputPrefix, Destination,
                               parsedOptions, _sidecar, (uint)Skipped, ExistingMetadata == false, Trim == false,
                               Track1Pregap, true, false, DumpSubchannel.Any, 0, false, false, false, false, false,
                               true, errorLog, false, 64, true, true, false);

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

        async void WorkFinished() => await Dispatcher.UIThread.InvokeAsync(() =>
        {
            CloseVisible     = true;
            StopVisible      = false;
            Progress1Visible = false;
            Progress2Visible = false;
        });

        async void EndProgress2() => await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Progress2Visible = false;
        });

        async void UpdateProgress2(string text, long current, long maximum) =>
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Progress2Text          = text;
                Progress2Indeterminate = false;

                Progress2MaxValue = maximum;
                Progress2Value    = current;
            });

        async void InitProgress2() => await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Progress2Visible = true;
        });

        async void EndProgress() => await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Progress1Visible = false;
        });

        async void UpdateProgress(string text, long current, long maximum) =>
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ProgressText          = text;
                ProgressIndeterminate = false;

                ProgressMaxValue = maximum;
                ProgressValue    = current;
            });

        async void InitProgress() => await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Progress1Visible = true;
        });

        async void PulseProgress(string text) => await Dispatcher.UIThread.InvokeAsync(() =>
        {
            ProgressText          = text;
            ProgressIndeterminate = true;
        });

        async void StoppingErrorMessage(string text) => await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            ErrorMessage(text);

            await MessageBoxManager.GetMessageBoxStandardWindow("Error", $"{text}", ButtonEnum.Ok, Icon.Error).
                                    ShowDialog(_view);

            WorkFinished();
        });

        async void ErrorMessage(string text) => await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Log += text + Environment.NewLine;
        });

        async void UpdateStatus(string text) => await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Log += text + Environment.NewLine;
        });
    }
}